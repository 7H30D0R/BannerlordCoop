using Common.Messaging;
using GameInterface.Services.MobileParties.Data;
using System;

namespace GameInterface.Services.MobileParties.Messages.Scope
{
    public record SendTargettedPartyUpdate : ICommand
    {
        public Guid TargetClient { get; }
        public MobilePartyUpdateData UpdateData { get; }
        public string EntityId => UpdateData.Behavior.PartyId;

        public SendTargettedPartyUpdate(Guid targetClient, MobilePartyUpdateData updateData)
        {
            TargetClient = targetClient;
            UpdateData = updateData;
        }
    }
}