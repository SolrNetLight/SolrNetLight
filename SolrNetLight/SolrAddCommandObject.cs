using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight
{
    [DataContract]
    public class SolrAddCommandObject<T>
    {
        [DataMember(Name="add")]
        public SolrCommandObject<T> AddCommand { get; set; }

        public SolrAddCommandObject(T obj)
        {
            AddCommand = new SolrCommandObject<T>();
            AddCommand.Doc = obj;
        }
    }
}
