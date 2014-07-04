using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using SolrNetLight.Commands;

namespace SolrNetLight.Commands
{
    [DataContract]
    public class SolrCommitRootCommandObject
    {
        [DataMember(Name = "optimize")]
        public SolrCommitCommandOptimizeObject Optimize { get; set; }

        [DataMember(Name = "commit")]
        public Object CommitCommand { get; set; }

        public SolrCommitRootCommandObject()
        {
            Optimize = new SolrCommitCommandOptimizeObject();
            CommitCommand = new Object();
        }
    }
}
