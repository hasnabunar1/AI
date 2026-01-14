//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using AiAgents.FashionAgent.Domain;
using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Domain.Enums;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace AiAgents.FashionAgent.Infrastructure
{
    public class DatabaseSeeder
    {
        private readonly FashionAgentDbContext _context;

        public DatabaseSeeder(FashionAgentDbContext context)
        {
            _context = context;
        }

        public async Task SeedFromCsvAsync(string csvPath, CancellationToken ct)
        {
            if (_context.ClothingItems.Any())
                return;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<CsvClothingItemMap>();
            var records = csv.GetRecords<CsvClothingItem>().ToList();

            // Deterministički split:  70% za training, 30% za queue
            var shuffled = records.OrderBy(x => x.ID).ToList();
            var splitIndex = (int)(shuffled.Count * 0.7);

            var trainingItems = shuffled.Take(splitIndex).ToList();
            var queueItems = shuffled.Skip(splitIndex).ToList();

            foreach (var record in trainingItems)
            {
                var item = MapToEntity(record);
                item.Status = ItemStatus.Reviewed;
                _context.ClothingItems.Add(item);
            }

            foreach (var record in queueItems)
            {
                var item = MapToEntity(record);
                item.Status = ItemStatus.Queued;
                _context.ClothingItems.Add(item);
            }

            _context.SystemSettings.Add(new SystemSettings());

            await _context.SaveChangesAsync(ct);
        }

        private ClothingItem MapToEntity(CsvClothingItem csv)
        {
            return new ClothingItem
            {
                Id = csv.ID,
                Brand = csv.Brand,
                Category = csv.Category,
                Color = csv.Color,
                Material = csv.Material,
                Style = csv.Style,
                Gender = csv.Gender,
                Season = csv.Season,
                Price = csv.PriceUSD,
                PopularityScore = csv.Popularity_Score,
                CustomerRating = csv.Customer_Rating,
                TrendStatus = csv.Trend_Status,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public class CsvClothingItem
    {
        public int ID { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Style { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public decimal PriceUSD { get; set; }
        public double Popularity_Score { get; set; }
        public double Customer_Rating { get; set; }
        public string Trend_Status { get; set; } = string.Empty;
    }

    public class CsvClothingItemMap : ClassMap<CsvClothingItem>
    {
        public CsvClothingItemMap()
        {
            Map(m => m.ID).Name("ID");
            Map(m => m.Brand).Name("Brand");
            Map(m => m.Category).Name("Category");
            Map(m => m.Color).Name("Color");
            Map(m => m.Material).Name("Material");
            Map(m => m.Style).Name("Style");
            Map(m => m.Gender).Name("Gender");
            Map(m => m.Season).Name("Season");
            Map(m => m.PriceUSD).Name("Price(USD)");
            Map(m => m.Popularity_Score).Name("Popularity_Score");
            Map(m => m.Customer_Rating).Name("Customer_Rating");
            Map(m => m.Trend_Status).Name("Trend_Status");
        }
    }
}