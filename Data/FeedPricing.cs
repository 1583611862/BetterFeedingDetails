namespace BetterFeedingDetails.Data
{
    internal static class FeedPricing
    {
        internal const int INDUSTRIAL_KCAL_PER_UNIT = 5000;
        internal const long INDUSTRIAL_BASE_VALUE = 100L;
        internal const int PELLET_KCAL_PER_UNIT = 4000;
        internal const long PELLET_BASE_VALUE = 40L;

        internal static float IndustrialCostPerKcal
        {
            get { return (float)INDUSTRIAL_BASE_VALUE / INDUSTRIAL_KCAL_PER_UNIT; }
        }

        internal static float PelletCostPerKcal
        {
            get { return (float)PELLET_BASE_VALUE / PELLET_KCAL_PER_UNIT; }
        }

        internal static double EstimateCost(FeedLedger ledger)
        {
            if (ledger == null)
            {
                return 0d;
            }
            double industrial = ledger.industrialSpentKcal * IndustrialCostPerKcal;
            double pellet = ledger.pelletSpentKcal * PelletCostPerKcal;
            return industrial + pellet;
        }

        internal static bool HasMeatOrOther(FeedLedger ledger)
        {
            if (ledger == null)
            {
                return false;
            }
            return ledger.meatSpentKcal > 0f || ledger.otherSpentKcal > 0f;
        }
    }
}
