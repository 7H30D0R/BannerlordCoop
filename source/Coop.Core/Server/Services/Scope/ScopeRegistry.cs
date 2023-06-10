using Coop.Core.Server.Services.EntityScope.Data;
using LiteNetLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coop.Core.Server.Services.EntityScope
{
    public interface IScopeRegistry : IEnumerable<KeyValuePair<NetPeer, ClientScope>>
    {
        public void CreateScope(NetPeer peer, Guid clientId);
        public void RemoveScope(NetPeer peer);
        public ClientScope GetScope(Guid clientId);
        public NetPeer GetPeer(Guid clientId);
        public Guid GetClientId(NetPeer peer);
    }

    public class ScopeRegistry : IScopeRegistry
    {
        private readonly Dictionary<NetPeer, ClientScope> clientScopesByPeer 
            = new Dictionary<NetPeer, ClientScope>();

        public void CreateScope(NetPeer peer, Guid clientId)
        {
            clientScopesByPeer[peer] = new ClientScope(clientId);
        }

        public void RemoveScope(NetPeer peer)
        {
            clientScopesByPeer.Remove(peer);
        }

        public ClientScope GetScope(Guid clientId)
        {
            return this.Where(pair => pair.Value.ClientId == clientId).First().Value;
        }

        public NetPeer GetPeer(Guid clientId)
        {
            return this.Where(pair => pair.Value.ClientId == clientId).First().Key;
        }

        public Guid GetClientId(NetPeer peer)
        {
            return this.Where(pair => pair.Key == peer).First().Value?.ClientId ?? Guid.Empty;
        }

        public IEnumerator GetEnumerator() => clientScopesByPeer.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<KeyValuePair<NetPeer, ClientScope>> IEnumerable<KeyValuePair<NetPeer, ClientScope>>.GetEnumerator()
            => (IEnumerator<KeyValuePair<NetPeer, ClientScope>>) GetEnumerator();
    }
}
