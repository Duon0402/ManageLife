using ManageLife.Commons;

namespace ManageLife.Helpers
{
    public static class Sm2Algorithm
    {
        /// <summary>
        /// quality: 0=Again, 2=Hard, 3=Good, 5=Easy
        /// </summary>
        public static (int repetitions, double easinessFactor, int intervalDays, DateTime nextReviewDate)
            Calculate(int repetitions, double easinessFactor, int intervalDays, int quality)
        {
            if (quality < 3)
            {
                repetitions = 0;
                intervalDays = 1;
            }
            else
            {
                repetitions++;
                intervalDays = repetitions switch
                {
                    1 => 1,
                    2 => 6,
                    _ => (int)Math.Round(intervalDays * easinessFactor)
                };
            }

            easinessFactor += 0.1 - (5 - quality) * (0.08 + (5 - quality) * 0.02);
            easinessFactor = Math.Max(1.3, easinessFactor);

            return (repetitions, easinessFactor, intervalDays, DateTime.UtcNow.Date.AddDays(intervalDays));
        }

        public static VocabMasteryLevel GetMasteryLevel(int repetitions, int intervalDays)
        {
            if (repetitions == 0) return VocabMasteryLevel.New;
            if (repetitions < 3) return VocabMasteryLevel.Learning;
            if (intervalDays < 21) return VocabMasteryLevel.Review;
            return VocabMasteryLevel.Mastered;
        }
    }
}
