using TaleWorlds.CampaignSystem.Party;
using HarmonyLib;
using TaleWorlds.Library;
using GameInterface.Extentions;
using Common.Messaging;
using TaleWorlds.CampaignSystem.Map;
using GameInterface.Services.MobileParties.Data;
using GameInterface.Services.MobileParties.Messages.Behavior;
using GameInterface.Services.MobileParties.Handlers;
using GameInterface.Utils;
using Common.Extensions;
using System.Reflection;
using System;
using TaleWorlds.CampaignSystem.Settlements;

namespace GameInterface.Services.MobileParties.Patches
{
    /// <summary>
    /// Handles changes in party behavior for the <see cref="MobilePartyAi"/> behavior synchronisation system.
    /// </summary>
    /// <seealso cref="MobilePartyBehaviorHandler"/>
    [HarmonyPatch(typeof(MobilePartyAi))]
    static class PartyBehaviorPatch
    {
        public static void SetAiBehavior(
            MobilePartyAi partyAi, AiBehavior newBehavior, AiBehavior defaultBehavior, IMapEntity targetMapEntity, Vec2 targetPoint, Settlement targetSettlement = null)
        {
            // Normal content of MobilePartyAi.SetAiBehavior
            SetShortTermBehavior(partyAi, newBehavior, targetMapEntity);
            SetBehaviorTarget(partyAi, targetPoint);
            UpdateBehavior(partyAi);

            // Set additional data to display behavior text
            // (tooltip when you hover over a party) correctly 
            _targetSettlementSetter(partyAi.GetMobileParty(), targetSettlement);
            _defaultBehaviorSetter(partyAi, defaultBehavior);
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetAiBehavior")]
        private static bool SetAiBehaviorPrefix(
            ref MobilePartyAi __instance, 
            ref AiBehavior newAiBehavior, 
            ref PartyBase targetPartyFigure, 
            ref Vec2 bestTargetPoint)
        {
            MobileParty party = __instance.GetMobileParty();

            bool hasTargetEntity = false;
            string targetEntityId = string.Empty;

            if (targetPartyFigure != null)
            {
                hasTargetEntity = true;
                targetEntityId = targetPartyFigure.IsSettlement
                    ? targetPartyFigure.Settlement.StringId
                    : targetPartyFigure.MobileParty.StringId;
            }

            bool hasTargetSettlement = false;
            string targetSettlementId = string.Empty;

            if (party.TargetSettlement != null)
            {
                hasTargetSettlement = true;
                targetSettlementId = party.TargetSettlement.StringId;
            }

            var data = new PartyBehaviorUpdateData(party.StringId, newAiBehavior, party.DefaultBehavior, hasTargetEntity, targetEntityId, hasTargetSettlement, targetSettlementId, bestTargetPoint);
            MessageBroker.Instance.Publish(__instance, new PartyBehaviorChangeAttempted(party, data));

            return false;
        }

        public static PartyBehaviorUpdateData CreateBehaviorUpdateData(MobileParty party)
        {
            bool hasTargetEntity = false;
            string targetEntityId = string.Empty;

            if (party.Ai.AiBehaviorPartyBase != null)
            {
                hasTargetEntity = true;
                targetEntityId = party.Ai.AiBehaviorPartyBase.IsSettlement
                    ? party.Ai.AiBehaviorPartyBase.Settlement.StringId
                    : party.Ai.AiBehaviorPartyBase.MobileParty.StringId;
            }

            var data = new PartyBehaviorUpdateData(
                party.StringId, party.ShortTermBehavior, party.DefaultBehavior,
                hasTargetEntity, targetEntityId, party.TargetSettlement != null, 
                party.TargetSettlement?.StringId ?? "", party.AiBehaviorTarget);

            return data;
        }

        static readonly Action<MobilePartyAi, AiBehavior, IMapEntity> SetShortTermBehavior = typeof(MobilePartyAi)
            .GetMethod("SetShortTermBehavior", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildDelegate<Action<MobilePartyAi, AiBehavior, IMapEntity>>();

        static readonly Action<MobilePartyAi, Vec2> SetBehaviorTarget = typeof(MobilePartyAi)
            .GetField("BehaviorTarget", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildUntypedSetter<MobilePartyAi, Vec2>();

        /*static readonly Action<MobilePartyAi, AiBehavior> SetDefaultBehavior = typeof(MobilePartyAi)
            .GetField("_defaultBehavior", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildUntypedSetter<MobilePartyAi, AiBehavior>();*/

        static readonly Action<MobilePartyAi> UpdateBehavior = typeof(MobilePartyAi)
            .GetMethod("UpdateBehavior", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildDelegate<Action<MobilePartyAi>>();

        private static Action<MobilePartyAi, AiBehavior> _defaultBehaviorSetter = typeof(MobilePartyAi)
            .GetField("_defaultBehavior", BindingFlags.NonPublic | BindingFlags.Instance)
            .BuildUntypedSetter<MobilePartyAi, AiBehavior>();

        static readonly Action<MobileParty, Settlement> _targetSettlementSetter = typeof(MobileParty)
            .GetField("_targetSettlement", BindingFlags.NonPublic | BindingFlags.Instance)
            .BuildUntypedSetter<MobileParty, Settlement>();
    }
}
