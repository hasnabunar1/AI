using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiAgents.FashionAgent.Domain
{
    public class SystemSettings
    {
        public int Id { get; set; } = 1;

        // Thresholds za odluke
        public double RecommendThreshold { get; set; } = 0.7;
        public double ArchiveThreshold { get; set; } = 0.3;

        // Retrain settings
        public bool RetrainEnabled { get; set; } = true;
        public int NewGoldSinceLastTrain { get; set; } = 0;
        public int GoldThresholdForRetrain { get; set; } = 20;

        public DateTime LastRetrainAt { get; set; } = DateTime.MinValue;
    }
}