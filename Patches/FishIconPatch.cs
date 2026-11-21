using BetterFeedingDetails.UI;
using HarmonyLib;

namespace BetterFeedingDetails.Patches
{
    [HarmonyPatch(typeof(GameItemElement), "ValidateSelect")]
    internal static class FishIconPatch
    {
        private static void Postfix(GameItemElement __instance)
        {
            FishIconBadges.Sync(__instance);
        }
    }
}
