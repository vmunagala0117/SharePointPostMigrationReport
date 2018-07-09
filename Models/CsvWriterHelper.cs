using CsvHelper;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class CsvWriterHelper
    {
        private static ILogger logger { get; set; }
        static CsvWriterHelper()
        {

        }
        public static void WriteCsvRecords<T>(IEnumerable<T> records, string fullPathFileName)
        {
            Type t = GetElementTypeOfEnumerable(records);
            if (t == typeof(System.String))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    foreach (var item in records)
                    {
                        csv.WriteField(item);
                        csv.NextRecord();
                    }
                }
            }
            else if (t == typeof(SPListItem))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<SPListItemClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else if (t == typeof(SPListItemCount))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<SPListItemCountClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else if (t == typeof(SPWebPart))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<SPWebPartClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else if (t == typeof(SPField))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<SPFieldClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else if (t == typeof(UserPermStatus))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<UserPermStatusClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else if (t == typeof(UserGroupStatus))
            {
                using (var csv = new CsvWriter(File.CreateText(fullPathFileName)))
                {
                    csv.Configuration.RegisterClassMap<UserGroupStatusClassMap>();
                    csv.WriteRecords(records);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        //https://benohead.com/c-get-element-type-enumerable-reflection/
        private static Type GetElementTypeOfEnumerable(object o)
        {
            var enumerable = o as IEnumerable;
            // if it's not an enumerable why do you call this method all ?
            if (enumerable == null)
                return null;

            Type[] interfaces = enumerable.GetType().GetInterfaces();
            Type elementType = (from i in interfaces
                                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                select i.GetGenericArguments()[0]).FirstOrDefault();

            //peek at the first element in the list if you couldn't determine the element type
            if (elementType == null || elementType == typeof(object))
            {
                object firstElement = enumerable.Cast<object>().FirstOrDefault();
                if (firstElement != null)
                    elementType = firstElement.GetType();
            }
            return elementType;
        }
    }
}
