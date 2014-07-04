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
using SolrNetLight.Exceptions;

namespace SolrNetLight {
    /// <summary>
    /// Consolidating interface, exposes all operations
    /// </summary>
    /// <typeparam name="T">Document type</typeparam>
    public interface ISolrOperations<T> : ISolrReadOnlyOperations<T> {
        /// <summary>
        /// Commits posted documents, 
        /// blocking until index changes are flushed to disk and
        /// blocking until a new searcher is opened and registered as the main query searcher, making the changes visible.
        /// </summary>
        ResponseHeader Commit();

        /// <summary>
        /// Rollbacks all add/deletes made to the index since the last commit.
        /// </summary>
        /// <returns></returns>
        ResponseHeader Rollback();


        /// <summary>
        /// Adds / updates a document
        /// </summary>
        /// <param name="doc">document to add/update</param>
        /// <returns></returns>
        ResponseHeader Add(T doc);

        /// <summary>
        /// Adds / updates a document with parameters
        /// </summary>
        /// <param name="doc">The document to add/update.</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        ResponseHeader Add(T doc, AddParameters parameters);

        /// <summary>
        /// Adds / updates a document with index-time boost
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        ResponseHeader AddWithBoost(T doc, double boost);

        /// <summary>
        /// Adds / updates a document with index-time boost and add parameters
        /// </summary>
        /// <param name="doc">The document to add/update.</param>
        /// <param name="boost">The index-time boost to apply.</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        ResponseHeader AddWithBoost(T doc, double boost, AddParameters parameters);


        /// <summary>
        /// Adds / updates several documents at once
        /// </summary>
        /// <param name="docs">documents to add/update</param>
        /// <returns></returns>
        [Obsolete("Use AddRange instead")]
        ResponseHeader Add(IEnumerable<T> docs);


        /// <summary>
        /// Adds / updates several documents at once
        /// </summary>
        /// <param name="docs">documents to add/update</param>
        /// <returns></returns>
        ResponseHeader AddRange(IEnumerable<T> docs);

        /// <summary>
        /// Adds / updates several documents at once
        /// </summary>
        /// <param name="docs">documents to add/update</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        [Obsolete("Use AddRange instead")]
        ResponseHeader Add(IEnumerable<T> docs, AddParameters parameters);

        /// <summary>
        /// Adds / updates several documents at once
        /// </summary>
        /// <param name="docs">documents to add/update</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        ResponseHeader AddRange(IEnumerable<T> docs, AddParameters parameters);

        /// <summary>
        /// Adds / updates documents with index-time boost
        /// </summary>
        /// <param name="docs">List of docs / boost. If boost is null, no boost is applied</param>
        /// <returns></returns>
        [Obsolete("Use AddRangeWithBoost instead")]
        ResponseHeader AddWithBoost(IEnumerable<KeyValuePair<T, double?>> docs);

        /// <summary>
        /// Adds / updates documents with index-time boost
        /// </summary>
        /// <param name="docs">List of docs / boost. If boost is null, no boost is applied</param>
        /// <returns></returns>
        ResponseHeader AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs);

        /// <summary>
        /// Adds / updates documents with index-time boost and add parameters
        /// </summary>
        /// <param name="docs">List of docs / boost. If boost is null, no boost is applied</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        [Obsolete("Use AddRangeWithBoost instead")]
        ResponseHeader AddWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters);

        /// <summary>
        /// Adds / updates documents with index-time boost and add parameters
        /// </summary>
        /// <param name="docs">List of docs / boost. If boost is null, no boost is applied</param>
        /// <param name="parameters">The add parameters.</param>
        /// <returns></returns>
        ResponseHeader AddRangeWithBoost(IEnumerable<KeyValuePair<T, double?>> docs, AddParameters parameters);

        /// <summary>
        /// Create the dictionary for use by the SolrSpellChecker. 
        /// In typical applications, one needs to build the dictionary before using it. 
        /// However, it may not always be necessary as it is possible to setup the spellchecker with a dictionary that already exists.
        /// </summary>
        ResponseHeader BuildSpellCheckDictionary();

    }
}