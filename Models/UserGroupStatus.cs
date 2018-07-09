using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UserGroupStatus
    {
        public string LoginName { get; set; }
        public string GroupName { get; set; }
        public UserStatus UserInGroupStatus { get; set; }
    }
}
