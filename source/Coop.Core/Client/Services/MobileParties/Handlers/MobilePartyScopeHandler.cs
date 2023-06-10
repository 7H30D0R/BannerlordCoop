using Common.Messaging;
using Coop.Core.Server.Services.MobileParties.Messages;
using GameInterface.Services.MobileParties.Messages.Scope;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Client.Services.MobileParties.Handlers
{
    internal class MobilePartyScopeHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;

        public MobilePartyScopeHandler(IMessageBroker messageBroker) 
        { 
            this.messageBroker = messageBroker;

            messageBroker.Subscribe<NetworkMobilePartyUpdate>(Handle_NetworkMobilePartyUpdate);
            messageBroker.Subscribe<NetworkMobilePartyHide>(Handle_NetworkMobilePartyHide);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<NetworkMobilePartyUpdate>(Handle_NetworkMobilePartyUpdate);
            messageBroker.Unsubscribe<NetworkMobilePartyHide>(Handle_NetworkMobilePartyHide);
        }

        private void Handle_NetworkMobilePartyUpdate(MessagePayload<NetworkMobilePartyUpdate> obj)
        {
            messageBroker.Publish(this, new UpdateMobileParty(obj.What.UpdateData));
        }

        private void Handle_NetworkMobilePartyHide(MessagePayload<NetworkMobilePartyHide> obj)
        {
            messageBroker.Publish(this, new HideMobileParty(obj.What.PartyId));
        }
    }
}
