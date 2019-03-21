using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SPWorkflow
    {
        public string WebUrl { get; set; }
        public string ListTitle { get; set; }
        public string WorkflowName { get; set; }
        public WorkflowType WorkflowType { get; set; }
    }
}
