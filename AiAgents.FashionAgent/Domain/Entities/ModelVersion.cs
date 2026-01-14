using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.FashionAgent.Domain.Entities
{
    public class ModelVersion
    {
        public int Id { get; set; }
        public string ModelPath { get; set; } = string.Empty;
        public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
        public int TrainingSamplesCount { get; set; }
        public double? Accuracy { get; set; }
        public bool IsActive { get; set; }

        public ICollection<TrendPrediction> Predictions { get; set; } = new List<TrendPrediction>();
    }
}