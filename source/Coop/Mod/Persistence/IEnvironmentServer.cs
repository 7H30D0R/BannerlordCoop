﻿using Coop.Mod.Persistence.Party;
using Coop.Mod.Persistence.RemoteAction;
using JetBrains.Annotations;
using Sync;
using Sync.Store;
using TaleWorlds.CampaignSystem;

namespace RemoteAction
{
    /// <summary>
    ///     Provides an abstraction layer between the persistence and the game for the server.
    /// </summary>
    public interface IEnvironmentServer
    {

        /// <summary>
        ///     Returns the shared object store for this server.
        /// </summary>
        [NotNull]
        SharedRemoteStore Store { get; }

        /// <summary>
        ///     Returns the queue to broadcast events to all clients. NotNull if persistence is initialized.
        /// </summary>
        [CanBeNull]
        EventBroadcastingQueue EventQueue { get; }

        /// <summary>
        ///     Returns a party given its party index.
        /// </summary>
        /// <param name="iPartyIndex"></param>
        /// <returns></returns>
        [CanBeNull]
        MobileParty GetMobilePartyByIndex(int iPartyIndex);
        
        /// <summary>
        ///     Gets the synchronization for <see cref="MobileParty"/> instances.
        /// </summary>
        MobilePartySync PartySync { get; }
    }
}
