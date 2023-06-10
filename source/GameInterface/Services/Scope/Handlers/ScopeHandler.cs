using Common.Messaging;
using GameInterface.Services.ObjectManager;
using GameInterface.Services.Scope.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;

namespace GameInterface.Services.Scope.Handlers
{
    internal class ScopeHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly IScopeManager scopeManager;
        private readonly IObjectManager objectManager;

        public ScopeHandler(IMessageBroker messageBroker, IScopeManager scopeManager, IObjectManager objectManager) 
        { 
            this.messageBroker = messageBroker;
            this.scopeManager = scopeManager;
            this.objectManager = objectManager;

            messageBroker.Subscribe<CreateScope>(Handle_CreateScope);
            messageBroker.Subscribe<RemoveScope>(Handle_RemoveScope);
            messageBroker.Subscribe<ClientSwitchedHero>(Handle_ClientSwitchedHero);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<CreateScope>(Handle_CreateScope);
            messageBroker.Unsubscribe<RemoveScope>(Handle_RemoveScope);
            messageBroker.Unsubscribe<ClientSwitchedHero>(Handle_ClientSwitchedHero);
        }

        private void Handle_CreateScope(MessagePayload<CreateScope> obj)
        {
            scopeManager.CreateScope(obj.What.ClientId);
        }

        private void Handle_RemoveScope(MessagePayload<RemoveScope> obj)
        {
            scopeManager.RemoveScope(obj.What.ClientId);
        }

        private void Handle_ClientSwitchedHero(MessagePayload<ClientSwitchedHero> obj)
        {
            if (!objectManager.TryGetObject(obj.What.HeroId, out Hero hero))
                return;

            scopeManager.SetClientHero(obj.What.ClientId, hero);
        }
    }
}
