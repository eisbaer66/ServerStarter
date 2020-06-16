using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ServerStarter.Server.Data.Repositories
{
    public class CommunityRepository : ICommunityRepository
    {
        private readonly ApplicationDbContext _context;

        public CommunityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.Community>> Get()
        {
            var dbcommunities = await _context.Communities
                                              .OrderBy(c => c.Order)
                                              .Select(c => new
                                                           {
                                                               c       = c,
                                                               servers = c.Servers.OrderBy(s => s.Order).ToArray()
                                                           })
                                              .ToArrayAsync();
            var communities = dbcommunities.Select(c => c.c);
            return communities;
        }

        public async Task<Models.Community> Get(Guid id)
        {
            var community = await _context.Communities
                                                    .OrderBy(c => c.Order)
                                                    .Select(c => new
                                                                 {
                                                                     c       = c,
                                                                     servers = c.Servers.OrderBy(s => s.Order).ToArray()
                                                                 })
                                                    .FirstOrDefaultAsync(c => c.c.Id == id);
            return community.c;
        }
    }
}