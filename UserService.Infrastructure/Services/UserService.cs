using AutoMapper;
using Microsoft.Extensions.Logging;
using SharedCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Services;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        private readonly IMapper _mapper;

        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IMapper mapper, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Guid> CreateUserAsync(CreateUserDto userDto)
        {
            var user = _mapper.Map<User>(userDto);
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Creating user with email: {Email}", userDto.Email);

            await _userRepository.AddAsync(user);
            return user.Id;
        }

        //public async Task<UserDto?> GetUserByIdAsync(Guid id)
        //{
        //    var user = await _userRepository.GetByIdAsync(id);
        //    return user is null ? null : _mapper.Map<UserDto>(user);
        //}

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null) return Result<UserDto>.Failure("User not found");

            var dto = _mapper.Map<UserDto>(user);
            return Result<UserDto>.Success(dto);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            _logger.LogInformation("Start getting all the users from db");
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<bool> UpdateUserAsync(Guid id, CreateUserDto userDto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null) return false;

            existingUser.FirstName = userDto.FirstName;
            existingUser.LastName = userDto.LastName;
            existingUser.Email = userDto.Email;
            existingUser.Role = userDto.Role;

            await _userRepository.UpdateAsync(existingUser);
            return true;
        }

        //Definire clasa Result - definim erori si cand sa le folosim
        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {Id} not found for deletion", id);
                return false;
            };
            await _userRepository.DeleteAsync(id);
            return true;
        }


    }
}
