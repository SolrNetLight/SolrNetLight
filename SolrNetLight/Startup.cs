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
using Microsoft.Practices.ServiceLocation;
using SolrNetLight.Impl;
using SolrNetLight.Impl.DocumentPropertyVisitors;
using SolrNetLight.Impl.FacetQuerySerializers;
using SolrNetLight.Impl.FieldParsers;
using SolrNetLight.Impl.FieldSerializers;
using SolrNetLight.Impl.QuerySerializers;
using SolrNetLight.Mapping;
using SolrNetLight.Utils;

namespace SolrNetLight {
    /// <summary>
    /// SolrNet initialization manager
    /// </summary>
    public static class Startup {
        public static readonly Container Container = new Container();

        static Startup() {
            InitContainer();
        }

        public static void InitContainer() {
            ServiceLocator.SetLocatorProvider(() => Container);
            Container.Clear();
            var mapper = new MemoizingMappingManager(new AttributesMappingManager());

            Container.Register<IReadOnlyMappingManager>(c => mapper);

            var fieldSerializer = new DefaultFieldSerializer();
            Container.Register<ISolrFieldSerializer>(c => fieldSerializer);
            Container.Register<ISolrQuerySerializer>(c => new DefaultQuerySerializer(c.GetInstance<ISolrFieldSerializer>()));
            Container.Register<ISolrFacetQuerySerializer>(c => new DefaultFacetQuerySerializer(c.GetInstance<ISolrQuerySerializer>(), c.GetInstance<ISolrFieldSerializer>()));
        }

        /// <summary>
        /// Initializes SolrNet with the built-in container
        /// </summary>
        /// <typeparam name="T">Document type</typeparam>
        /// <param name="serverURL">Solr URL (i.e. "http://localhost:8983/solr")</param>
        public static void Init<T>(string serverURL) {
            var connection = new SolrConnection(serverURL);

            Init<T>(connection);
        }

        /// <summary>
        /// Initializes SolrNet with the built-in container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        public static void Init<T>(ISolrConnection connection) {
            var connectionKey = string.Format("{0}.{1}.{2}", typeof(SolrConnection), typeof(T), connection.GetType());
            Container.Register(connectionKey, c => connection);

            var activator = new SolrDocumentActivator<T>();
            Container.Register<ISolrDocumentActivator<T>>(c => activator);
            Container.Register<ISolrQueryExecuter<T>>(c => new SolrQueryExecuter<T>(connection, c.GetInstance<ISolrQuerySerializer>(), c.GetInstance<ISolrFacetQuerySerializer>()));
            Container.Register<ISolrBasicOperations<T>>(c => new SolrBasicServer<T>(connection, c.GetInstance<ISolrQueryExecuter<T>>(), c.GetInstance<ISolrQuerySerializer>())); //todo
            Container.Register<ISolrBasicReadOnlyOperations<T>>(c => new SolrBasicServer<T>(connection, c.GetInstance<ISolrQueryExecuter<T>>(),  c.GetInstance<ISolrQuerySerializer>())); //todo
            Container.Register<ISolrOperations<T>>(c => new SolrServer<T>(c.GetInstance<ISolrBasicOperations<T>>(), Container.GetInstance<IReadOnlyMappingManager>()));
            Container.Register<ISolrReadOnlyOperations<T>>(c => new SolrServer<T>(c.GetInstance<ISolrBasicOperations<T>>(), Container.GetInstance<IReadOnlyMappingManager>()));
        }

    }
}
