using Common.Messaging;
using GameInterface.Services.MobileParties.Data;

namespace GameInterface.Services.MobileParties.Messages.Scope
{
    public record UpdateMobileParty : ICommand
    {
        public MobilePartyUpdateData UpdateData { get; }

        public UpdateMobileParty(MobilePartyUpdateData updateData)
        {
            UpdateData = updateData;
        }
    }
}