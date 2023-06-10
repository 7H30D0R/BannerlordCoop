using Common.Messaging;
using ProtoBuf;

namespace Coop.Core.Server.Services.MobileParties.Messages
{
    [ProtoContract(SkipConstructor = true)]
    public record NetworkMobilePartyHide : IEvent
    {
        [ProtoMember(1)]
        public string PartyId { get; }

        public NetworkMobilePartyHide(string partyId)
        {
            PartyId = partyId;
        }
    }
}