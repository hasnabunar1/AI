using AiAgents.FashionAgent.Application.Services;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.Infrastructure.Services;
using AiAgents.FashionAgent.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Web.Controllers;

public class RecommendationController : Controller
{
    private readonly FashionAgentDbContext _context;
    private readonly LearningService _learningService;
    private readonly RecommendationService _recommendationService;

    public RecommendationController(
        FashionAgentDbContext context,
        LearningService learningService,
        RecommendationService recommendationService)
    {
        _context = context;
        _learningService = learningService;
        _recommendationService = recommendationService;
    }

    public async Task<IActionResult> Index()
    {
        var preferences = await _learningService.AnalyzeFeedbackAsync();

        ViewBag.TotalItems = await _context.ClothingItems.CountAsync();
        ViewBag.TotalLikes = await _context.UserFeedbacks.CountAsync(x => x.FeedbackType == "like");
        ViewBag.TotalDislikes = await _context.UserFeedbacks.CountAsync(x => x.FeedbackType == "dislike");
        ViewBag.TotalFeedback = preferences.TotalFeedback;
        ViewBag.LearnedPreferences = preferences;
        ViewBag.HasLearned = preferences.TotalFeedback >= 5;

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
        string? gender, string? category, string? color, string? material,
        string? style, string? brand, string? trendStatus,
        decimal? minPrice, decimal? maxPrice)
    {
        // ✅ THIN CONTROLLER: samo poziva servis i vraća rezultat
        var recommendations = await _recommendationService.GetRecommendationsAsync(
            gender, category, color, material, style, brand, trendStatus, minPrice, maxPrice);

        return PartialView("_RecommendationResults", recommendations);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackModel model)
    {
        if (model == null || model.ItemId <= 0)
            return BadRequest("Invalid feedback data.");

        // ✅ THIN CONTROLLER: delegira application servisu
        var result = await _recommendationService.SubmitFeedbackAsync(
            model.ItemId, model.Feedback, model.LikeReason, model.DislikeReason);

        return Ok(new
        {
            success = result.Success,
            message = result.Message,
            totalFeedback = result.TotalFeedback,
            topPriority = result.TopPriority,
            avoidPriority = result.AvoidPriority
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