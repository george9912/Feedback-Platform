using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                        .AsNoTracking()
                        .OrderBy(u => u.DisplayName)
                        .ThenBy(u => u.Email)
                        .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users
                       .AsNoTracking()
                       .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetByAzureAdObjectIdAsync(string azureAdObjectId)
            => await _context.Users.FirstOrDefaultAsync(x => x.AzureAdObjectId == azureAdObjectId);

        public async Task<User?> GetByAzureAdObjectIdOrEmailAsync(string? azureAdObjectId, string? email)
        {
            return await _context.Users.FirstOrDefaultAsync(x =>
                (!string.IsNullOrEmpty(azureAdObjectId) && x.AzureAdObjectId == azureAdObjectId) ||
                (!string.IsNullOrEmpty(email) && x.Email == email));
        }

        public async Task<IEnumerable<User>> SearchAsync(string query, int page, int pageSize)
        {
            var q = query.Trim().ToLower();

            return await _context.Users
                .AsNoTracking()
                .Where(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(q)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(q)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(q)) ||
                    (u.Email != null && u.Email.ToLower().Contains(q)))
                .OrderBy(u => u.DisplayName)
                .ThenBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountSearchAsync(string query)
        {
            var q = query.Trim().ToLower();

            return await _context.Users
                .AsNoTracking()
                .CountAsync(u =>
                    (u.DisplayName != null && u.DisplayName.ToLower().Contains(q)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(q)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(q)) ||
                    (u.Email != null && u.Email.ToLower().Contains(q)));
        }
    }
}
