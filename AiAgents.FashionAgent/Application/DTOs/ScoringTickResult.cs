using AiAgents.FashionAgent.Domain.Enums;

namespace AiAgents.FashionAgent.Application.DTOs
{
    public class ScoringTickResult
    {
        public int ClothingItemId { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public double ProbabilityTrending { get; set; }
        public TrendDecision Decision { get; set; }
        public ItemStatus NewStatus { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}