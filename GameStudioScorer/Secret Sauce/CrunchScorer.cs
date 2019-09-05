using System;
using System.Collections.Generic;
using GameStudioScorer.Utils;
using GameStudioScorer.Extensions;
using GameStudioScorer.IGDB;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace GameStudioScorer.Crunch
{
	public class CrunchScorer
	{
		//The folder of the Glasdoor scrapers relative to the application directory.
		public const string SEARCH_SCRAPER_PATH = "glassdoor-search-scraper";
		public const string REVIEW_SCRAPER_PATH = "glassdoor-review-scraper";

		//The likelihood of crunch for the 7 major genres based on 50 different games.
		//Calculated externally.
		//Genres are: Action, Adventure, RPG, Simulation, Strategy, Sports, Puzzle
		public static float[] GENRE_SCORES = new float[] { 0.48f, 0.12f, 0.40f, 0.08f, 0.40f, 0.32f, 0.20f };


		/// <summary>
		/// Gets a score representing how likely a studio is to crunch based on how
		/// frequently they put out games. The more often they put out games, the more
		/// likely they are to crunch. However, it also accounts for employees. A studio
		/// putting out a game a year with 2000 staff is less likely to be crunching than
		/// the same for a studio with 20 staff.
		/// </summary>
		/// <returns>The crunch overtime score.</returns>
		/// <param name="years">The year values for release dates of all of a studio's games.</param>
		/// <param name="employeeCount">How many employees work at the studio.</param>
		public static float GetCrunchOvertimeScore(int[] years, int employeeCount)
		{
			//Log information
			if (Logger.VERBOSE)
				Logger.Log("Finding crunch over time score.");

			//This is to counteract a bug, where the games that haven't been released yet
			//are returned as '1's.
			List<float> yearsF = new List<float>();
			for (int a = 0; a < years.Length; a++)
			{
				if (years[a].ToString().Length >= 4)
					yearsF.Add(years[a]);
			}

			//Create a list of x-values. Basically just an array of incrementing values
			//that matches in length to the years.
			float[] inputs = MathUtils.GenInputs(yearsF.Count);

			//When plotted, the years form an exponential graph when sorted. The steeper
			//it is, the less frequently they put out games.
			BestFit bf = MathUtils.ExpRegression(inputs, yearsF.ToArray());
			ExponentialEquation exp = (ExponentialEquation)bf.equation;

			//Return the rate. The higher the rate (steeper curve), the less likely to crunch.
			//Takes into account the number of employees.
			float x = exp.r * employeeCount;
			return x;
		}

		/// <summary>
		/// Gets the likelihood of crunching based off the genres of various games
		/// a studio has put out.
		/// </summary>
		/// <returns>The genre score.</returns>
		/// <param name="name">The name of the Studio, which is passed to the IGDB API.</param>
		/// <param name="aliases">What it's saved as in the cache. This may differ from the name retrieved from the Giantbomb API.</param>
		/// <param name="DEBUG">Is this Studio in Debug mode?</param>
		public static float GetGenreScore(string name, List<string> aliases, bool DEBUG)
		{
			//Log information
			if (Logger.VERBOSE)
				Logger.Log("Finding genre score.");

			//If the Studio is not being forced to recaculate values, check if it
			//exists in the cache. If it does, return the values. Otherwise, continue.
			if (!DEBUG)
			{
				StudioInfo si = LocalCacheManager.GetCachedInfo(aliases[0]);
				if (si.id != "-1" && !DEBUG)
					return si.GenreScore;
			}

			//Get the genres of all released games from IGDB and return their average as the score.
			int[] genres = IGDBInterfacer.GetGenres(name);

			Logger.Log(name + ", " + aliases.ToArray().GetString(), Logger.LogLevel.DEBUG, true);

			//Uses a Unit Circle based system.
			float totalX = 0.0f;
			float totalY = 0.0f;

			if (genres != null && genres.Length > 0)
			{
				
				for (int i = 0; i < genres.Length; i++)
				{
					//Get the angle because it uses a unit-circle based system.
					//See the ReadMe or Whitepaper for more info.
					float angle = (float)((i / 7) * 2 * Math.PI);

					totalX += genres[i] * (float)Math.Cos(angle);
					totalY += genres[i] * (float)Math.Sin(angle);
				}
			}
			else
			{
				//If an exception was thrown, it means the name doesn't exist on IGDB.
				//Then we use the different aliases.

				bool broken = false;
				foreach (string alias in aliases)
				{
					Console.WriteLine(alias);
					genres = IGDBInterfacer.GetGenres(alias);

					//This means this name doesn't exist either, so try the next one.
					if (genres == null || genres.Length <= 0)
						continue;

					for (int i = 0; i < genres.Length; i++)
					{
						//Get the angle because it uses a unit-circle based system.
						//See the ReadMe or Whitepaper for more info.
						float angle = (float)((i / 7) * 2 * Math.PI);

						totalX += genres[i] * (float)Math.Cos(angle);
						totalY += genres[i] * (float)Math.Sin(angle);
					}

					broken = true;
					break;
				}

				//If no genres were found, throw an exception.
				if(!broken)
					throw new Exception(name + " doesn't exist in IGDB!");
			}

			//Average
			totalX /= genres.Length;
			totalY /= genres.Length;

			//Normalize
			float hyp = (float)Math.Sqrt((totalX * totalX) + (totalY * totalY));

			totalX /= hyp;
			totalY /= hyp;

			//Return the value.
			return (float)(Math.Atan(totalY / totalX) / (2 * Math.PI));
		}

		/// <summary>
		/// Get the likelihood of crunching based off of Company Glassdoor reviews.
		/// </summary>
		/// <returns>The review score.</returns>
		/// <param name="studioName">The name of the Studio.</param>
		public static float[] GetReviewScore(string studioName)
		{
			//Log information
			if(Logger.VERBOSE)
				Logger.Log("Finding review score.");

			//Create the absolute paths to the two scrapers.
			string searchScraper = AppDomain.CurrentDomain.BaseDirectory + SEARCH_SCRAPER_PATH;
			string reviewScraper = AppDomain.CurrentDomain.BaseDirectory + REVIEW_SCRAPER_PATH;

			//Execute the search scraper. This finds the link to company reviews by
			//scraping the Glassdoor search page.
			string searchCommand = "main.py --headless --name \"" + studioName + "\" --browser \"" + ConfigurationManager.AppSettings["browser"] + "\"";

			Process p = new Process();
			p.StartInfo = new ProcessStartInfo()
			{
				WorkingDirectory = searchScraper,
				FileName = "python.exe",
				Arguments = searchCommand,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = !(MainClass.DEBUG_MODE || MainClass.options.verbose)
			};

			p.Start();


			//Logging
			if (Logger.VERBOSE)
				Logger.Log("Glassdoor search scraper started... (This may take up to a minute)");

			while (!p.HasExited) {}

			//Get the last line of output, which will be the link for the Studio's reviews.
			string currentLine = "";
			while (p.StandardOutput.Peek() > -1)
			{
				currentLine = p.StandardOutput.ReadLine();
			}

			if (currentLine == "null")
				return new float[] { 0.5f, 0.5f };

			//Execute the review scraper. This takes the link from the search scraper and
			//gets the overall ratings from the top 40 reviews, or as many reviews that exist.
			string reviewCommand = "main.py --headless --url \"" + currentLine + "\" -f reviews.csv --limit 40 --browser \"" + ConfigurationManager.AppSettings["browser"] + "\"";

			p = new Process();
			p.StartInfo = new ProcessStartInfo()
			{
				WorkingDirectory = reviewScraper,
				FileName = "python.exe",
				Arguments = reviewCommand,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = !(MainClass.DEBUG_MODE || MainClass.options.verbose)
			};

			//Logging
			if (Logger.VERBOSE)
				Logger.Log("Glassdoor review scraper started... (This may take up to a minute)");

			p.Start();

			while (!p.HasExited) {}

			//If an error occurs, just return 0.5 for each.
			currentLine = "";
			while (p.StandardOutput.Peek() > -1)
			{
				currentLine = p.StandardOutput.ReadLine();
			}

			if (currentLine == "null")
				return new float[] { 0.5f, 0.5f };


			//Add up all the ratings, average them, and then normalize them, since they're
			//all between 1 and 5.
			float ratingTotal = 0.0f;
			float conTotal = 0.0f;

			//Searches all of the cons for terms that would imply crunch is used.
			string[] lines = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "/glassdoor-review-scraper/reviews.csv");
			for (int a = 1; a < lines.Length; a++)
			{
				string[] split = lines[a].Split(',');

				split[0] = split[0].ToLower();
				if (split[0].Contains("overtime") || split[0].Contains("work life balance") || split[0].Contains("work-life balance") || split[0].Contains("crunch"))
					conTotal++;

				ratingTotal += float.Parse(split[1]);
			}

			//Average the values.
			ratingTotal /= lines.Length - 1;
			conTotal /= lines.Length - 1;

			//Return the values.
			return new float[] { conTotal, ratingTotal / 5};
		}
	}
}




//JUST SOME NOTES. IF I FORGOT TO TAKE THIS OUT IT'S MY BAD.

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