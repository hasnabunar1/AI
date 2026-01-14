using AiAgents.FashionAgent.Domain;
using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Domain.Enums;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.ML;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Services
{
    public class ScoringService
    {
        private readonly FashionAgentDbContext _context;
        private readonly IFashionTrendClassifier _classifier;

        public ScoringService(
            FashionAgentDbContext context,
            IFashionTrendClassifier classifier)
        {
            _context = context;
            _classifier = classifier;
        }

        public async Task<TrendPrediction> ScoreAsync(ClothingItem item, CancellationToken ct)
        {
            var activeModel = await _context.ModelVersions
                .FirstOrDefaultAsync(x => x.IsActive, ct);

            if (activeModel == null)
                throw new InvalidOperationException("No active model found.  Please train a model first.");

            var probability = await _classifier.PredictAsync(item, ct);

            var settings = await GetSettingsAsync(ct);

            var decision = ApplyThresholds(probability, settings);

            var prediction = new TrendPrediction
            {
                ClothingItemId = item.Id,
                Probability = probability,
                Decision = decision,
                ModelVersionId = activeModel.Id,
                PredictedAt = DateTime.UtcNow
            };

            return prediction;
        }

        public async Task ApplyDecisionAsync(ClothingItem item, TrendPrediction prediction, CancellationToken ct)
        {
            _context.TrendPredictions.Add(prediction);

            item.Status = prediction.Decision switch
            {
                TrendDecision.Recommend => ItemStatus.Recommended,
                TrendDecision.Archive => ItemStatus.Archived,
                TrendDecision.PendingReview => ItemStatus.PendingReview,
                _ => item.Status
            };
            item.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
        }

        private TrendDecision ApplyThresholds(double probability, SystemSettings settings)
        {
            if (probability >= settings.RecommendThreshold)
                return TrendDecision.Recommend;

            if (probability <= settings.ArchiveThreshold)
                return TrendDecision.Archive;

            return TrendDecision.PendingReview;
        }

        private async Task<SystemSettings> GetSettingsAsync(CancellationToken ct)
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync(ct);
            return settings ?? new SystemSettings();
        }
    }
}