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

    public static class Aggregates
    {
        public static IIncludableQueryable<CommunityQueue, ApplicationUser> CommunityQueue(ApplicationDbContext dbContext)
        {
            return dbContext.CommunitiesQueues
                             .Include(q => q.Community.Servers)
                             .Include(q => q.Entries)
                             .ThenInclude(e => e.User);
        }
    }

    class CommunityQueueRepository : ICommunityQueueRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CommunityQueueRepository(ApplicationDbContext dbContext)
        {
            _dbContext  = dbContext  ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CommunityQueue> Get(Guid communityId)
        {
            return await Aggregates.CommunityQueue(_dbContext)
                                   .FirstOrDefaultAsync(q => q.Community.Id == communityId);
        }

        public async Task<Community[]> GetWaitingCommunityIds()
        {
            return await Aggregates.CommunityQueue(_dbContext)
                                   .Where(p => p.Entries.Count > 0)
                                   .Select(p => p.Community)
                                   .ToArrayAsync();
        }

        public async Task<CommunityQueue[]> GetQueuedFor(string userId)
        {
            return await Aggregates.CommunityQueue(_dbContext)
                                   .Where(p => p.Entries.Any(e => e.User.Id == userId))
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

    class GetAllQueuesCommunityQueueRepositoryCache : ICommunityQueueRepository
    {
        private readonly ApplicationDbContext              _dbContext;
        private readonly ICommunityQueueRepository         _repository;
        private          IDictionary<Guid, CommunityQueue> _queues = null;

        public GetAllQueuesCommunityQueueRepositoryCache(ICommunityQueueRepository repository, ApplicationDbContext dbContext)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _dbContext  = dbContext  ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<CommunityQueue> Get(Guid communityId)
        {
            if (_queues == null)
            {
                var queues = await Aggregates.CommunityQueue(_dbContext).ToListAsync();
                _queues = queues.ToDictionary(q => q.Id);
            }

            if (!_queues.ContainsKey(communityId))
                return null;
            return _queues[communityId];
        }

        public async Task<Community[]> GetWaitingCommunityIds()
        {
            return await _repository.GetWaitingCommunityIds();
        }

        public async Task<CommunityQueue[]> GetQueuedFor(string userId)
        {
            return await _repository.GetQueuedFor(userId);
        }

        public async Task<bool> IsQueuedFor(string userId, Guid communityId)
        {
            var queue = await Get(communityId);
            return queue.Entries.Select(e => e.User.Id).Contains(userId);
        }

        public void Add(CommunityQueue queue)
        {
            _repository.Add(queue);
            _queues = null;
        }
    }
}