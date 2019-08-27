using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GameStudioScorer.Crunch;
using GameStudioScorer.Extensions;

namespace GameStudioScorer
{
	class MainClass
	{
		//The Game Studios to get a score for.
		public static string[] GAME_STUDIOS = { "Rockstar Games" };

		//Should any of the Game Studios be used in Debug Mode?
		//Copy them from the GAME_STUDIOS array. They *must* be in both.
		//This will cause them to ignore any cached values and forcibly recalculate
		//a score. This is good if they're outdated.
		public static string[] DEBUG_MODE = { };

		public static void Main(string[] args)
		{
			//Prints scores.
			List<KeyValuePair<string, float[]>> scores = GetScores(GAME_STUDIOS);
			foreach (KeyValuePair<string, float[]> s in scores)
			{
				Console.WriteLine("{0, -20}: {1, -5}, {2, 10}, {3, 25}", s.Key, s.Value[0], s.Value[1], s.Value[2]);
			}
		}

		public static List<KeyValuePair<string, float[]>> GetScores(string[] studios)
		{
			Dictionary<string, float[]> dict = new Dictionary<string, float[]>();
			foreach (string studio in studios)
			{
				//Gets a StudioInfo object, containing all sorts of goodies.
				StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio, DEBUG_MODE.Contains(studio));

				//TODO Eventually need to make this use logarithmic regression
				//Add the different values to the dictionary.
				dict.Add(studio, new float[]{ 
					si.CrunchOvertimeScore, si.GenreScore, si.ReviewScore
				});

				//If we force-calculated new values or if it doesn't already exist,
				//cache the Studio.
				if (DEBUG_MODE.Contains(studio) || LocalCacheManager.GetCachedInfo(studio).id == "-1")
					LocalCacheManager.SaveCachedInfo(si);
			}

			//Order things so it looks nice.
			return dict.OrderByDescending(x => x.Value[0]).ToList();
		}
	}
}
