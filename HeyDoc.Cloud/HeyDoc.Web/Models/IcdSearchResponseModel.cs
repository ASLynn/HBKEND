using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeyDoc.Web.Models
{
    public class IcdSearchResponseModel
    {
        public IList<IcdEntityModel> destinationEntities { get; set; }
        public bool error { get; set; }
        public string errorMessage { get; set; }
        public int guessType { get; set; }
        public bool resultChopped { get; set; }
        public string uniqueSearchId { get; set; }
        public bool wordSuggestionsChopped { get; set; }
        public string words { get; set; }
        public string releaseId { get; set; }
        public string linearizationName { get; set; }
    }
}