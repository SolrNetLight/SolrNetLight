using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight
{
    [DataContract]
    public class SolrCommandObject<T>
    {
        [DataMember(Name = "doc")]
        public T Doc { get; set; }

    }
}
