using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTOs
{
    public class UpsertGraphUserDto
    {
        public string AzureAdObjectId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
