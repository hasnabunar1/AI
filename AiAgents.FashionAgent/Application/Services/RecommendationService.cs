using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Services;

public class RecommendationService
{
    private readonly FashionAgentDbContext _context;
    private readonly LearningService _learningService;

    public RecommendationService(FashionAgentDbContext context, LearningService learningService)
    {
        _context = context;
        _learningService = learningService;
    }

    /// <summary>
    /// Scoring/sort/top-20 logika - premješteno iz controllera
    /// </summary>
    public async Task<List<ClothingItem>> GetRecommendationsAsync(
        string? gender, string? category, string? color, string? material,
        string? style, string? brand, string? trendStatus,
        decimal? minPrice, decimal? maxPrice)
    {
        var preferences = await _learningService.AnalyzeFeedbackAsync();

        var query = _context.ClothingItems.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(gender)) query = query.Where(x => x.Gender == gender);
        if (!string.IsNullOrEmpty(category)) query = query.Where(x => x.Category == category);
        if (!string.IsNullOrEmpty(color)) query = query.Where(x => x.Color == color);
        if (!string.IsNullOrEmpty(material)) query = query.Where(x => x.Material == material);
        if (!string.IsNullOrEmpty(style)) query = query.Where(x => x.Style == style);
        if (!string.IsNullOrEmpty(brand)) query = query.Where(x => x.Brand == brand);
        if (!string.IsNullOrEmpty(trendStatus)) query = query.Where(x => x.TrendStatus == trendStatus);
        if (minPrice.HasValue) query = query.Where(x => x.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(x => x.Price <= maxPrice.Value);

        var items = await query.ToListAsync();

        // 🧠 AI LEARNING: Score, sort, and take top 20
        return items
            .Select(item => new { Item = item, Score = _learningService.CalculateItemScore(item, preferences) })
            .OrderByDescending(x => x.Score)
            .Take(20)
            .Select(x => x.Item)
            .ToList();
    }

    /// <summary>
    /// Submit feedback i inkrementiraj NewGoldSinceLastTrain za retrain signal
    /// </summary>
    public async Task<FeedbackResult> SubmitFeedbackAsync(int itemId, string feedbackType, string? likeReason, string? dislikeReason)
    {
        var feedback = new UserFeedback
        {
            ClothingItemId = itemId,
            FeedbackType = feedbackType,
            DislikeReason = dislikeReason,
            LikeReason = likeReason,
            SubmittedAt = DateTime.UtcNow
        };

        _context.UserFeedbacks.Add(feedback);

        // 🔥 KRITIČNO: Inkrementiraj NewGoldSinceLastTrain za retrain runner signal
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        if (settings != null)
        {
            settings.NewGoldSinceLastTrain++;
        }

        await _context.SaveChangesAsync();

        var preferences = await _learningService.AnalyzeFeedbackAsync();

        return new FeedbackResult
        {
            Success = true,
            Message = "Feedback submitted!",
            TotalFeedback = preferences.TotalFeedback,
            TopPriority = preferences.TopPriority,
            AvoidPriority = preferences.AvoidPriority
        };
    }
}

public class FeedbackResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public int TotalFeedback { get; set; }
    public string TopPriority { get; set; } = "";
    public string AvoidPriority { get; set; } = "";
}