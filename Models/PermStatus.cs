using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class PermStatus
    {
        public string Name { get; set; }
        public List<string> CurrentPermissions { get; set; }
        public List<string> ExpectedPermissions { get; set; }
        public bool IsGroup { get; set; }

    }
}
