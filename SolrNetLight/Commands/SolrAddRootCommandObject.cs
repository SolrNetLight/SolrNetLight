using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight.Commands
{
    [DataContract]
    public class SolrAddRootCommandObject<T>
    {
        [DataMember(Name="add")]
        public SolrAddCommandObject<T> AddCommand { get; set; }

        public SolrAddRootCommandObject(T obj)
        {
            AddCommand = new SolrAddCommandObject<T>();
            AddCommand.Doc = obj;
        }
    }
}
