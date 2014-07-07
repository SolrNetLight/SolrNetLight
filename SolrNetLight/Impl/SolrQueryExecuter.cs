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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using SolrNetLight.Commands.Parameters;
using SolrNetLight.Exceptions;
using SolrNetLight.Utils;
using Newtonsoft.Json;
using SolrNetLight;
using Newtonsoft.Json.Linq;
using System.Reflection;
using SolrNetLight.Attributes;
using System.Collections;
using System.Runtime.Serialization;

namespace SolrNetLight.Impl
{
    /// <summary>
    /// Executes queries
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public class SolrQueryExecuter<T> : ISolrQueryExecuter<T>
    {
        private readonly ISolrConnection connection;
        private readonly ISolrQuerySerializer querySerializer;
        private readonly ISolrFacetQuerySerializer facetQuerySerializer;

        /// <summary>
        /// When the row count is not defined, use this row count by default
        /// </summary>
        public int DefaultRows { get; set; }

        /// <summary>
        /// When row limit is not defined, this value is used
        /// </summary>
        public static readonly int ConstDefaultRows = 100000000;

        /// <summary>
        /// Default Solr query handler
        /// </summary>
        public static readonly string DefaultHandler = "/select";

        /// <summary>
        /// Default Solr handler for More Like This queries
        /// </summary>
        public static readonly string DefaultMoreLikeThisHandler = "/mlt";

        /// <summary>
        /// Solr query request handler to use. By default "/select"
        /// </summary>
        public string Handler { get; set; }

        /// <summary>
        /// Solr request handler to use for MoreLikeThis-handler queries. By default "/mlt"
        /// </summary>
        public string MoreLikeThisHandler { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resultParser"></param>
        /// <param name="connection"></param>
        /// <param name="querySerializer"></param>
        /// <param name="facetQuerySerializer"></param>
        /// <param name="mlthResultParser"></param>
        public SolrQueryExecuter(ISolrConnection connection, ISolrQuerySerializer querySerializer, ISolrFacetQuerySerializer facetQuerySerializer)
        {
            this.connection = connection;
            this.querySerializer = querySerializer;
            this.facetQuerySerializer = facetQuerySerializer;
            DefaultRows = ConstDefaultRows;
            Handler = DefaultHandler;
            MoreLikeThisHandler = DefaultMoreLikeThisHandler;
        }

        /// <summary>
        /// Serializes common query parameters
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetCommonParameters(CommonQueryOptions options)
        {
            if (options == null)
                yield break;

            if (options.Start.HasValue)
                yield return KV.Create("start", options.Start.ToString());

            var rows = options.Rows.HasValue ? options.Rows.Value : DefaultRows;
            yield return KV.Create("rows", rows.ToString());

            if (options.Fields != null && options.Fields.Count > 0)
                yield return KV.Create("fl", string.Join(",", options.Fields.ToArray()));

            foreach (var p in GetFilterQueries(options.FilterQueries))
                yield return p;

            foreach (var p in GetFacetFieldOptions(options.Facet))
                yield return p;

            if (options.ExtraParams != null)
                foreach (var p in options.ExtraParams)
                    yield return p;
        }

        /// <summary>
        /// Gets Solr parameters for all defined query options
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetAllParameters(ISolrQuery Query, QueryOptions options)
        {
            yield return KV.Create("q", querySerializer.Serialize(Query));
            if (options == null)
                yield break;

            foreach (var p in GetCommonParameters(options))
                yield return p;

            if (options.OrderBy != null && options.OrderBy.Count > 0)
                yield return KV.Create("sort", string.Join(",", options.OrderBy.Select(x => x.ToString()).ToArray()));

            foreach (var p in GetTermsParameters(options))
                yield return p;

            foreach (var p in GetTermVectorQueryOptions(options))
                yield return p;
        }

        /// <summary>
        /// Gets Solr parameters for facet queries
        /// </summary>
        /// <param name="fp"></param>
        /// <returns></returns>
        public IEnumerable<KeyValuePair<string, string>> GetFacetFieldOptions(FacetParameters fp)
        {
            if (fp == null)
                yield break;
            if (fp.Queries == null || fp.Queries.Count == 0)
                yield break;

            yield return KV.Create("facet", "true");

            foreach (var fq in fp.Queries)
                foreach (var fqv in facetQuerySerializer.Serialize(fq))
                    yield return fqv;

            if (fp.Prefix != null)
                yield return KV.Create("facet.prefix", fp.Prefix);
            if (fp.EnumCacheMinDf.HasValue)
                yield return KV.Create("facet.enum.cache.minDf", fp.EnumCacheMinDf.ToString());
            if (fp.Limit.HasValue)
                yield return KV.Create("facet.limit", fp.Limit.ToString());
            if (fp.MinCount.HasValue)
                yield return KV.Create("facet.mincount", fp.MinCount.ToString());
            if (fp.Missing.HasValue)
                yield return KV.Create("facet.missing", fp.Missing.ToString().ToLowerInvariant());
            if (fp.Offset.HasValue)
                yield return KV.Create("facet.offset", fp.Offset.ToString());
            if (fp.Sort.HasValue)
                yield return KV.Create("facet.sort", fp.Sort.ToString().ToLowerInvariant());
        }

        /// <summary>
        /// Gets Solr parameters for defined filter queries
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> GetFilterQueries(ICollection<ISolrQuery> filterQueries)
        {
            if (filterQueries == null || filterQueries.Count == 0)
                yield break;
            foreach (var fq in filterQueries)
            {
                yield return new KeyValuePair<string, string>("fq", querySerializer.Serialize(fq));
            }
        }


        public static IEnumerable<string> GetTermVectorParameterOptions(TermVectorParameterOptions o)
        {
            if ((o & TermVectorParameterOptions.All) == TermVectorParameterOptions.All)
            {
                yield return "tv.all";
            }
            else
            {
                if ((o & TermVectorParameterOptions.TermFrequency_InverseDocumentFrequency) == TermVectorParameterOptions.TermFrequency_InverseDocumentFrequency)
                {
                    yield return "tv.tf";
                    yield return "tv.df";
                    yield return "tv.tf_idf";
                }
                if ((o & TermVectorParameterOptions.Offsets) == TermVectorParameterOptions.Offsets)
                    yield return "tv.offsets";
                if ((o & TermVectorParameterOptions.Positions) == TermVectorParameterOptions.Positions)
                    yield return "tv.positions";
                if ((o & TermVectorParameterOptions.DocumentFrequency) == TermVectorParameterOptions.DocumentFrequency)
                    yield return "tv.df";
                if ((o & TermVectorParameterOptions.TermFrequency) == TermVectorParameterOptions.TermFrequency)
                    yield return "tv.tf";
            }
        }

        /// <summary>
        /// Gets the Solr parameters for collapse queries
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> GetTermVectorQueryOptions(QueryOptions options)
        {
            if (options.TermVector == null || !options.TermVector.Fields.Any())
                yield break;

            yield return KV.Create("tv", "true");
            if (options.TermVector.Fields != null)
            {
                var fields = string.Join(",", options.TermVector.Fields.ToArray());
                if (!string.IsNullOrEmpty(fields))
                    yield return KV.Create("tv.fl", fields);
            }

            foreach (var o in GetTermVectorParameterOptions(options.TermVector.Options).Distinct())
                yield return KV.Create(o, "true");
        }


        /// <summary>
        /// Gets solr parameters for terms component
        /// </summary>
        /// <param name="Options"></param>
        /// <returns></returns>
        public static IEnumerable<KeyValuePair<string, string>> GetTermsParameters(QueryOptions Options)
        {
            var terms = Options.Terms;
            if (terms == null)
                yield break;
            if (terms.Fields == null || !terms.Fields.Any())
                throw new SolrNetException("Terms field can't be empty or null");
            yield return KV.Create("terms", "true");
            foreach (var field in terms.Fields)
                yield return KV.Create("terms.fl", field);
            if (!string.IsNullOrEmpty(terms.Prefix))
                yield return KV.Create("terms.prefix", terms.Prefix);
            if (terms.Sort != null)
                yield return KV.Create("terms.sort", terms.Sort.ToString());
            if (terms.Limit.HasValue)
                yield return KV.Create("terms.limit", terms.Limit.ToString());
            if (!string.IsNullOrEmpty(terms.Lower))
                yield return KV.Create("terms.lower", terms.Lower);
            if (terms.LowerInclude.HasValue)
                yield return KV.Create("terms.lower.incl", terms.LowerInclude.ToString().ToLowerInvariant());
            if (!string.IsNullOrEmpty(terms.Upper))
                yield return KV.Create("terms.upper", terms.Upper);
            if (terms.UpperInclude.HasValue)
                yield return KV.Create("terms.upper.incl", terms.UpperInclude.ToString().ToLowerInvariant());
            if (terms.MaxCount.HasValue)
                yield return KV.Create("terms.maxcount", terms.MaxCount.ToString());
            if (terms.MinCount.HasValue)
                yield return KV.Create("terms.mincount", terms.MinCount.ToString());
            if (terms.Raw.HasValue)
                yield return KV.Create("terms.raw", terms.Raw.ToString().ToLowerInvariant());
            if (!string.IsNullOrEmpty(terms.Regex))
                yield return KV.Create("terms.regex", terms.Regex);
        }

        /// <summary>
        /// Executes the query and returns results
        /// </summary>
        /// <returns>query results</returns>
        public SolrQueryResults<T> Execute(ISolrQuery q, QueryOptions options)
        {
            var param = GetAllParameters(q, options);

            var results = new SolrQueryResults<T>();
            string json = connection.Get(Handler, param);

            var rootProduct = JsonConvert.DeserializeObject<SolrResponse<T>>(json);

            JObject obj = JObject.Parse(json);

            Dictionary<string, object> dictionnaryProperties = new Dictionary<string, object>();
            PropertyInfo[] myPropertyInfo;

            myPropertyInfo = Type.GetType(typeof(T).AssemblyQualifiedName).GetProperties();
            for (int i = 0; i < myPropertyInfo.Length; i++)
            {
                dictionnaryProperties = GetPropertyAttributes(myPropertyInfo[i], dictionnaryProperties);
            }

            for (int i = 0; i < rootProduct.Response.NumFound; i++)
            {
                foreach (var item in obj["response"]["docs"][i])
                {
                    var attribute = dictionnaryProperties.Where(x => item.ToString().Contains(x.Key)).FirstOrDefault();

                    if (!string.IsNullOrEmpty(attribute.Key))
                    {
                        PropertyInfo pInfo = attribute.Value as PropertyInfo;
                        bool stringSingle = pInfo.PropertyType.IsAssignableFrom(typeof(Dictionary<String, Single>));
                        bool stringString = pInfo.PropertyType.IsAssignableFrom(typeof(Dictionary<String, String>));
                        bool stringInt = pInfo.PropertyType.IsAssignableFrom(typeof(Dictionary<String, Int32>));

                        if (stringSingle)
                        {
                            var dico = pInfo.GetValue(rootProduct.Response.Data[i], null) as Dictionary<string, Single>;
                            if (dico == null)
                            {
                                dico = new Dictionary<string, Single>();
                            }

                            foreach (var value in item.ToList())
                            {
                                dico.Add((item as JProperty).Name, Single.Parse(value.ToString()));
                            }
                            pInfo.SetValue(rootProduct.Response.Data[i], dico, null);
                        }

                        if (stringString)
                        {
                            var dico = pInfo.GetValue(rootProduct.Response.Data[i], null) as Dictionary<string, string>;
                            if (dico == null)
                            {
                                dico = new Dictionary<string, string>();
                            }

                            foreach (var value in item.ToList())
                            {
                                dico.Add((item as JProperty).Name, value.ToString());
                            }
                            pInfo.SetValue(rootProduct.Response.Data[i], dico, null);
                        }

                        if (stringInt)
                        {
                            var dico = pInfo.GetValue(rootProduct.Response.Data[i], null) as Dictionary<string, int>;
                            if (dico == null)
                            {
                                dico = new Dictionary<string, int>();
                            }

                            foreach (var value in item.ToList())
                            {
                                dico.Add((item as JProperty).Name, int.Parse(value.ToString()));
                            }
                            pInfo.SetValue(rootProduct.Response.Data[i], dico, null);
                        }
                    }


                }

                results.Add(rootProduct.Response.Data[i]);

            }

            //Facet Names
            if (rootProduct.Facets != null)
            {
                foreach (var item in rootProduct.Facets.FacetFields)
                {
                    var jsonItem = (from j in obj["facet_counts"]["facet_fields"]
                                    where j.Path.Contains(item.Key)
                                    select j).First();



                    if (jsonItem != null)
                    {
                        item.Value.Clear();
                        var jsonArray = jsonItem.First();
                        if (jsonArray != null)
                        {
                            var array = jsonArray.ToList();
                            for (int i = 0; i < array.Count; i = i + 2)
                            {
                                int value = int.Parse(array[i + 1].ToString());
                                string key = array[i].ToString();
                                item.Value.Add(new KeyValuePair<string, int>(key, value));
                            }
                        }
                    }


                }

                results.FacetFields = rootProduct.Facets.FacetFields;
            }
            
            return results;
        }


        public static Dictionary<string, object> GetPropertyAttributes(PropertyInfo property, Dictionary<string, object> dic)
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
                        dic.Add(dataMemberName, property);
                    }
                }

            }
            return dic;
        }

    }
}