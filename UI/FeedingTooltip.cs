using System;
using System.Collections.Generic;
using System.Linq;
using BetterFeedingDetails.Data;
using UnityEngine;

namespace BetterFeedingDetails.UI
{
    internal static class FeedingTooltip
    {
        private const string PELLET_LABEL = "污泥饲料粒";
        private const string INDUSTRIAL_LABEL = "工业鱼饲料";
        private const string MEAT_LABEL = "肉类";
        private const string OTHER_LABEL = "其他食物";

        private const RenderHandler.ColorPalette MONEY_COLOR = RenderHandler.ColorPalette.LightYellow;

        internal static void OnTooltipLate(RichTextBuilder builder, GameItem item)
        {
            if (builder == null || item == null)
            {
                return;
            }
            if (!FeedLedgerStore.IsTrackableFish(item))
            {
                return;
            }
            try
            {
                FeedLedger ledger = FeedLedgerStore.Load(item);
                AppendEconomicLines(builder, item, ledger);
            }
            catch (Exception e)
            {
                ModEntry.LogError("tooltip render failed: " + e);
            }
        }

        private static void AppendEconomicLines(RichTextBuilder builder, GameItem item, FeedLedger ledger)
        {
            builder.AddLine(" ", ignoreEmpty: true, RenderHandler.ColorPalette.None);

            int expectedRevenue = EstimateButcherRevenue(item);

            bool hasLedger = ledger != null && ledger.SpentKcal > 0f;
            if (!hasLedger)
            {
                builder.AddLine(Loc("bfd.value", FormatMoney(expectedRevenue)), ignoreEmpty: true, MONEY_COLOR);
                return;
            }

            double cost = FeedPricing.EstimateCost(ledger);
            bool hasUnpriced = FeedPricing.HasMeatOrOther(ledger);
            double profit = expectedRevenue - cost;

            string costText = FormatMoney(cost) + (hasUnpriced ? "+" : string.Empty);
            bool isProfit = profit >= 0d;
            RenderHandler.ColorPalette profitColor = isProfit ? RenderHandler.ColorPalette.Green : RenderHandler.ColorPalette.LightRed;
            string sign = isProfit ? "+" : string.Empty;
            string profitText = sign + FormatMoney(profit) + (hasUnpriced ? "?" : string.Empty);

            builder.Add(Loc("bfd.value", FormatMoney(expectedRevenue)), MONEY_COLOR, "    ");
            builder.Add(Loc("bfd.cost", costText), MONEY_COLOR, "    ");
            builder.AddLine(Loc("bfd.profit", profitText), ignoreEmpty: true, profitColor);

            builder.AddLine(Loc("bfd.foodTitle"), ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
            AppendFoodLine(builder, INDUSTRIAL_LABEL, ledger.industrialSpentKcal, ledger.industrialAbsorbedKcal, FeedPricing.IndustrialCostPerKcal, true);
            AppendFoodLine(builder, PELLET_LABEL, ledger.pelletSpentKcal, ledger.pelletAbsorbedKcal, FeedPricing.PelletCostPerKcal, true);
            AppendFoodLine(builder, MEAT_LABEL, ledger.meatSpentKcal, ledger.meatAbsorbedKcal, 0f, false);
            AppendFoodLine(builder, OTHER_LABEL, ledger.otherSpentKcal, ledger.otherAbsorbedKcal, 0f, false);

            if (hasUnpriced)
            {
                builder.AddLine(Loc("bfd.unpricedNote"), ignoreEmpty: true, RenderHandler.ColorPalette.Yellow);
            }
            builder.AddLine(Loc("bfd.estimateNote"), ignoreEmpty: true, RenderHandler.ColorPalette.Gray);
        }

        private static void AppendFoodLine(RichTextBuilder builder, string label, float spentKcal, float absorbedKcal, float costPerKcal, bool priced)
        {
            if (spentKcal <= 0f)
            {
                return;
            }
            string amount;
            if (priced)
            {
                double lineCost = spentKcal * costPerKcal;
                amount = Mathf.RoundToInt(spentKcal) + " kcal  " + FormatMoney(lineCost);
            }
            else
            {
                amount = Mathf.RoundToInt(spentKcal) + " kcal  --";
            }
            builder.AddLine("  " + label + " x" + Mathf.RoundToInt(absorbedKcal) + "  " + amount, ignoreEmpty: true, RenderHandler.ColorPalette.LightCyan);
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
                case "bfd.cost": return "花费：" + arg;
                case "bfd.profit": return "当前利润：" + arg;
                case "bfd.foodTitle": return "食物消耗：";
                case "bfd.unpricedNote": return "* 肉类/其他食物缺少可确认单价，未计入花费";
                case "bfd.estimateNote": return "* 价值/利润按当前屠宰基础价值估算";
                default: return null;
            }
        }
    }
}
