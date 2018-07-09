using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Common
{
    public static class Utility
    {
        public static IEnumerable<XElement> GetChildElements(this XmlNode xn)
        {
            XmlNodeReader xnr = new XmlNodeReader(xn);
            //Load XElement 
            XElement listMetadatas = XElement.Load(xnr);
            //Search collection of elements 
            IEnumerable<XElement> childElements = from el in listMetadatas.Elements()
                                                  select el;
            return childElements;
        }

        public static XmlNode GetXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlReader);
                return xmlDoc;
            }
        }
    }
}
