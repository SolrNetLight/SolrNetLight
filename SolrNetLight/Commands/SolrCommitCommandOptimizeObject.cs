using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace SolrNetLight.Commands
{
    [DataContract]
    public class SolrCommitCommandOptimizeObject
    {
        /// <summary>
        /// Block until a new searcher is opened and registered as the main query searcher, making the changes visible. 
        /// Default is true
        /// </summary>
        [DataMember(Name = "waitSearcher")]
        public bool? WaitSearcher { get; set; }

        /// <summary>
        /// Merge segments with deletes away
        /// Default is false
        /// </summary>
        [DataMember(Name = "expungeDeletes")]
        public bool? ExpungeDeletes { get; set; }

        /// <summary>
        /// Optimizes down to, at most, this number of segments
        /// Default is 1
        /// </summary>
        [DataMember(Name = "maxSegments")]
        public int? MaxSegments { get; set; }
    }
}
