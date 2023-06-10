using ProtoBuf;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;
using GameInterface.Services.MobileParties.Handlers;

namespace GameInterface.Services.MobileParties.Data
{
    /// <summary>
    /// Contains the data used for <see cref="MobilePartyAi"/> behavior synchronisation.
    /// </summary>
    /// <seealso cref="MobilePartyBehaviorHandler"/>
    [ProtoContract(SkipConstructor = true)]
    public record PartyBehaviorUpdateData
    {
        [ProtoMember(1)]
        public string PartyId { get; }

        [ProtoMember(2)]
        public AiBehavior Behavior { get; }

        [ProtoMember(3)]
        public AiBehavior DefaultBehavior { get; }

        [ProtoMember(4)]
        public bool HasTarget { get; }

        [ProtoMember(5)]
        public string TargetId { get; }

        [ProtoMember(6)]
        public bool HasTargetSettlement { get; }

        [ProtoMember(7)]
        public string TargetSettlementId { get; }

        [ProtoMember(8)]
        public float TargetPointX { get; }

        [ProtoMember(9)]
        public float TargetPointY { get; }

        public PartyBehaviorUpdateData(string partyId, AiBehavior aiBehavior, AiBehavior defaultBehavior,  bool hasTarget, string targetId, bool hasTargetSettlement, string targetSettlementId, Vec2 targetPoint)
        {
            PartyId = partyId;
            Behavior = aiBehavior;
            DefaultBehavior = defaultBehavior;
            HasTarget = hasTarget;
            TargetId = targetId;
            HasTargetSettlement = hasTargetSettlement;
            TargetSettlementId = targetSettlementId;
            TargetPointX = targetPoint.X;
            TargetPointY = targetPoint.Y;
        }
    }
}
