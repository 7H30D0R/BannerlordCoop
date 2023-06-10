using Common.Messaging;
using Common.Network;
using Coop.Core.Server.Connections.Messages;
using Coop.Core.Templates.ServiceTemplate.Messages;
using GameInterface.Services.Scope.Messages;
using System;
using System.Linq;

namespace Coop.Core.Server.Services.EntityScope.Handlers
{
    internal class ScopeHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly IScopeRegistry scopeRegistry;

        public ScopeHandler(IMessageBroker messageBroker, IScopeRegistry scopeRegistry)
        {
            this.messageBroker = messageBroker;
            this.scopeRegistry = scopeRegistry;

            messageBroker.Subscribe<PlayerConnected>(Handle_PlayerConnected);
            messageBroker.Subscribe<PlayerDisconnected>(Handle_PlayerDisconnected);
            messageBroker.Subscribe<EntityEnteredScope>(Handle_EntityEnteredScope);
            messageBroker.Subscribe<EntityLeftScope>(Handle_EntityLeftScope);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<PlayerConnected>(Handle_PlayerConnected);
            messageBroker.Unsubscribe<PlayerDisconnected>(Handle_PlayerDisconnected);
            messageBroker.Unsubscribe<EntityEnteredScope>(Handle_EntityEnteredScope);
            messageBroker.Unsubscribe<EntityLeftScope>(Handle_EntityLeftScope);
        }

        private void Handle_PlayerConnected(MessagePayload<PlayerConnected> obj) 
        {
            Guid clientId = Guid.NewGuid();
            scopeRegistry.CreateScope(obj.What.PlayerId, clientId);

            messageBroker.Publish(this, new CreateScope(clientId));
        }

        private void Handle_PlayerDisconnected(MessagePayload<PlayerDisconnected> obj)
        {
            Guid clientId = scopeRegistry.GetClientId(obj.What.PlayerId);
            scopeRegistry.RemoveScope(obj.What.PlayerId);

            messageBroker.Publish(this, new RemoveScope(clientId));

        }

        private void Handle_EntityEnteredScope(MessagePayload<EntityEnteredScope> obj)
        {
            scopeRegistry.GetScope(obj.What.ClientId).Entities.Add(obj.What.EntityId);
        }

        private void Handle_EntityLeftScope(MessagePayload<EntityLeftScope> obj)
        {
            scopeRegistry.GetScope(obj.What.ClientId).Entities.Remove(obj.What.EntityId);

        }
    }
}