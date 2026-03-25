using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;

namespace UserService.Application.Services
{
    public interface IGraphUserSyncService
    {
        Task<List<GraphUserDto>> GetTenantUsersAsync(CancellationToken cancellationToken = default);
    }
}
