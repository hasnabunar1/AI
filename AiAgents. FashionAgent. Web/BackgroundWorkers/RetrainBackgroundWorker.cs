using AiAgents.FashionAgent.Application.Runners;

namespace AiAgents.FashionAgent.Web.BackgroundWorkers
{
    public class RetrainBackgroundWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RetrainBackgroundWorker> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

        public RetrainBackgroundWorker(
            IServiceProvider serviceProvider,
            ILogger<RetrainBackgroundWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Retrain Agent started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var runner = scope.ServiceProvider.GetRequiredService<FashionRetrainAgentRunner>();

                    var result = await runner.StepAsync(stoppingToken);

                    if (result != null && result.RetrainPerformed)
                    {
                        _logger.LogInformation(
                            "Model retrained!  Version {Id}, Accuracy:  {Accuracy:P2}, Samples:  {Samples}",
                            result.NewModelVersionId,
                            result.NewModelAccuracy,
                            result.TrainingSamplesUsed);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in retrain agent");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}