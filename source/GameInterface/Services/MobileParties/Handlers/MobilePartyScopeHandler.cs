using Common.Messaging;
using GameInterface.Services.MobileParties.Data;
using GameInterface.Services.MobileParties.Interfaces;
using GameInterface.Services.MobileParties.Messages.Scope;
using GameInterface.Services.MobileParties.Patches;
using GameInterface.Services.ObjectManager;
using GameInterface.Services.Scope.Enums;
using GameInterface.Services.Scope.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace GameInterface.Services.MobileParties.Handlers
{
    internal class MobilePartyScopeHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly IObjectManager objectManager;
        private readonly IMobilePartyInterface partyInterface;

        public MobilePartyScopeHandler(
            IMessageBroker messageBroker, 
            IObjectManager objectManager, 
            IMobilePartyInterface partyInterface) 
        {
            this.messageBroker = messageBroker;
            this.objectManager = objectManager;
            this.partyInterface = partyInterface;

            messageBroker.Subscribe<UpdateMobileParty>(Handle_UpdateMobileParty);
            messageBroker.Subscribe<HideMobileParty>(Handle_HideMobileParty);
            messageBroker.Subscribe<EntityEnteredScope>(Handle_EntityEnteredScope);
            messageBroker.Subscribe<EntityLeftScope>(Handle_EntityLeftScope);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<UpdateMobileParty>(Handle_UpdateMobileParty);
            messageBroker.Unsubscribe<HideMobileParty>(Handle_HideMobileParty);
            messageBroker.Unsubscribe<EntityEnteredScope>(Handle_EntityEnteredScope);
            messageBroker.Unsubscribe<EntityLeftScope>(Handle_EntityLeftScope);
        }

        private void Handle_UpdateMobileParty(MessagePayload<UpdateMobileParty> obj)
        {
            var data = obj.What.UpdateData;

            if (!objectManager.TryGetObject(data.Behavior.PartyId, out MobileParty party))
                return;

            party.Position2D = new Vec2(data.PositionX, data.PositionY);
            partyInterface.UpdatePartyBehavior(data.Behavior);
        }

        private void Handle_HideMobileParty(MessagePayload<HideMobileParty> obj)
        {
            if (!objectManager.TryGetObject(obj.What.PartyId, out MobileParty party))
                return;

            if (party.IsMainParty)
                return;

            // This should hopefully make it invisible
            // but there is probably a better way to do this
            party.Position2D = new Vec2(100000, 100000);
        }

        private void Handle_EntityEnteredScope(MessagePayload<EntityEnteredScope> obj)
        {
            if (obj.What.EntityType != EntityType.MobileParty)
                return;

            if (!objectManager.TryGetObject(obj.What.EntityId, out MobileParty party))
                return;

            PartyBehaviorUpdateData behaviorUpdate = PartyBehaviorPatch.CreateBehaviorUpdateData(party);
            var data = new MobilePartyUpdateData(behaviorUpdate, party.Position2D);

            messageBroker.Publish(this, new SendTargettedPartyUpdate(obj.What.ClientId, data));
        }

        private void Handle_EntityLeftScope(MessagePayload<EntityLeftScope> obj)
        {
            if (obj.What.EntityType != EntityType.MobileParty)
                return;

            messageBroker.Publish(this, new SendTargettedHideParty(obj.What.ClientId, obj.What.EntityId));
        }
    }
}
