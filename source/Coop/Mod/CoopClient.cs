using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Common;
using Coop.Mod.Config;
using Coop.Mod.Data;
using Coop.Mod.GameSync;
using Coop.Mod.Managers;
using Coop.Mod.Persistence;
using Coop.Mod.Persistence.RemoteAction;
using Coop.Mod.Serializers;
using Coop.Mod.Serializers.Custom;
using Coop.NetImpl;
using Coop.NetImpl.LiteNet;
using CoopFramework;
using JetBrains.Annotations;
using Network;
using Network.Infrastructure;
using Network.Protocol;
using NLog;
using RailgunNet.Connection.Client;
using RailgunNet.Logic;
using Sync.Store;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using System.Reflection;
using Logger = NLog.Logger;
using SandBox.View.Map;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.Engine;
using SandBox;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;
using TaleWorlds.Engine.Screens;

namespace Coop.Mod
{
    class GameClientPacketHandlerAttribute : PacketHandlerAttribute
    {
        public GameClientPacketHandlerAttribute(ECoopClientState state, EPacket eType)
        {
            State = state;
            Type = eType;
        }
    }

    public class CoopClient : IUpdateable, IClientAccess
    {
        #region Private
        private const int MaxReconnectAttempts = 2;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<CoopClient> m_Instance =
            new Lazy<CoopClient>(() => new CoopClient(new ClientConfiguration()));
        private readonly CoopClientSM m_CoopClientSM;

        [NotNull] private readonly LiteNetManagerClient m_NetManager;
        private readonly UpdateableList m_Updateables = new UpdateableList();

        private MBGameManager gameManager;

        private int m_ReconnectAttempts = MaxReconnectAttempts;
        private Guid m_HeroGUID;
        #endregion
        public Action<PersistenceClient> OnPersistenceInitialized;

        public Action<RemoteStoreClient> RemoteStoreCreated;

        public CoopClient(ClientConfiguration config)
        {
            Session = new GameSession();
            Session.OnConnectionDestroyed += ConnectionDestroyed;
            m_NetManager = new LiteNetManagerClient(Session, config);
            m_Updateables.Add(m_NetManager);
            Events = new CoopEvents();
            m_CoopClientSM = new CoopClientSM();
            Synchronization = new CoopSyncClient(this);

            #region State Machine Callbacks
            m_CoopClientSM.CharacterCreationState.OnEntry(CreateCharacter);
            m_CoopClientSM.ReceivingGameDataState.OnEntry(SendClientRequestInitialWorldData);
            m_CoopClientSM.PlayingState.OnEntry(SendGameLoaded);
            #endregion

            Init();
        }

        /// <summary>
        ///     Object store shared with the server if connected. Otherwise null.
        /// </summary>
        [CanBeNull]
        public RemoteStoreClient SyncedObjectStore { get; private set; }

        [CanBeNull] public PersistenceClient Persistence { get; private set; }
        
        [NotNull] public CoopSyncClient Synchronization { get; }

        [NotNull] public GameSession Session { get; }

        public static CoopClient Instance => m_Instance.Value;
        public CoopEvents Events { get; }

        public bool ClientConnected
        {
            get
            {
                if (Session.Connection == null)
                {
                    return false;
                }

                return Session.Connection.State.Equals(EClientConnectionState.Connected);
            }
        }

        public bool ClientPlaying
        {
            get
            {
                return m_CoopClientSM.State.Equals(ECoopClientState.Playing);
            }
        }

        public RemoteStoreClient GetStore()
        {
            return SyncedObjectStore;
        }

        public RailClientRoom GetRoom()
        {
            return Persistence?.Room;
        }

        public void Update(TimeSpan frameTime)
        {
            m_Updateables.UpdateAll(frameTime);
        }
        public int Priority { get; } = UpdatePriority.MainLoop.Update;

        public string Connect(IPAddress ip, int iPort)
        {
            return m_NetManager.Connect(ip, iPort);
        }

        public void Disconnect()
        {
            m_NetManager.Disconnect(EDisconnectReason.ClientLeft);
        }

        private void Init()
        {
            Session.OnConnectionCreated += ConnectionCreated;
            if (Session.Connection != null)
            {
                ConnectionCreated(Session.Connection);
            }
        }

        private void TryInitPersistence()
        {
            ConnectionClient con = Session.Connection;
            if (con == null || !m_CoopClientSM.State.Equals(ECoopClientState.Playing)) return;

            if (Persistence == null)
            {
                Persistence = new PersistenceClient(new GameEnvironmentClient());

                m_Updateables.Add(Persistence);
                OnPersistenceInitialized?.Invoke(Persistence);
            }

            Persistence.SetConnection(con);
        }
        private void ConnectionCreated(ConnectionClient con)
        {
            if (con == null)
            {
                throw new ArgumentNullException(nameof(con));
            }

            Session.Connection.OnConnected += ConnectionEstablished;
        }

        private void ConnectionEstablished(ConnectionClient con)
        {
            if (m_CoopClientSM.State.Equals(ECoopClientState.MainManu))
            {
                if (Coop.IsServer)
                {
                    m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.IsServer);
                }
                else
                {
                    Session.Connection.Dispatcher.RegisterPacketHandler(ReceiveRequireCreateCharacter);
                    Session.Connection.Dispatcher.RegisterPacketHandler(ReceiveCharacterExists);

                    Session.Connection.Send(
                        new Packet(
                            EPacket.Client_RequestParty,
                            new Client_Request_Party(new PlatformAPI().GetPlayerID().ToString()).Serialize()));
                }

                SyncedObjectStore = new RemoteStoreClient(con, new SerializableFactory());
                RemoteStoreCreated?.Invoke(SyncedObjectStore);

                #region events
                Session.Connection.OnDisconnected += ConnectionClosed;
                #endregion

                // Handler Registration
                //Session.Connection.Dispatcher.RegisterPacketHandler(ReceiveInitialWorldData);
                Session.Connection.Dispatcher.RegisterPacketHandler(ReceivePartyId);
                Session.Connection.Dispatcher.RegisterPacketHandler(ReceiveGameData);

                Session.Connection.Dispatcher.RegisterStateMachine(this, m_CoopClientSM);
            }
        }

        private void CreateCharacter()
        {
            if (gameManager == null)
            {
                gameManager = new ClientCharacterCreatorManager();
                MBGameManager.StartNewGame(gameManager);

                ClientCharacterCreatorManager.OnCharacterCreationFinishedEvent += CharacterCreationOver;

                // Remove listener when disconnected
                Session.OnConnectionDestroyed += (reason) => { ClientCharacterCreatorManager.OnCharacterCreationFinishedEvent -= CharacterCreationOver; };
            }
        }

        private void ConnectionClosed(EDisconnectReason eReason)
        {
            Persistence?.SetConnection(null);
            SyncedObjectStore = null;
        }

        private void ConnectionDestroyed(EDisconnectReason eReason)
        {
            switch (eReason)
            {
                case EDisconnectReason.Timeout:
                case EDisconnectReason.Unknown:
                    TryReconnect();
                    break;
            }
        }

        private void TryReconnect()
        {
            if (m_ReconnectAttempts > 0)
            {
                Logger.Info(
                    "Reconnect attempt [{currentAttempt}/{max}].",
                    m_ReconnectAttempts,
                    MaxReconnectAttempts);
                --m_ReconnectAttempts;
                m_NetManager.Reconnect();
            }
        }

        #region MainMenu
        [GameClientPacketHandler(ECoopClientState.MainManu, EPacket.Server_RequireCharacterCreation)]
        private void ReceiveRequireCreateCharacter(ConnectionBase connection, Packet packet)
        {
            m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.RequiresCharacterCreation);
        }

        [GameClientPacketHandler(ECoopClientState.MainManu, EPacket.Server_NotifyCharacterExists)]
        private void ReceiveCharacterExists(ConnectionBase connection, Packet packet)
        {
            m_HeroGUID = CommonSerializer.Deserialize<Guid>(packet.Payload);
            m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.CharacterExists);
        }
        #endregion

        #region ClientCharacterCreation

        public void CharacterCreationOver()
        {
            PlayerHeroSerializer playerHeroSerialized = new PlayerHeroSerializer(Hero.MainHero);
            byte[] data = CommonSerializer.Serialize(playerHeroSerialized);

            Session.Connection.Send(
                new Packet(
                    EPacket.Client_RequestGameData,
                    data));

            m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.CharacterCreated);
        }

        [GameClientPacketHandler(ECoopClientState.ReceivingGameData, EPacket.Server_GameData)]
        public void ReceiveGameData(ConnectionBase connection, Packet packet)
        {
            SaveData saveData = SaveData.Deserialize(packet.Payload);

            m_HeroGUID = saveData.PlayerId;

            if (Game.Current != null)
            {
                ScreenManager.PopScreen();
                GameStateManager.Current.CleanStates(0);
                GameStateManager.Current = TaleWorlds.MountAndBlade.Module.CurrentModule.GlobalGameStateManager;
            }

            gameManager = new ClientManager(saveData.LoadResult, saveData.PlayerId);

            MBGameManager.StartNewGame(gameManager);

            ClientManager.OnPostLoadFinishedEvent += (sender, eventArgs) =>
            {
                saveData.AssosiateIds();

                m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.GameDataReceived);
            };
        }
        #endregion

        #region ClientAwaitingWorldData
        private void SendClientRequestInitialWorldData()
        {
            if(Coop.IsServer)
            {
                Session.Connection.Send(
                new Packet(
                    EPacket.Client_DeclineWorldData,
                    new Client_DeclineWorldData().Serialize()));
                m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.GameDataReceived);
                m_CoopClientSM.StateMachine.Fire(ECoopClientTrigger.GameLoaded);
            }
            else
            {
                Session.Connection.Send(
                    new Packet(EPacket.Client_RequestWorldData));
            }
            
        }
        #endregion

        #region ClientPlaying
        public void SendGameLoaded()
        {
            Session.Connection.Send(
                new Packet(EPacket.Client_Loaded));
            TryInitPersistence();
            if(!Coop.IsServer)
            {
                // When the server is running in the same game instance, sync is already initialized.
                Initializer.SetupSyncAfterLoad();
            }            
            Session.Connection.Send(
                new Packet(EPacket.Client_PartyChanged, CommonSerializer.Serialize(m_HeroGUID)));
        }
		

        [GameClientPacketHandler(ECoopClientState.CharacterCreation, EPacket.Server_HeroId)]
        private void ReceivePartyId(ConnectionBase connection, Packet packet)
        {
            m_HeroGUID = CommonSerializer.Deserialize<Guid>(packet.Payload);
        }
        #endregion

        public override string ToString()
        {
            if (Session.Connection == null)
            {
                return "Client not connected.";
            }

            string sLeadingWhitespace = "       ";
            string sRet =
                $"{Session.Connection.Latency,-5}{Session.Connection.State,-30}{Session.Connection.Network}";
            sRet += Environment.NewLine + sLeadingWhitespace;
            if (Persistence != null)
            {
                IEnumerable<RailEntityBase> controlledEntity = Persistence.Room.LocalEntities;
                sRet += $"Controlling {controlledEntity.Count()} entities.";
                foreach (RailEntityBase entity in controlledEntity)
                {
                    sRet += Environment.NewLine + sLeadingWhitespace + entity;
                }
            }

            return sRet;
        }

        
    }
}