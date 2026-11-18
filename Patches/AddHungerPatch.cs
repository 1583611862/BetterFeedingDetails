using System;
using HarmonyLib;
using BetterFeedingDetails.Data;

namespace BetterFeedingDetails.Patches
{
    [HarmonyPatch(typeof(AquacultureHelper), "AddHunger")]
    internal static class AddHungerPatch
    {
        private struct FeedState
        {
            internal float KcalBefore;
            internal float SpentKcal;
            internal float PelletShare;
            internal float IndustrialShare;
            internal float MeatShare;
            internal bool Valid;
        }

        private static void Prefix(GameItem stock, float kcal, float pelletShare, float industrialShare, float meatShare, out FeedState __state)
        {
            __state = new FeedState
            {
                KcalBefore = 0f,
                SpentKcal = 0f,
                PelletShare = 0f,
                IndustrialShare = 0f,
                MeatShare = 0f,
                Valid = false
            };

            try
            {
                if (stock == null || kcal <= 0f)
                {
                    return;
                }
                if (FeedLedgerStore.IsTrackableFish(stock))
                {
                    __state.KcalBefore = AquacultureHelper.GetKcalEaten(stock);
                    __state.SpentKcal = kcal;
                    __state.PelletShare = Clamp01(pelletShare);
                    __state.IndustrialShare = Clamp01(industrialShare);
                    __state.MeatShare = Clamp01(meatShare);
                    __state.Valid = true;
                }
            }
            catch (Exception e)
            {
                ModEntry.LogError("AddHunger prefix failed: " + e);
                __state.Valid = false;
            }
        }

        private static void Postfix(GameItem stock, FeedState __state)
        {
            if (!__state.Valid || stock == null)
            {
                return;
            }
            try
            {
                float kcalAfter = AquacultureHelper.GetKcalEaten(stock);
                float absorbed = kcalAfter - __state.KcalBefore;
                if (absorbed < 0f)
                {
                    absorbed = 0f;
                }

                float totalShare = __state.PelletShare + __state.IndustrialShare + __state.MeatShare;
                float otherShare = 1f - totalShare;
                if (otherShare < 0f)
                {
                    otherShare = 0f;
                }

                float spent = __state.SpentKcal;

                FeedLedger ledger = FeedLedgerStore.Load(stock);
                if (ledger == null)
                {
                    ledger = FeedLedger.CreateNew();
                }

                ledger.pelletSpentKcal += spent * __state.PelletShare;
                ledger.industrialSpentKcal += spent * __state.IndustrialShare;
                ledger.meatSpentKcal += spent * __state.MeatShare;
                ledger.otherSpentKcal += spent * otherShare;

                ledger.pelletAbsorbedKcal += absorbed * __state.PelletShare;
                ledger.industrialAbsorbedKcal += absorbed * __state.IndustrialShare;
                ledger.meatAbsorbedKcal += absorbed * __state.MeatShare;
                ledger.otherAbsorbedKcal += absorbed * otherShare;

                ledger.feedEvents++;

                FeedLedgerStore.Save(stock, ledger);
            }
            catch (Exception e)
            {
                ModEntry.LogError("AddHunger postfix failed: " + e);
            }
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }
            if (value > 1f)
            {
                return 1f;
            }
            return value;
        }
    }
}
