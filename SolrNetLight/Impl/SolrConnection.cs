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

        public string Post(string relativeUrl, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            using (var content = new MemoryStream(bytes))
            {
                return PostStream(relativeUrl, "application/json", content, null); //"application/json" "text/xml; charset=utf-8"
            }
        }

        private Stream _content;

        public string PostStream(string relativeUrl, string contentType, Stream content, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            AutoResetEvent allDone = new AutoResetEvent(false);
            string contents = string.Empty;

            _content = content;


            var u = new UriBuilder(serverURL);
            u.Path += relativeUrl;
            u.Query = GetQuery(parameters);

            var request = HttpWebRequest.Create(u.Uri);
            request.Method = "POST";

            if (contentType != null)
                request.ContentType = contentType;

            try
            {



                IAsyncResult result = request.BeginGetRequestStream(callback =>
                {
                    var endRequest = (HttpWebRequest)callback.AsyncState;
                    var postStream = (Stream)request.EndGetRequestStream(callback);

                    using (var reader = new StreamReader(content))
                    {
                        string postData = reader.ReadToEnd();
                        byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                        // Write to the request stream.
                        postStream.Write(byteArray, 0, postData.Length);
                        postStream.Dispose();

                        endRequest.BeginGetResponse(callback2 =>
                            {
                                HttpWebRequest request2 = (HttpWebRequest)callback2.AsyncState;

                                // End the operation
                                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callback2);
                                Stream streamResponse = response.GetResponseStream();
                                StreamReader streamRead = new StreamReader(streamResponse);
                                contents = streamRead.ReadToEnd();
                                // Close the stream object
                                allDone.Set();

                            }, endRequest);

                        allDone.WaitOne();

                    }
                    allDone.Set();


                }, request);

                allDone.WaitOne();
                
                return contents;
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
                throw new SolrConnectionException(msg, e, request.RequestUri.ToString());
            }
        }

        public string Get(string relativeUrl, IEnumerable<KeyValuePair<string, string>> parameters)
        {
            var u = new UriBuilder(serverURL);
            u.Path += relativeUrl;
            u.Query = GetQuery(parameters);
            //u.Query += @"&wt=json";

            HttpWebRequest request = HttpWebRequest.CreateHttp(u.Uri);
            request.Method = "GET";

            try
            {
                var response = GetResponse(request);
                return response.Data;

            }
            catch (WebException e)
            {
                throw new SolrConnectionException(e, u.Uri.ToString());
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

        /// <summary>
        /// Gets http response, returns (etag, data)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private SolrResponse GetResponse(HttpWebRequest request)
        {
            AutoResetEvent allDone = new AutoResetEvent(false);
            SolrResponse response = new SolrResponse();

            var endResponse = request.BeginGetResponse(callback =>
            {
                var endRequest = (HttpWebRequest)callback.AsyncState;
                var endGetResponse = (HttpWebResponse)request.EndGetResponse(callback);

                using (var stream = endGetResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    response = new SolrResponse(null, reader.ReadToEnd());
                    allDone.Set();
                }



            }, request);

            allDone.WaitOne();

            return response;
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