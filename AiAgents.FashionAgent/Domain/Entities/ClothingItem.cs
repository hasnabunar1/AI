using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiAgents.FashionAgent.Domain.Enums;

namespace AiAgents.FashionAgent.Domain.Entities
{
    public class ClothingItem
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double PopularityScore { get; set; }
        public double CustomerRating { get; set; }
        public string TrendStatus { get; set; } = string.Empty;

        // Agent-managed properties
        public ItemStatus Status { get; set; } = ItemStatus.Queued;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }

        // Za ML feature extraction
        public string GetFeatureText()
        {
            return $"{Brand} {Category} {Color} {Material} {Style} {Gender} {Season} {PopularityScore} {CustomerRating}";
        }
    }
}