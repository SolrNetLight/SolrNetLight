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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SolrNetLight.Exceptions;
using SolrNetLight.Utils;
using HttpUtility = SolrNetLight.Utils.HttpUtility;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace SolrNetLight.Impl
{
    /// <summary>
    /// Manages HTTP connection with Solr
    /// </summary>
    public class SolrConnection : ISolrConnection
    {
        private string serverURL;
        private string version = "2.2";

        /// <summary>
        /// Manages HTTP connection with Solr
        /// </summary>
        /// <param name="serverURL">URL to Solr</param>
        public SolrConnection(string serverURL)
        {
            ServerURL = serverURL;
            Timeout = -1;
        }

        /// <summary>
        /// URL to Solr
        /// </summary>
        public string ServerURL
        {
            get { return serverURL; }
            set
            {
                serverURL = value;
            }
        }

        /// <summary>
        /// Solr XML response syntax version
        /// </summary>
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// HTTP connection timeout
        /// </summary>
        public int Timeout { get; set; }

        public async Task<string> Post(string relativeUrl, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var content = new MemoryStream(bytes))
            {
                return await PostStream(relativeUrl, "application/json", content, null); //"application/json" "text/xml; charset=utf-8"
            }
        }

        private Stream _content;

        public async Task<string> PostStream(string relativeUrl, string contentType, Stream content, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var u = new UriBuilder(serverURL);
            u.Path += relativeUrl;
            u.Query = GetQuery(parameters);

            HttpClient httpClient = new HttpClient();
           
            StringContent sc = null;
            using (var reader = new StreamReader(content))
                    {
                        string postData = reader.ReadToEnd();
                        sc = new StringContent(postData, Encoding.UTF8, contentType);
                
            }
           
            try
            {
                 HttpResponseMessage response = await httpClient.PostAsync(u.Uri, sc);
                 return await response.Content.ReadAsStringAsync();

            }


            catch (WebException e)
            {
                var msg = e.Message;
                if (e.Response != null)
                {
                    using (var s = e.Response.GetResponseStream())
                    using (var sr = new StreamReader(s))
                        msg = sr.ReadToEnd();
                }
                throw new SolrConnectionException(msg, e, relativeUrl);
            }
        }

        public async Task<string> Get(string relativeUrl, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var u = new UriBuilder(serverURL);
            u.Path += relativeUrl;
            u.Query = GetQuery(parameters);

            //HttpWebRequest request = HttpWebRequest.CreateHttp(u.Uri);
            //request.Method = "GET";

            try
            {
                //var response = GetResponse(request);

                HttpClient httpClient = new HttpClient();
                return await httpClient.GetStringAsync(u.Uri);

            }
            catch (WebException e)
            {
                throw new SolrConnectionException(e, u.Uri.ToString());
            }
        }

        private Task MakeAsyncRequest(Uri uri, string method)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "application/Json";
            request.Method = method;
            return Task.Factory.FromAsync(request.BeginGetResponse, asyncResult => request.EndGetResponse(asyncResult), null)
                                .ContinueWith(t => this.ReadFromStreamResponse(t.Result));
        }

        private string ReadFromStreamResponse(WebResponse webResponse)
        {
            using (var s = webResponse.GetResponseStream())
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Gets the Query 
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private string GetQuery(IEnumerable<KeyValuePair<string, string>> parameters, string format = "json")
        {
            var param = new List<KeyValuePair<string, string>>();
            if (parameters != null)
                param.AddRange(parameters);

            param.Add(KV.Create("version", version));
            if (!string.IsNullOrEmpty(format))
            {
                param.Add(KV.Create("wt", "json"));

            }
            return string.Join("&", param
                .Select(kv => KV.Create(HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value)))
                .Select(kv => string.Format("{0}={1}", kv.Key, kv.Value))
                .ToArray());
        }

        private struct SolrResponse
        {
            public string ETag { get; private set; }
            public string Data { get; private set; }
            public SolrResponse(string eTag, string data)
                : this()
            {
                ETag = eTag;
                Data = data;
            }
        }







    }
}