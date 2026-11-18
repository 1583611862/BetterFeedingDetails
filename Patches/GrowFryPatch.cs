using System;
using System.Collections.Generic;
using HarmonyLib;
using BetterFeedingDetails.Data;

namespace BetterFeedingDetails.Patches
{
    [HarmonyPatch(typeof(AquacultureHelper), "GrowFry")]
    internal static class GrowFryPatch
    {
        private struct FryState
        {
            internal FeedLedger Ledger;
            internal int ShareCount;
            internal bool Valid;
        }

        private static void Prefix(GameItem fry, out FryState __state)
        {
            __state = new FryState { Ledger = null, ShareCount = 0, Valid = false };
            try
            {
                if (fry == null || !fry.IsTag("FISH_FRY_TAG"))
                {
                    return;
                }
                FeedLedger ledger = FeedLedgerStore.Load(fry);
                if (ledger == null)
                {
                    return;
                }
                int count = fry.GetTagReadonly("FISH_GROUP_COUNT_TAG").GetInt();
                if (count <= 1)
                {
                    return;
                }
                __state.Ledger = ledger;
                __state.ShareCount = count;
                __state.Valid = true;
            }
            catch (Exception e)
            {
                ModEntry.LogError("GrowFry prefix failed: " + e);
                __state.Valid = false;
            }
        }

        private static void Postfix(GameGridInventory tankGrid, Dictionary<GameItem, float> gainedGrams, FryState __state)
        {
            if (!__state.Valid)
            {
                return;
            }
            try
            {
                if (tankGrid == null || gainedGrams == null)
                {
                    return;
                }
                FeedLedger split = __state.Ledger.SplitShared(__state.ShareCount);
                foreach (KeyValuePair<GameItem, float> pair in gainedGrams)
                {
                    GameItem child = pair.Key;
                    if (child != null && child.IsTag("FISH_ITEM_TAG"))
                    {
                        FeedLedgerStore.Save(child, split);
                    }
                }
            }
            catch (Exception e)
            {
                ModEntry.LogError("GrowFry postfix failed: " + e);
            }
        }
    }
}
