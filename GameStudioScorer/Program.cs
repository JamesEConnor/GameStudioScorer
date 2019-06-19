using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using GameStudioScorer.Crunch;

namespace GameStudioScorer
{
	class MainClass
	{
		static string[] GAME_STUDIOS = { "Rockstar Games", "Schell Games", "Epic Games", "BioWare", "CD Projekt", "IO Interactive", "Ubisoft" };

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
				StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio);
				dict.Add(studio, CrunchScorer.GetCrunchOvertimeScore(si.GameYears, si.employeeCount));
			}

			return dict.OrderByDescending(x => x.Value).ToList();
		}
	}
}
