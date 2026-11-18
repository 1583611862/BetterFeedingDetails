using BetterFeedingDetails.UI;

namespace BetterFeedingDetails
{
    public class ModEntry : IMod
    {
        internal static ModLog Log;
        internal static string ModId = "better_feeding_details";

        public void Init(ModManifest manifest)
        {
            ModId = manifest.ID;
            Log = new ModLog(manifest);
        }

        public void OnEnable()
        {
            ModHook.OnCreateTooltipLate += FeedingTooltip.OnTooltipLate;
            LogInfo("enabled.");
        }

        public void OnDisable()
        {
            ModHook.OnCreateTooltipLate -= FeedingTooltip.OnTooltipLate;
            LogInfo("disabled.");
        }

        internal static void LogInfo(string message)
        {
            if (Log != null)
            {
                Log.Log("[BetterFeedingDetails] " + message);
            }
        }

        internal static void LogError(string message)
        {
            if (Log != null)
            {
                Log.Error("[BetterFeedingDetails] " + message);
            }
        }
    }
}
