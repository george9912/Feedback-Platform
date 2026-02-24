using SharedCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;

namespace UserService.Application.Services
{
    public interface IUserService
    {
        Task<Guid> CreateUserAsync(CreateUserDto userDto);
        Task<Result<UserDto>?> GetUserByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(Guid id, CreateUserDto userDto);
        Task<bool> DeleteUserAsync(Guid id);
    }
}
