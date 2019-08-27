using System;
using GameStudioScorer.Extensions;
using GameStudioScorer.XML;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Xml;
using System.Collections.Generic;

namespace GameStudioScorer.Giantbomb
{
	public class GiantBombInterfacer
	{
		public const string API_KEY = "***REMOVED***";

		public static StudioInfo GetStudio(string name, bool DEBUG)
		{
			StudioInfo si;
			if (!DEBUG)
			{
				si = LocalCacheManager.GetCachedInfo(name);
				si.alias = name;
				if (si.id != "-1")
					return si;
			}

			int employeeCount = Extensions.Extensions.GetEmployeeCount(name);
			string[] gameInfo = GetGBInfo(name);

			int[] gameYears = new int[gameInfo.Length - 2];

			for (int a = 0; a < gameInfo.Length - 2; a++)
			{
				gameYears[a] = int.Parse(gameInfo[a]);
			}

			Array.Sort(gameYears);

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

		public static string[] GetGBInfo(string name)
		{
			string url = "https://www.giantbomb.com/api/companies/?api_key=" +
				API_KEY + "&filter=name:" + Uri.EscapeDataString(name);

			string text = MakeRequest(url);
			Companies[] companies = XmlHandler.DeserializeCompanies(text);


			url = "https://www.giantbomb.com/api/company/" +
				companies[0].guid +
				"/?api_key=" +
				API_KEY;

			text = MakeRequest(url);
			Company company = XmlHandler.DeserializeCompany(text);




			url = "https://www.giantbomb.com/api/games/?api_key=" +
				API_KEY + "&filter=id:";

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

			text = MakeRequest(url);
			Games[] games = XmlHandler.DeserializeGames(text);


			List<string> usedGames = new List<string>();
			List<string> result = new List<string>();
			for (int a = 0; a < games.Length; a++)
			{
				if (!usedGames.Contains(games[a].name))
				{
					if(games[a].expected_release_year.ToString().Length >= 4)
						result.Add(games[a].expected_release_year.ToString());
					else
						result.Add(games[a].original_release_date.Year.ToString());
					usedGames.Add(games[a].name);
				}
			}
			result.Add(company.guid);
			result.Add(company.name);

			return result.ToArray();
		}

		private static string MakeRequest(string url)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.UserAgent = "GameStudioScorer";
			WebResponse response = request.GetResponse();

			Stream stream = response.GetResponseStream();
			StreamReader reader = new StreamReader(stream);
			string text = reader.ReadToEnd();

			reader.Close();
			reader.Dispose();
			stream.Close();
			stream.Dispose();

			response.Close();
			response.Dispose();

			return text;
		}
	}
}
