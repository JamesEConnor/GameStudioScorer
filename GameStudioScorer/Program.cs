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
		static string[] GAME_STUDIOS = { "Rockstar Games" };
		static string[] DEBUG_MODE = { };

		public static void Main(string[] args)
		{
			List<KeyValuePair<string, float>> scores = GetScores(GAME_STUDIOS);
			foreach (KeyValuePair<string, float> s in scores)
			{
				Console.WriteLine("{0, -20}: {1, -5}", s.Key, s.Value);
			}
		}

		public static List<KeyValuePair<string, float>> GetScores(string[] studios)
		{
			Dictionary<string, float> dict = new Dictionary<string, float>();
			foreach (string studio in studios)
			{
				StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio, DEBUG_MODE.Contains(studio));
				//si.GenreScore = CrunchScorer.GetGenreScore(si.name, studio, DEBUG_MODE.Contains(studio));

				//TODO Eventually need to make this use logarithmic regression
				dict.Add(studio, si.ReviewScore);

				si.name = studio;
				if (LocalCacheManager.GetCachedInfo(studio).id == "-1" || DEBUG_MODE.Contains(studio))
					LocalCacheManager.SaveCachedInfo(si);
			}

			return dict.OrderByDescending(x => x.Value).ToList();
		}
	}
}
