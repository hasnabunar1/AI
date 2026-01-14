using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Infrastructure;
using AiAgents.FashionAgent.ML;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Services
{
    public class TrainingService
    {
        private readonly FashionAgentDbContext _context;
        private readonly IFashionTrendClassifier _classifier;

        public TrainingService(
            FashionAgentDbContext context,
            IFashionTrendClassifier classifier)
        {
            _context = context;
            _classifier = classifier;
        }

        public async Task<ModelVersion> TrainModelAsync(bool activate, CancellationToken ct)
        {
            var trainingData = await _context.ClothingItems
                .Where(x => x.TrendStatus == "Trending" || x.TrendStatus == "Outdated"
                         || x.TrendStatus == "Emerging" || x.TrendStatus == "Classic")
                .ToListAsync(ct);

            var modelPath = $"Models/fashion_model_{DateTime.UtcNow:yyyyMMddHHmmss}. zip";
            var accuracy = await _classifier.TrainAsync(trainingData, modelPath, ct);

            var newVersion = new ModelVersion
            {
                ModelPath = modelPath,
                TrainedAt = DateTime.UtcNow,
                TrainingSamplesCount = trainingData.Count,
                Accuracy = accuracy,
                IsActive = false
            };

            _context.ModelVersions.Add(newVersion);

            if (activate)
            {
                var currentActive = await _context.ModelVersions
                    .FirstOrDefaultAsync(x => x.IsActive, ct);

                if (currentActive != null)
                    currentActive.IsActive = false;

                newVersion.IsActive = true;

                await _classifier.LoadModelAsync(modelPath, ct);
            }

            var settings = await _context.SystemSettings.FirstOrDefaultAsync(ct);
            if (settings != null)
            {
                settings.NewGoldSinceLastTrain = 0;
                settings.LastRetrainAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);

            return newVersion;
        }
    }
}
