using ProtoBuf;
using TaleWorlds.Library;

namespace GameInterface.Services.MobileParties.Data
{
    [ProtoContract(SkipConstructor = true)]
    public record MobilePartyUpdateData
    {
        [ProtoMember(1)]
        public PartyBehaviorUpdateData Behavior { get; }

        [ProtoMember(2)]
        public float PositionX { get; }

        [ProtoMember(3)]
        public float PositionY { get; }

        public MobilePartyUpdateData(PartyBehaviorUpdateData behavior, Vec2 position)
        {
            Behavior = behavior;
            PositionX = position.X;
            PositionY = position.Y;
        }
    }
}
