using Controller;
using Microsoft.Practices.Unity;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.IO;
using CsvHelper;
using NLog;
using Unity;
using Unity.Resolution;
using System.Diagnostics;

namespace SiteValidation
{
    class Program
    {
        //Initialize csvWriter & DirectoryInfo object
        public static CsvWriter csv = null;
        public static DirectoryInfo dirInfo = null;
        //Initialize logger
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        //Initialize timer to check elapsed time for each function
        private static Stopwatch watch = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            try
            {
                //Register and initialize containers
                var container = new UnityContainer();
                container.RegisterType<ISharePointWebValidationService, WebValidationServiceController>();
                container.RegisterType<ISharePointListValidationService, ListValidationServiceController>();
                container.RegisterType<IUserMapping, UserMappingController>();

                //TODO: What id userMapping is empty.. currently its set to an empty constructor.
                IUserMapping userMapping = new UserMappingController();
                if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["UserMappingFilePath"]))
                {
                    userMapping = container.Resolve<IUserMapping>(
                       new ParameterOverrides
                       {
                        {"filePath", ConfigurationManager.AppSettings["UserMappingFilePath"] }
                       });
                }

                var siteRelativeUrls = ConfigurationManager.AppSettings["SiteRelativeUrls"].Split(new char[] { ';' });
                var userMappingFile = ConfigurationManager.AppSettings["UserMappingFilePath"];
                foreach (var siteRelativeUrl in siteRelativeUrls)
                {
                    var sRelativeUrl = siteRelativeUrl.Equals("/") ? "" : siteRelativeUrl;

                    var sourceSiteUrl = ConfigurationManager.AppSettings["SourceSiteHost"] + sRelativeUrl;
                    var targetSiteUrl = ConfigurationManager.AppSettings["TargetSiteHost"] + sRelativeUrl;

                    //initialize source connection object
                    var srcSPCredObject = new SPConnection(ConfigurationManager.AppSettings["SourceSiteType"], sourceSiteUrl, sRelativeUrl, ConfigurationManager.AppSettings["SourceUserName"], ConfigurationManager.AppSettings["SourcePassword"]);
                    var tgtSPCredObject = new SPConnection(ConfigurationManager.AppSettings["TargetSiteType"], targetSiteUrl, sRelativeUrl, ConfigurationManager.AppSettings["TargetUserName"], ConfigurationManager.AppSettings["TargetPassword"]);

                    var spWebValidationService = container.Resolve<ISharePointWebValidationService>(
                        new ParameterOverrides
                        {
                            {"sourceCreds", srcSPCredObject },
                            {"targetCreds", tgtSPCredObject},
                            {"userMapping", userMapping},
                            {"logger", logger }
                        });

                    var spListValidationService = container.Resolve<ISharePointListValidationService>(
                        new ParameterOverrides
                        {
                            {"sourceCreds", srcSPCredObject },
                            {"targetCreds", tgtSPCredObject},
                            {"userMapping", userMapping},
                            {"logger", logger }
                        });

                    //Create base directory first based on the siteRelativeUrl
                    var targetFilePath = Path.Combine(ConfigurationManager.AppSettings["LogDirectory"], (targetSiteUrl.Contains("https://") ? targetSiteUrl.Replace("https://", "") : targetSiteUrl.Replace("http://", "")));
                    dirInfo = Directory.CreateDirectory(targetFilePath);

                    //Get missing Sites from the site collection
                    logger.Log(LogLevel.Info, $"Validating Sites and Lists for {targetSiteUrl}");

                    logger.Log(LogLevel.Info, $"Checking for missing sites");

                    watch.Restart();
                    var missingSites = spWebValidationService.MissingSitesV1();
                    watch.Stop();
                    logger.Log(LogLevel.Info, $"Missing Sites Elapsed Time: {watch.Elapsed.Seconds}");

                    if (missingSites.Count > 0)
                        CsvWriterHelper.WriteCsvRecords(missingSites, Path.Combine(dirInfo.FullName, "missingSites.csv"));

                    //Perform site collection operations
                    SiteCollectionValidationOperations(spWebValidationService);

                    //Perform web & list operations
                    var webUrls = spWebValidationService.GetAllSourceWebUrls();

                    //TODO: Revisit this again
                    /*
                    if (webUrls.Count() == 1)
                    {
                        if (webUrls.First().ToLower() == sourceSiteUrl.ToLower())
                        {
                            WebValidationOperations(spWebValidationService);
                            ListValidationOperations(spListValidationService);
                        }
                        continue;
                    }
                    */

                    foreach (var webUrl in webUrls)
                    {

                        if (missingSites.Count > 0)
                        {
                            var match = missingSites.Exists(s => s.ToLower() == webUrl.ToLower());
                            if (match)
                                continue;
                        }

                        if (webUrl.ToLower() == sourceSiteUrl.ToLower())
                        {
                            WebValidationOperations(spWebValidationService);
                            ListValidationOperations(spListValidationService);
                            continue;
                        }

                        // relative URL --> the very last part
                        var relativeUrl = webUrl.ToLower().Replace(sourceSiteUrl.ToLower(), "");

                        targetSiteUrl = ConfigurationManager.AppSettings["TargetSiteHost"] + sRelativeUrl + relativeUrl;

                        targetFilePath = Path.Combine(ConfigurationManager.AppSettings["LogDirectory"], (targetSiteUrl.Contains("https://") ? targetSiteUrl.Replace("https://", "") : targetSiteUrl.Replace("http://", "")));
                        dirInfo = Directory.CreateDirectory(targetFilePath);

                        //initialize source connection object
                        srcSPCredObject = new SPConnection(ConfigurationManager.AppSettings["SourceSiteType"], webUrl, relativeUrl, ConfigurationManager.AppSettings["SourceUserName"], ConfigurationManager.AppSettings["SourcePassword"]);
                        tgtSPCredObject = new SPConnection(ConfigurationManager.AppSettings["TargetSiteType"], targetSiteUrl, relativeUrl, ConfigurationManager.AppSettings["TargetUserName"], ConfigurationManager.AppSettings["TargetPassword"]);


                        logger.Log(LogLevel.Info, $"Validating Sites and Lists for {targetSiteUrl}");

                        spWebValidationService = container.Resolve<ISharePointWebValidationService>(
                        new ParameterOverrides
                        {
                            {"sourceCreds", srcSPCredObject },
                            {"targetCreds", tgtSPCredObject},
                            {"userMapping", userMapping},
                            {"logger", logger }
                        });
                        WebValidationOperations(spWebValidationService);

                        spListValidationService = container.Resolve<ISharePointListValidationService>(
                        new ParameterOverrides
                        {
                            {"sourceCreds", srcSPCredObject },
                            {"targetCreds", tgtSPCredObject},
                            {"userMapping", userMapping},
                            {"logger", logger }
                        });
                        ListValidationOperations(spListValidationService);
                    }
                }
                logger.Log(LogLevel.Info, $"Validation Complete");
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }
        private static void SiteCollectionValidationOperations(ISharePointWebValidationService spWebValidationService)
        {
            watch.Restart();
            logger.Log(LogLevel.Info, $"Checking for missing site content types");
            var missingContentTypes = spWebValidationService.MissingContentTypes();
            if (missingContentTypes.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingContentTypes, Path.Combine(dirInfo.FullName, "missingContentTypes.csv"));
            watch.Stop();
            logger.Log(LogLevel.Info, $"Missing site content types Elapsed Time: {watch.Elapsed.Seconds}");

            watch.Restart();
            logger.Log(LogLevel.Info, $"Checking for missing site columns");
            var missingSiteColumns = spWebValidationService.MissingSiteColumns();
            if (missingSiteColumns.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingSiteColumns, Path.Combine(dirInfo.FullName, "missingSiteColumns.csv"));
            watch.Stop();
            logger.Log(LogLevel.Info, $"Missing site columns elapsed time: {watch.Elapsed.Seconds}");

            logger.Log(LogLevel.Info, $"Checking for missing site groups");
            var missingGroups = spWebValidationService.MissingSiteGroupsV1();
            if (missingGroups.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingGroups, Path.Combine(dirInfo.FullName, "missingSiteGroups.csv"));
            watch.Stop();
            logger.Log(LogLevel.Info, $"Missing Site Groups Elapsed Time: {watch.Elapsed.Seconds}");

            //TODO: custom permission levels
            /*
            logger.Log(LogLevel.Info, $"Checking for missing users in groups");
            var missingUsersInGroups = spWebValidationService.MissingUsersInGroups();
            if (missingUsersInGroups.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingUsersInGroups, Path.Combine(dirInfo.FullName, "missingUsersInGroups.csv"));

            
            logger.Log(LogLevel.Info, $"Checking for site user permissions");
            var mismatchUserPerms = spWebValidationService.CheckUserPermissions();
            if (mismatchUserPerms.Count > 0)
                CsvWriterHelper.WriteCsvRecords(mismatchUserPerms, Path.Combine(dirInfo.FullName, "mismatchUserPerms.csv"));
            */
        }

        private static void WebValidationOperations(ISharePointWebValidationService spWebValidationService)
        {
            logger.Log(LogLevel.Info, $"Checking for mismatches in web permissions inheritance");
            var mismatchWebPermsInheritance = spWebValidationService.CheckWebPermissionsInheritance();
            if (mismatchWebPermsInheritance.Count > 0)
                CsvWriterHelper.WriteCsvRecords(mismatchWebPermsInheritance, Path.Combine(dirInfo.FullName, "mismatchWebPermsInheritance.csv"));

            logger.Log(LogLevel.Info, $"Checking for missing web groups");
            var missingGroups = spWebValidationService.MissingWebGroups();
            if (missingGroups.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingGroups, Path.Combine(dirInfo.FullName, "missingWebGroups.csv"));
        }

        private static void ListValidationOperations(ISharePointListValidationService spListValidationService)
        {

            logger.Log(LogLevel.Info, $"Checking for missing lists");
            var missingLists = spListValidationService.MissingLists();
            if (missingLists.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingLists, Path.Combine(dirInfo.FullName, "missingLists.csv"));

            logger.Log(LogLevel.Info, $"Checking for list item count mismatch");
            var listItemsCountMismatch = spListValidationService.GetListsItemsCountMismatch();
            if (listItemsCountMismatch.Count > 0)
                CsvWriterHelper.WriteCsvRecords(listItemsCountMismatch, Path.Combine(dirInfo.FullName, "listItemsCountMismatch.csv"));

            logger.Log(LogLevel.Info, $"Checking for missing list fields");
            var missingFields = spListValidationService.MissingListColumnsV1();
            if (missingFields.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingFields, Path.Combine(dirInfo.FullName, "missingFields.csv"));

            logger.Log(LogLevel.Info, $"Checking for missing list views");
            var missingListViews = spListValidationService.MissingListViews();
            if (missingListViews.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingListViews, Path.Combine(dirInfo.FullName, "missingListViews.csv"));

            logger.Log(LogLevel.Info, $"Checking for missing list items");
            var missingListItems = spListValidationService.MissingListItemsV1();
            if (missingListItems.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingListItems, Path.Combine(dirInfo.FullName, "missingListItems.csv"));


            /*
            logger.Log(LogLevel.Info, $"Checking for missing list items by modified date");
            var missingListItemsByModifiedDate = spListValidationService.MissingListItems();
            if (missingListItemsByModifiedDate.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingListItemsByModifiedDate, Path.Combine(dirInfo.FullName, "missingListItemsByModifiedDate.csv"));
            */

            logger.Log(LogLevel.Info, $"Checking for missing webparts");
            var missingWebParts = spListValidationService.MissingWebParts();
            if (missingWebParts.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingWebParts, Path.Combine(dirInfo.FullName, "missingWebParts.csv"));

            logger.Log(LogLevel.Info, $"Checking for missing worflows");
            var missingWorkflows = spListValidationService.MissingWorkflows();
            if (missingWorkflows.Count > 0)
                CsvWriterHelper.WriteCsvRecords(missingWorkflows, Path.Combine(dirInfo.FullName, "missingWorkflows.csv"));

        }
    }
}
