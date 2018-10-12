using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SPConnection
    {
        public SPConnection(string siteType, string siteUrl, string relativeWebUrl, string uname, string password)
        {
            this.SiteType = (Enums.SiteType)Enum.Parse(typeof(Enums.SiteType), siteType);
            this.SiteUrl = siteUrl;
            this.WebRelativeUrl = relativeWebUrl;
            this.UserName = uname.Contains("\\") ? uname.Split(new char[] { '\\' })[1] : uname;
            this.Password = password;
            this.Domain = uname.Contains("\\") ? uname.Split(new char[] { '\\' })[0] : "";
        }
        public string SiteUrl { get; set; }
        public string WebRelativeUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public SiteType SiteType { get; set; }
    }
}
