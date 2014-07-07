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


	    public string Execute(ISolrConnection connection) {
            string flux = string.Empty;
            JObject json = new JObject();
            foreach (var item in this.documents)
            {
                var cmd = new SolrAddRootCommandObject<T>(item.Key);

                flux = JsonConvert.SerializeObject(cmd);

                PropertyInfo[] myPropertyInfo;
                Dictionary<string, string> dictionnaryProperties = new Dictionary<string, string>();

                myPropertyInfo = Type.GetType(typeof(T).AssemblyQualifiedName).GetProperties();
                for (int i = 0; i < myPropertyInfo.Length; i++)
                {
                    dictionnaryProperties = GetPropertyAttributes(myPropertyInfo[i], dictionnaryProperties);
                }

                json = JObject.Parse(flux);
                string formattedFlux = removeFields(json.SelectToken("add.doc"), new List<string>() { "phone_" }).ToString();
            }

            return connection.Post("/update", json.ToString());
		}

        private JContainer removeFields(JToken token, List<string> fields)
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
                removeFields(el, fields);
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

        public static Dictionary<string, string> GetPropertyAttributes(PropertyInfo property, Dictionary<string, string> dic)
        {
            //Dictionary<string, object> attribs = new Dictionary<string, object>();
            // look for attributes that takes one constructor argument
            foreach (var attribData in property.GetCustomAttributes(false))
            {
                if (attribData is DataMemberAttribute)
                {
                    string dataMemberName = ((DataMemberAttribute)attribData).Name;
                    bool isDictionnary = dataMemberName.Contains("_");
                    if (isDictionnary && property.PropertyType.Name == "IDictionary`2")
                    {
                        dic.Add(dataMemberName, property.Name);
                    }
                }

            }
            return dic;
        }
	}
}