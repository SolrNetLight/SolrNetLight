using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SolrNetLight.Exceptions;
using SolrNetLight.Utils;

namespace SolrNetLight.Impl
{
	/// <summary>
	/// Manages HTTP connection with Solr, uses POST request instead of GET in order to handle large requests
	/// </summary>
	public class PostSolrConnection : ISolrConnection
	{
		private readonly ISolrConnection conn;
		private readonly string serverUrl;

		public PostSolrConnection(ISolrConnection conn, string serverUrl)
		{
			this.conn = conn;
			this.serverUrl = serverUrl;
		}

		public async Task<string> Post(string relativeUrl, string s)
		{
			return await conn.Post(relativeUrl, s);
		}

		public async Task<string> Get(string relativeUrl, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			var u = new UriBuilder(serverUrl);
			u.Path += relativeUrl;
			var request = (HttpWebRequest)WebRequest.Create(u.Uri);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			var qs = string.Join("&", parameters
				.Select(kv => string.Format("{0}={1}", HttpUtility.UrlEncode(kv.Key), HttpUtility.UrlEncode(kv.Value)))
				.ToArray());

            try
            {
                HttpClient httpClient = new HttpClient();
                StringContent queryString = new StringContent(qs);
                HttpResponseMessage response = await httpClient.PostAsync(u.Uri, queryString);

                return await response.Content.ReadAsStringAsync();

            }
            catch (WebException e)
            {
                throw new SolrConnectionException(e);
            }
		}

		public async Task<string> PostStream(string relativeUrl, string contentType, System.IO.Stream content, IEnumerable<KeyValuePair<string, string>> getParameters) {
			return await conn.PostStream(relativeUrl, contentType, content, getParameters);
		}


       
    }
}
