using System;
using System.Collections.Generic;
using GameStudioScorer.Utils;

namespace GameStudioScorer.Crunch
{
	public class CrunchScorer
	{
		public static float GetCrunchOvertimeScore(int[] years, int employeeCount)
		{
			List<float> yearsF = new List<float>();
			for (int a = 0; a < years.Length; a++)
				if(years[a].ToString().Length >= 4)
					yearsF.Add(years[a]);

			float[] inputs = MathUtils.GenInputs(yearsF.Count);

			BestFit bf = MathUtils.ExpRegression(inputs, yearsF.ToArray());
			ExponentialEquation exp = (ExponentialEquation)bf.equation;
			//Console.WriteLine(exp.A + " * (" + exp.r + ")^x");

			LogarithmicEquation log = new LogarithmicEquation(1/exp.A, exp.r, employeeCount);
			//Console.WriteLine(log.a + ", " + log.b + ", " + log.c);
			//Console.WriteLine(MathUtils.LogarithmicIntegral(log, 1f, 8f) + "," + (7f * log.GetValue(8f)));
			const int length = 7;
			return MathUtils.LogarithmicIntegral(log, 1f, length + 1)/(length * log.GetValue(length + 1));
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