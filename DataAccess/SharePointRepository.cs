using Microsoft.SharePoint.Client;
using Common.Enums;
using Common.Interfaces;
using OfficeDevPnP.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using NLog;
using Microsoft.SharePoint.Client.WebParts;
using Polly;
using Microsoft.SharePoint.Client.UserProfiles;
using System.Threading;


namespace DataAccess
{
    public class SharePointRepository : ISharePointRepository
    {
        private ILogger logger { get; set; }

        //private Polly.Wrap.PolicyWrap retryPolicy { get; set; }
        private Polly.Retry.RetryPolicy retryPolicy { get; set; }

        public SharePointRepository(ILogger logger)
        {
            this.logger = logger;

            var waitAndRetryPolicy = Policy.Handle<Exception>().WaitAndRetry(
                 retryCount: 3, // Retry up to 3 times! - should be enough that we eventually succeed.
                 sleepDurationProvider: attempt => TimeSpan.FromMinutes(1), // Wait 1 minute between each try.
                 onRetry: (exception, calculatedWaitDuration, retryCount, context) => // Capture some info for logging!
                 {
                     //var methodThatRaisedException = context["methodName"];
                     logger.Log(LogLevel.Info, $"Retrying Attempt: {retryCount}");
                     logger.Log(LogLevel.Error, exception, context.ExecutionKey);
                 });

            this.retryPolicy = waitAndRetryPolicy;

            /*
            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    exceptionsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(3),
                    onBreak: (ex, breakDelay) =>
                    {
                        logger.Log(LogLevel.Info, ".Breaker logging: Breaking the circuit for " + breakDelay.TotalMilliseconds + "ms!", ConsoleColor.Magenta);
                        logger.Log(LogLevel.Info, "..due to: " + ex.Message, ConsoleColor.Magenta);
                    },
                    onReset: () => logger.Log(LogLevel.Info, ".Breaker logging: Call ok! Closed the circuit again!", ConsoleColor.Magenta),
                    onHalfOpen: () => logger.Log(LogLevel.Info, ".Breaker logging: Half-open: Next call is a trial!", ConsoleColor.Magenta)
                );
            this.retryPolicy = Policy.Wrap(new Policy[] { waitAndRetryPolicy, circuitBreakerPolicy });
            */
        }
        public ClientContext GetSPOContext(string siteUrl, string tenantUser, string tenantUserPassword)
        {
            try
            {
                ClientContext cc = new AuthenticationManager().GetSharePointOnlineAuthenticatedContextTenant(siteUrl, tenantUser, tenantUserPassword);
                return cc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ClientContext GetSP2013Context(string siteUrl, string userName, string passWord, string domain)
        {
            try
            {
                //const System.Security.Authentication.SslProtocols _Tls12 = (System.Security.Authentication.SslProtocols)0x00000C00;
                //const System.Net.SecurityProtocolType Tls12 = (System.Net.SecurityProtocolType)_Tls12;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3 | System.Net.SecurityProtocolType.Tls | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;

                ClientContext cc = new AuthenticationManager().GetNetworkCredentialAuthenticatedContext(siteUrl, userName, passWord, domain);
                cc.ExecuteQuery();
                return cc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Web Operations
        public IEnumerable<string> GetAllWebUrls(ClientContext cc)
        {
            try
            {
                return cc.Site.GetAllWebUrls();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool WebExists(ClientContext cc, string url)
        {
            try
            {
                return cc.WebExistsFullUrl(url);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IDictionary<string, List<string>> GetWebUserGroups(ClientContext cc)
        {
            var results = new Dictionary<string, List<string>>();
            try
            {
                var groups = cc.LoadQuery(cc.Web.SiteGroups.Include(g => g.Title, g => g.Users));
                cc.ExecuteQuery();
                foreach (var group in groups)
                {
                    List<string> users = new List<string>();
                    foreach (var user in group.Users)
                    {
                        users.Add(user.LoginName);
                    }
                    results.Add(group.Title, users);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, cc.Url);
            }
            return results;
        }

        public List<string> GetWebGroups(ClientContext cc)
        {
            var results = new List<string>();
            try
            {
                var groups = cc.LoadQuery(cc.Web.SiteGroups.Include(g => g.Title, g => g.Users));
                cc.ExecuteQuery();
                foreach (var group in groups)
                {
                    results.Add(group.Title);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, cc.Url);
            }
            return results;
        }

        public bool GroupExists(ClientContext cc, string groupTitle)
        {
            try
            {
                return cc.Web.GroupExists(groupTitle);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};Group:{groupTitle}");
                throw ex;
            }
        }
        public bool UserExistsInGroup(ClientContext cc, string groupTitle, string userLoginName)
        {
            try
            {
                return cc.Web.IsUserInGroup(groupTitle, userLoginName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};Group:{groupTitle};User:{userLoginName}");
                throw ex;
            }
        }
        public IEnumerable<string> GetSiteContentTypes(ClientContext cc)
        {
            try
            {
                var ctypes = cc.LoadQuery(cc.Web.ContentTypes.Include(c => c.Name).Where(c => c.ReadOnly == false && c.Hidden == false));
                cc.ExecuteQuery();
                return ctypes.Select(c => c.Name);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }
        public bool CheckIfSiteContentTypeExists(ClientContext cc, string contentTypeName)
        {
            try
            {
                return cc.Web.ContentTypeExistsByName(contentTypeName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};ContentTypeName:{contentTypeName}");
                throw ex;
            }
        }

        public List<string> GetWebCustomFields(ClientContext cc)
        {
            try
            {
                var results = new List<string>();
                var fields = cc.LoadQuery(cc.Web.Fields.Where(f => !f.Hidden && !f.ReadOnlyField && f.Group != "_Hidden"));
                cc.ExecuteQuery();
                foreach (Field field in fields)
                {
                    try
                    {
                        var properties = ParseSchemaXml(field.SchemaXml);
                        if (properties.ContainsKey("SourceID") && properties["SourceID"] != "http://schemas.microsoft.com/sharepoint/v3")
                        {
                            results.Add(field.EntityPropertyName);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }

        public bool CheckFieldExists(ClientContext cc, string fieldName)
        {
            try
            {
                return cc.Web.FieldExistsByName(fieldName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};FieldName:{fieldName}");
                throw ex;
            }
        }

        public IDictionary<string, UserPermissions> GetWebUserPermissions(ClientContext cc)
        {
            var results = new Dictionary<string, UserPermissions>();
            try
            {
                var users = cc.LoadQuery(cc.Web.SiteUsers.Include(u => u.LoginName, u => u.Email, u => u.Title));
                cc.ExecuteQuery();
                foreach (var user in users)
                {
                    var permissionResult = UserPermissions.None;
                    var userPermissions = cc.Web.GetUserEffectivePermissions(user.LoginName);

                    cc.ExecuteQuery();
                    if (userPermissions.Value.Has(PermissionKind.ManagePermissions))
                    {
                        //Full Control
                        permissionResult = UserPermissions.FullControl;
                    }
                    else if (userPermissions.Value.Has(PermissionKind.ManageLists))
                    {
                        //Edit
                        permissionResult = UserPermissions.Edit;
                    }
                    else if (userPermissions.Value.Has(PermissionKind.ViewListItems))
                    {
                        //Read
                        permissionResult = UserPermissions.Read;
                    }
                    else
                    {
                        permissionResult = UserPermissions.Limited;
                    }
                    results.Add(user.LoginName, permissionResult);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
            }
            return results;
        }

        public UserPermissions CheckUserPermissions(ClientContext cc, string userLoginName)
        {
            try
            {
                var permissionResult = UserPermissions.None;
                var users = cc.LoadQuery(cc.Web.SiteUsers.Include(u => u.LoginName).Where(u => u.LoginName == userLoginName));
                cc.ExecuteQuery();
                var user = users.FirstOrDefault();
                if (user != null)
                {
                    var userPermissions = cc.Web.GetUserEffectivePermissions(user.LoginName);
                    cc.ExecuteQuery();
                    if (userPermissions.Value.Has(PermissionKind.ManagePermissions))
                    {
                        //Full Control
                        permissionResult = UserPermissions.FullControl;
                    }
                    else if (userPermissions.Value.Has(PermissionKind.ManageLists))
                    {
                        //Edit
                        permissionResult = UserPermissions.Edit;
                    }
                    else if (userPermissions.Value.Has(PermissionKind.ViewListItems))
                    {
                        //Read
                        permissionResult = UserPermissions.Read;
                    }
                    else
                    {
                        permissionResult = UserPermissions.Limited;
                    }
                }
                return permissionResult;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }

        public List<SPAlert> GetAlertsInformation(ClientContext cc)
        {
            try
            {
                var alertResults = new List<SPAlert>();
                var users = cc.LoadQuery(cc.Web.SiteUsers.Include(u => u.LoginName, u => u.Email, u => u.Title));
                cc.ExecuteQuery();
                foreach (var user in users)
                {
                    cc.Load(user.Alerts);
                    cc.ExecuteQuery();
                    foreach (var alert in user.Alerts)
                    {
                        var alertResult = new SPAlert();
                        alertResult.Title = alert.Title;
                        alertResult.UserLoginName = user.LoginName;
                        alertResult.ListUrl = alert.ListUrl;
                        alertResult.Filter = alert.Filter;
                        alertResult.Status = Enum.GetName(typeof(AlertStatus), alert.Status);

                        alertResults.Add(alertResult);
                    }
                }
                return alertResults;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }
        #endregion

        #region List Operations       

        public IEnumerable<string> GetLists(ClientContext cc)
        {
            try
            {
                var results = new List<string>();
                cc.Load(cc.Web);
                var lists = cc.LoadQuery(cc.Web.Lists.Where(l => l.Hidden == false &&
                                                    l.Title != "AppPages" &&
                                                    l.Title != "Style Library" &&
                                                    l.Title != "Form Templates" &&
                                                    l.Title != "Site Assets" &&
                                                    l.Title != "Reusable Content" &&
                                                    l.Title != "Content and Structure Reports" &&
                                                    l.Title != "Site Collection Documents" &&
                                                    l.Title != "MicroFeed" &&
                                                    l.Title != "Hub Settings" &&
                                                    l.Title != "Workflow Tasks").Include(l => l.Title));
                cc.ExecuteQuery();
                return lists.Select(l => l.Title);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }

        public bool ListExists(ClientContext cc, string listName)
        {
            try
            {
                return cc.Web.ListExists(listName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public int GetListBaseTemplate(ClientContext cc, string listName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                return list.BaseTemplate;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public int GetListItemCount(ClientContext cc, string listName)
        {
            try
            {

                var list = cc.Web.GetListByTitle(listName);
                return list.ItemCount;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public IEnumerable<string> GetListContentTypes(ClientContext cc, string listName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                if (list.ContentTypesEnabled && list.AllowContentTypes)
                {
                    var ctypes = cc.LoadQuery(list.ContentTypes.Include(c => c.Name));
                    cc.ExecuteQuery();
                    return ctypes.Select(c => c.Name);
                }
                else
                {
                    //does not allow content types
                    return null;
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public bool CheckIfContentTypeExists(ClientContext cc, string listName, string contentTypeName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                return list.ContentTypeExistsByName(contentTypeName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName};ContentTypeName:{contentTypeName}");
                throw ex;
            }
        }

        public IEnumerable<string> GetListColumns(ClientContext cc, string listName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                List<string> fieldNames = new List<string>();
                var fields = cc.LoadQuery(list.Fields.Where(f => !f.Hidden && !f.ReadOnlyField && f.Group != "_Hidden"));
                cc.ExecuteQuery();
                foreach (var field in fields)
                {
                    fieldNames.Add(field.EntityPropertyName);
                }
                return fieldNames;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public bool GetListFieldExistsByName(ClientContext cc, string listName, string fieldName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                return list.FieldExistsByName(fieldName);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName};Field:{fieldName}");
                throw ex;
            }
        }

        public List<SPListItem> GetListItems(ClientContext cc, string listName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                var spTimeZone = cc.Web.RegionalSettings.TimeZone;
                cc.Load(spTimeZone);
                cc.ExecuteQuery();

                //2.Resolve System.TimeZoneInfo from Microsoft.SharePoint.Client.TimeZone 
                var fixedTimeZoneName = spTimeZone.Description.Replace("and", "&");
                var timeZoneInfo = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.DisplayName == fixedTimeZoneName);

                var isDocLib = (list.BaseType == BaseType.DocumentLibrary);
                var results = new List<SPListItem>();

                //looping through 500 items each
                ListItemCollectionPosition itemPosition = null;
                while (true)
                {
                    CamlQuery camlQuery = new CamlQuery();
                    camlQuery.ListItemCollectionPosition = itemPosition;

                    /*
                   var viewFields = "<ViewFields><FieldRef Name='EncodedAbsUrl' /> " +
                              "<FieldRef Name = 'Author' />" +
                              "<FieldRef Name='Editor' /> <FieldRef Name = 'Created' />" +
                              "<FieldRef Name='Modified' /> <FieldRef Name = 'ID' /> <FieldRef Name = 'Title' /> <FieldRef Name = 'Name' />" +
                              "<FieldRef Name='FileRef' /> <FieldRef Name = 'FileDirRef' /></ViewFields>";

                    camlQuery.ViewXml = $"<View Scope='RecursiveAll'>{viewFields}<RowLimit>500</RowLimit></View>";*/

                    camlQuery.ViewXml = "<View Scope='RecursiveAll'><RowLimit>500</RowLimit></View>";

                    ListItemCollection listItems = list.GetItems(camlQuery);
                    cc.Load(listItems);
                    cc.ExecuteQuery();

                    itemPosition = listItems.ListItemCollectionPosition;                   

                    foreach (ListItem listItem in listItems)
                    {
                        results.Add(new SPListItem()
                        {
                            Title = (!isDocLib) ? ((listItem["Title"] == null) ? string.Empty : listItem["Title"].ToString()) : string.Empty,
                            FileRef = listItem["FileRef"].ToString(),
                            FileDirRef = listItem["FileDirRef"].ToString(),
                            //FileType = (listItem["File_x0020_Type"] == null) ? string.Empty : listItem["File_x0020_Type"].ToString(),                            
                            ID = listItem["ID"].ToString(),
                            ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(listItem["Modified"].ToString()), timeZoneInfo),
                            ListServerTemplate = list.BaseTemplate
                            //Name = name
                        });
                    }
                    if (itemPosition == null)
                        break;
                }
                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        public bool GetListItemExists(ClientContext cc, string listName, SPListItem listItem)
        {
            try
            {
                //var retValue = retryPolicy.Execute(() =>  {
                var list = cc.Web.GetListByTitle(listName);
                logger.Log(LogLevel.Info, $"Processing Site:{cc.Url};List:{listName};ListItem:{listItem.FileRef}");

                CamlQuery camlQuery = new CamlQuery();
                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='FileRef'/><Value Type='Text'>" + listItem.FileRef + @"</Value></Eq>
                                                    <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";

                    //camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                    //                <Query>
                    //                    <Where>                                            
                    //                         <Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>1</Value></Eq>                                            
                    //                    </Where>
                    //                </Query>
                    //               </View>";
                }
                else
                {
                    //check for Title
                    if (!String.IsNullOrEmpty(listItem.Title) && (!listItem.Title.Contains("<") || !listItem.Title.Contains(">")))
                        camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='Title'/><Value Type='Text'>" + listItem.Title + @"</Value></Eq>
                                                    <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";
                    else
                        camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='ID'/><Value Type='Number'>" + listItem.ID + @"</Value></Eq>
                                                    <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";
                }

                var listItems = list.GetItems(camlQuery);
                cc.Load(listItems);
                int retryCount = 5;
                while (retryCount > 0)
                {
                    try
                    {
                        cc.ExecuteQuery();
                        retryCount = 0;
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex, $"RetryError: {retryCount} For Site:{cc.Url};List:{listName};ListItem:{listItem.FileRef}");
                        retryCount--;
                        Thread.Sleep(1000);
                        if (retryCount == 0)
                            throw ex;
                    }
                }

                if (listItems.Count >= 1)
                {
                    return true;
                }
                else
                    return false;

                //});
                //return retValue;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName};ListItem:{listItem.FileRef}");
                throw ex;
            }
        }

        public bool GetListItemExistsByModifiedDate(ClientContext cc, string listName, SPListItem listItem)
        {
            try
            {
                var retValue = retryPolicy.Execute(() =>
                {
                    var list = cc.Web.GetListByTitle(listName);
                    CamlQuery camlQuery = new CamlQuery();
                    if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='Modified'/><Value Type='DateTime' IncludeTimeValue='TRUE'>" + listItem.ModifiedDate.ToString("yyyy-MM-ddTHH:mm:ssZ") + @"</Value></Eq>
                                                    <And>
                                                        <Eq><FieldRef Name='FileRef'/><Value Type='Text'>" + listItem.FileRef + @"</Value></Eq>
                                                        <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                    </And>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";

                        //camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                        //                <Query>
                        //                    <Where>                                            
                        //                         <Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>1</Value></Eq>                                            
                        //                    </Where>
                        //                </Query>
                        //               </View>";
                    }
                    else
                    {
                        //check for Title
                        if (!String.IsNullOrEmpty(listItem.Title) && (!listItem.Title.Contains("<") || !listItem.Title.Contains(">")))
                            camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='Title'/><Value Type='Text'>" + listItem.Title + @"</Value></Eq>
                                                    <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                    <And>
                                                        <Eq><FieldRef Name='Modified'/><Value Type='DateTime' IncludeTimeValue='FALSE'>" + listItem.ModifiedDate.ToString("MM/dd/yyyy") + @"</Value></Eq>
                                                    </And>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";
                        else
                            camlQuery.ViewXml = @"<View Scope='RecursiveAll'>
                                        <Query>
                                            <Where>
                                                <And>
                                                    <Eq><FieldRef Name='ID'/><Value Type='Number'>" + listItem.ID + @"</Value></Eq>
                                                    <Eq><FieldRef Name='FileDirRef'/><Value Type='Lookup'>" + listItem.FileDirRef + @"</Value></Eq>
                                                    <And>
                                                        <Eq><FieldRef Name='Modified'/><Value Type='DateTime' IncludeTimeValue='FALSE'>" + listItem.ModifiedDate.ToString("MM/dd/yyyy") + @"</Value></Eq>
                                                    </And>
                                                </And>
                                            </Where>
                                        </Query>
                                       </View>";
                    }

                    var listItems = list.GetItems(camlQuery);
                    cc.Load(listItems);
                    cc.ExecuteQuery();
                    if (listItems.Count >= 1)
                    {
                        return true;
                    }
                    else
                        return false;
                });

                return retValue;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName};ListItem:{listItem.FileRef}");
                throw ex;
            }
        }

        //TODO
        public void GetListPermissions(ClientContext cc, string listName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                var roleAssignments = cc.LoadQuery(list.RoleAssignments.Include(r => r.Member, r => r.Member.PrincipalType,
                                                                                r => r.RoleDefinitionBindings.Include(b => b.Name)));
                cc.ExecuteQuery();

                foreach (var listRoleAssignment in roleAssignments)
                {
                    //cc.Load(listRoleAssignment.Member);
                    //cc.Load(listRoleAssignment.RoleDefinitionBindings);
                    //cc.ExecuteQuery();
                    foreach (var roleDefinitionBinding in listRoleAssignment.RoleDefinitionBindings)
                    {
                        //cc.Load(roleDefinitionBinding);
                        //cc.ExecuteQuery();

                        //roleDefinitionBinding.Name
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url};List:{listName}");
                throw ex;
            }
        }

        //Get WikiPage Webparts
        public List<SPWebPart> GetWikiPageWebParts(ClientContext cc)
        {
            try
            {
                var results = new List<SPWebPart>();
                cc.Load(cc.Web);
                var lists = cc.LoadQuery(cc.Web.Lists.Where(l => l.Hidden == false && (l.BaseTemplate == 119 || l.BaseTemplate == 850))
                    .Include(l => l.Title));
                cc.ExecuteQuery();
                foreach (var list in lists)
                {
                    var getItems = GetListItems(cc, list.Title);
                    foreach (var item in getItems)
                    {
                        var file = cc.Web.GetFileByServerRelativeUrl(item.FileRef);
                        var limitedWebPartManager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
                        cc.Load(limitedWebPartManager.WebParts, wps => wps.Include(wp => wp.WebPart.Title, wp => wp.WebPart.Hidden, wp => wp.WebPart.IsClosed));
                        //if you want more props --> http://www.c-sharpcorner.com/article/retrieve-webparts-from-page-using-csom-with-powershell-for-s/
                        cc.ExecuteQuery();

                        foreach (var webPartDefinition in limitedWebPartManager.WebParts)
                        {
                            results.Add(new SPWebPart
                            {
                                FileRelativeUrl = item.FileRef,
                                WebPartTitle = webPartDefinition.WebPart.Title,
                                WebPartStatus = (webPartDefinition.WebPart.Hidden) ? WebPartStatus.Hidden : (webPartDefinition.WebPart.IsClosed) ? WebPartStatus.Closed : WebPartStatus.Present
                            });
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{cc.Url}");
                throw ex;
            }
        }

        public bool CheckIfWebPartPresent(ClientContext cc, string fileRelativeUrl, string webPartTitle)
        {
            var file = cc.Web.GetFileByServerRelativeUrl(fileRelativeUrl);
            var limitedWebPartManager = file.GetLimitedWebPartManager(PersonalizationScope.Shared);
            var webPartDefinitions = cc.LoadQuery(limitedWebPartManager.WebParts
                .Include(wps => wps.WebPart.Title, wps => wps.WebPart.Hidden, wps => wps.WebPart.IsClosed)
                .Where(wps => wps.WebPart.Title == webPartTitle && !wps.WebPart.IsClosed && !wps.WebPart.Hidden));
            cc.ExecuteQuery();
            var webPartDefinition = webPartDefinitions.FirstOrDefault();
            if (webPartDefinition != null)
                return true;
            else
                return false;
        }

        public List<string> GetListViews(ClientContext cc, string listName)
        {
            var results = new List<string>();
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                var views = cc.LoadQuery(list.Views.Include(v => v.Title, v => v.Id, v => v.Hidden));
                cc.ExecuteQuery();
                foreach (var view in views)
                {
                    results.Add(view.Title);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, cc.Url);
            }
            return results;
        }

        public bool CheckIfListViewExists(ClientContext cc, string listName, string viewName)
        {
            try
            {
                var list = cc.Web.GetListByTitle(listName);
                var view = list.GetViewByName(viewName);
                if (view == null)
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private Dictionary<string, string> ParseSchemaXml(string schemaXml)
        {
            var properties = new Dictionary<string, string>();
            var xdoc = XDocument.Parse(schemaXml);
            var attributes = xdoc.Descendants("Field").Attributes();
            foreach (var attr in attributes)
            {
                if (!properties.ContainsKey(attr.Name.LocalName))
                    properties.Add(attr.Name.LocalName, attr.Value);
            }
            return properties;
        }

        //TODO: Navigation Validation
        private void GetNavigationLinks(ClientContext cc)
        {
            var nav = cc.Web.Navigation;
            cc.Load(nav, c => c.TopNavigationBar, c => c.QuickLaunch);
            cc.ExecuteQuery();
            GetNavNodes(cc, nav.QuickLaunch);
            GetNavNodes(cc, nav.TopNavigationBar);
        }

        private void GetNavNodes(ClientContext cc, NavigationNodeCollection navigationNodes)
        {
            foreach (var navNode in navigationNodes)
            {
                //Console.Write(navNode.Title, navNode.Url, navNode.Id, cc.Web.Url)
                cc.Load(navNode.Children);
                cc.ExecuteQuery();
                if (navNode.Children.Count > 0)
                {
                    GetNavNodes(cc, navNode.Children);
                }
            }
        }

        //TODO: Get Web Permissions
        private void GetWebPermissions(ClientContext cc)
        {
            var roleAssignments = cc.LoadQuery(cc.Web.RoleAssignments.Include(r => r.Member, r => r.Member.PrincipalType,
                                                                            r => r.RoleDefinitionBindings.Include(b => b.Name)));
            foreach (var roleAssignment in roleAssignments)
            {
                if (roleAssignment.Member.PrincipalType == Microsoft.SharePoint.Client.Utilities.PrincipalType.User)
                {
                    //Get the Display Name of the user and/or other properties
                    PeopleManager peopleManager = new PeopleManager(cc);
                    string[] profilePropertyNames = new string[] { "PreferredName", "Department", "Title" };
                    UserProfilePropertiesForUser profilePropertiesForUser = new UserProfilePropertiesForUser(
                        cc, roleAssignment.Member.LoginName, profilePropertyNames);
                    IEnumerable<string> profilePropertyValues = peopleManager.GetUserProfilePropertiesFor(profilePropertiesForUser);

                    //Get the permissions
                    Console.Write(roleAssignment.Member.LoginName, roleAssignment.Member.Title);
                }
            }
            cc.ExecuteQuery();
        }
    }
}
