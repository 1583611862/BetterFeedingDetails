using BetterFeedingDetails.UI;
using HarmonyLib;

namespace BetterFeedingDetails.Patches
{
    [HarmonyPatch(typeof(AquacultureHelper), "InitFishItem")]
    internal static class FishIconPatch
    {
        private static void Postfix(GameItem item)
        {
            FishIconBadges.Attach(item);
        }
    }
}
