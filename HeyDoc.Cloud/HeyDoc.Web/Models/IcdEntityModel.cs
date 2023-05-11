using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class IcdEntityModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string theCode { get; set; }
        public string chapter { get; set; }
        public IList<IcdEntityModel> descendants { get; set; }
        public int entityType { get; set; }
        public bool hasCodingNote { get; set; }
        public bool hasMaternalChapterLink { get; set; }
        public bool important { get; set; }
        public bool isLeaf { get; set; }
        public bool isResidualOther { get; set; }
        public bool isResidualUnspecified { get; set; }
        public int postcoordinationAvailability { get; set; }
        public bool propertiesTruncated { get; set; }
        public double score { get; set; }
        public string stemId { get; set; }
        public bool titleIsASearchResult { get; set; }
        public bool titleIsTopScore { get; set; }
    }
}