using Common.Messaging;

namespace GameInterface.Services.MobileParties.Messages.Scope
{
    public record HideMobileParty : ICommand
    {
        public string PartyId { get; }

        public HideMobileParty(string partyId) 
        { 
            PartyId = partyId;
        }
    }
}