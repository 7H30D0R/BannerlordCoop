using HarmonyLib;
using SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements;

namespace GameInterface.Services.Villages.Patches
{
    /// <summary>
    /// Disables village functionality. Will be removed when
    /// village synchronisation is ready to be implemented.
    /// </summary>
    [HarmonyPatch(typeof(Village))]
    internal class DisableVillagePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Village.DailyTick))]
        private static bool DisableDailyTick() => false;
    }

    [HarmonyPatch(typeof(VillagerCampaignBehavior))]
    internal class DisableVillageCampaignBehavior
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VillagerCampaignBehavior.RegisterEvents))]
        private static bool DisableRegisterEvents() => false;
    }

    [HarmonyPatch(typeof(VillageGoodProductionCampaignBehavior))]
    internal class DisabbleVillageGoodProductionCampaignBehavior
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VillageGoodProductionCampaignBehavior.RegisterEvents))]
        private static bool DisableRegisterEvents() => false;
    }

    [HarmonyPatch(typeof(VillageHealCampaignBehavior))]
    internal class DisabbleVillageHealCampaignBehavior
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(VillageHealCampaignBehavior.RegisterEvents))]
        private static bool DisableRegisterEvents() => false;
    }
}
