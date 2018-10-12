using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public sealed class UserMappingClassMap : ClassMap<UserMapping>
    {
        public UserMappingClassMap()
        {
            Map(m => m.OldUpn).Name("Old UPN");
            Map(m => m.NewUpn).Name("New UPN");
        }
    }
    public sealed class SPListItemClassMap : ClassMap<SPListItem>
    {
        public SPListItemClassMap()
        {
            Map(m => m.ID).Name("ID");
            Map(m => m.Title).Name("Title");
            Map(m => m.Name).Name("Name");
            Map(m => m.FileDirRef).Name("FileDirRef");
            Map(m => m.FileRef).Name("FileRef");
            Map(m => m.ModifiedDate).Name("ModifiedDate");
        }
    }
    public sealed class SPListItemCountClassMap : ClassMap<SPListItemCount>
    {
        public SPListItemCountClassMap()
        {
            Map(m => m.ListName).Name("ListName");
            Map(m => m.CurrentValue).Name("CurrentValue");
            Map(m => m.ExpectedValue).Name("ExpectedValue");
        }
    }
    public sealed class SPListViewClassMap : ClassMap<SPListView>
    {
        public SPListViewClassMap()
        {
            Map(m => m.ListName).Name("ListName");
            Map(m => m.ViewName).Name("ViewName");
        }
    }
    public sealed class SPWebPartClassMap : ClassMap<SPWebPart>
    {
        public SPWebPartClassMap()
        {
            Map(m => m.FileRelativeUrl).Name("FileRelativeUrl");
            Map(m => m.WebPartTitle).Name("WebPartTitle");
        }
    }
    public sealed class SPFieldClassMap : ClassMap<SPField>
    {
        public SPFieldClassMap()
        {
            Map(m => m.Url).Name("Url");
            Map(m => m.FieldName).Name("FieldName");
        }
    }
    public sealed class UserPermStatusClassMap : ClassMap<UserPermStatus>
    {
        public UserPermStatusClassMap()
        {
            Map(m => m.LoginName).Name("LoginName");
            Map(m => m.CurrentPermissions).Name("CurrentPermissions");
            Map(m => m.ExpectedPermissions).Name("ExpectedPermissions");
            Map(m => m.UserStatus).Name("UserStatus");
        }
    }
    public sealed class UserGroupStatusClassMap : ClassMap<UserGroupStatus>
    {
        public UserGroupStatusClassMap()
        {
            Map(m => m.LoginName).Name("LoginName");
            Map(m => m.GroupName).Name("GroupName");
            Map(m => m.UserInGroupStatus).Name("UserInGroupStatus");
        }
    }    
}
