using Common;
using Common.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{    
    public class UserMappingController : IUserMapping
    {
        private List<UserMapping> UserMappingsList { get; set; }
        public UserMappingController()
        {
            UserMappingsList = new List<UserMapping>();
        }
        public UserMappingController(string filePath)
        {
            using (var csv = new CsvReader(File.OpenText(filePath)))
            {
                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.RegisterClassMap<UserMappingClassMap>();
                this.UserMappingsList = csv.GetRecords<UserMapping>().ToList();
            }
        }

        public string FindNewUpn(string oldUpnLogin)
        {
            var newUpnFound =  UserMappingsList.Find(x => x.OldUpn.ToLower() == oldUpnLogin.ToLower());
            if (newUpnFound != null)
            {
                return newUpnFound.NewUpn;
            }
            else
                return string.Empty;
        }

        public List<UserMapping> GetUserMappingList()
        {
            return UserMappingsList;
        }
    }
}
