using Common.Interfaces;
using DataAccess;
using Microsoft.Practices.Unity;
using System;
using Common;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using Common.Enums;
using NLog;
using Unity;
using Unity.Resolution;

namespace Controller
{
    public class WebValidationServiceController : ISharePointWebValidationService
    {
        private ISharePointRepository SharePointRepository { get; set; }
        private ISharePointRepository2007 SharePointRepository2007 { get; set; }
        private IUserMapping UserMapping { get; set; }
        private ILogger logger { get; set; }
        private SPConnection SourceSiteCreds { get; set; }
        private SPConnection TargetSiteCreds { get; set; }
        private ClientContext SourceClientContext { get; set; }
        private ClientContext TargetClientContext { get; set; }
        private WSSContext SourceWSSContext { get; set; }
        private WSSContext TargetWSSContext { get; set; }

        public WebValidationServiceController()
        {
        }

        public WebValidationServiceController(SPConnection sourceCreds, SPConnection targetCreds, IUserMapping userMapping, ILogger logger)
        {
            var container = new UnityContainer();
            container.RegisterType<ISharePointRepository, SharePointRepository>();
            this.SharePointRepository = container.Resolve<ISharePointRepository>(
                    new ParameterOverrides
                    {
                       {"logger", logger }
                    });

            container.RegisterType<ISharePointRepository2007, SharePointRepository2007>();
            this.SharePointRepository2007 = container.Resolve<ISharePointRepository2007>(
                    new ParameterOverrides
                    {
                       {"logger", logger }
                    });

            this.SourceSiteCreds = sourceCreds;
            this.TargetSiteCreds = targetCreds;

            if (sourceCreds.SiteType == SiteType.WSS)
                this.SourceWSSContext = this.SharePointRepository2007.GetSPContext(SourceSiteCreds.SiteUrl, SourceSiteCreds.UserName, SourceSiteCreds.Password);
            else if (sourceCreds.SiteType == SiteType.SharePointOnPremises)
                this.SourceClientContext = this.SharePointRepository.GetSP2013Context(SourceSiteCreds.SiteUrl, SourceSiteCreds.UserName, SourceSiteCreds.Password, SourceSiteCreds.Domain);
            else
                this.SourceClientContext = this.SharePointRepository.GetSPOContext(SourceSiteCreds.SiteUrl, SourceSiteCreds.UserName, SourceSiteCreds.Password);

            if (targetCreds.SiteType == SiteType.WSS)
                this.TargetWSSContext = this.SharePointRepository2007.GetSPContext(TargetSiteCreds.SiteUrl, TargetSiteCreds.UserName, TargetSiteCreds.Password);
            else if (targetCreds.SiteType == SiteType.SharePointOnPremises)
                this.TargetClientContext = this.SharePointRepository.GetSP2013Context(TargetSiteCreds.SiteUrl, TargetSiteCreds.UserName, TargetSiteCreds.Password, TargetSiteCreds.Domain);
            else
                this.TargetClientContext = this.SharePointRepository.GetSPOContext(TargetSiteCreds.SiteUrl, TargetSiteCreds.UserName, TargetSiteCreds.Password);

            this.UserMapping = userMapping;
            this.logger = logger;
        }
        public IEnumerable<string> GetAllSourceWebUrls()
        {
            try
            {
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    return this.SharePointRepository2007.GetAllWebUrls();
                else
                    return this.SharePointRepository.GetAllWebUrls(SourceClientContext);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<UserPermStatus> CheckUserPermissions()
        {
            var results = new List<UserPermStatus>();
            try
            {
                var userPermissions = this.SharePointRepository.GetWebUserPermissions(SourceClientContext);
                foreach (var userPermission in userPermissions)
                {
                    string newUpn = string.Empty;
                    if (UserMapping != null)
                        newUpn = UserMapping.FindNewUpn(userPermission.Key);

                    if (!String.IsNullOrEmpty(newUpn))
                    {
                        var targetUserPermission = this.SharePointRepository.CheckUserPermissions(TargetClientContext, newUpn);
                        //Report mismatch perms b/w source & target
                        if (userPermission.Value != targetUserPermission)
                        {
                            results.Add(new UserPermStatus
                            {
                                CurrentPermissions = targetUserPermission,
                                ExpectedPermissions = userPermission.Value,
                                UserStatus = UserStatus.PresentInUserMapping,
                                LoginName = newUpn
                            });
                        }
                    }
                    else
                    {
                        //User not in the mapping file
                        results.Add(new UserPermStatus
                        {
                            CurrentPermissions = UserPermissions.None,
                            ExpectedPermissions = userPermission.Value,
                            UserStatus = UserStatus.NotPresentInUserMapping,
                            LoginName = userPermission.Key
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

        public List<string> MissingContentTypes()
        {
            var results = new List<string>();
            try
            {
                IEnumerable<String> contentTypes = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    contentTypes = this.SharePointRepository2007.GetSiteContentTypes();
                else
                    contentTypes = this.SharePointRepository.GetSiteContentTypes(SourceClientContext);
                foreach (var contentType in contentTypes)
                {
                    if (!this.SharePointRepository.CheckIfSiteContentTypeExists(TargetClientContext, contentType))
                    {
                        results.Add(contentType);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

        public List<string> MissingGroups()
        {
            var results = new List<string>();
            try
            {
                List<string> groups = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    groups = this.SharePointRepository2007.GetWebGroups();
                else
                    groups = this.SharePointRepository.GetWebGroups(SourceClientContext);
                foreach (var group in groups)
                {
                    if (!this.SharePointRepository.GroupExists(TargetClientContext, group))
                    {
                        results.Add(group);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

        public List<UserGroupStatus> MissingUsersInGroups()
        {
            var results = new List<UserGroupStatus>();
            try
            {
                IDictionary<string, List<string>> usersGroups = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    usersGroups = this.SharePointRepository2007.GetWebUserGroups();
                else
                    usersGroups = this.SharePointRepository.GetWebUserGroups(SourceClientContext);

                foreach (var userGroup in usersGroups)
                {
                    foreach (var user in userGroup.Value)
                    {
                        string newUpn = user;
                        if (UserMapping != null && UserMapping.GetUserMappingList().Count > 0)
                            newUpn = UserMapping.FindNewUpn(user);

                        if (newUpn == string.Empty)
                        {
                            //Missing in the users mapping
                            results.Add(new UserGroupStatus
                            {
                                LoginName = user,
                                GroupName = userGroup.Key,
                                UserInGroupStatus = UserStatus.NotPresentInUserMapping
                            });
                            continue;
                        }
                        if (!this.SharePointRepository.UserExistsInGroup(TargetClientContext, userGroup.Key, newUpn))
                        {
                            //misisng in the group
                            results.Add(new UserGroupStatus
                            {
                                LoginName = newUpn,
                                GroupName = userGroup.Key,
                                UserInGroupStatus = UserStatus.NotPresentInSharePointGroup
                            });
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

        public List<SPField> MissingSiteColumns()
        {
            List<SPField> missingFieldNames = new List<SPField>();
            try
            {
                List<string> sourceFieldNames = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    sourceFieldNames = this.SharePointRepository2007.GetWebCustomFields();
                else
                    sourceFieldNames = this.SharePointRepository.GetWebCustomFields(SourceClientContext);

                foreach (var fieldName in sourceFieldNames)
                {
                    if (!this.SharePointRepository.CheckFieldExists(TargetClientContext, fieldName))
                    {
                        missingFieldNames.Add(new SPField
                        {
                            Url = TargetClientContext.Url,
                            FieldName = fieldName
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return missingFieldNames;
        }

        public List<string> MissingSites()
        {
            var missingSites = new List<string>();
            try
            {
                //Get target host URI
                Uri targetUri = new Uri(TargetSiteCreds.SiteUrl);
                //string targetHost = targetUri.GetLeftPart(UriPartial.Authority);
                string targetHost = TargetSiteCreds.SiteUrl;
                if (!String.IsNullOrEmpty(TargetSiteCreds.WebRelativeUrl))
                    targetHost = targetHost.Replace(TargetSiteCreds.WebRelativeUrl, "");

                //Get source host URI
                Uri sourceUri = new Uri(SourceSiteCreds.SiteUrl);
                //string sourceHost = sourceUri.GetLeftPart(UriPartial.Authority).ToString();
                string sourceHost = SourceSiteCreds.SiteUrl;
                if (!String.IsNullOrEmpty(SourceSiteCreds.WebRelativeUrl))
                    sourceHost = sourceHost.Replace(SourceSiteCreds.WebRelativeUrl, "");

                Uri sourceHostUri = new Uri(sourceHost);

                //Get all webs from the source
                IEnumerable<string> webs = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                    webs = this.SharePointRepository2007.GetAllWebUrls();
                else
                    webs = this.SharePointRepository.GetAllWebUrls(SourceClientContext);

                foreach (var web in webs)
                {                    
                    //var webExists = this.SharePointRepository.WebExists(TargetClientContext, targetHost + "/" + relativeUri.ToString());
                    var webExists = this.SharePointRepository.WebExists(TargetClientContext, targetHost + web.ToLower().Replace(sourceHost.ToLower(), ""));
                    if (!webExists)
                    {
                        missingSites.Add(web);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return missingSites;
        }
    }
}
