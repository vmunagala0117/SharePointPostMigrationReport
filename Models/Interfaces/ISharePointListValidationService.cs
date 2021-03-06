﻿using System;
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
        List<SPField> MissingListColumnsV1();
        List<SPListView> MissingListViews();
        List<SPListItem> MissingListItems();
        List<SPListItem> MissingListItemsV1();
        List<SPWebPart> MissingWebParts();
        List<SPWorkflow> MissingWorkflows();
    }
}
