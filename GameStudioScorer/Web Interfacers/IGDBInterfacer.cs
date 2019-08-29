using System.Net;
using System.Linq;
using GameStudioScorer.Extensions;
using unirest_net.http;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using GameStudioScorer.Crunch;
using System.Configuration;
using GameStudioScorer.Utils;
using System.Text.RegularExpressions;

namespace GameStudioScorer.IGDB
{
	public class IGDBInterfacer
	{
		//The API Key for the IGDB Interfacer. It just occurred to me that I need to move this to the config file.
		public static string API_KEY = "";

		/// <summary>
		/// Gets the genres of all games released by a company.
		/// </summary>
		/// <returns>The genres, with values between 0 and 6 (inclusive).</returns>
		/// <param name="name">The name of the game Studio.</param>
		public static int[] GetGenres(string name)
		{
			//Gets the API Key from the app.config file.
			if (API_KEY == "")
				API_KEY = ConfigurationManager.AppSettings["IGDBkey"];

			//Make a request to the IGDB company endpoint. This will search for the company
			//and return the name, id, and genres of it's games.
			HttpResponse<string> company_response = Unirest.post("https://api-v3.igdb.com/companies")
				   .header("user-key", API_KEY)
				   .header("Accept", "application/json")
				   .body("fields name,id,developed.genres,published.genres; where name ~*\"" + name.ToLower() + "\";")
				   .asString();

			//Handle an edge case where an ID from IGDB is not formatted into a Game JSON object.
			string regex = Regex.Replace(company_response.Body, "\\s{2,}", "");
			MatchCollection collection = Regex.Matches(regex, "(?<=},)\\d+,?");
			foreach (Match match in collection)
				company_response.Body = company_response.Body.Replace(match.Value, "");

			//Deserializes the response from Json to a list of companies.
			List<Company> list = (List<Company>)JsonConvert.DeserializeObject(company_response.Body, typeof(List<Company>));

			//Get all of the available genres, changes them to values between 0 and 6, and stores them.
			List<int> genres = new List<int>();

			if (list.Count <= 0)
				return null;

			//Developed Games
			if(list[0].developed != null)
				foreach (Game g in list[0].developed)
					if(g.genres != null)
						genres.Add(EvaluateGenres(g.genres));

			//Published Games
			if(list[0].published != null)
				foreach (Game g in list[0].published)
					if(g.genres != null)
						genres.Add(EvaluateGenres(g.genres));

			return genres.ToArray();
		}

		/// <summary>
		/// Takes in the genres as denoted by IGDB genre codes and returns a value.
		/// </summary>
		/// <returns>A number representing a game's genre. 0 - 6.</returns>
		/// <param name="genres">The genres provided by IGDB.</param>
		public static int EvaluateGenres(int[] genres)
		{
			//These are all explicitly provided genres represented by specific genre
			//codes in the IGDB. 'Action' is not given it's own code. If it belongs
			//to a specific genre, return that.
			//9 = Puzzle
			//12 = RPG
			//13 = Simulator
			//14 = Sport
			//15 = Strategy
			//31 = Adventure
			foreach (int i in genres)
			{
				if (i == 9)
					return 1;
				else if (i == 12)
					return 2;
				else if (i == 13)
					return 3;
				else if (i == 14)
					return 4;
				else if (i == 15)
					return 5;
				else if (i == 31)
					return 6;
			}

			//If it doesn't belong to a specific genre, check all the ones provided
			//and figure out which one it should belong to.
			//Action, Puzzle, RPG, Simulator, Sport, Strategy, Adventure
			int[] genre_points = new int[7];
			foreach (int i in genres)
			{
				if (i == 4 || i == 5 || i == 25 || i == 33)
					genre_points[0]++;
				else if (i == 2 || i == 7 || i == 8 || i == 26 || i == 30)
					genre_points[1]++;
				else if (i == 10)
					genre_points[4]++;
				else if (i == 11 || i == 16 || i == 24)
					genre_points[5]++;
				else if (i == 32)
					genre_points[6]++;
			}

			//Whichever genre the game most likely belongs to, return it.
			return genre_points.MaxIndex();
		}

		/* IGDB Genre Codes:
		 	2	Point and Click 			(Puzzle)
			4	Fighting					(Action)
			5	Shooter						(Action)
			7	Music						(Puzzle)
			8	Platform					(Puzzle)
			9	Puzzle						(SPECIFIC GENRE)
			10	Racing						(Sports)
			11	Real-Time Strategy			(Strategy)
			12	RPG							(SPECIFIC GENRE)
			13	Simulator					(SPECIFIC GENRE)
			14	Sport						(SPECIFIC GENRE)
			15	Strategy					(SPECIFIC GENRE)
			16	Turn-Based Strategy			(Strategy)
			24	Tactical					(Strategy)
			25	Hack and Slash/Beat'em Up	(Action)
			26	Quiz/Trivia					(Puzzle)
			30	Pinball						(Puzzle)
			31	Adventure					(SPECIFIC GENRE)
			32	Indie						(Adventure) *Note: While I don't like this classification, it was the best one I could come up with.
			33	Arcade						(Action)
		 */
	}
}


//The Studio Info struct. This stores all the information as it's passed between scorers.
namespace GameStudioScorer
{
	public struct StudioInfo
	{
		//The id of the studio, as assigned by Giantbomb's API.
		public string id;
		//The name of the studio, as provided by Giantbomb's API.
		public string name;
		//The years of all released games, according to Giantbomb's API.
		public int[] GameYears;
		//The number of employees according to data scraped from Wikipedia.
		public int employeeCount;
		//The name of the studio, as stored in the local cache.
		public string alias;

		//The crunch over time score.
		public float CrunchOvertimeScore
		{
			get
			{
				//This just uses the years and employee count values.
				//Since it isn't actually cached and doesn't in itself require
				//a web request, we can simply call the function.
				return Crunch.CrunchScorer.GetCrunchOvertimeScore(GameYears, employeeCount);
			}
		}

		public float ReviewScore
		{
			get
			{
				//Since this uses a lengthy and expensive web request system,
				//we have to make sure we don't already have a cached value before
				//calling it.
				if (!_setReviewScore)
					_ReviewScore = Crunch.CrunchScorer.GetReviewScore(name);

				//At this point, one way or the other, the score has been set.
				_setReviewScore = true;
				return _ReviewScore;
			}
			set
			{
				//If you set it, make sure you keep track of that.
				_setReviewScore = true;
				_ReviewScore = value;
			}
		}

		private float _ReviewScore;
		private bool _setReviewScore;

		public float GenreScore
		{
			get
			{
				//Since this uses a lengthy and expensive web request system,
				//we have to make sure we don't already have a cached value before
				//calling it.
				if (!_setGenreScore)
					_GenreScore = CrunchScorer.GetGenreScore(name, alias, MainClass.DEBUG_MODE.Contains(alias));

				//At this point, one way or the other, the score has been set.
				_setGenreScore = true;
				return _GenreScore;
			}
			set
			{
				//If you set it, make sure you keep track of that.
				_setGenreScore = true;
				_GenreScore = value;
			}
		}

		private float _GenreScore;
		private bool _setGenreScore;
	}
}