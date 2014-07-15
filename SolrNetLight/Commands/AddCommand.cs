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
using System.Reflection;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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
            this.parameters = parameters;
        }


        /// <summary>
        /// Executes this command
        /// </summary>
        /// <param name="connection">The Solr connection</param>
        /// <returns></returns>
	    public async Task<string> Execute(ISolrConnection connection) {
            string flux = string.Empty;
            JObject json = new JObject();
            foreach (var item in this.documents)
            {
                var cmd = new SolrAddRootCommandObject<T>(item.Key);

                flux = JsonConvert.SerializeObject(cmd);

                PropertyInfo[] myPropertyInfo;
                Dictionary<string, object> dictionnaryProperties = new Dictionary<string, object>();

                myPropertyInfo = Type.GetType(typeof(T).AssemblyQualifiedName).GetProperties();
                for (int i = 0; i < myPropertyInfo.Length; i++)
                {
                    dictionnaryProperties = myPropertyInfo[i].GetPropertyAttributes(dictionnaryProperties);
                }

                json = JObject.Parse(flux);
                
                var fieldDictionaryList = new List<string>();

                foreach (var dictionaryItem in dictionnaryProperties)
                {
                    fieldDictionaryList.Add(dictionaryItem.Key);
                }

                string formattedFlux = RemoveFields(json.SelectToken("add.doc"), fieldDictionaryList).ToString();
            }

            return await connection.Post("/update", json.ToString());
		}

        private JContainer RemoveFields(JToken token, List<string> fields)
        {
            JContainer container = token as JContainer;
            if (container == null) return null;

            List<JToken> removeList = new List<JToken>();

            List<JProperty> propertiesToAdd = new List<JProperty>();

            foreach (JToken el in container.Children())
            {
                JProperty p = el as JProperty;
                if (p != null && fields.Contains(p.Name))
                {
                    foreach (var item in p.Value.ToString().Split(','))
                    {
                        string[] keyValue = item.Split(':');
                        string propertySufix = Regex.Replace(keyValue[0],"[^0-9a-zA-Z]+","");
                        string propertyValue = Regex.Replace(keyValue[1], "[^0-9a-zA-Z]+", "");
                        propertiesToAdd.Add(new JProperty(string.Concat(p.Name, propertySufix), propertyValue));
                    }
                    
                    
                    removeList.Add(el);
                }
                RemoveFields(el, fields);
            }

            foreach (JToken el in removeList)
            {
                el.Remove();
            }

            foreach (JToken item in propertiesToAdd)
            {
                container.Add(item);
            }

            return container;
        }

    }
}