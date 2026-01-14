namespace AiAgents.FashionAgent.Web.Models
{
    public class FeedbackModel
    {
        public int ItemId { get; set; }
        public string Feedback { get; set; } = string.Empty; // "like" or "dislike"
        public string? DislikeReason { get; set; }
        public string? LikeReason { get; set; }
    }
}