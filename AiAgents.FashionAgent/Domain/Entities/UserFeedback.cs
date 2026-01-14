namespace AiAgents.FashionAgent.Domain.Entities
{
    public class UserFeedback
    {
        public int Id { get; set; }
        public int ClothingItemId { get; set; }
        public string FeedbackType { get; set; } = string.Empty; // "like" or "dislike"
        public string? DislikeReason { get; set; } // "color", "price", "material", "style", "brand", "other"
        public string? LikeReason { get; set; } // "color", "price", "material", "style", "brand", "trending", "everything"
        public DateTime SubmittedAt { get; set; }

        // Navigation property
        public ClothingItem? ClothingItem { get; set; }
    }
}