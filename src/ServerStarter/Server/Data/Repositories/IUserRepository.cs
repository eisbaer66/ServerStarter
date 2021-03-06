﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ServerStarter.Server.Models;

namespace ServerStarter.Server.Data.Repositories
{
    public interface IUserRepository
    {
        Task<ApplicationUser>        FindAsync(string                 userId);
    }

    class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ApplicationUser> FindAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
}
