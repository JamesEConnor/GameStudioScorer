using System.Net;
using System.Linq;
using GameStudioScorer.Extensions;
using unirest_net.http;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using GameStudioScorer.JSON;

namespace GameStudioScorer.IGDB
{
	public class IGDBInterfacer
	{
		public const string API_KEY = "***REMOVED***";

		public static int[] GetGenres(string name)
		{
			HttpResponse<string> company_response = Unirest.post("https://api-v3.igdb.com/companies")
				   .header("user-key", API_KEY)
				   .header("Accept", "application/json")
				   .body("fields name,id,developed.genres,published.genres; where name ~*\"" + name.ToLower() + "\";")
				   .asString();

			Company comp = JsonHandler.DeserializeCompany(company_response.Body);

			List<int> genres = new List<int>();
			foreach (Game g in comp.developed)
				genres.Add(EvaluateGenres(g.genres));
			foreach (Game g in comp.published)
				genres.Add(EvaluateGenres(g.genres));

			return genres.ToArray();
		}

		public static int EvaluateGenres(int[] genres)
		{
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

			return genre_points.MaxIndex();
		}
	}
}

namespace GameStudioScorer
{
	public struct StudioInfo
	{
		public string id;
		public string name;
		public int[] GameYears;
		public int employeeCount;

		public float CrunchOvertimeScore
		{
			get
			{
				return Crunch.CrunchScorer.GetCrunchOvertimeScore(GameYears, employeeCount);
			}
		}

		public float GenreScore;
	}
}