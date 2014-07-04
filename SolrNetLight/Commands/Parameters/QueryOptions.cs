#region license
// Copyright (c) 2007-2010 Mauricio Scheffer
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
//  
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Collections.Generic;

namespace SolrNetLight.Commands.Parameters {
    /// <summary>
    /// Query options
    /// </summary>
	public partial class QueryOptions: CommonQueryOptions {

		/// <summary>
		/// Sort order.
		/// By default, it's "score desc"
		/// </summary>
		public ICollection<SortOrder> OrderBy { get; set; }

        /// <summary>
        /// Terms parameters
        /// </summary>
        public TermsParameters Terms { get; set; }


		/// <summary>
		/// This parameter can be used to collapse - or group - documents by the unique values of a specified field. Included in the results are the number of
		/// records by document key and by field value
		/// </summary>
		public TermVectorParameters TermVector { get; set; }


	    public QueryOptions() {
			OrderBy = new List<SortOrder>();
		}
	}
}