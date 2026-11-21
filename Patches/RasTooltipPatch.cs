using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BetterFeedingDetails.UI;
using HarmonyLib;

namespace BetterFeedingDetails.Patches
{
    [HarmonyPatch(typeof(MachineRAS), "CreateTooltip")]
    internal static class RasTooltipPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo anchor = AccessTools.Method(typeof(MachineFeedDispenser), "CreateSlottedTooltip");
            MethodInfo injection = AccessTools.Method(typeof(FeedingTooltip), "AppendRasLines");
            if (anchor == null || injection == null)
            {
                return codes;
            }
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && anchor.Equals(codes[i].operand))
                {
                    codes.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, injection)
                    });
                    break;
                }
            }
            return codes;
        }
    }
}
