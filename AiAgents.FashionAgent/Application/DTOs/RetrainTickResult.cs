namespace AiAgents.FashionAgent.Application.DTOs
{
    public class RetrainTickResult
    {
        public bool RetrainPerformed { get; set; }
        public int? NewModelVersionId { get; set; }
        public int TrainingSamplesUsed { get; set; }
        public double? NewModelAccuracy { get; set; }
        public DateTime? TrainedAt { get; set; }
    }
}