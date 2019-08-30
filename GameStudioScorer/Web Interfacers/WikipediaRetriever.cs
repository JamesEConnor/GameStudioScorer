using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GameStudioScorer.Utils;

namespace GameStudioScorer.Wiki
{
	public class WikipediaRetriever
	{
		/// <summary>
		/// Uses Wikipedia and follows redirects to find the actual topic name.
		/// </summary>
		/// <returns>The name for the Wikipedia article.</returns>
		/// <param name="topic">The original topic to follow redirects for.</param>
		public static string GetActualTopic(string topic)
		{
			//Keeps track of whether the program was redirected.
			bool redirect;

			string[] lines;

			//Get the raw text for the page, and go through each line.
			do
			{
				redirect = false;

				string html = GetHTMLCode("https://en.wikipedia.org/wiki/" + topic + "?action=raw");
				lines = html.Split('\n');

				//Handle Wikipedia topic redirect.
				if (lines[0].StartsWith("#redirect", StringComparison.CurrentCultureIgnoreCase))
				{
					topic = Regex.Replace(lines[0], "#redirect", "", RegexOptions.IgnoreCase)
									.Trim();
					topic = topic.Substring(2, topic.Length - 4);

					redirect = true;
				}
			} while (redirect);

			return topic;
		}

		/// <summary>
		/// Gets information from the sidebar of a Wikipedia page.
		/// </summary>
		/// <returns>A dictionary, where the key is the label and the value is the information.</returns>
		/// <param name="topic">The last part of a Wikipedia URL. For instance, in the
		/// url "https://www.wikipedia.com/wiki/Rockstar_Games", the topic is
		/// "Rockstar_Games".</param>
		public static Dictionary<string, string> GetWikiInfo(string topic)
		{
			Dictionary<string, string> result = new Dictionary<string, string>();

			string[] lines;

			//Keeps track of whether the program was redirected.
			bool redirect;

			//Get the raw text for the page, and go through each line.
			do
			{
				redirect = false;

				string html = GetHTMLCode("https://en.wikipedia.org/wiki/" + topic + "?action=raw");
				lines = html.Split('\n');

				//Handle Wikipedia topic redirect.
				if (lines[0].StartsWith("#redirect", StringComparison.CurrentCultureIgnoreCase))
				{
					topic = Regex.Replace(lines[0], "#redirect", "", RegexOptions.IgnoreCase)
					                .Trim();
					topic = topic.Substring(2, topic.Length - 4);

					if(Logger.VERBOSE)
						Logger.Log("Wikipedia redirect, new topic: " + topic, Logger.LogLevel.DEBUG, true);
					
					redirect = true;
				}
			} while (redirect);


			foreach (string str in lines)
			{
				//If it starts with a '|', it's part of the sidebar. If it contains '='
				//that means it's a new label.
				if (str.StartsWith("|", StringComparison.CurrentCulture) && str.Contains(" = "))
				{
					//Split between the equal sign and add the new information.
					string val = str.Remove(0, 2);
					string[] keyval = val.Split(new string[] { " = " }, 2, StringSplitOptions.None);

					if(!result.ContainsKey(keyval[0]))
						result.Add(keyval[0], keyval[1]);
				}
			}

			return result;
		}

		/// <summary>
		/// Makes a web request and returns the HTML.
		/// </summary>
		/// <returns>The HTML code from the URI.</returns>
		/// <param name="uri">The URI to get the code from.</param>
		public static string GetHTMLCode(string uri)
		{
			//Make the request and get the response.
			HttpWebRequest http = (HttpWebRequest)WebRequest.Create(uri);
			HttpWebResponse response = (HttpWebResponse)http.GetResponse();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				//If the request was valid, read the text.
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

				//Get the code.
				string html = readStream.ReadToEnd();

				//Clean up.
				readStream.Close();
				readStream.Dispose();

				stream.Close();
				stream.Dispose();

				response.Close();
				response.Dispose();

				//Return!
				return html;
			}
			else
			{
				//If the request wasn't valid, clean up and die.
				response.Close();
				response.Dispose();

				throw new Exception("Error occurred while processing web request: " + response.StatusCode);
			}
		}
	}
}
