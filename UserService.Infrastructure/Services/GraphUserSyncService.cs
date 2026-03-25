using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Application.DTOs;
using UserService.Application.Services;

namespace UserService.Infrastructure.Services
{
    public class GraphUserSyncService : IGraphUserSyncService
    {
        private readonly GraphServiceClient _graphClient;

        public GraphUserSyncService(GraphServiceClient graphClient)
        {
            _graphClient = graphClient;
        }

        public async Task<List<GraphUserDto>> GetTenantUsersAsync(CancellationToken cancellationToken = default)
        {
            var users = new List<GraphUserDto>();

            var page = await _graphClient.Users.GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Select = new[]
                {
                    "id",
                    "displayName",
                    "mail",
                    "userPrincipalName",
                    "givenName",
                    "surname"
                };
                requestConfig.QueryParameters.Top = 999;
            }, cancellationToken);

            while (page != null)
            {
                if (page.Value != null)
                {
                    foreach (var user in page.Value)
                    {
                        if (string.IsNullOrWhiteSpace(user.Id))
                            continue;

                        users.Add(new GraphUserDto
                        {
                            AzureAdObjectId = user.Id,
                            DisplayName = user.DisplayName ?? string.Empty,
                            Mail = user.Mail,
                            UserPrincipalName = user.UserPrincipalName,
                            GivenName = user.GivenName,
                            Surname = user.Surname
                        });
                    }
                }

                if (string.IsNullOrWhiteSpace(page.OdataNextLink))
                    break;

                page = await _graphClient.Users
                    .WithUrl(page.OdataNextLink)
                    .GetAsync(cancellationToken: cancellationToken);
            }

            return users;
        }
    }
}
