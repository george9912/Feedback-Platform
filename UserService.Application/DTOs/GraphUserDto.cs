using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTOs
{
    public class GraphUserDto
    {
        public string AzureAdObjectId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Mail { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }

        public string? UserType { get; set; }       // Member / Guest
        public bool? AccountEnabled { get; set; }  
    }
}
