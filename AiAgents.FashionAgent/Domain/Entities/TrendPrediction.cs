using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiAgents.FashionAgent.Domain.Enums;

namespace AiAgents.FashionAgent.Domain.Entities
{
    public class TrendPrediction
    {
        public int Id { get; set; }
        public int ClothingItemId { get; set; }
        public ClothingItem ClothingItem { get; set; } = null!;

        public double Probability { get; set; }
        public TrendDecision Decision { get; set; }
        public int ModelVersionId { get; set; }
        public ModelVersion ModelVersion { get; set; } = null!;

        public DateTime PredictedAt { get; set; } = DateTime.UtcNow;

        // Za Learn ciklus - ručne korekcije
        public bool? ManualLabel { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}