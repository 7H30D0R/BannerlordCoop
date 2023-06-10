using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace GameInterface.Services.MapEvents.Patches
{
    [HarmonyPatch(typeof(MapEvent))]
    internal class DisableMapEventPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Constructor)]
        private static bool DisableConstructor() => false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MapEvent.Initialize))]
        private static bool DisableInitialize() => false;
    }

    [HarmonyPatch(typeof(StartBattleAction))]
    internal class DisableStartBattleAction
    {
        [HarmonyPrefix]
        [HarmonyPatch("ApplyInternal")]
        private static bool DisableApplyInternal () => false;
    }

    [HarmonyPatch(typeof(EncounterManager))]
    internal class DisableEncounterManagerMapEvents
    {
        [HarmonyPrefix]
        [HarmonyPatch("StartSettlementEncounter")]
        private static bool DisableStartSettlementEncounter(MobileParty attackerParty, Settlement settlement)
        {
            if (attackerParty.ShortTermBehavior == AiBehavior.AssaultSettlement && 
                attackerParty.ShortTermTargetSettlement == settlement)
            {
                return false;
            }

            return true;
        }
    }

}
