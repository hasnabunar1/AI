using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Infrastructure.Services
{
    public class LearnedPreferences
    {
        public Dictionary<string, int> LikedReasons { get; set; } = new();
        public Dictionary<string, int> DislikedReasons { get; set; } = new();
        public string TopPriority { get; set; } = "rating";
        public string AvoidPriority { get; set; } = "";
        public double PreferredMaxPrice { get; set; } = 500;
        public List<string> PreferredColors { get; set; } = new();
        public List<string> PreferredMaterials { get; set; } = new();
        public List<string> PreferredTrendStatuses { get; set; } = new();
        public List<string> AvoidColors { get; set; } = new();
        public List<string> AvoidMaterials { get; set; } = new();
        public int TotalFeedback { get; set; }
        public DateTime LastLearningDate { get; set; }
    }

    public class LearningService
    {
        private readonly FashionAgentDbContext _context;

        public LearningService(FashionAgentDbContext context)
        {
            _context = context;
        }

        public async Task<LearnedPreferences> AnalyzeFeedbackAsync()
        {
            var preferences = new LearnedPreferences();

            var allFeedback = await _context.UserFeedbacks
                .Include(f => f.ClothingItem)
                .ToListAsync();

            preferences.TotalFeedback = allFeedback.Count;
            preferences.LastLearningDate = DateTime.UtcNow;

            if (allFeedback.Count == 0)
            {
                return preferences;
            }

            // Analiziraj LIKE razloge
            var likeReasons = allFeedback
                .Where(f => f.FeedbackType == "like" && !string.IsNullOrEmpty(f.LikeReason))
                .GroupBy(f => f.LikeReason)
                .ToDictionary(g => g.Key!, g => g.Count());

            preferences.LikedReasons = likeReasons;

            // Analiziraj DISLIKE razloge
            var dislikeReasons = allFeedback
                .Where(f => f.FeedbackType == "dislike" && !string.IsNullOrEmpty(f.DislikeReason))
                .GroupBy(f => f.DislikeReason)
                .ToDictionary(g => g.Key!, g => g.Count());

            preferences.DislikedReasons = dislikeReasons;

            // Odredi TOP prioritet za sortiranje
            if (likeReasons.Any())
            {
                preferences.TopPriority = likeReasons.OrderByDescending(x => x.Value).First().Key;
            }

            // Odredi šta IZBJEGAVATI
            if (dislikeReasons.Any())
            {
                preferences.AvoidPriority = dislikeReasons.OrderByDescending(x => x.Value).First().Key;
            }

            // Analiziraj LIKED artikle - koje boje, materijale, cijene vole
            var likedItems = allFeedback
                .Where(f => f.FeedbackType == "like" && f.ClothingItem != null)
                .Select(f => f.ClothingItem!)
                .ToList();

            if (likedItems.Any())
            {
                // Prosječna cijena liked artikala
                preferences.PreferredMaxPrice = likedItems.Average(x => (double)x.Price) * 1.3;

                // Najpopularnije boje
                preferences.PreferredColors = likedItems
                    .GroupBy(x => x.Color)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList();

                // Najpopularniji materijali
                preferences.PreferredMaterials = likedItems
                    .GroupBy(x => x.Material)
                    .OrderByDescending(g => g.Count())
                    .Take(3)
                    .Select(g => g.Key)
                    .ToList();

                // Najpopularniji trend statusi
                preferences.PreferredTrendStatuses = likedItems
                    .GroupBy(x => x.TrendStatus)
                    .OrderByDescending(g => g.Count())
                    .Take(2)
                    .Select(g => g.Key)
                    .ToList();
            }

            // Analiziraj DISLIKED artikle - šta izbjegavati
            var dislikedItems = allFeedback
                .Where(f => f.FeedbackType == "dislike" && f.ClothingItem != null)
                .Select(f => f.ClothingItem!)
                .ToList();

            if (dislikedItems.Any())
            {
                // Boje koje treba izbjegavati
                preferences.AvoidColors = dislikedItems
                    .GroupBy(x => x.Color)
                    .OrderByDescending(g => g.Count())
                    .Take(2)
                    .Select(g => g.Key)
                    .ToList();

                // Materijali koje treba izbjegavati
                preferences.AvoidMaterials = dislikedItems
                    .GroupBy(x => x.Material)
                    .OrderByDescending(g => g.Count())
                    .Take(2)
                    .Select(g => g.Key)
                    .ToList();
            }

            return preferences;
        }

        public double CalculateItemScore(Domain.Entities.ClothingItem item, LearnedPreferences prefs)
        {
            double score = 0;

            // Bazni score od ratinga
            score += item.CustomerRating * 10;

            // BOOST za preferirane boje (+20)
            if (prefs.PreferredColors.Contains(item.Color))
                score += 20;

            // BOOST za preferirane materijale (+15)
            if (prefs.PreferredMaterials.Contains(item.Material))
                score += 15;

            // BOOST za preferirane trend statuse (+25)
            if (prefs.PreferredTrendStatuses.Contains(item.TrendStatus))
                score += 25;

            // BOOST za artikle ispod preferirane cijene (+20)
            if ((double)item.Price <= prefs.PreferredMaxPrice)
                score += 20;

            // PENALTY za boje koje treba izbjegavati (-30)
            if (prefs.AvoidColors.Contains(item.Color))
                score -= 30;

            // PENALTY za materijale koje treba izbjegavati (-25)
            if (prefs.AvoidMaterials.Contains(item.Material))
                score -= 25;

            // PENALTY za preskupe artikle (-20)
            if ((double)item.Price > prefs.PreferredMaxPrice * 1.5)
                score -= 20;

            // Dodatni BOOST bazirano na top prioritetu
            switch (prefs.TopPriority)
            {
                case "price":
                    score += (500 - (double)item.Price) / 10; // Jeftiniji = veći score
                    break;
                case "trending":
                    if (item.TrendStatus == "Trending") score += 30;
                    break;
                case "style":
                    score += item.PopularityScore * 5;
                    break;
                case "material":
                    if (prefs.PreferredMaterials.Contains(item.Material)) score += 20;
                    break;
                case "color":
                    if (prefs.PreferredColors.Contains(item.Color)) score += 20;
                    break;
                case "brand":
                    score += item.CustomerRating * 5;
                    break;
            }

            return score;
        }
    }
}