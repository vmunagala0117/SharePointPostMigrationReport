using Common.Interfaces;
using DataAccess;
using Microsoft.Practices.Unity;
using System;
using Common;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using Common.Enums;
using NLog;
using System.Collections.Concurrent;
using Unity;
using Unity.Resolution;

namespace Controller
{
    public class ListValidationServiceController : ISharePointListValidationService
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

        private List<string> _getExistingLists = new List<string>();
        private List<string> GetExistingLists
        {
            get
            {
                if (_getExistingLists.Count == 0 /*&& SourceClientContext != null && TargetClientContext != null*/)
                {
                    try
                    {
                        IEnumerable<string> lists = null;
                        if (SourceSiteCreds.SiteType == SiteType.WSS)
                        {
                            lists = this.SharePointRepository2007.GetLists();
                        }
                        else
                        {
                            lists = this.SharePointRepository.GetLists(SourceClientContext);
                        }
                        foreach (var list in lists)
                        {
                            if (this.SharePointRepository.ListExists(TargetClientContext, list))
                            {
                                _getExistingLists.Add(list);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(LogLevel.Error, ex);
                    }
                    return _getExistingLists;
                }
                else
                {
                    return _getExistingLists;
                }
            }
            set
            {
                value = _getExistingLists;
            }
        }
        public ListValidationServiceController() { }
        public ListValidationServiceController(SPConnection sourceCreds, SPConnection targetCreds, IUserMapping userMapping, ILogger logger)
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

            //re-initialize _getExistingLists
            _getExistingLists = new List<string>();
        }

        public List<string> MissingLists()
        {
            var results = new List<string>();
            try
            {
                IEnumerable<string> lists = null;
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                {
                    lists = this.SharePointRepository2007.GetLists();
                }
                else
                {
                    lists = this.SharePointRepository.GetLists(SourceClientContext);
                }
                foreach (var list in lists)
                {
                    if (!this.SharePointRepository.ListExists(TargetClientContext, list))
                    {
                        results.Add(list);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

        public List<SPListItemCount> GetListsItemsCountMismatch()
        {
            var results = new List<SPListItemCount>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    int sListItemCount = 0;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                        sListItemCount = this.SharePointRepository2007.GetListItemCount(list);
                    else
                        sListItemCount = this.SharePointRepository.GetListItemCount(SourceClientContext, list);
                    int tListItemCount = this.SharePointRepository.GetListItemCount(TargetClientContext, list);
                    if (sListItemCount != tListItemCount)
                    {
                        results.Add(new SPListItemCount()
                        {
                            CurrentValue = tListItemCount,
                            ExpectedValue = sListItemCount,
                            ListName = list
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        public List<SPField> MissingListColumns()
        {
            var results = new List<SPField>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    IEnumerable<string> listFields = null;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                    {
                        listFields = this.SharePointRepository2007.GetListColumns(list);
                    }
                    else
                    {
                        listFields = this.SharePointRepository.GetListColumns(SourceClientContext, list);
                    }
                    foreach (var listField in listFields)
                    {
                        if (!this.SharePointRepository.GetListFieldExistsByName(TargetClientContext, list, listField))
                        {
                            results.Add(new SPField
                            {
                                Url = $"{TargetClientContext.Url}/Lists/{list}",
                                FieldName = listField
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        public List<SPListView> MissingListViews()
        {
            var results = new List<SPListView>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    IEnumerable<string> listViews = null;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                    {
                        listViews = this.SharePointRepository2007.GetListViews(list);
                    }
                    else
                    {
                        listViews = this.SharePointRepository.GetListViews(SourceClientContext, list);
                    }
                    foreach (var listView in listViews)
                    {
                        if (!this.SharePointRepository.CheckIfListViewExists(TargetClientContext, list, listView))
                        {
                            results.Add(new SPListView
                            {
                                ListName = list,
                                ViewName = listView
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        #region TEST
        private void GetListItemsInventory(string list)
        {
            List<SPListItem> listItems = this.SharePointRepository2007.GetListItems(list);
            CsvWriterHelper.WriteCsvRecords(listItems, System.IO.Path.Combine(@"D:\Logs", "2007ListItems.csv"));

            List<SPListItem> listItems2 = this.SharePointRepository.GetListItems(TargetClientContext, list);
            CsvWriterHelper.WriteCsvRecords(listItems2, System.IO.Path.Combine(@"D:\Logs", "2013ListItems.csv"));
        }
        #endregion

        //CORRECT MODULE
        public List<SPListItem> MissingListItems()
        {
            var results = new List<SPListItem>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    #region Test - REMOVE
                    /*if (list == "Documents")
                        GetListItemsInventory(list);*/
                    #endregion

                    IEnumerable<SPListItem> listItems = null;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                    {
                        listItems = this.SharePointRepository2007.GetListItems(list);
                    }
                    else
                    {
                        listItems = this.SharePointRepository.GetListItems(SourceClientContext, list);
                    }

                    string targetHost = new Uri(this.TargetSiteCreds.SiteUrl).GetLeftPart(UriPartial.Authority);
                    string targetRootSiteRelativeUrl = this.TargetSiteCreds.SiteUrl.Replace(targetHost, "");
                    if (!String.IsNullOrEmpty(this.TargetSiteCreds.WebRelativeUrl))
                        targetRootSiteRelativeUrl = targetRootSiteRelativeUrl.Replace(this.TargetSiteCreds.WebRelativeUrl, "");

                    if (targetRootSiteRelativeUrl.EndsWith("/"))
                        targetRootSiteRelativeUrl = targetRootSiteRelativeUrl.Substring(0, targetRootSiteRelativeUrl.Length - 1);

                    foreach (var listItem in listItems)
                    {
                        //check if the listItem is starting with current web's relative URL?
                        if (!listItem.FileDirRef.StartsWith(targetRootSiteRelativeUrl) && !listItem.FileRef.StartsWith(targetRootSiteRelativeUrl))
                        {
                            if (listItem.FileDirRef.StartsWith("/"))
                                listItem.FileDirRef = listItem.FileDirRef.Substring(1);

                            if (listItem.FileRef.StartsWith("/"))
                                listItem.FileRef = listItem.FileRef.Substring(1);

                            listItem.FileDirRef = targetRootSiteRelativeUrl + "/" + listItem.FileDirRef;
                            listItem.FileRef = targetRootSiteRelativeUrl + "/" + listItem.FileRef;
                        }

                        if (!this.SharePointRepository.GetListItemExists(TargetClientContext, list, listItem))
                        {
                            logger.Log(LogLevel.Info, $"Missing ListItem:{listItem.FileRef};List:{list}");
                            results.Add(listItem);
                        }
                        else
                        {
                            //Check if files were changed based on modified date
                            //if (!this.SharePointRepository.GetListItemExistsByModifiedDate(TargetClientContext, list, listItem))
                            //    results.Add(listItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        //Missing List Items based on modified date -- for missing items without modified see above module.
        public List<SPListItem> MissingListItems2()
        {
            var results = new List<SPListItem>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    IEnumerable<SPListItem> listItems = null;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                    {
                        this.SharePointRepository2007.GetListItems(list);
                    }
                    else
                    {
                        listItems = this.SharePointRepository.GetListItems(SourceClientContext, list);
                    }

                    foreach (var listItem in listItems)
                    {
                        if (!this.SharePointRepository.GetListItemExistsByModifiedDate(TargetClientContext, list, listItem))
                        {
                            logger.Log(LogLevel.Info, $"Missing ListItem:{listItem.FileRef};List:{list}");
                            results.Add(listItem);
                        }
                        else
                        {
                            //Check if files were changed based on modified date
                            //if (!this.SharePointRepository.GetListItemExistsByModifiedDate(TargetClientContext, list, listItem))
                            //    results.Add(listItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        public List<SPListItem> MissingListItemsByModifiedDate()
        {
            var results = new List<SPListItem>();
            foreach (var list in this.GetExistingLists)
            {
                try
                {
                    IEnumerable<SPListItem> listItems = null;
                    if (SourceSiteCreds.SiteType == SiteType.WSS)
                    {
                        this.SharePointRepository2007.GetListItems(list);
                    }
                    else
                    {
                        listItems = this.SharePointRepository.GetListItems(SourceClientContext, list);
                    }
                    foreach (var listItem in listItems)
                    {
                        if (!this.SharePointRepository.GetListItemExistsByModifiedDate(TargetClientContext, list, listItem))
                        {
                            logger.Log(LogLevel.Info, $"Missing ListItem:{listItem.FileRef};List:{list}");
                            results.Add(listItem);
                        }
                        else
                        {
                            //Check if files were changed based on modified date
                            //if (!this.SharePointRepository.GetListItemExistsByModifiedDate(TargetClientContext, list, listItem))
                            //    results.Add(listItem);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(LogLevel.Error, ex);
                }
            }
            return results;
        }

        public List<SPWebPart> MissingWebParts()
        {
            var results = new List<SPWebPart>();
            try
            {
                var spWebParts = new List<SPWebPart>();
                if (SourceSiteCreds.SiteType == SiteType.WSS)
                {
                    spWebParts = this.SharePointRepository2007.GetWikiPageWebParts();
                }
                else
                {
                    spWebParts = spWebParts = this.SharePointRepository.GetWikiPageWebParts(SourceClientContext);
                }

                string targetHost = new Uri(this.TargetSiteCreds.SiteUrl).GetLeftPart(UriPartial.Authority);
                string targetRootSiteRelativeUrl = this.TargetSiteCreds.SiteUrl.Replace(targetHost, "");
                if (!String.IsNullOrEmpty(this.TargetSiteCreds.WebRelativeUrl))
                    targetRootSiteRelativeUrl = targetRootSiteRelativeUrl.Replace(this.TargetSiteCreds.WebRelativeUrl, "");

                if (targetRootSiteRelativeUrl.EndsWith("/"))
                    targetRootSiteRelativeUrl = targetRootSiteRelativeUrl.Substring(0, targetRootSiteRelativeUrl.Length - 1);

                foreach (var webPart in spWebParts)
                {
                    if (webPart.WebPartStatus == WebPartStatus.Present)
                    {
                        if (!webPart.FileRelativeUrl.StartsWith(targetRootSiteRelativeUrl))
                        {
                            
                            if (webPart.FileRelativeUrl.StartsWith("/"))
                                webPart.FileRelativeUrl = webPart.FileRelativeUrl.Substring(1);

                            webPart.FileRelativeUrl = targetRootSiteRelativeUrl + "/" + webPart.FileRelativeUrl;
                        }

                        if (!this.SharePointRepository.CheckIfWebPartPresent(TargetClientContext, webPart.FileRelativeUrl, webPart.WebPartTitle))
                        {
                            results.Add(new SPWebPart()
                            {
                                FileRelativeUrl = webPart.FileRelativeUrl,
                                WebPartTitle = webPart.WebPartTitle
                            });
                        }
                    }
                    else
                    {
                        logger.Log(LogLevel.Info, $"WebPart:{webPart.WebPartTitle};Status:{webPart.WebPartStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
            return results;
        }

    }
}
