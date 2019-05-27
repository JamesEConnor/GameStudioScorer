﻿using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace GameStudioScorer.Wiki
{
	public class WikipediaRetriever
	{
		public static Dictionary<string, string> GetWikiInfo(string topic)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			string html = GetHTMLCode("https://en.wikipedia.org/wiki/" + topic);
			string sidebar = html.Split(new string[] { "<tbody>", "</tbody>" }, StringSplitOptions.None)[1];

			string[] split = sidebar.Split(new string[] { "<tr>", "</tr>" }, StringSplitOptions.None);

			foreach (string str in split)
			{
				string s = Regex.Replace(str, "[ ]", "_");
				s = Regex.Replace(s, "<.*?>", " ");
				s = Regex.Replace(s, "[ ]+", " ");
				if (s.IndexOf(" ", StringComparison.Ordinal) == 0)
					s = s.Substring(1);

				if (s.Trim().Length != 0)
				{
					string[] elements = s.Split(new char[] { ' ' }, 2);
					elements[1] = Regex.Replace(elements[1], "&#[0-9]+;", " ");
					result.Add(elements[0], elements[1]);
				}
			}

			return result;
		}

		public static string GetHTMLCode(string uri)
		{
			HttpWebRequest http = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)http.GetResponse();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				Stream stream = response.GetResponseStream();
				StreamReader readStream = null;

				if (response.CharacterSet == null)
				{
					readStream = new StreamReader(stream);
				}
				else
				{
					readStream = new StreamReader(stream, Encoding.GetEncoding(response.CharacterSet));
				}

				string html = readStream.ReadToEnd();

				readStream.Close();
				readStream.Dispose();

				stream.Close();
				stream.Dispose();

				return html;
			}
			else
			{
				throw new Exception("Error occurred while processing web request: " + response.StatusCode);
			}

			response.Close();
			response.Dispose();
		}
	}
}
