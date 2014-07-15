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
using System.Linq;
using System.Threading.Tasks;
using SolrNetLight.Commands.Parameters;
using SolrNetLight.Exceptions;

namespace SolrNetLight.Impl {
    /// <summary>
    /// Main component to interact with Solr
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public class SolrServer<T> : ISolrOperations<T> {
        private readonly ISolrBasicOperations<T> basicServer;
        private readonly IReadOnlyMappingManager mappingManager;

        public SolrServer(ISolrBasicOperations<T> basicServer, IReadOnlyMappingManager mappingManager) {
            this.basicServer = basicServer;
            this.mappingManager = mappingManager;
            //this._schemaMappingValidator = _schemaMappingValidator;
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<SolrQueryResults<T>> Query(ISolrQuery query, QueryOptions options)
        {
            return await basicServer.Query(query, options);
        }



        public async Task<SolrQueryResults<T>> Query(string q)
        {
            return await Query(new SolrQuery(q));
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public async Task<SolrQueryResults<T>> Query(string q, ICollection<SortOrder> orders) {
            return await Query(new SolrQuery(q), orders);
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<SolrQueryResults<T>> Query(string q, QueryOptions options) {
            return  await basicServer.Query(new SolrQuery(q), options);
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public async Task<SolrQueryResults<T>> Query(ISolrQuery q) {
            return await Query(q, new QueryOptions());
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public async Task<SolrQueryResults<T>> Query(ISolrQuery query, ICollection<SortOrder> orders) {
            return await Query(query, new QueryOptions { OrderBy = orders });
        }

        /// <summary>
        /// Executes a facet field query only
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public async Task<ICollection<KeyValuePair<string, int>>> FacetFieldQuery(SolrFacetFieldQuery facet) {
            var r = await basicServer.Query(SolrQuery.All, new QueryOptions {
                Rows = 0,
                Facet = new FacetParameters {
                    Queries = new[] {facet},
                },
            });
            return r.FacetFields[facet.Field];
        }

        public async Task<ResponseHeader> BuildSpellCheckDictionary() {
            var r = await basicServer.Query(SolrQuery.All, new QueryOptions {
                Rows = 0,
                //SpellCheck = new SpellCheckingParameters { Build = true },
            });
            return r.Header;
        }

        public async Task<ResponseHeader> AddWithBoost(T doc, double boost)
        {
            return await AddWithBoost(doc, boost, null);
        }

        public async Task<ResponseHeader> AddWithBoost(T doc, double boost, AddParameters parameters)
        {
            return await ((ISolrOperations<T>)this).AddRangeWithBoost(new[] { new KeyValuePair<T, double?>(doc, boost) }, parameters);
        }

      
        public async Task<ResponseHeader> AddRange(IEnumerable<T> docs) {
            return await AddRange(docs, null);
        }

        public async Task<ResponseHeader> AddRange(IEnumerable<T> docs, AddParameters parameters) {
            return await basicServer.AddWithBoost(docs.Select(d => new KeyValuePair<T, double?>(d, null)), parameters);
        }


        public async Task<ResponseHeader> AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs)
        {
            return await ((ISolrOperations<T>)this).AddRangeWithBoost(docs, null);
        }


        public async Task<ResponseHeader> AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters)
        {
            return await basicServer.AddWithBoost(docs, parameters);
        }


        private object GetId(T doc) {
            var uniqueKey = mappingManager.GetUniqueKey(typeof(T));
            if (uniqueKey == null)
                throw new SolrNetException(string.Format("This operation requires a unique key, but type '{0}' has no declared unique key", typeof(T)));
            var prop = uniqueKey.Property;
            return prop.GetValue(doc, null);
        }

        public async Task<ResponseHeader> Commit() {
            return await basicServer.Commit(null);
        }

        /// <summary>
        /// Rollbacks all add/deletes made to the index since the last commit.
        /// </summary>
        /// <returns></returns>
        public async Task<ResponseHeader> Rollback() {
            return await basicServer.Rollback();
        }


        public async Task<ResponseHeader> Add(T doc)
        {
            return await Add(doc, null);
        }

        public async Task<ResponseHeader> Add(T doc, AddParameters parameters)
        {
            return await AddRange(new[] { doc }, parameters);
        }

    }
}