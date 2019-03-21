using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISharePointWebValidationService
    {
        IEnumerable<string> GetAllSourceWebUrls();
        List<string> MissingContentTypes();
        List<SPField> MissingSiteColumns();
        List<string> MissingSites();
        List<string> MissingSitesV1();
        List<string> MissingSiteGroups();
        List<string> MissingSiteGroupsV1();
        List<string> MissingWebGroups();
        List<UserGroupStatus> MissingUsersInGroups();
        List<UserPermStatus> CheckUserPermissions();
        List<SPWebUniquePermissions> CheckWebPermissionsInheritance();
    }
}
