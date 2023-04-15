﻿using Common.Logging;
using Common.Messaging;
using Common.PacketHandlers;
using LiteNetLib;
using Missions.Messages;
using Missions.Services.Network.Messages;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Missions.Services.Network
{
    public class EventQueueManager : EventPacketHandler, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetLogger<EventQueueManager>();

        Dictionary<NetPeer, ConcurrentQueue<INetworkEvent>> Queues = new Dictionary<NetPeer, ConcurrentQueue<INetworkEvent>>();

        Dictionary<NetPeer, bool> ReadyPeers = new Dictionary<NetPeer, bool>();

        public EventQueueManager(IMessageBroker messageBroker, IPacketManager packetManager) : base(messageBroker, packetManager)
        {
            packetManager.RegisterPacketHandler(this);

            messageBroker.Subscribe<PeerConnected>(Handle_PeerConnected);
            messageBroker.Subscribe<PeerDisconnected>(Handle_PeerDisconnect);
            messageBroker.Subscribe<PeerReady>(Handle_PeerReady);
        }

        private void Handle_PeerReady(MessagePayload<PeerReady> obj)
        {
            var peer = obj.What.Peer;

            if(ReadyPeers.ContainsKey(peer) == false)
            {
                Logger.Error("Tried to process queue for peer that was " +
                    "not registered {endpoint}, registered peers {readyPeers}", 
                    peer.EndPoint, ReadyPeers);
                return;
            }

            if(Queues.ContainsKey(peer) == false)
            {
                Logger.Error("Tried to process queue for peer that was " +
                    "not registered {endpoint}, registered peers {readyPeers}",
                    peer.EndPoint, Queues);
                return;
            }

            while (Queues[peer].IsEmpty == false)
            {
                if (Queues[peer].TryDequeue(out var message))
                {
                    PublishEvent(peer, message);
                }
            }

            ReadyPeers[peer] = true;
        }

        private void Handle_PeerConnected(MessagePayload<PeerConnected> obj)
        {
            var peer = obj.What.Peer;
            if (peer == null)
            {
                Logger.Warning("Peer was null when expecting non-null peer");
                return;
            }

            ReadyPeers.Add(peer, false);
            Queues.Add(peer, new ConcurrentQueue<INetworkEvent>());
        }

        private void Handle_PeerDisconnect(MessagePayload<PeerDisconnected> obj)
        {
            var peer = obj.What.NetPeer;

            ReadyPeers.Remove(peer);
            Queues.Remove(peer);
        }

        public override void HandlePacket(NetPeer peer, IPacket packet)
        {
            if(ReadyPeers.TryGetValue(peer, out var ready))
            {
                if (ready)
                {
                    base.HandlePacket(peer, packet);
                }
                else
                {
                    EventPacket convertedPacket = (EventPacket)packet;

                    if(convertedPacket.Event is NetworkMissionJoinInfo)
                    {
                        base.HandlePacket(peer, packet);
                        return;
                    }

                    Queues[peer].Enqueue(convertedPacket.Event);
                }
            }
            else
            {
                Logger.Error("Tried to process message for unconnected peer {endpoint}", peer.EndPoint);
            }
        }

        ~EventQueueManager() => Dispose();

        public override void Dispose()
        {
            base.Dispose();
            ReadyPeers.Clear();
            Queues.Clear();
        }
    }
}
