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
        public SPConnection(string siteType, string site, string uname, string password, string domain = "")
        {
            this.SiteType = (Enums.SiteType) Enum.Parse(typeof(Enums.SiteType), siteType);
            this.SiteUrl = site;
            this.UserName = uname;
            this.Password = password;
            this.Domain = domain;
        }
        public string SiteUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public SiteType SiteType { get; set; }
    }
}
