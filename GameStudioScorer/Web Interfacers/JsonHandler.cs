using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameStudioScorer.IGDB;

namespace GameStudioScorer.JSON
{
	public class JsonHandler
	{
		public static Company DeserializeCompany(string json)
		{
			Company result = new Company();
			Match comp_id = Regex.Match(json, "(?<=\"id\": )\\d+(?=,)");
			Match comp_name = Regex.Match(json, "(?<=\"name\": \").+ (?=\")");
			string json_developed = Regex.Match(json, "(?<=\"developed\": \\[).+(?=],)", RegexOptions.Singleline).Value;
			string json_published = Regex.Match(json, "(?<=\"published\": \\[).+(?=])", RegexOptions.Singleline).Value;

			Console.WriteLine(json_developed);

			MatchCollection developed_ids = Regex.Matches(json_developed, "(?<=\"id\": )\\d+(?=,)");
			MatchCollection developed_genres = Regex.Matches(json_developed, "(?<=\"genres\": \\[).+?(?=\\])", RegexOptions.Singleline);

			MatchCollection published_ids = Regex.Matches(json_published, "(?<=\"id\": )\\d+(?=,)");
			MatchCollection published_genres = Regex.Matches(json_published, "(?<=\"genres\": \\[).+?(?=\\])", RegexOptions.Singleline);

			result.id = int.Parse(comp_id.Value);
			result.name = comp_name.Value;

			List<Game> games = new List<Game>();
			for (int a = 0; a < developed_ids.Count; a++)
			{
				MatchCollection genre_ids = Regex.Matches(developed_genres[a].Value, "\\d+");
				int[] genres = new int[genre_ids.Count];
				for (int b = 0; b < genres.Length; b++)
					genres[b] = int.Parse(genre_ids[b].Value);

				games.Add(new Game
				{
					id = int.Parse(developed_ids[a].Value),
					genres = genres
				});
			}

			result.developed = games.ToArray();

			games.Clear();
			for (int a = 0; a < published_ids.Count; a++)
			{
				MatchCollection genre_ids = Regex.Matches(published_genres[a].Value, "\\d+");
				int[] genres = new int[genre_ids.Count];
				for (int b = 0; b < genres.Length; b++)
					genres[b] = int.Parse(genre_ids[b].Value);

				games.Add(new Game
				{
					id = int.Parse(published_ids[a].Value),
					genres = genres
				});
			}

			result.published = games.ToArray();

			return result;
		}
	}
}
