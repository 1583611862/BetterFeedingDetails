using System;
using System.Globalization;

namespace BetterFeedingDetails.Data
{
    internal static class FeedLedgerStore
    {
        internal const string LEDGER_TAG = "BFD_FEED_LEDGER";
        private const char SEP = '|';

        internal static bool IsTrackableFish(GameItem item)
        {
            if (item == null)
            {
                return false;
            }
            if (AquacultureHelper.IsDead(item))
            {
                return false;
            }
            return item.IsTag("FISH_ITEM_TAG") || item.IsTag("FISH_FRY_TAG");
        }

        internal static FeedLedger Load(GameItem item)
        {
            if (item == null || !item.IsTag(LEDGER_TAG))
            {
                return null;
            }
            try
            {
                string raw = item.GetTagReadonly(LEDGER_TAG).GetString();
                if (string.IsNullOrEmpty(raw))
                {
                    return null;
                }
                return Deserialize(raw);
            }
            catch (Exception e)
            {
                ModEntry.LogError("load ledger failed: " + e);
                return null;
            }
        }

        internal static void Save(GameItem item, FeedLedger ledger)
        {
            if (item == null || ledger == null)
            {
                return;
            }
            try
            {
                string raw = Serialize(ledger);
                item.ModifyTag(LEDGER_TAG, delegate(TagState state)
                {
                    state.Enable().SetString(raw);
                });
            }
            catch (Exception e)
            {
                ModEntry.LogError("save ledger failed: " + e);
            }
        }

        private static string Serialize(FeedLedger ledger)
        {
            return string.Join(SEP.ToString(), new string[]
            {
                ledger.version.ToString(CultureInfo.InvariantCulture),
                F(ledger.industrialSpentKcal),
                F(ledger.pelletSpentKcal),
                F(ledger.meatSpentKcal),
                F(ledger.otherSpentKcal),
                F(ledger.industrialAbsorbedKcal),
                F(ledger.pelletAbsorbedKcal),
                F(ledger.meatAbsorbedKcal),
                F(ledger.otherAbsorbedKcal),
                ledger.feedEvents.ToString(CultureInfo.InvariantCulture)
            });
        }

        private static FeedLedger Deserialize(string raw)
        {
            string[] parts = raw.Split(new char[] { SEP });
            if (parts.Length < 10)
            {
                return null;
            }
            FeedLedger ledger = new FeedLedger();
            int version;
            if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out version))
            {
                return null;
            }
            ledger.version = version;
            ledger.industrialSpentKcal = P(parts[1]);
            ledger.pelletSpentKcal = P(parts[2]);
            ledger.meatSpentKcal = P(parts[3]);
            ledger.otherSpentKcal = P(parts[4]);
            ledger.industrialAbsorbedKcal = P(parts[5]);
            ledger.pelletAbsorbedKcal = P(parts[6]);
            ledger.meatAbsorbedKcal = P(parts[7]);
            ledger.otherAbsorbedKcal = P(parts[8]);
            int events;
            if (int.TryParse(parts[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out events))
            {
                ledger.feedEvents = events;
            }
            return ledger;
        }

        private static string F(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static float P(string value)
        {
            float result;
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            return 0f;
        }
    }
}
