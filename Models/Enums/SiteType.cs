using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{   
    public enum SiteType
    {
        SharePointOnline,
        SharePointOnPremises, //for SP 2010 and above
        WSS //  for SP 2007 version or below
    }
}
