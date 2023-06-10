using Common.Messaging;
using Coop.Core.Server.Services.EntityScope;
using Coop.Core.Server.Services.MobileParties.Messages;
using GameInterface.Services.MobileParties.Messages.Scope;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Server.Services.MobileParties.Handlers
{
    internal class MobilePartyScopeHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly IScopeNetwork scopeNetwork;
        public MobilePartyScopeHandler(IMessageBroker messageBroker, IScopeNetwork scopeNetwork) 
        { 
            this.messageBroker = messageBroker;
            this.scopeNetwork = scopeNetwork;

            messageBroker.Subscribe<SendTargettedPartyUpdate>(Handle_SendTargettedPartyUpdate);
            messageBroker.Subscribe<SendTargettedHideParty>(Handle_SendTargettedHideParty);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<SendTargettedPartyUpdate>(Handle_SendTargettedPartyUpdate);
            messageBroker.Unsubscribe<SendTargettedHideParty>(Handle_SendTargettedHideParty);
        }

        private void Handle_SendTargettedPartyUpdate(MessagePayload<SendTargettedPartyUpdate> obj)
        {
            scopeNetwork.Send(
                obj.What.TargetClient,
                new NetworkMobilePartyUpdate(obj.What.UpdateData)
            );
        }

        private void Handle_SendTargettedHideParty(MessagePayload<SendTargettedHideParty> obj)
        {
            scopeNetwork.Send(
                obj.What.TargetClient,
                new NetworkMobilePartyHide(obj.What.PartyId)
            );
        }
    }
}
