using SolrNetLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using SolrNetLight.Facet;

namespace SolrNetLight
{
    [DataContract]
    public class SolrResponse<T>
    {
        [DataMember(Name="response")]
        public SolrResponseBase<T> Response { get; set; }

        [DataMember(Name = "facet_counts")]
        public FacetCounts Facets { get; set; }

    }
}
