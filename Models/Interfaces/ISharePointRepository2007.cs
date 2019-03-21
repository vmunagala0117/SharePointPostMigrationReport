using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISharePointRepository2007
    {
        WSSContext GetSPContext(string siteUrl, string user, string password, string domain = "");
        bool ListExists(string listName);
        IEnumerable<string> GetLists();
        int GetListItemCount(string listName);
        List<SPListItem> GetListItems(string listName);
        IEnumerable<string> GetListColumns(string listName);
        List<SPWebPart> GetWikiPageWebParts();
        List<string> GetSiteGroups();
        List<string> GetWebGroups();
        List<string> GetWebCustomFields();
        IEnumerable<string> GetAllWebUrls();
        IEnumerable<string> GetSiteContentTypes();
        IDictionary<string, List<string>> GetWebUserGroups();
        IEnumerable<string> GetListViews(string listName);
        List<SPWorkflow> GetWorkflows(string listName);
    }
}
