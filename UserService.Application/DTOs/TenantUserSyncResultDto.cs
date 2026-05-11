using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserService.Application.DTOs
{
    public class TenantUserSyncResultDto
    {
        public int FetchedFromGraph { get; set; }
        public int ImportedOrUpdated { get; set; }
        public int Skipped { get; set; }
    }
}
