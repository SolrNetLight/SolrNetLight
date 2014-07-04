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
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SolrNetLight.Utils;
using SolrNetLight;

namespace SolrNetLight.Commands {
	/// <summary>
	/// Adds / updates documents to solr
	/// </summary>
	/// <typeparam name="T">Document type</typeparam>
	public class AddCommand<T> : ISolrCommand {
	    private readonly IEnumerable<KeyValuePair<T, double?>> documents = new List<KeyValuePair<T, double?>>();
	    private readonly AddParameters parameters;

        /// <summary>
        /// Adds / updates documents to solr
        /// </summary>
        /// <param name="documents"></param>
        /// <param name="serializer"></param>
        /// <param name="parameters"></param>
	    public AddCommand(IEnumerable<KeyValuePair<T, double?>> documents, AddParameters parameters) {
            this.documents = documents;
            //documentSerializer = serializer;
            this.parameters = parameters;
        }


	    public string Execute(ISolrConnection connection) {
            string flux = string.Empty;
            foreach (var item in this.documents)
            {
                var cmd = new SolrAddRootCommandObject<T>(item.Key);

                flux = JsonConvert.SerializeObject(cmd);
                
                //connection.Post("/update", o);
            }

			return connection.Post("/update", flux);
		}
	}
}