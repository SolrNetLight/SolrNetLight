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
        public SolrQueryResults<T> Query(ISolrQuery query, QueryOptions options) {
            return basicServer.Query(query, options);
        }

       

        public SolrQueryResults<T> Query(string q) {
            return Query(new SolrQuery(q));
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public SolrQueryResults<T> Query(string q, ICollection<SortOrder> orders) {
            return Query(new SolrQuery(q), orders);
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public SolrQueryResults<T> Query(string q, QueryOptions options) {
            return basicServer.Query(new SolrQuery(q), options);
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public SolrQueryResults<T> Query(ISolrQuery q) {
            return Query(q, new QueryOptions());
        }

        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        public SolrQueryResults<T> Query(ISolrQuery query, ICollection<SortOrder> orders) {
            return Query(query, new QueryOptions { OrderBy = orders });
        }

        /// <summary>
        /// Executes a facet field query only
        /// </summary>
        /// <param name="facet"></param>
        /// <returns></returns>
        public ICollection<KeyValuePair<string, int>> FacetFieldQuery(SolrFacetFieldQuery facet) {
            var r = basicServer.Query(SolrQuery.All, new QueryOptions {
                Rows = 0,
                Facet = new FacetParameters {
                    Queries = new[] {facet},
                },
            });
            return r.FacetFields[facet.Field];
        }

        public ResponseHeader BuildSpellCheckDictionary() {
            var r = basicServer.Query(SolrQuery.All, new QueryOptions {
                Rows = 0,
                //SpellCheck = new SpellCheckingParameters { Build = true },
            });
            return r.Header;
        }

        public ResponseHeader AddWithBoost(T doc, double boost) {
            return AddWithBoost(doc, boost, null);
        }

        public ResponseHeader AddWithBoost(T doc, double boost, AddParameters parameters) {
            return ((ISolrOperations<T>)this).AddRangeWithBoost(new[] { new KeyValuePair<T, double?>(doc, boost) }, parameters);
        }

        [Obsolete("Use AddRange instead")]
        public ResponseHeader Add(IEnumerable<T> docs) {
            return Add(docs, null);
        }

        public ResponseHeader AddRange(IEnumerable<T> docs) {
            return AddRange(docs, null);
        }

        [Obsolete("Use AddRange instead")]
        public ResponseHeader Add(IEnumerable<T> docs, AddParameters parameters) {
            return basicServer.AddWithBoost(docs.Select(d => new KeyValuePair<T, double?>(d, null)), parameters);
        }

        public ResponseHeader AddRange(IEnumerable<T> docs, AddParameters parameters) {
            return basicServer.AddWithBoost(docs.Select(d => new KeyValuePair<T, double?>(d, null)), parameters);
        }

        [Obsolete("Use AddRangeWithBoost instead")]
        ResponseHeader ISolrOperations<T>.AddWithBoost(IEnumerable<KeyValuePair<T, double?>> docs) {
            return ((ISolrOperations<T>)this).AddWithBoost(docs, null);
        }

        public ResponseHeader AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs) {
            return ((ISolrOperations<T>)this).AddRangeWithBoost(docs, null);
        }

        [Obsolete("Use AddRangeWithBoost instead")]
        ResponseHeader ISolrOperations<T>.AddWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters) {
            return basicServer.AddWithBoost(docs, parameters);
        }

        public ResponseHeader AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters) {
            return basicServer.AddWithBoost(docs, parameters);
        }


        private object GetId(T doc) {
            var uniqueKey = mappingManager.GetUniqueKey(typeof(T));
            if (uniqueKey == null)
                throw new SolrNetException(string.Format("This operation requires a unique key, but type '{0}' has no declared unique key", typeof(T)));
            var prop = uniqueKey.Property;
            return prop.GetValue(doc, null);
        }

        public ResponseHeader Commit() {
            return basicServer.Commit(null);
        }

        /// <summary>
        /// Rollbacks all add/deletes made to the index since the last commit.
        /// </summary>
        /// <returns></returns>
        public ResponseHeader Rollback() {
            return basicServer.Rollback();
        }


        public ResponseHeader Add(T doc) {
            return Add(doc, null);
        }

        public ResponseHeader Add(T doc, AddParameters parameters) {
            return AddRange(new[] { doc }, parameters);
        }

    }
}