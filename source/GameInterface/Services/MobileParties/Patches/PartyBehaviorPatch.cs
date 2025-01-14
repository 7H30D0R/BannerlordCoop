﻿using TaleWorlds.CampaignSystem.Party;
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
using Common.Logging;

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
            MobilePartyAi partyAi, AiBehavior newBehavior, IMapEntity targetMapEntity, Vec2 targetPoint)
        {
            SetShortTermBehavior(partyAi, newBehavior, targetMapEntity);
            SetBehaviorTarget(partyAi, targetPoint);
            UpdateBehavior(partyAi);
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

            var data = new PartyBehaviorUpdateData(party.StringId, newAiBehavior, hasTargetEntity, targetEntityId, bestTargetPoint);
            var message = new PartyBehaviorChangeAttempted(party, data);
            MessageBroker.Instance.Publish(__instance, message);

            return false;
        }

        static readonly Action<MobilePartyAi, AiBehavior, IMapEntity> SetShortTermBehavior = typeof(MobilePartyAi)
            .GetMethod("SetShortTermBehavior", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildDelegate<Action<MobilePartyAi, AiBehavior, IMapEntity>>();

        static readonly Action<MobilePartyAi, Vec2> SetBehaviorTarget = typeof(MobilePartyAi)
            .GetField("BehaviorTarget", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildUntypedSetter<MobilePartyAi, Vec2>();

        static readonly Action<MobilePartyAi> UpdateBehavior = typeof(MobilePartyAi)
            .GetMethod("UpdateBehavior", BindingFlags.Instance | BindingFlags.NonPublic)
            .BuildDelegate<Action<MobilePartyAi>>();
    }
}
