using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ServerStarter.Server.Models
{
    public class CommunityQueue
    {
        [Key]
        public Guid Id
        {
            get;
            set;
        }

        public Community                 Community { get; set; }
        public ISet<CommunityQueueEntry> Entries   { get; set; }

        public CommunityQueue()
        {
        }

        public CommunityQueue(Community community)
        {
            Id        = community.Id;
            Community = community;
            Entries   = new HashSet<CommunityQueueEntry>();
        }

        public CommunityQueueEntry Get(string userId)
        {
            return Entries.FirstOrDefault(m => m.User.Id == userId);
        }

        public bool Contains(string userId)
        {
            return Entries.Any(m => m.User.Id == userId);
        }

        public CommunityQueueEntry Add(ApplicationUser user)
        {
            var entry = new CommunityQueueEntry
                                      {
                                          Id    = Guid.NewGuid(),
                                          Queue = this,
                                          User  = user,
                                      };
            Entries.Add(entry);

            return entry;
        }

        public void Remove(CommunityQueueEntry user)
        {
            Entries.Remove(user);
        }
    }

    public class CommunityQueueEntry
    {
        [Key]
        public Guid Id { get; set; }

        public CommunityQueue  Queue { get; set; }
        public ApplicationUser User  { get; set; }
    }
}