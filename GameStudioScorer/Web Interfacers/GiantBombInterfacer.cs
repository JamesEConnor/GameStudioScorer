using System;
using GameStudioScorer.Extensions;
using GameStudioScorer.XML;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;
using System.Configuration;

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

			//Check to see if there's a cached value, unless we are forcibly retrieving
			//new values.
			StudioInfo si;
			if (!DEBUG)
			{
				si = LocalCacheManager.GetCachedInfo(name);
				si.alias = name;
				if (si.id != "-1")
					return si;
			}

			//Get the employee count from Wikipedia.
			int employeeCount = Extensions.Extensions.GetEmployeeCount(name);
			//Get information on release dates of games.
			string[] gameInfo = GetGBInfo(name);

			//Change the game info into years, excluding the last two entries,
			//since they're different pieces of information.
			int[] gameYears = new int[gameInfo.Length - 2];

			//Parse all of the actual years.
			for (int a = 0; a < gameInfo.Length - 2; a++)
			{
				gameYears[a] = int.Parse(gameInfo[a]);
			}

			//Sort them together in ascending order.
			Array.Sort(gameYears);

			//Return the new information, including the ID and name from game info.
			si = new StudioInfo
			{
				id = gameInfo[gameInfo.Length - 2],
				name = gameInfo[gameInfo.Length - 1],
				employeeCount = employeeCount,
				GameYears = gameYears,
				alias = name
			};

			//LocalCacheManager.SaveCachedInfo(si);
			return si;
		}

		/// <summary>
		/// Get the information for a studio from Giantbomb's API.
		/// </summary>
		/// <returns>An array of studio information, where [[0] - [n - 2]) are years
		/// of game releases, [n - 2] is the studio's ID, and [n - 1] is the name.</returns>
		/// <param name="name">Name.</param>
		public static string[] GetGBInfo(string name)
		{
			//A request to the Giantbomb API to search for the studio.
			string url = "https://www.giantbomb.com/api/companies/?api_key=" +
				API_KEY + "&filter=name:" + Uri.EscapeDataString(name);

			//Make a web request and get the result.
			string text = MakeRequest(url);
			//Deserialize the result from XML.
			Companies[] companies = XmlHandler.DeserializeCompanies(text);


			//A request to the Giantbomb API for the studio's specifics.
			url = "https://www.giantbomb.com/api/company/" +
				companies[0].guid +
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
			}
			foreach (string id in company.published_games)
			{
				if (!addedIDs.Contains(id))
				{
					url += id + "|";
					addedIDs.Add(id);
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
			result.Add(company.name);

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

			//Return result
			return text;
		}
	}
}
