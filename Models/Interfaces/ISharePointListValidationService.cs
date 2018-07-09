using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISharePointListValidationService
    {
        List<string> MissingLists();
        List<SPListItemCount> GetListsItemsCountMismatch();
        List<SPField> MissingListColumns();
        List<SPListItem> MissingListItems();
        List<SPWebPart> MissingWebParts();
    }
}
