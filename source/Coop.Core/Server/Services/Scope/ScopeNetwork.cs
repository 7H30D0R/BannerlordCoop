using Common.Messaging;
using Common.Network;
using Coop.Core.Server.Services.EntityScope.Data;
using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coop.Core.Server.Services.EntityScope
{
    public interface IScopeNetwork
    {
        public void Broadcast(string entityId, IMessage message);
        public void Send(Guid clientId, IMessage message);
    }

    public class ScopeNetwork : IScopeNetwork
    {
        private readonly INetwork network;
        private readonly IScopeRegistry scopeRegistry;
        public ScopeNetwork(INetwork network, IScopeRegistry scopeRegistry) 
        {
            this.network = network;
            this.scopeRegistry = scopeRegistry;
        }

        public void Broadcast(string entityId, IMessage message)
        {
            foreach (KeyValuePair<NetPeer, ClientScope> pair in scopeRegistry)
            {
                if (pair.Value.Entities.Contains(entityId))
                {
                    network.Send(pair.Key, message);
                }
            }
        }

        public void Send(Guid clientId, IMessage message) 
        {
            NetPeer targetPeer = scopeRegistry.GetPeer(clientId);
            
            if (targetPeer == null)
                return;

            network.Send(targetPeer, message);
        }
    }
}
