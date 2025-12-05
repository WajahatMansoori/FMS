using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Base
{
    public class UserClaimsDTO
    {
        public int UserID { get; set; }
        public int UserRole { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
    }
}
