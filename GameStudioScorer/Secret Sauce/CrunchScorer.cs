using System;
using System.Collections.Generic;
using GameStudioScorer.Utils;
using GameStudioScorer.IGDB;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace GameStudioScorer.Crunch
{
	public class CrunchScorer
	{
		public const string SEARCH_SCRAPER_PATH = "glassdoor-search-scraper";
		public const string REVIEW_SCRAPER_PATH = "glassdoor-review-scraper";

		public static float[] GENRE_SCORES = new float[] { 0.48f, 0.12f, 0.40f, 0.08f, 0.40f, 0.32f, 0.20f };

		public static float GetCrunchOvertimeScore(int[] years, int employeeCount)
		{
			List<float> yearsF = new List<float>();
			for (int a = 0; a < years.Length; a++)
			{
				if (years[a].ToString().Length >= 4)
					yearsF.Add(years[a]);
			}

			float[] inputs = MathUtils.GenInputs(yearsF.Count);

			BestFit bf = MathUtils.ExpRegression(inputs, yearsF.ToArray());
			ExponentialEquation exp = (ExponentialEquation)bf.equation;

			LogarithmicEquation log = new LogarithmicEquation(1/exp.A, exp.r, employeeCount);
			//Console.WriteLine(log.a + ", " + log.b + ", " + log.c);
			const int length = 20;
			return (MathUtils.LogarithmicIntegral(log, 1f, length + 1)/(length * log.GetValue(length + 1)) - 0.5f) * 2;
		}

		public static float GetGenreScore(string name, string savedName, bool DEBUG)
		{
			StudioInfo si = LocalCacheManager.GetCachedInfo(savedName);
			if (si.id != "-1" && !DEBUG)
				return si.GenreScore;

			try
			{
				int[] genres = IGDBInterfacer.GetGenres(savedName);
				float total = 0.0f;
				foreach (int i in genres)
					total += GENRE_SCORES[i];

				return total / genres.Length;
			}
			catch(Exception e)
			{
				int[] genres = IGDBInterfacer.GetGenres(name);
				float total = 0.0f;
				foreach (int i in genres)
					total += GENRE_SCORES[i];

				return total / genres.Length;
			}
		}

		public static float GetReviewScore(string studioName)
		{
			string searchScraper = AppDomain.CurrentDomain.BaseDirectory + SEARCH_SCRAPER_PATH;
			string reviewScraper = AppDomain.CurrentDomain.BaseDirectory + REVIEW_SCRAPER_PATH;

			string searchCommand = "main.py --headless --name \"" + studioName + "\" --browser \"" + ConfigurationManager.AppSettings["browser"] + "\"";

			Process p = new Process();
			p.StartInfo = new ProcessStartInfo()
			{
				WorkingDirectory = searchScraper,
				FileName = "python.exe",
				Arguments = searchCommand,
				UseShellExecute = false,
				RedirectStandardOutput = true,
			};

			p.Start();

			while (!p.HasExited) {}

			Console.WriteLine("EXIT ONE");

			string currentLine = "";
			while (p.StandardOutput.Peek() > -1)
			{
				currentLine = p.StandardOutput.ReadLine();
			}

			if (currentLine == "null")
				return 0.5f;

			string reviewCommand = "main.py --headless --url \"" + currentLine + "\" -f reviews.csv --limit 40 --browser \"" + ConfigurationManager.AppSettings["browser"] + "\"";
			Console.WriteLine(reviewCommand);

			p = new Process();
			p.StartInfo = new ProcessStartInfo()
			{
				WorkingDirectory = reviewScraper,
				FileName = "python.exe",
				Arguments = reviewCommand,
				UseShellExecute = false,
				RedirectStandardOutput = true,
			};

			p.Start();

			while (!p.HasExited) {}

			float ratingTotal = 0.0f;

			string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/glassdoor-review-scraper/reviews.csv");
			for (int a = 1; a < lines.Length; a++)
			{
				ratingTotal += float.Parse(lines[a]);
			}

			ratingTotal /= lines.Length - 1;
			return ratingTotal / 5;
		}
	}
}

/*
 * 1165.3447
 * 532.27819
 * 733.8162
 * 489.28341
 */

/* 
 * 0.927385129854	Rockstar		Worst			0.9273852
 * 0.89632079363	Schell			Best			0.8963208
 * 0.92178463022	Epic			2nd Worst		0.9217846
 * 0.919793170921	Bioware			Middle			0.9197932
 * 0.920726941583	CD Projekt Red	2nd Best		0.920727
 */