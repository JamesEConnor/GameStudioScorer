using System;
using GameStudioScorer.Utils;

namespace GameStudioScorer.Crunch
{
	public class CrunchScorer
	{
		public static float GetCrunchOvertimeScore(int[] years, int employeeCount)
		{
			float[] yearsF = new float[years.Length];
			for (int a = 0; a < years.Length; a++)
				yearsF[a] = years[a];

			float[] inputs = MathUtils.GenInputs(years.Length);

			BestFit bf = MathUtils.ExpRegression(inputs, yearsF);
			ExponentialEquation exp = (ExponentialEquation)bf.equation;
			//Console.WriteLine(exp.A + " * (" + exp.r + ")^x");

			LogarithmicEquation log = new LogarithmicEquation(1/exp.A, exp.r, 1f / employeeCount);
			//return log.GetValue(1f * (float)Math.Pow(10, 11));
			return log.GetValue(230000);
		}
	}
}
