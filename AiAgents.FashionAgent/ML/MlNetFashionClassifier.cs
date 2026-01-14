using AiAgents.FashionAgent.Domain.Entities;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace AiAgents.FashionAgent.ML
{
    public class MlNetFashionClassifier : IFashionTrendClassifier
    {
        private readonly MLContext _mlContext;
        private ITransformer? _model;
        private PredictionEngine<FashionInput, FashionPrediction>? _predictionEngine;

        public MlNetFashionClassifier()
        {
            _mlContext = new MLContext(seed: 42);
        }

        public Task<double> PredictAsync(ClothingItem item, CancellationToken ct)
        {
            if (_predictionEngine == null)
                throw new InvalidOperationException("Model not loaded.  Call LoadModelAsync first.");

            var input = new FashionInput
            {
                Brand = item.Brand,
                Category = item.Category,
                Color = item.Color,
                Material = item.Material,
                Style = item.Style,
                Gender = item.Gender,
                Season = item.Season,
                Price = (float)item.Price,
                PopularityScore = (float)item.PopularityScore,
                CustomerRating = (float)item.CustomerRating
            };

            var prediction = _predictionEngine.Predict(input);

            return Task.FromResult((double)prediction.Probability);
        }

        public Task<double> TrainAsync(List<ClothingItem> trainingData, string modelPath, CancellationToken ct)
        {
            var data = trainingData.Select(item => new FashionInput
            {
                Brand = item.Brand,
                Category = item.Category,
                Color = item.Color,
                Material = item.Material,
                Style = item.Style,
                Gender = item.Gender,
                Season = item.Season,
                Price = (float)item.Price,
                PopularityScore = (float)item.PopularityScore,
                CustomerRating = (float)item.CustomerRating,
                IsTrending = item.TrendStatus == "Trending" || item.TrendStatus == "Emerging"
            }).ToList();

            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("BrandEncoded", "Brand")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("CategoryEncoded", "Category"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("ColorEncoded", "Color"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("MaterialEncoded", "Material"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("StyleEncoded", "Style"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("GenderEncoded", "Gender"))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    "BrandEncoded", "CategoryEncoded", "ColorEncoded",
                    "MaterialEncoded", "StyleEncoded", "GenderEncoded",
                    "Price", "PopularityScore", "CustomerRating"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(
                    labelColumnName: "IsTrending",
                    featureColumnName: "Features"));

            _model = pipeline.Fit(split.TrainSet);

            var predictions = _model.Transform(split.TestSet);
            var metrics = _mlContext.BinaryClassification.Evaluate(predictions, "IsTrending");

            var directory = Path.GetDirectoryName(modelPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            _mlContext.Model.Save(_model, dataView.Schema, modelPath);

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FashionInput, FashionPrediction>(_model);

            return Task.FromResult(metrics.Accuracy);
        }

        public Task LoadModelAsync(string modelPath, CancellationToken ct)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"Model file not found: {modelPath}");

            _model = _mlContext.Model.Load(modelPath, out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<FashionInput, FashionPrediction>(_model);

            return Task.CompletedTask;
        }
    }

    public class FashionInput
    {
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public float Price { get; set; }
        public float PopularityScore { get; set; }
        public float CustomerRating { get; set; }
        public bool IsTrending { get; set; }
    }

    public class FashionPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }

        public float Probability { get; set; }
    }
}