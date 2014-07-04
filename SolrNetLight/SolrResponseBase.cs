using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight
{
    [DataContract]
    public class SolrResponseBase<T>
    {
         [DataMember(Name = "numFound")]
        public int NumFound { get; set; }

         [DataMember(Name = "start")]
        public int Start { get; set; }

        [DataMember(Name="docs")]
        public List<T> Data { get; set; }

    }
}
