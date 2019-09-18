using System;
using GameStudioScorer.Extensions;
using GameStudioScorer.XML;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Configuration;
using GameStudioScorer.Utils;
using System.Text.RegularExpressions;

namespace GameStudioScorer.Giantbomb
{
	public class GiantBombInterfacer
	{
		//Oh wow, another API Key that I have to move to the config file...
		public static string API_KEY = "";

		/// <summary>
		/// Gets studio information from the Giantbomb API
		/// </summary>
		/// <returns>A struct representing the various information.</returns>
		/// <param name="name">The name of the studio.</param>
		/// <param name="DEBUG">Is the studio in debug mode?</param>
		public static StudioInfo GetStudio(string name, bool DEBUG)
		{
			//If the API key is not set, load it from the app.config file.
			if (API_KEY == "")
				API_KEY = ConfigurationManager.AppSettings["GBkey"];

			//Logging
			if (Logger.VERBOSE)
				Logger.Log(name + " in DEBUG mode: [" + DEBUG.ToString().ToUpper() + "]");

			//Check to see if there's a cached value, unless we are forcibly retrieving
			//new values.
			StudioInfo si;
			if (!MainClass.options.force)
			{
				si = LocalCacheManager.GetCachedInfo(name);
				si.aliases = Extensions.Extensions.CreateAliasList(name);

				if (si.id != "-1")
					return si;
			}

			//Get the employee count from Wikipedia.
			int employeeCount = Extensions.Extensions.GetEmployeeCount(name);

			//Get an alternative topic from Wikipedia.
			string wikiName = Wiki.WikipediaRetriever.GetActualTopic(name);

			name = Regex.Replace(name, "\\(company\\)", "", RegexOptions.IgnoreCase).Trim();

			//Get information on release dates of games.
			string[] gameInfo = GetGBInfo(name, wikiName);

			//Change the game info into years, excluding the last two entries,
			//since they're different pieces of information.
			int[] gameYears = new int[gameInfo.Length - 3];

			//Parse all of the actual years.
			for (int a = 0; a < gameInfo.Length - 3; a++)
			{
				gameYears[a] = int.Parse(gameInfo[a]);
			}

			//Sort them together in ascending order.
			Array.Sort(gameYears);

			//Return the new information, including the ID and name from game info.
			si = new StudioInfo
			{
				id = gameInfo[gameInfo.Length - 3],
				name = gameInfo[gameInfo.Length - 2],
				companyName = gameInfo[gameInfo.Length - 1],
				employeeCount = employeeCount,
				gameYears = gameYears,
				aliases = Extensions.Extensions.CreateAliasList(name)
			};

			si.aliases.Insert(0, si.companyName);

			//LocalCacheManager.SaveCachedInfo(si);
			return si;
		}

		/// <summary>
		/// Get the information for a studio from Giantbomb's API.
		/// </summary>
		/// <returns>An array of studio information, where [[0] - [n - 2]) are years
		/// of game releases, [n - 2] is the studio's ID, and [n - 1] is the name.</returns>
		/// <param name="name">Name.</param>
		public static string[] GetGBInfo(string name, string wikiName)
		{
			name = name.Replace("(company)", "").TrimEnd(new char[] { ' ' });

			//A request to the Giantbomb API to search for the studio.
			string url = "https://www.giantbomb.com/api/companies/?api_key=" +
				API_KEY + "&filter=name:" + Uri.EscapeDataString(name);

			//Make a web request and get the result.
			string text = MakeRequest(url);
			//Deserialize the result from XML.
			Companies[] companies = XmlHandler.DeserializeCompanies(text);

			//If no companies were returned, try some aliases.
			if (companies.Length == 0)
			{
				List<string> aliases = Extensions.Extensions.CreateAliasList(name);
				aliases.Insert(0, wikiName);

				foreach (string alias in aliases)
				{
					url = "https://www.giantbomb.com/api/companies/?api_key=" +
						API_KEY + "&filter=name:" + Uri.EscapeDataString(alias);

					//Make a web request and get the result.
					text = MakeRequest(url);
					//Deserialize the result from XML.
					companies = XmlHandler.DeserializeCompanies(text);

					if (companies.Length > 0)
						break;
				}

				if (companies.Length == 0)
					throw new Exception(name + " doesn't exist on Giantbomb!");
			}

			//Handles an edge case where the first company listed in the Giantbomb response is not the actual company.
			int index = 0;
			for (int a = 0; a < companies.Length; a++)
			{
				if (companies[a].name.ToLower() == name.ToLower())
					index = a;
			}

			/*int lowestDistance = int.MaxValue;
			int lowestIndex = 0;
			for (int a = 0; a < companies.Length; a++)
			{
				int diff = Extensions.Extensions.LevenshteinDistance(name, companies[a].name);
				if (diff < lowestDistance)
				{
					lowestDistance = diff;
					lowestIndex = a;
				}
			}*/

			//A request to the Giantbomb API for the studio's specifics.
			url = "https://www.giantbomb.com/api/company/" +
				companies[index].guid +
				"/?api_key=" +
				API_KEY;

			//Make a request and store the result.
			text = MakeRequest(url);
			//Deserialize the result from XML.
			Company company = XmlHandler.DeserializeCompany(text);




			//A request to the Giantbomb API for the game release years.
			url = "https://www.giantbomb.com/api/games/?api_key=" +
				API_KEY + "&filter=id:";

			//Add in all of the necessary game IDs.
			List<string> addedIDs = new List<string>();
			foreach (string id in company.developed_games)
			{
				if (!addedIDs.Contains(id))
				{
					url += id + "|";
					addedIDs.Add(id);
				}

				//Giantbomb API restricts requests to 100 results.
				if (addedIDs.Count >= 100)
					break;
			}

			//Giantbomb API resitricts requests to 100 results.
			if (addedIDs.Count < 100)
			{
				foreach (string id in company.published_games)
				{
					if (!addedIDs.Contains(id))
					{
						url += id + "|";
						addedIDs.Add(id);
					}

					//Giantbomb API restricts requests to 100 results.
					if (addedIDs.Count >= 100)
						break;
				}
			}
			url = url.Substring(0, url.Length - 1);

			//Make the request.
			text = MakeRequest(url);
			//Deserialize the result from XML.
			Games[] games = XmlHandler.DeserializeGames(text);

			//Get all games and add them, taking care to not repeat any.
			List<string> usedGames = new List<string>();
			List<string> result = new List<string>();
			for (int a = 0; a < games.Length; a++)
			{
				if (!usedGames.Contains(games[a].name))
				{
					//This is because sometimes the release year is "expected release year"
					//and sometimes it's "original release date".
					if(games[a].expected_release_year.ToString().Length >= 4)
						result.Add(games[a].expected_release_year.ToString());
					else
						result.Add(games[a].original_release_date.Year.ToString());
					usedGames.Add(games[a].name);
				}
			}

			result.Add(company.guid);

			result.Add(company.name.removeCompanySuffix());
			result.Add(company.name);
			Console.WriteLine(company.name.removeCompanySuffix());

			//Return the game information, including the years, ID, and studio name.
			return result.ToArray();
		}

		/// <summary>
		/// Make a request to a URL.
		/// </summary>
		/// <returns>The request's response text.</returns>
		/// <param name="url">The URI to make a request to.</param>
		private static string MakeRequest(string url)
		{
			//Create a request and set the User Agent.
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "GameStudioScorer";
			WebResponse response = request.GetResponse();

			//Get the response text.
			Stream stream = response.GetResponseStream();
			StreamReader reader = new StreamReader(stream);
			string text = reader.ReadToEnd();

			//Clean up
			reader.Close();
			reader.Dispose();
			stream.Close();
			stream.Dispose();

			response.Close();
			response.Dispose();

			//Check status code
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(text);
			string status = doc.SelectSingleNode("response/status_code").Value;

			//Error handling
			switch (status)
			{
				case "100": //Invalid API Key
					throw new Exception("Giantbomb Error: Invalid API key. Make sure you have it setup correctly in the app.config file. (Check the ReadMe for more info)");
				case "101": //Object not found
					throw new Exception("Giantbomb Error: Object not found. Something went wrong.");
				case "102": //Error in URL format
					throw new Exception("Giantbomb Error: Error in URL format. Check your spelling.");
				case "104": //Filter error
					throw new Exception("Giantbomb Error: Filter error. Check your spelling.");
			}

			//Return result
			return text;
		}
	}
}
