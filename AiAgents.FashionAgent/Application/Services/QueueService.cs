using AiAgents.FashionAgent.Domain.Entities;
using AiAgents.FashionAgent.Domain.Enums;
using AiAgents.FashionAgent.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AiAgents.FashionAgent.Application.Services
{
    public class QueueService
    {
        private readonly FashionAgentDbContext _context;

        public QueueService(FashionAgentDbContext context)
        {
            _context = context;
        }

        public async Task<ClothingItem?> DequeueNextAsync(CancellationToken ct)
        {
            var item = await _context.ClothingItems
                .Where(x => x.Status == ItemStatus.Queued)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (item != null)
            {
                item.Status = ItemStatus.Processing;
                await _context.SaveChangesAsync(ct);
            }

            return item;
        }

        public async Task EnqueueAsync(ClothingItem item, CancellationToken ct)
        {
            item.Status = ItemStatus.Queued;
            item.CreatedAt = DateTime.UtcNow;
            _context.ClothingItems.Add(item);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<int> GetQueuedCountAsync(CancellationToken ct)
        {
            return await _context.ClothingItems
                .CountAsync(x => x.Status == ItemStatus.Queued, ct);
        }
    }
}