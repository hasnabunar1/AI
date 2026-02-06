using AiAgents.FashionAgent.Application.DTOs;
using AiAgents.FashionAgent.Application.Services;
using AiAgents.FashionAgent.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Runners;

public class FashionRetrainAgentRunner
{
    private readonly FashionAgentDbContext _context;
    private readonly TrainingService _trainingService;
    private readonly RecommendationService _recommendationService;

    public FashionRetrainAgentRunner(
        FashionAgentDbContext context,
        TrainingService trainingService,
        RecommendationService recommendationService)
    {
        _context = context;
        _trainingService = trainingService;
        _recommendationService = recommendationService;
    }

    public async Task<RetrainTickResult?> StepAsync(CancellationToken ct)
    {
        // ===== SENSE =====
        var settings = await _context.SystemSettings.FirstOrDefaultAsync(ct);

        if (settings == null)
            return null;

        // ===== THINK =====
        bool shouldRetrain = ShouldRetrain(settings);

        if (!shouldRetrain)
            return null;

        // ===== ACT + LEARN =====
        var newModel = await _trainingService.TrainModelAsync(activate: true, ct);

        // 🔥 KRITIČNO: Resetuj brojač nakon uspješnog retraina
        settings.NewGoldSinceLastTrain = 0;
        settings.LastRetrainAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return new RetrainTickResult
        {
            RetrainPerformed = true,
            NewModelVersionId = newModel.Id,
            TrainingSamplesUsed = newModel.TrainingSamplesCount,
            NewModelAccuracy = newModel.Accuracy,
            TrainedAt = newModel.TrainedAt
        };
    }

    private bool ShouldRetrain(Domain.SystemSettings settings)
    {
        if (!settings.RetrainEnabled)
            return false;

        // ✅ Koristi NewGoldSinceLastTrain signal iz SubmitFeedback
        if (settings.NewGoldSinceLastTrain < settings.GoldThresholdForRetrain)
            return false;

        return true;
    }
}