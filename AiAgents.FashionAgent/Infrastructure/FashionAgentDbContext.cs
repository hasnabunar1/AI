//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using AiAgents.FashionAgent.Domain;
using AiAgents.FashionAgent.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace AiAgents.FashionAgent.Infrastructure
{
    public class FashionAgentDbContext : DbContext
    {
        public FashionAgentDbContext(DbContextOptions<FashionAgentDbContext> options)
            : base(options)
        {
        }

        public DbSet<ClothingItem> ClothingItems => Set<ClothingItem>();
        public DbSet<TrendPrediction> TrendPredictions => Set<TrendPrediction>();
        public DbSet<ModelVersion> ModelVersions => Set<ModelVersion>();
        public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
        public DbSet<UserFeedback> UserFeedbacks => Set<UserFeedback>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClothingItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.Color).HasMaxLength(50);
                entity.Property(e => e.Material).HasMaxLength(50);
                entity.Property(e => e.Style).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(20);
                entity.Property(e => e.Season).HasMaxLength(50);
                entity.Property(e => e.Price).HasPrecision(10, 2);
            });

            modelBuilder.Entity<TrendPrediction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ClothingItem)
                    .WithMany()
                    .HasForeignKey(e => e.ClothingItemId);
                entity.HasOne(e => e.ModelVersion)
                    .WithMany(m => m.Predictions)
                    .HasForeignKey(e => e.ModelVersionId);
            });

            modelBuilder.Entity<ModelVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ModelPath).HasMaxLength(500);
            });

            modelBuilder.Entity<SystemSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}