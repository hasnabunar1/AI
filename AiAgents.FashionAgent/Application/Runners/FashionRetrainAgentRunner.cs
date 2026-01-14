using AiAgents.FashionAgent.Application.DTOs;
using AiAgents.FashionAgent.Application.Services;
using AiAgents.FashionAgent.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Runners
{
    public class FashionRetrainAgentRunner
    {
        private readonly FashionAgentDbContext _context;
        private readonly TrainingService _trainingService;

        public FashionRetrainAgentRunner(
            FashionAgentDbContext context,
            TrainingService trainingService)
        {
            _context = context;
            _trainingService = trainingService;
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
                return null; // Nije vrijeme za retrain

            // ===== ACT + LEARN =====
            var newModel = await _trainingService.TrainModelAsync(activate: true, ct);

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

            if (settings.NewGoldSinceLastTrain < settings.GoldThresholdForRetrain)
                return false;

            return true;
        }
    }
}