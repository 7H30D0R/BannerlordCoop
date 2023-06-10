using Common.Messaging;
using GameInterface.Services.MobileParties.Data;
using System;

namespace GameInterface.Services.MobileParties.Messages.Scope
{
    public record SendTargettedHideParty : ICommand
    {
        public Guid TargetClient { get; }
        public string PartyId { get; }

        public SendTargettedHideParty(Guid targetClient, string partyId)
        {
            TargetClient = targetClient;
            PartyId = partyId;
        }
    }
}