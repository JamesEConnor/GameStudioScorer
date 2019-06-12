using System;
using System.Collections.Generic;
using System.Linq;
using GameStudioScorer.Crunch;

namespace GameStudioScorer
{
	class MainClass
	{
		static string[] GAME_STUDIOS = { "Rockstar Games", "Schell Games", "Epic Games", "BioWare", "CD Projekt" };

		public static void Main(string[] args)
		{
			Dictionary<string, float> scores = GetScores(GAME_STUDIOS);
			foreach (string s in scores.Keys)
				Console.WriteLine("{0, -20}: {1, -5}", s, scores[s]);
		}

		public static Dictionary<string, float> GetScores(string[] studios)
		{
			Dictionary<string, float> dict = new Dictionary<string, float>();
			foreach (string studio in studios)
			{
				StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio);
				dict.Add(studio, CrunchScorer.GetCrunchOvertimeScore(si.GameYears, si.employeeCount));
			}

			dict.OrderBy(x => x.Value);

			return dict;
		}
	}
}
