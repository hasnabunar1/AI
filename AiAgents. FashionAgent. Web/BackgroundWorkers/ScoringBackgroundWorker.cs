using AiAgents.FashionAgent.Application.Runners;

namespace AiAgents.FashionAgent.Web.BackgroundWorkers
{
    public class ScoringBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ScoringBackgroundWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

        public ScoringBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<ScoringBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scoring Agent started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<FashionScoringAgentRunner>();

                    var result = await runner.StepAsync(stoppingToken);

                    if (result != null)
                    {
                        _logger.LogInformation(
                            "Processed item {Id}:  {Brand} {Category} -> {Decision} ({Probability:P0})",
                            result.ClothingItemId,
                            result.Brand,
                            result.Category,
                            result.Decision,
                            result.ProbabilityTrending);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in scoring agent");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}