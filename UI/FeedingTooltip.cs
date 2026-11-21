using System;
using System.Collections.Generic;
using System.Linq;
using BetterFeedingDetails.Data;
using UnityEngine;

namespace BetterFeedingDetails.UI
{
    internal static class FeedingTooltip
    {
        private const RenderHandler.ColorPalette MONEY_COLOR = RenderHandler.ColorPalette.LightYellow;
        private const RenderHandler.ColorPalette DAILY_FOOD_COLOR = RenderHandler.ColorPalette.LightCyan;
        private const RenderHandler.ColorPalette DAYS_LEFT_COLOR = RenderHandler.ColorPalette.Green;

        internal static void OnTooltipLate(RichTextBuilder builder, GameItem item)
        {
            if (builder == null || item == null)
            {
                return;
            }
            try
            {
                if (FeedLedgerStore.IsTrackableFish(item))
                {
                    AppendValueLine(builder, item);
                }
            }
            catch (Exception e)
            {
                ModEntry.LogError("tooltip render failed: " + e);
            }
        }

        private static void AppendValueLine(RichTextBuilder builder, GameItem item)
        {
            builder.AddLine(" ", ignoreEmpty: true, RenderHandler.ColorPalette.None);
            builder.AddLine(Loc("bfd.value", FormatMoney(EstimateButcherRevenue(item))), ignoreEmpty: true, MONEY_COLOR);
        }

        internal static void AppendRasLines(RichTextBuilder builder, GameItem machine)
        {
            if (builder == null || machine == null || !machine.IsTag("RAS_TAG"))
            {
                return;
            }
            GameGridInventory tank = MachineRAS.GetTankGrid(machine);
            float needKcal = 0f;
            if (tank != null)
            {
                foreach (GameItem stock in tank.childItems)
                {
                    if (stock == null || !AquacultureHelper.IsLiveStock(stock))
                    {
                        continue;
                    }
                    FishSpecies species = AquacultureHelper.GetSpecies(stock);
                    if (species == null)
                    {
                        continue;
                    }
                    needKcal += AquacultureHelper.GetExpectedDrainTonight(stock, species);
                }
            }
            builder.AddLine(" ", ignoreEmpty: true, RenderHandler.ColorPalette.None);
            builder.AddLine(Loc("bfd.dailyFood", Mathf.RoundToInt(needKcal).ToString()), ignoreEmpty: true, DAILY_FOOD_COLOR);

            string days = "--";
            if (needKcal > 0f)
            {
                float remainKcal = GetRemainingFeedKcal(machine);
                days = (remainKcal / needKcal).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
            }
            builder.AddLine(Loc("bfd.daysLeft", days), ignoreEmpty: true, DAYS_LEFT_COLOR);
        }

        private static int GetRemainingFeedKcal(GameItem machine)
        {
            GameSlotInventory slot = MachineRAS.GetDispenserSlot(machine);
            GameItem dispenser = slot == null ? null : slot.childItem;
            if (dispenser == null || !dispenser.IsTag("FEED_DISPENSER_TAG"))
            {
                return 0;
            }
            return Mathf.Max(0, dispenser.GetTagReadonly(MachineFeedDispenser.STORED_KCAL_TAG).GetInt());
        }

        private static int EstimateButcherRevenue(GameItem item)
        {
            FishSpecies species = AquacultureHelper.GetSpecies(item);
            if (species == null)
            {
                return 0;
            }
            int meatKcal = AquacultureHelper.IsDead(item)
                ? item.GetTagReadonly("FISH_CARCASS_KCAL_TAG").GetInt()
                : AquacultureHelper.GetMeatKcal(item);

            List<string> pieces = AquacultureItems.BreakDownMeat(species, meatKcal);
            int revenue = pieces.Sum(AquacultureItems.GetMeatValue);

            int scrapKcal = AquacultureItems.GetLeftoverScrapKcal(species, meatKcal);
            if (scrapKcal > 0)
            {
                revenue += AquacultureItems.GetFishScrapValue(scrapKcal);
            }

            int graftCount = AquacultureHelper.GetSkinGraftCount(species, AquacultureHelper.GetWeightGrams(item));
            revenue += graftCount * AquacultureItems.SKIN_GRAFT_VALUE;

            if (AquacultureHelper.IsPelletTainted(item))
            {
                revenue = revenue * 25 / 100;
            }
            return revenue;
        }

        private static string FormatMoney(double value)
        {
            return value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        }

        private static string Loc(string key, string arg = null)
        {
            string builtIn = BuiltIn(key, arg);
            if (builtIn != null)
            {
                return builtIn;
            }
            string raw = ModHelper.GetLocalized("better_feeding_details", key);
            if (string.IsNullOrEmpty(raw) || raw == key)
            {
                return arg ?? key;
            }
            return arg == null ? raw : string.Format(raw, arg);
        }

        private static string BuiltIn(string key, string arg)
        {
            switch (key)
            {
                case "bfd.value": return "价值：" + arg;
                case "bfd.dailyFood": return "每日食物需要量：" + arg + " kcal";
                case "bfd.daysLeft": return "剩余饲料可维持：" + arg + " 天";
                default: return null;
            }
        }
    }
}
