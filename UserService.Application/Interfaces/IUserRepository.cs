using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Domain.Entities;

namespace UserService.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);


        Task<User?> GetByAzureAdObjectIdAsync(string azureAdObjectId);
        Task<User?> GetByAzureAdObjectIdOrEmailAsync(string? azureAdObjectId, string? email);
        Task<IEnumerable<User>> SearchAsync(string query, int page, int pageSize);
        Task<int> CountSearchAsync(string query);
    }
}
