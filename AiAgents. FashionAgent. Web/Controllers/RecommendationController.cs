using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.Infrastructure.Services;
using AiAgents.FashionAgent.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Web.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly FashionAgentDbContext _context;
        private readonly LearningService _learningService;

        public RecommendationController(FashionAgentDbContext context, LearningService learningService)
        {
            _context = context;
            _learningService = learningService;
        }

        public async Task<IActionResult> Index()
        {
            // Get learned preferences
            var preferences = await _learningService.AnalyzeFeedbackAsync();

            // Stats for display
            ViewBag.TotalItems = await _context.ClothingItems.CountAsync();
            ViewBag.TotalLikes = await _context.UserFeedbacks.CountAsync(x => x.FeedbackType == "like");
            ViewBag.TotalDislikes = await _context.UserFeedbacks.CountAsync(x => x.FeedbackType == "dislike");
            ViewBag.TotalFeedback = preferences.TotalFeedback;

            // What agent has learned
            ViewBag.LearnedPreferences = preferences;
            ViewBag.HasLearned = preferences.TotalFeedback >= 5;

            // Dropdown options
            ViewBag.Genders = await _context.ClothingItems.Select(x => x.Gender).Distinct().ToListAsync();
            ViewBag.Categories = await _context.ClothingItems.Select(x => x.Category).Distinct().ToListAsync();
            ViewBag.Colors = await _context.ClothingItems.Select(x => x.Color).Distinct().ToListAsync();
            ViewBag.Materials = await _context.ClothingItems.Select(x => x.Material).Distinct().ToListAsync();
            ViewBag.Styles = await _context.ClothingItems.Select(x => x.Style).Distinct().ToListAsync();
            ViewBag.Brands = await _context.ClothingItems.Select(x => x.Brand).Distinct().ToListAsync();
            ViewBag.TrendStatuses = await _context.ClothingItems.Select(x => x.TrendStatus).Distinct().ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRecommendations(
            string? gender,
            string? category,
            string? color,
            string? material,
            string? style,
            string? brand,
            string? trendStatus,
            decimal? minPrice,
            decimal? maxPrice)
        {
            // Get learned preferences
            var preferences = await _learningService.AnalyzeFeedbackAsync();

            // Start with all items
            var query = _context.ClothingItems.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(gender))
                query = query.Where(x => x.Gender == gender);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(x => x.Category == category);

            if (!string.IsNullOrEmpty(color))
                query = query.Where(x => x.Color == color);

            if (!string.IsNullOrEmpty(material))
                query = query.Where(x => x.Material == material);

            if (!string.IsNullOrEmpty(style))
                query = query.Where(x => x.Style == style);

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(x => x.Brand == brand);

            if (!string.IsNullOrEmpty(trendStatus))
                query = query.Where(x => x.TrendStatus == trendStatus);

            if (minPrice.HasValue)
                query = query.Where(x => x.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(x => x.Price <= maxPrice.Value);

            // Get items
            var items = await query.ToListAsync();

            // 🧠 AI LEARNING:  Calculate score for each item and sort
            var scoredItems = items
                .Select(item => new
                {
                    Item = item,
                    Score = _learningService.CalculateItemScore(item, preferences)
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.Item)
                .Take(20)
                .ToList();

            return PartialView("_RecommendationResults", scoredItems);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackModel model)
        {
            if (model == null || model.ItemId <= 0)
            {
                return BadRequest("Invalid feedback data.");
            }

            var feedback = new UserFeedback
            {
                ClothingItemId = model.ItemId,
                FeedbackType = model.Feedback,
                DislikeReason = model.DislikeReason,
                LikeReason = model.LikeReason,
                SubmittedAt = DateTime.UtcNow
            };

            _context.UserFeedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            var totalFeedback = await _context.UserFeedbacks.CountAsync();
            var shouldRetrain = totalFeedback % 50 == 0;

            // Get updated preferences after new feedback
            var preferences = await _learningService.AnalyzeFeedbackAsync();

            return Ok(new
            {
                success = true,
                message = "Feedback submitted! ",
                shouldRetrain = shouldRetrain,
                totalFeedback = totalFeedback,
                topPriority = preferences.TopPriority,
                avoidPriority = preferences.AvoidPriority
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLearningStats()
        {
            var preferences = await _learningService.AnalyzeFeedbackAsync();

            return Json(new
            {
                totalFeedback = preferences.TotalFeedback,
                likedReasons = preferences.LikedReasons,
                dislikedReasons = preferences.DislikedReasons,
                topPriority = preferences.TopPriority,
                avoidPriority = preferences.AvoidPriority,
                preferredColors = preferences.PreferredColors,
                preferredMaterials = preferences.PreferredMaterials,
                preferredTrendStatuses = preferences.PreferredTrendStatuses,
                preferredMaxPrice = preferences.PreferredMaxPrice,
                avoidColors = preferences.AvoidColors,
                avoidMaterials = preferences.AvoidMaterials
            });
        }
    }
}