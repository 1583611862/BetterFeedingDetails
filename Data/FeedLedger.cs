using System;

namespace BetterFeedingDetails.Data
{
    [Serializable]
    public class FeedLedger
    {
        public int version;

        public float industrialSpentKcal;
        public float pelletSpentKcal;
        public float meatSpentKcal;
        public float otherSpentKcal;

        public float industrialAbsorbedKcal;
        public float pelletAbsorbedKcal;
        public float meatAbsorbedKcal;
        public float otherAbsorbedKcal;

        public int feedEvents;

        public float SpentKcal
        {
            get { return industrialSpentKcal + pelletSpentKcal + meatSpentKcal + otherSpentKcal; }
        }

        public float AbsorbedKcal
        {
            get { return industrialAbsorbedKcal + pelletAbsorbedKcal + meatAbsorbedKcal + otherAbsorbedKcal; }
        }

        public static FeedLedger CreateNew()
        {
            return new FeedLedger { version = 1 };
        }

        public FeedLedger SplitShared(int shareCount)
        {
            if (shareCount <= 1)
            {
                return this;
            }
            float divisor = shareCount;
            return new FeedLedger
            {
                version = version,
                industrialSpentKcal = industrialSpentKcal / divisor,
                pelletSpentKcal = pelletSpentKcal / divisor,
                meatSpentKcal = meatSpentKcal / divisor,
                otherSpentKcal = otherSpentKcal / divisor,
                industrialAbsorbedKcal = industrialAbsorbedKcal / divisor,
                pelletAbsorbedKcal = pelletAbsorbedKcal / divisor,
                meatAbsorbedKcal = meatAbsorbedKcal / divisor,
                otherAbsorbedKcal = otherAbsorbedKcal / divisor,
                feedEvents = feedEvents / shareCount
            };
        }
    }
}
