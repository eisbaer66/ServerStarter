﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ServerStarter.Server.Data;
using ServerStarter.Server.Data.Repositories;
using ServerStarter.Server.Data.Repositories.Queues;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Services
{
    public class UserJoinedEventArgs
    {
        public Guid CommunityId { get; set; }
        public string UserId { get; set; }
    }
    public class UserLeftEventArgs
    {
        public Guid   CommunityId { get; set; }
        public string UserId      { get; set; }
    }

    class CommunityQueueService : ICommunityQueueService
    {
        private readonly ILogger<CommunityQueueService> _logger;
        private readonly ApplicationDbContext           _dbContext;
        private readonly IMessaging                     _messaging;
        private readonly ICommunityRepository           _communityRepository;
        private readonly IUserRepository                _users;
        private readonly ICommunityQueueRepository      _repository;

        public CommunityQueueService(ILogger<CommunityQueueService> logger,
                                     ICommunityQueueRepository      repository,
                                     ICommunityRepository           communityRepository,
                                     IUserRepository                users,
                                     ApplicationDbContext           dbContext,
                                     IMessaging                     messaging)
        {
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
            _repository          = repository          ?? throw new ArgumentNullException(nameof(repository));
            _communityRepository = communityRepository ?? throw new ArgumentNullException(nameof(communityRepository));
            _users               = users               ?? throw new ArgumentNullException(nameof(users));
            _dbContext           = dbContext           ?? throw new ArgumentNullException(nameof(dbContext));
            _messaging      = messaging;
        }

        public async Task Join(Guid communityId, string userId)
        {
            var  queue = await _repository.Get(communityId);
            if (queue == null)
            {
                var community = await _communityRepository.Get(communityId);
                if (community == null)
                {
                    _logger.LogWarning("could not queue user {UserId} for community {CommunityId} - community does not exist", userId, communityId);
                    return;
                }
                queue = new CommunityQueue(community);
                _repository.Add(queue);
            }

            if (queue.Contains(userId))
                return;

            var user = await _users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("could not queue user {UserId} for community {CommunityId} - user does not exist", userId, communityId);
                return;
            }

            var entry = queue.Add(user);
            _dbContext.Add(entry);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("enqueued {UserId} for {CommunityId}", userId, communityId);
            _messaging.UserJoinedNotification(this, new UserJoinedEventArgs{CommunityId = communityId, UserId = userId});
        }

        public async Task Leave(Guid communityId, string userId)
        {
            var queue = await _repository.Get(communityId);
            if (queue == null)
                return;

            var queueItem = queue.Get(userId);
            if (queueItem == null)
                return;

            queue.Remove(queueItem);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("dequeued {UserId} for {CommunityId}", userId, communityId);
            _messaging.UserLeftNotification(this, new UserLeftEventArgs { CommunityId = communityId, UserId = userId });
        }

        public async Task<IList<ApplicationUser>> GetQueuedPlayers(Guid communityId)
        {
            var queue = await _repository.Get(communityId);
            if (queue == null)
                return new ApplicationUser[0];

            return queue.Entries.Select(e => e.User).ToList();
        }

        public async Task<Community[]> GetWaitingCommunityIds()
        {
            return await _repository.GetWaitingCommunityIds();
        }

        public async Task LeaveAllQueues(string userId)
        {
            IEnumerable<CommunityQueue> queues = await _repository.GetQueuedFor(userId);
            foreach (var q in queues)
            {
                var user = q.Entries.FirstOrDefault(u => u.User.Id == userId);
                if (user == null)
                    return;

                q.Remove(user);
                _messaging.UserLeftNotification(this, new UserLeftEventArgs { CommunityId = q.Community.Id, UserId = userId });
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<CommunityQueue[]> GetQueuedCommunity(string userId)
        {
            return await _repository.GetQueuedFor(userId);
        }
        public async Task<bool> Contains(string userId, Guid communityId)
        {
            return await _repository.IsQueuedFor(userId, communityId);
        }
    }
}