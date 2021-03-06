﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SPListItem
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string FileDirRef { get; set; }
        public string FileRef { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string EncodedAbsUrl { get; set; }
        public int ListBaseType { get; set; }
        //public string FileType { get; set; }          


        public int GetListItemHashCode()
        {
            if (this.ListBaseType != 1 && !String.IsNullOrEmpty(this.Title))
                return (this.FileDirRef + this.Title).GetHashCode();
            else
                return GetListItemDefaultHashCode();
        }

        public int GetListItemDefaultHashCode()
        {
            return (this.FileDirRef + this.FileRef).GetHashCode();
        }
    }
}
