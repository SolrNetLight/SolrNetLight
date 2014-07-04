using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight.Facet
{
    [DataContract]
    public class FacetCounts
    {
        [DataMember(Name = "facet_queries")]
        public FacetQueries FacetQueries { get; set; }
        [DataMember(Name = "facet_fields")]
        public IDictionary<string, ICollection<KeyValuePair<string, int>>> FacetFields { get; set; }
        [DataMember(Name = "facet_ranges")]
        public FacetRanges FacetRanges { get; set; }
    }
}

