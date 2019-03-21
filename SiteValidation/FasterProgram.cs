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

namespace SiteValidation
{
    class FasterProgram
    {
        //Initialize csvWriter & DirectoryInfo object
        public static CsvWriter csv = null;
        public static DirectoryInfo dirInfo = null;

        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            try
            {
                //Register and initialize containers
                var container = new UnityContainer();
                container.RegisterType<ISharePointWebValidationService, WebValidationServiceController>();
                container.RegisterType<ISharePointListValidationService, ListValidationServiceController>();
                container.RegisterType<IUserMapping, UserMappingController>();

                IUserMapping userMapping = null;
                if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["UserMappingFilePath"]))
                {
                    container.RegisterType<IUserMapping, UserMappingController>();
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
                    var sRelativeUrl = siteRelativeUrl;
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

                    //Create base directory first based on the sRelativeUrl
                    dirInfo = Directory.CreateDirectory(ConfigurationManager.AppSettings["LogDirectory"] + sRelativeUrl);

                    //Get missing Sites from the site collection
                    logger.Log(LogLevel.Info, $"Validating Sites and Lists for {targetSiteUrl}");

                    Task<List<string>> t_missingSites = Task.Factory.StartNew<List<string>>(() =>
                    {
                        logger.Log(LogLevel.Info, $"Checking for missing sites");
                        return spWebValidationService.MissingSites();
                    });

                    t_missingSites.Wait();
                    var missingSites = t_missingSites.Result;

                    if (missingSites.Count > 0)
                        CsvWriterHelper.WriteCsvRecords(missingSites, Path.Combine(dirInfo.FullName, "missingSites.csv"));

                    //Perform site collection operations
                    SiteCollectionValidationOperations(spWebValidationService);

                    //Perform web & list operations
                    var webUrls = spWebValidationService.GetAllSourceWebUrls();

                    foreach (var webUrl in webUrls)
                    {
                        var webUri = new Uri(webUrl, true);
                        var relativeUri = webUri.MakeRelativeUri(webUri);
                        dirInfo = Directory.CreateDirectory(dirInfo.FullName + relativeUri.ToString());

                        if (webUrl == sourceSiteUrl)
                        {
                            WebValidationOperations(spWebValidationService);
                            ListValidationOperations(spListValidationService);
                            continue;
                        }

                        if (missingSites.Count > 0)
                        {
                            var match = missingSites.Exists(s => s == webUrl);
                            if (match)
                                continue;
                        }

                        targetSiteUrl = ConfigurationManager.AppSettings["TargetSiteHost"] + "/" + relativeUri.ToString();

                        //initialize source connection object
                        srcSPCredObject = new SPConnection(ConfigurationManager.AppSettings["SourceSiteType"], webUrl, relativeUri.ToString(), ConfigurationManager.AppSettings["SourceUserName"], ConfigurationManager.AppSettings["SourcePassword"]);
                        tgtSPCredObject = new SPConnection(ConfigurationManager.AppSettings["TargetSiteType"], targetSiteUrl, relativeUri.ToString(), ConfigurationManager.AppSettings["TargetUserName"], ConfigurationManager.AppSettings["TargetPassword"]);

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


                /*
                List<Task> tasks = new List<Task>();
                foreach (var siteRelativeUrl in siteRelativeUrls)
                {
                    //Passing the task with parameters -- arg = siteRelativeUrl.
                    Task t = Task.Factory.StartNew((arg) =>
                    {
                        var sRelativeUrl = (string)arg;
                        var sourceSiteUrl = ConfigurationManager.AppSettings["SourceSiteHost"] + sRelativeUrl;
                        var targetSiteUrl = ConfigurationManager.AppSettings["TargetSiteHost"] + sRelativeUrl;

                        //initialize source connection object
                        var srcSPCredObject = new SPConnection(sourceSiteUrl, ConfigurationManager.AppSettings["SourceUserName"], ConfigurationManager.AppSettings["SourcePassword"]);
                        var tgtSPCredObject = new SPConnection(targetSiteUrl, ConfigurationManager.AppSettings["TargetUserName"], ConfigurationManager.AppSettings["TargetPassword"]);

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

                        //Create base directory first based on the sRelativeUrl
                        dirInfo = Directory.CreateDirectory(ConfigurationManager.AppSettings["LogDirectory"] + sRelativeUrl);

                        //Get missing Sites from the site collection
                        logger.Log(LogLevel.Info, $"Validating Sites and Lists for {targetSiteUrl}");

                        Task<List<string>> t_missingSites = Task.Factory.StartNew<List<string>>(() =>
                        {
                            logger.Log(LogLevel.Info, $"Checking for missing sites");
                            return spWebValidationService.MissingSites();
                        });

                        t_missingSites.Wait();
                        var missingSites = t_missingSites.Result;

                        if (missingSites.Count > 0)
                            CsvWriterHelper.WriteCsvRecords(missingSites, Path.Combine(dirInfo.FullName, "missingSites.csv"));

                        //Perform site collection operations
                        SiteCollectionValidationOperations(spWebValidationService);

                        //Perform web & list operations
                        var webUrls = spWebValidationService.GetAllSourceWebUrls();

                        foreach (var webUrl in webUrls)
                        {
                            var webUri = new Uri(webUrl, true);
                            var relativeUri = webUri.MakeRelativeUri(webUri);
                            dirInfo = Directory.CreateDirectory(dirInfo.FullName + relativeUri.ToString());

                            if (webUrl == sourceSiteUrl)
                            {
                                WebValidationOperations(spWebValidationService);
                                ListValidationOperations(spListValidationService);
                                continue;
                            }

                            if (missingSites.Count > 0)
                            {
                                var match = missingSites.Exists(s => s == webUrl);
                                if (match)
                                    continue;
                            }

                            targetSiteUrl = ConfigurationManager.AppSettings["TargetSiteHost"] + "/" + relativeUri.ToString();

                            //initialize source connection object
                            srcSPCredObject = new SPConnection(webUrl, ConfigurationManager.AppSettings["SourceUserName"], ConfigurationManager.AppSettings["SourcePassword"]);
                            tgtSPCredObject = new SPConnection(targetSiteUrl, ConfigurationManager.AppSettings["TargetUserName"], ConfigurationManager.AppSettings["TargetPassword"]);

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
                    },
                        siteRelativeUrl // pass symbol as an arg so task receives proper value to process:
                    );

                    tasks.Add(t);
                }
                //wait for all the tasks to complete.
                Task.WaitAll(tasks.ToArray());
                }
                */

                logger.Log(LogLevel.Info, $"Validation Complete");
                Console.ReadKey();
            }
            catch (AggregateException ae)
            {
                ae = ae.Flatten();  // could have a tree of exceptions, so flatten first:
                foreach (Exception ex in ae.InnerExceptions)
                    logger.Log(LogLevel.Error, ex);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex);
            }
        }

        private static void WebValidationOperations(ISharePointWebValidationService spWebValidationService)
        {
            Task<List<string>> t_missingGroups = Task.Factory.StartNew<List<string>>(() =>
                {
                    logger.Log(LogLevel.Info, $"Checking for missing web groups");
                    return spWebValidationService.MissingWebGroups();
                }
            );

            Task<List<UserGroupStatus>> t_missingUsersInGroups = Task.Factory.StartNew<List<UserGroupStatus>>(() =>
                {
                    logger.Log(LogLevel.Info, $"Checking for missing users in groups");
                    return spWebValidationService.MissingUsersInGroups();
                }
            );

            Task.WaitAll(new Task[] { t_missingGroups, t_missingUsersInGroups });

            if (t_missingGroups.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingGroups.Result, Path.Combine(dirInfo.FullName, "missingGroups.csv"));

            if (t_missingUsersInGroups.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingUsersInGroups.Result, Path.Combine(dirInfo.FullName, "missingUsersInGroups.csv"));
        }

        private static void SiteCollectionValidationOperations(ISharePointWebValidationService spWebValidationService)
        {
            Task<List<UserPermStatus>> t_mismatchUserPerms = Task.Factory.StartNew<List<UserPermStatus>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for site user permissions");
                return spWebValidationService.CheckUserPermissions();
            });

            Task<List<string>> t_missingContentTypes = Task.Factory.StartNew<List<string>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing site content types");
                return spWebValidationService.MissingContentTypes();
            });

            Task<List<SPField>> t_missingSiteColumns = Task.Factory.StartNew<List<SPField>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing site columns");
                return spWebValidationService.MissingSiteColumns();
            });

            Task.WaitAll(new Task[] { t_mismatchUserPerms, t_missingContentTypes, t_missingSiteColumns });

            if (t_mismatchUserPerms.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_mismatchUserPerms.Result, Path.Combine(dirInfo.FullName, "mismatchUserPerms.csv"));

            if (t_missingContentTypes.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingContentTypes.Result, Path.Combine(dirInfo.FullName, "missingContentTypes.csv"));

            if (t_missingSiteColumns.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingSiteColumns.Result, Path.Combine(dirInfo.FullName, "missingSiteColumns.csv"));
        }

        private static void ListValidationOperations(ISharePointListValidationService spListValidationService)
        {
            Task<List<string>> t_missingLists = Task.Factory.StartNew<List<string>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing lists");
                return spListValidationService.MissingLists();
            });

            Task<List<SPListItemCount>> t_listItemsCountMismatch = Task.Factory.StartNew<List<SPListItemCount>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for list item count mismatch");
                return spListValidationService.GetListsItemsCountMismatch();
            });

            Task<List<SPField>> t_missingFields = Task.Factory.StartNew<List<SPField>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing list fields");
                return spListValidationService.MissingListColumns();
            });

            Task<List<SPListItem>> t_missingListItems = Task.Factory.StartNew<List<SPListItem>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing list items");
                return spListValidationService.MissingListItems();
            });

            Task<List<SPWebPart>> t_missingWebParts = Task.Factory.StartNew<List<SPWebPart>>(() =>
            {
                logger.Log(LogLevel.Info, $"Checking for missing webparts");
                return spListValidationService.MissingWebParts();
            });

            Task.WaitAll(new Task[] { t_missingLists, t_listItemsCountMismatch, t_missingFields, t_missingListItems, t_missingWebParts });

            if (t_missingLists.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingLists.Result, Path.Combine(dirInfo.FullName, "missingLists.csv"));

            if (t_listItemsCountMismatch.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_listItemsCountMismatch.Result, Path.Combine(dirInfo.FullName, "listItemsCountMismatch.csv"));

            if (t_missingFields.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingFields.Result, Path.Combine(dirInfo.FullName, "missingFields.csv"));

            if (t_missingListItems.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingListItems.Result, Path.Combine(dirInfo.FullName, "missingListItems.csv"));

            if (t_missingWebParts.Result.Count > 0)
                CsvWriterHelper.WriteCsvRecords(t_missingWebParts.Result, Path.Combine(dirInfo.FullName, "missingWebParts.csv"));
        }
    }
}
