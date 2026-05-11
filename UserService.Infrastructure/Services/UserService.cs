using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
            _logger.LogInformation("Creating a new user with email {Email}", userDto.Email);

            var user = _mapper.Map<User>(userDto);
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.AddAsync(user);

            return user.Id;
        }

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

        public async Task<UserDto?> GetUserByAzureAdObjectIdOrEmailAsync(string? azureAdObjectId, string? email)
        {
            var user = await _userRepository.GetByAzureAdObjectIdOrEmailAsync(azureAdObjectId, email);

            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> EnsureCurrentUserExistsAsync(EnsureCurrentUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AzureAdObjectId))
                throw new ArgumentException("AzureAdObjectId is required.");

            var existingUser = await _userRepository.GetByAzureAdObjectIdAsync(dto.AzureAdObjectId);

            if (existingUser != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Email))
                    existingUser.Email = dto.Email;

                if (!string.IsNullOrWhiteSpace(dto.DisplayName))
                    existingUser.DisplayName = dto.DisplayName;

                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                    existingUser.FirstName = dto.FirstName;

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                    existingUser.LastName = dto.LastName;

                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);

                return _mapper.Map<UserDto>(existingUser);
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                AzureAdObjectId = dto.AzureAdObjectId,
                Email = dto.Email ?? string.Empty,
                DisplayName = dto.DisplayName ?? string.Empty,
                FirstName = dto.FirstName ?? string.Empty,
                LastName = dto.LastName ?? string.Empty,
                Role = "Employee",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);

            return _mapper.Map<UserDto>(newUser);
        }

        public async Task<UserDto> UpsertFromGraphAsync(UpsertGraphUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.AzureAdObjectId))
                throw new ArgumentException("AzureAdObjectId is required.");

            var existingUser = await _userRepository.GetByAzureAdObjectIdAsync(dto.AzureAdObjectId);

            if (existingUser != null)
            {
                existingUser.Email = dto.Email;
                existingUser.UserPrincipalName = dto.UserPrincipalName;
                existingUser.DisplayName = dto.DisplayName;
                existingUser.FirstName = dto.FirstName;
                existingUser.LastName = dto.LastName;
                existingUser.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(existingUser);

                return _mapper.Map<UserDto>(existingUser);
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                AzureAdObjectId = dto.AzureAdObjectId,
                Email = dto.Email,
                UserPrincipalName = dto.UserPrincipalName,
                DisplayName = dto.DisplayName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Role = "Employee",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(newUser);

            return _mapper.Map<UserDto>(newUser);
        }

    }
}
