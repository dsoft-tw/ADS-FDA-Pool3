﻿using System;
using System.Configuration;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace dsoft.ads.web.Models
{
	public class OpenFDAQuery
	{
		public enum FDAReportSource
		{
			drug,
			device,
			food
		}

		public enum FDAReportType
		{
			@event,
			label,
			enforcement
		}

		public OpenFDAQuery ()
		{
			this.baseUrl = ConfigurationManager.AppSettings ["baseUrl"];
			this.apiKey = ConfigurationManager.AppSettings["fdaApiKey"];

			this.querySearch = "";
			this.queryCount = "";
			this.queryLimit = 0;
			this.querySkip = 0;
		}

		#region Properties
		public string baseUrl { get; set; }
		public string apiKey { get; set; }

		public FDAReportSource source { get; set; }
		public FDAReportType type { get; set; }
		public string querySearch { get; set; }
		public string queryCount { get; set; }
		public int queryLimit { get; set; }
		public int querySkip { get; set; }
		public OpenFDAResponse response { get; set; }
		public string responseRaw { get; set; }
		#endregion

		private string GetUrl()
		{
			// TODO:  add validation and URL encoding based on user input options
			if (String.IsNullOrEmpty (baseUrl))
				return String.Empty;

			string url = String.Format("{0}/{1}/{2}.json?api_key={3}", baseUrl, source, type, apiKey);
			if (!String.IsNullOrEmpty(querySearch))
				url = String.Format("{0}&search={1}", url, querySearch);
			if (!String.IsNullOrEmpty(queryCount))
				url = String.Format("{0}&count={1}", url, queryCount);
			if (queryLimit > 0)
				url = String.Format("{0}&limit={1}", url, queryLimit);
			if (querySkip > 0)
				url = String.Format("{0}&skip={1}", url, querySkip);

			return url;
		}

		public bool RunQuery()
		{
			this.response = null;

			try
			{
				string url = GetUrl ();
				if (String.IsNullOrEmpty(url))
					throw new Exception("No URL specified.");
				
				WebRequest request = WebRequest.Create(url) as WebRequest;
				using (WebResponse response = request.GetResponse() as WebResponse)
				{
					if (response is HttpWebResponse)
					{
						HttpWebResponse testResponse = response as HttpWebResponse;
						if (testResponse.StatusCode != HttpStatusCode.OK)
							throw new Exception(String.Format("Server error (HTTP {0}: {1}).", testResponse.StatusCode, testResponse.StatusDescription));
					}

					Stream stream = response.GetResponseStream();
					StreamReader sr = new StreamReader(stream);
					string json = sr.ReadToEnd();
					this.responseRaw = json;
					this.response = JsonConvert.DeserializeObject<OpenFDAResponse> (json);
					this.response.SetStateLists();
					return true;
				}
			}
			catch (Exception ex)
			{
				// TODO: log
			}

			return false;
		}
	}
}

