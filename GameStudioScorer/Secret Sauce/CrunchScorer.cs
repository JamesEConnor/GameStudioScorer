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
			return log.GetValue(2500);
		}
	}
}

/*
 * 1165.3447
 * 532.27819
 * 733.8162
 * 489.28341
 */