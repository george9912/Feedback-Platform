using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    public class TenantUserSyncAppService : ITenantUserSyncAppService
    {
        private readonly IGraphUserSyncService _graphUserSyncService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<TenantUserSyncAppService> _logger;
        private readonly string _companyDomain;

        public TenantUserSyncAppService(
            IGraphUserSyncService graphUserSyncService,
            IUserRepository userRepository,
            ILogger<TenantUserSyncAppService> logger,
            IConfiguration configuration)
        {
            _graphUserSyncService = graphUserSyncService;
            _userRepository = userRepository;
            _logger = logger;
            _companyDomain = configuration["CompanyDirectory:AllowedEmailDomain"] ?? throw new InvalidOperationException("CompanyDirectory:AllowedEmailDomain is missing.");
        }

        public async Task<TenantUserSyncResultDto> SyncTenantUsersAsync(CancellationToken cancellationToken = default)
        {
            var graphUsers = await _graphUserSyncService.GetTenantUsersAsync(cancellationToken);

            int importedOrUpdated = 0;
            int skipped = 0;

            foreach (var graphUser in graphUsers)
            {
                var email = graphUser.Mail ?? graphUser.UserPrincipalName;

                if (string.IsNullOrWhiteSpace(email))
                {
                    skipped++;
                    continue;
                }

                if (!string.Equals(graphUser.UserType, "Member", StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    continue;
                }

                if (graphUser.AccountEnabled == false)
                {
                    skipped++;
                    continue;
                }

                if (!email.EndsWith($"@{_companyDomain}", StringComparison.OrdinalIgnoreCase))
                {
                    skipped++;
                    continue;
                }

                var existingUser =
                    await _userRepository.GetByAzureAdObjectIdAsync(graphUser.AzureAdObjectId)
                    ?? await _userRepository.GetByAzureAdObjectIdOrEmailAsync(null, email);

                if (existingUser != null)
                {
                    existingUser.AzureAdObjectId ??= graphUser.AzureAdObjectId;
                    existingUser.Email = email;
                    existingUser.UserPrincipalName = graphUser.UserPrincipalName ?? string.Empty;
                    existingUser.DisplayName = graphUser.DisplayName;
                    existingUser.FirstName = graphUser.GivenName ?? string.Empty;
                    existingUser.LastName = graphUser.Surname ?? string.Empty;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    await _userRepository.UpdateAsync(existingUser);
                }
                else
                {
                    var newUser = new User
                    {
                        Id = Guid.NewGuid(),
                        AzureAdObjectId = graphUser.AzureAdObjectId,
                        Email = email,
                        UserPrincipalName = graphUser.UserPrincipalName ?? string.Empty,
                        DisplayName = graphUser.DisplayName,
                        FirstName = graphUser.GivenName ?? string.Empty,
                        LastName = graphUser.Surname ?? string.Empty,
                        Role = "Employee",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _userRepository.AddAsync(newUser);
                }

                importedOrUpdated++;
            }

            _logger.LogInformation(
                "Tenant sync finished. Fetched={Fetched}, ImportedOrUpdated={Imported}, Skipped={Skipped}",
                graphUsers.Count, importedOrUpdated, skipped);

            return new TenantUserSyncResultDto
            {
                FetchedFromGraph = graphUsers.Count,
                ImportedOrUpdated = importedOrUpdated,
                Skipped = skipped
            };
        }
    }
}
