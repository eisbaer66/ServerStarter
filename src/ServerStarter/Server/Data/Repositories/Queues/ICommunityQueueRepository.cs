using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Data.Repositories.Queues
{
    internal interface ICommunityQueueRepository
    {
        Task<CommunityQueue>   Get(Guid communityId);
        Task<Community[]>      GetWaitingCommunityIds();
        Task<CommunityQueue[]> GetQueuedFor(string userId);
        Task<bool>             IsQueuedFor(string  userId, Guid communityId);
        void                   Add(CommunityQueue queue);
    }

    class CommunityQueueRepository : ICommunityQueueRepository
    {
        private readonly ApplicationDbContext _dbContext;

        private IIncludableQueryable<CommunityQueue, ApplicationUser> Aggregate
        {
            get
            {
                return _dbContext.CommunitiesQueues
                                 .Include(q => q.Community.Servers)
                                 .Include(q => q.Entries)
                                 .ThenInclude(e => e.User);
            }
        }

        public CommunityQueueRepository(ApplicationDbContext dbContext)
        {
            _dbContext  = dbContext  ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CommunityQueue> Get(Guid communityId)
        {
            return await Aggregate.FirstOrDefaultAsync(q => q.Community.Id == communityId);
        }

        public async Task<Community[]> GetWaitingCommunityIds()
        {
            return await Aggregate.Where(p => p.Entries.Count > 0)
                                  .Select(p => p.Community)
                                  .ToArrayAsync();
        }

        public async Task<CommunityQueue[]> GetQueuedFor(string userId)
        {
            return await Aggregate.Where(p => p.Entries.Any(e => e.User.Id == userId))
                                  .Select(p => p)
                                  .ToArrayAsync();
        }

        public async Task<bool> IsQueuedFor(string userId, Guid communityId)
        {
            return await _dbContext.CommunitiesQueues.AnyAsync(q => q.Community.Id == communityId && q.Entries.Any(e => e.User.Id == userId));
        }

        public void Add(CommunityQueue queue)
        {
            _dbContext.CommunitiesQueues.Add(queue);
        }
    }
}