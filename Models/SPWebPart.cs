using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SPWebPart
    {
        public string FileRelativeUrl { get; set; }
        public string WebPartTitle { get; set; }
        public WebPartStatus WebPartStatus { get; set; }
    }
}
