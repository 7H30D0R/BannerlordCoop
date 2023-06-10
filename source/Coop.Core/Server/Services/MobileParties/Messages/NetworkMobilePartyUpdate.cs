using Common.Messaging;
using GameInterface.Services.MobileParties.Data;
using ProtoBuf;

namespace Coop.Core.Server.Services.MobileParties.Messages
{
    [ProtoContract(SkipConstructor = true)]
    public record NetworkMobilePartyUpdate : IEvent
    {
        [ProtoMember(1)]
        public MobilePartyUpdateData UpdateData { get; }

        public NetworkMobilePartyUpdate(MobilePartyUpdateData updateData)
        {
            UpdateData = updateData;
        }
    }
}