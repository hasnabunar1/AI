using AiAgents.FashionAgent.Application.DTOs;
using AiAgents.FashionAgent.Application.Services;

namespace AiAgents.FashionAgent.Application.Runners
{
    public class FashionScoringAgentRunner
    {
        private readonly QueueService _queueService;
        private readonly ScoringService _scoringService;

        public FashionScoringAgentRunner(
            QueueService queueService,
            ScoringService scoringService)
        {
            _queueService = queueService;
            _scoringService = scoringService;
        }

        public async Task<ScoringTickResult?> StepAsync(CancellationToken ct)
        {
            // ===== SENSE =====
            // Uzmi sljedeći artikl iz queue-a
            var item = await _queueService.DequeueNextAsync(ct);

            if (item == null)
                return null; // Nema artikala za obradu

            // ===== THINK =====
            // ML model predviđa vjerovatnoću + donosi odluku
            var prediction = await _scoringService.ScoreAsync(item, ct);

            // ===== ACT =====
            // Spremi rezultat u bazu i ažuriraj status
            await _scoringService.ApplyDecisionAsync(item, prediction, ct);

            return new ScoringTickResult
            {
                ClothingItemId = item.Id,
                Brand = item.Brand,
                Category = item.Category,
                ProbabilityTrending = prediction.Probability,
                Decision = prediction.Decision,
                NewStatus = item.Status,
                ProcessedAt = DateTime.UtcNow
            };
        }
    }
}