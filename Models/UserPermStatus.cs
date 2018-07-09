using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UserPermStatus
    {
        public string LoginName { get; set; }
        public UserPermissions CurrentPermissions { get; set; }
        public UserPermissions ExpectedPermissions { get; set; }
        public UserStatus UserStatus { get; set; }
    }
}
