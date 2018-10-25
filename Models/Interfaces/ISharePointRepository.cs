using Common.Enums;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISharePointRepository
    {
        ClientContext GetSPOContext(string siteUrl, string tenantUser, string tenantUserPassword);
        ClientContext GetSP2013Context(string siteUrl, string userName, string passWord, string domain);
        IEnumerable<string> GetAllWebUrls(ClientContext cc);
        bool WebExists(ClientContext cc, string url);
        List<string> GetWebGroups(ClientContext cc);
        IDictionary<string, List<string>> GetWebUserGroups(ClientContext cc);
        bool GroupExists(ClientContext cc, string groupTitle);
        bool UserExistsInGroup(ClientContext cc, string groupTitle, string userLoginName);
        IEnumerable<string> GetSiteContentTypes(ClientContext cc);
        bool CheckIfSiteContentTypeExists(ClientContext cc, string contentTypeName);
        List<string> GetWebCustomFields(ClientContext cc);
        bool CheckFieldExists(ClientContext cc, string fieldName);
        List<SPAlert> GetAlertsInformation(ClientContext cc);
        IDictionary<string, UserPermissions> GetWebUserPermissions(ClientContext cc);
        UserPermissions CheckUserPermissions(ClientContext cc, string userLoginName);
        IEnumerable<string> GetLists(ClientContext cc);
        int GetListBaseTemplate(ClientContext cc, string listName);
        int GetListItemCount(ClientContext cc, string listName);
        bool ListExists(ClientContext cc, string listName);
        List<string> GetListViews(ClientContext cc, string listName);
        bool CheckIfListViewExists(ClientContext cc, string listName, string viewName);
        IEnumerable<string> GetListContentTypes(ClientContext cc, string listName);
        bool CheckIfContentTypeExists(ClientContext cc, string listName, string contentTypeName);
        IEnumerable<string> GetListColumns(ClientContext cc, string listName);
        bool GetListFieldExistsByName(ClientContext cc, string listName, string fieldName);
        List<SPListItem> GetListItems(ClientContext cc, string listName);
        bool GetListItemExists(ClientContext cc, string listName, SPListItem listItem);
        bool GetListItemExistsByModifiedDate(ClientContext cc, string listName, SPListItem listItem);
        void GetListPermissions(ClientContext cc, string listName);
        List<SPWebPart> GetWikiPageWebParts(ClientContext cc);
        bool CheckIfWebPartPresent(ClientContext cc, string fileRelativeUrl, string webPartTitle);
    }
}
