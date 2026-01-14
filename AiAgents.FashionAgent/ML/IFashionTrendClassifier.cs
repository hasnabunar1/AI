//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using AiAgents.FashionAgent.Domain.Entities;

namespace AiAgents.FashionAgent.ML
{
    public interface IFashionTrendClassifier
    {
        Task<double> PredictAsync(ClothingItem item, CancellationToken ct);
        Task<double> TrainAsync(List<ClothingItem> trainingData, string modelPath, CancellationToken ct);
        Task LoadModelAsync(string modelPath, CancellationToken ct);
    }
}