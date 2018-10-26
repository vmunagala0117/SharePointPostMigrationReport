using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using NLog;
using Common.Enums;
using Common.Interfaces;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.IO;
using HtmlAgilityPack;

namespace DataAccess
{
    public class SharePointRepository2007 : ISharePointRepository2007
    {
        private ILogger logger { get; set; }
        private WSSContext SPContext { get; set; }

        public SharePointRepository2007(ILogger logger)
        {
            this.logger = logger;
        }

        public WSSContext GetSPContext(string siteUrl, string user, string password, string domain = "")
        {
            SPContext = new WSSContext(siteUrl, user, password, domain);
            return SPContext;
        }


        #region List Operations
        public IEnumerable<string> GetLists()
        {
            try
            {
                //Get Lists
                XmlNode xn = SPContext.WSSLists.GetListCollection();
                var elements = xn.GetChildElements();
                //filter out reserved or hidden lists
                return elements
                    .Where(e => (e.Attribute("Hidden") == null || (e.Attribute("Hidden") != null && e.Attribute("Hidden").Value.ToLower() != "true")) &&
                            e.Attribute("Title").Value != "Style Library" &&
                            e.Attribute("Title").Value != "Content and Structure Reports" &&
                            e.Attribute("Title").Value != "Workflow Tasks" &&
                            e.Attribute("Title").Value != "Site Collection Documents" &&
                            e.Attribute("Title").Value != "Form Templates" &&
                            e.Attribute("Title").Value != "Reusable Content" &&
                            e.Attribute("Title").Value != "Site Assets")
                    .Select(e => e.Attribute("Title").Value);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSLists.Url}");
                throw ex;
            }
        }

        public bool ListExists(string listName)
        {
            try
            {
                //Get Lists
                XmlNode xn = SPContext.WSSLists.GetList(listName);
                var elements = xn.GetChildElements();
                var result = elements.Select(e => e.Attribute("Title").Value);
                if (result != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSLists.Url}");
                throw ex;
            }
        }

        public int GetListItemCount(string listName)
        {
            try
            {
                //Get Lists
                XmlNode xn = SPContext.WSSLists.GetList(listName);
                return Int32.Parse(xn.Attributes["ItemCount"].Value);

            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSLists.Url}");
                throw ex;
            }
        }

        //https://docs.microsoft.com/en-us/previous-versions/office/developer/sharepoint-services/ms772599(v=office.12)
        //http://www.iptech-group.com/blog/pagination-dans-sharepoint-2007-getlistitems-webservice
        //https://sharepointweblog.blogspot.com/2009/04/pagination-using-sharepoint-2007-web.html
        public List<SPListItem> GetListItems(string listName)
        {
            try
            {
                var results = new List<SPListItem>();

                //Get List's Regional Setting
                XmlNode xn = SPContext.WSSLists.GetList(listName);
                var elements = xn.GetChildElements();
                var regionalSettingsElement = elements.FirstOrDefault(e => e.Name.LocalName.Equals("RegionalSettings"));
                //https://stackoverflow.com/questions/4265766/is-there-a-way-to-get-a-sharepoint-sites-locale-with-web-services
                var timeZone = regionalSettingsElement.Elements().Where(e => e.Name.LocalName == "TimeZone").Single().Value;
                var utcOffset = new TimeSpan(0, int.Parse(timeZone), 0);
                var timeZoneInfo = TimeZoneInfo.GetSystemTimeZones().Where(t => t.BaseUtcOffset.Equals(utcOffset)).First();
                var listBaseType = int.Parse(xn.Attributes["BaseType"].Value);

                //XML Document object
                XmlDocument xmlDoc = new System.Xml.XmlDocument();
                XmlNode ndQuery = xmlDoc.CreateNode(XmlNodeType.Element, "Query", "");//Query
                XmlNode ndViewFields = xmlDoc.CreateNode(XmlNodeType.Element, "ViewFields", "");//Views fields
                XmlNode ndQueryOptions = xmlDoc.CreateNode(XmlNodeType.Element, "QueryOptions", "");//Options
                ndViewFields.InnerXml = "<FieldRef Name='EncodedAbsUrl' /> " +
                              "<FieldRef Name='LinkFilename' /> <FieldRef Name = 'Author' />" +
                              "<FieldRef Name='Editor' /> <FieldRef Name = 'Created' />" +
                              "<FieldRef Name='Modified' /> <FieldRef Name = 'ID' /> <FieldRef Name = 'Title' /> <FieldRef Name = 'Name' />" +
                              "<FieldRef Name='FileRef' /> <FieldRef Name = 'FileDirRef' />";

                ndQueryOptions.InnerXml = "<ViewAttributes Scope='RecursiveAll' IncludeRootFolder='True' />";
                XmlNode ndListItems = null;
                bool flag;
                do
                {
                    flag = false;
                    ndListItems = SPContext.WSSLists.GetListItems(listName, null, ndQuery, ndViewFields, "500", ndQueryOptions, null);
                    if (ndListItems != null)
                    {
                        XmlNode xmlPosition = ndListItems.SelectSingleNode("//@ListItemCollectionPositionNext");
                        foreach (XmlNode node in ndListItems.ChildNodes)
                        {
                            if (node.Name == "rs:data")
                            {
                                XmlNodeReader objReader = new XmlNodeReader(node);
                                while (objReader.Read())
                                {
                                    //SPList Item
                                    if (objReader["ows_EncodedAbsUrl"] != null && objReader["ows_LinkFilename"] != null)
                                    {
                                        results.Add(new SPListItem()
                                        {
                                            FileDirRef = objReader["ows_FileDirRef"].ToString().Contains(";#") ? objReader["ows_FileDirRef"].ToString().ToLower().Split(new string[] { ";#" }, StringSplitOptions.None)[1] : objReader["ows_FileDirRef"].ToString().ToLower(),
                                            FileRef = objReader["ows_FileRef"].ToString().Contains(";#") ? objReader["ows_FileRef"].ToString().ToLower().Split(new string[] { ";#" }, StringSplitOptions.None)[1] : objReader["ows_FileRef"].ToString().ToLower(),
                                            ID = objReader["ows_ID"],
                                            ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Parse(objReader["ows_Modified"].ToString()), timeZoneInfo),
                                            Title = objReader["ows_Title"],
                                            Name = objReader["ows_Name"],
                                            EncodedAbsUrl = Uri.UnescapeDataString(objReader["ows_EncodedAbsUrl"]),
                                            ListBaseType = listBaseType
                                        });
                                    }
                                }
                            }
                        }
                        if (xmlPosition != null)
                        {
                            ndQueryOptions.InnerXml = "<Paging ListItemCollectionPositionNext='" + xmlPosition.InnerXml + "' /><MeetingInstanceID>-1</MeetingInstanceID><ViewAttributes Scope='RecursiveAll'  IncludeRootFolder='True' />";
                            flag = true;
                        }
                    }
                } while (flag);
                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSLists.Url}");
                throw ex;
            }
        }

        public IEnumerable<string> GetListColumns(string listName)
        {
            try
            {
                //Get List
                XmlNode xn = SPContext.WSSLists.GetList(listName);
                var elements = xn.GetChildElements();
                var fieldsElement = elements.FirstOrDefault(e => e.Name.LocalName.Equals("Fields"));
                if (fieldsElement == null)
                    throw new Exception("List does not contain any fields");
                var fieldElements = fieldsElement.GetXmlNode().GetChildElements();
                /*var listCustomFields = fieldElements.Select(f => f.Attribute("StaticName").Value);*/
                var listCustomFields = fieldElements.Where(e => e.Attribute("SourceID") != null && !e.Attribute("SourceID").Value.Equals("http://schemas.microsoft.com/sharepoint/v3") &&
                            (e.Attribute("ReadOnly") == null || (e.Attribute("ReadOnly") != null && e.Attribute("ReadOnly").Value.ToLower() != "true")) &&
                            (e.Attribute("Hidden") == null || (e.Attribute("Hidden") != null && e.Attribute("Hidden").Value.ToLower() != "true")))
                    .Select(f => f.Attribute("StaticName").Value);
                return listCustomFields;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSLists.Url}");
                throw ex;
            }
        }

        public IEnumerable<string> GetListViews(string listName)
        {
            try
            {
                //Get List
                XmlNode xn = SPContext.WSSViews.GetViewCollection(listName);
                var elements = xn.GetChildElements();
                var views = elements.Where(e => e.Attribute("Hidden") == null || (e.Attribute("Hidden") != null && e.Attribute("Hidden").Value.ToLower() != "true"))
                                    .Select(e => e.Attribute("DisplayName").Value);
                return views.ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSUserGroup.Url}");
                throw ex;
            }
        }

        public List<SPWebPart> GetWikiPageWebParts()
        {
            try
            {
                var results = new List<SPWebPart>();
                //Get Lists
                XmlNode xn = SPContext.WSSLists.GetListCollection();
                var elements = xn.GetChildElements();
                //filter out reserved or hidden lists

                var lists = elements
                    .Where(e => (e.Attribute("Hidden") == null || (e.Attribute("Hidden") != null && e.Attribute("Hidden").Value.ToLower() != "true")) &&
                           (e.Attribute("ServerTemplate").Value == "119" || e.Attribute("ServerTemplate").Value == "850"))
                    .Select(e => e.Attribute("Title").Value);

                foreach (var list in lists)
                {
                    var getItems = GetListItems(list);
                    foreach (var item in getItems)
                    {

                        string getWebPageResponseString = SPContext.WSSWebPartPages.GetWebPartPage(item.EncodedAbsUrl, Common.WSSWebPartPages.SPWebServiceBehavior.Version3);

                        if (string.IsNullOrEmpty(getWebPageResponseString))
                            continue;

                        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(getWebPageResponseString);
                        var webPartNodes = htmlDocument.DocumentNode.SelectNodes("//webpart");
                        if (webPartNodes == null)
                            continue;
                        foreach(var webPart in webPartNodes)
                        {
                            var title = webPart.SelectSingleNode("title").InnerText;
                            var isVisible = webPart.SelectSingleNode("isvisible").InnerText;
                            //var isClosed = webPart.SelectSingleNode("isincluded").InnerText;
                            results.Add(new SPWebPart
                            {
                                FileRelativeUrl = item.FileRef,
                                WebPartTitle = title,
                                WebPartStatus = (isVisible.ToLower() == "false") ? WebPartStatus.Hidden : WebPartStatus.Present
                            });                            
                        }
                    }
                }
                return results;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSWebPartPages.Url}");
                throw ex;
            }
        }
        #endregion

        #region Web Operations
        public List<string> GetWebGroups()
        {
            try
            {
                XmlNode xn = SPContext.WSSUserGroup.GetGroupCollectionFromWeb();
                var elements = xn.GetChildElements().Elements();
                var groups = elements.Select(e => e.Attribute("Name").Value);
                return groups.ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSUserGroup.Url}");
                throw ex;
            }
            return null;
        }

        public List<string> GetWebCustomFields()
        {
            try
            {
                XmlNode xn = SPContext.WSSWebs.GetColumns();
                var elements = xn.GetChildElements();
                var fields = elements
                    .Where(e => e.Attribute("SourceID") != null && e.Attribute("SourceID").Value.Equals("http://schemas.microsoft.com/sharepoint/v3") &&
                            e.Attribute("Group") != null && !e.Attribute("Group").Value.Equals("_Hidden") &&
                            (e.Attribute("ReadOnly") == null || (e.Attribute("ReadOnly") != null && e.Attribute("ReadOnly").Value != "TRUE")) &&
                            (e.Attribute("Sealed") == null || (e.Attribute("Sealed") != null && e.Attribute("Sealed").Value != "TRUE")))
                    .Select(e => e.Attribute("Name").Value);
                return fields.ToList();
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSWebs.Url}");
                throw ex;
            }
        }

        public IEnumerable<string> GetAllWebUrls()
        {
            try
            {
                XmlNode xn = SPContext.WSSWebs.GetAllSubWebCollection();
                //XmlNode xn = SPContext.WSSWebs.GetWebCollection();
                var elements = xn.GetChildElements();
                var currentWebUrl = SPContext.WSSWebs.Url.Replace("/_vti_bin/Webs.asmx", "");
                var webs = elements.Where(e => e.Attribute("Url").Value.StartsWith(currentWebUrl, StringComparison.CurrentCultureIgnoreCase)).Select(e => e.Attribute("Url").Value);
                return webs;
            }
            catch (FaultException fe)
            {
                MessageFault mf = fe.CreateMessageFault();
                if (mf.HasDetail)
                {
                    XmlElement fexe = mf.GetDetail<XmlElement>();
                    Console.WriteLine("\tError: " + fexe.OuterXml);
                }
                throw fe;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSWebs.Url}");
                throw ex;
            }
        }

        public IDictionary<string, List<string>> GetWebUserGroups()
        {
            var results = new Dictionary<string, List<string>>();
            foreach (var group in GetWebGroups())
            {
                XmlNode xn = SPContext.WSSUserGroup.GetUserCollectionFromGroup(group);
                var elements = xn.GetChildElements().Elements();
                var users = new List<string>();
                if (elements.Count() > 0)
                {
                    users = elements.Select(e => e.Attribute("LoginName").Value).ToList();
                }
                results.Add(group, users);
            }
            return results;
        }

        public IEnumerable<string> GetSiteContentTypes()
        {
            try
            {
                XmlNode xn = SPContext.WSSWebs.GetContentTypes();
                var elements = xn.GetChildElements();
                var ctypes = elements
                   .Where(e => e.Attribute("Group") != null && e.Attribute("Group").Value.ToLower() != "_hidden")
                   .Select(e => e.Attribute("Name").Value);
                return ctypes;
            }
            catch (FaultException fe)
            {
                MessageFault mf = fe.CreateMessageFault();
                if (mf.HasDetail)
                {
                    XmlElement fexe = mf.GetDetail<XmlElement>();
                    Console.WriteLine("\tError: " + fexe.OuterXml);
                }
                throw fe;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, $"Site:{SPContext.WSSWebs.Url}");
                throw ex;
            }
        }
        #endregion
    }
}
