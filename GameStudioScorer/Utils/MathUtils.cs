using System;
using GameStudioScorer.Utils;

namespace GameStudioScorer.Utils
{
	public class MathUtils
	{
		public static BestFit ExpRegression(float[] x, float[] y)
		{
			BestFit bestLin = LinRegression(x, Log(y));
			LinearEquation eq = (LinearEquation)bestLin.equation;
			bestLin = LinRegression(x, y);
			float A = ((LinearEquation)bestLin.equation).b;
			float r = (float)Math.Pow(10, eq.m);

			ExponentialEquation exp = new ExponentialEquation(A, r);
			return new BestFit(exp, x, y);
		}

		public static BestFit LinRegression(float[] x, float[] y)
		{
			if (x.Length != y.Length)
				throw new Exception("Error: x-values and y-values must be of equal length!");

			//m = (n*sum(products) - sum(x's)sum(y's))/(n*sum(x squares) - sum(x's)^2)

			float productSum = 0f;
			float productSquares = 0f;

			for (int a = 0; a < x.Length; a++)
			{
				productSum += x[a] * y[a];
				productSquares += x[a] * x[a];
			}

			float dividend = (x.Length * productSum) - (Sum(x) * Sum(y));
			float divisor = (x.Length * productSquares) - (float)Math.Pow(Sum(x), 2.0);
			float m = dividend / divisor;

			float b = (Sum(y) - m * (Sum(x))) / x.Length;

			LinearEquation eq = new LinearEquation(m, b);
			return new BestFit(eq, x, y);
		}

		public static float LogarithmicIntegral(LogarithmicEquation e, float c1, float c2)
		{
			float A = e.c * c1;
			float area1 = (float)((e.a * c1 * (Math.Log(A, Math.E) - 1)) / Math.Log(e.b, Math.E));

			A = e.c * c2;
			float area2 = (float)((e.a * c2 * (Math.Log(A, Math.E) - 1)) / Math.Log(e.b, Math.E));
			return area2 - area1;
		}

		public static float[] GenInputs(int length)
		{
			if (length <= 0)
				throw new Exception("Cannot have a negative or empty input array!");

			float[] inputs = new float[length];
			for (int a = 0; a < inputs.Length; a++)
				inputs[a] = a;

			return inputs;
		}

		public static float Sum(float[] array)
		{
			float total = 0.0f;

			foreach (float num in array)
				total += num;

			return total;
		}

		public static float[] Log(float[] array)
		{
			float[] result = new float[array.Length];
			for (int a = 0; a < array.Length; a++)
				result[a] = (float)Math.Log(array[a]);

			return result;
		}
	}

	public interface Equation
	{
		float GetValue(float x);
	}

	public class LinearEquation : Equation
	{
		public float m;
		public float b;

		public LinearEquation(float _m, float _b)
		{
			m = _m;
			b = _b;
		}

		public float GetValue(float x)
		{
			return m * x + b;
		}
	}

	public class ExponentialEquation : Equation
	{
		public float A;
		public float r;

		public ExponentialEquation(float _A, float _r)
		{
			A = _A;
			r = _r;
		}

		public float GetValue(float x)
		{
			return A * (float)Math.Pow(r, x);
		}
	}

	public class LogarithmicEquation : Equation
	{
		public float a;
		public float b;
		public float c;

		public LogarithmicEquation(float _a, float _b, float _c)
		{
			a = _a;
			b = _b;
			c = _c;
		}

		public float GetValue(float x)
		{
			return a * (float)Math.Log(x * c, b);
		}
	}

	public class BestFit
	{
		public Equation equation;
		public float[][] ios;

		public BestFit(Equation eq, float[] x, float[] y)
		{
			if (x.Length != y.Length)
				throw new Exception("Error: x-values and y-values must be of equal length!");
	
			equation = eq;
			ios = new float[2][];
			ios[0] = x;
			ios[1] = y;
		}

		public float[] GetResidualErrors()
		{
			float[] results = new float[ios[0].Length];
			for (int a = 0; a < results.Length; a++)
				results[a] = Math.Abs(equation.GetValue(ios[0][a]) - ios[1][a]);

			return results;
		}

		public float GetSSE()
		{
			float[] errors = GetResidualErrors();
			float total = 0;
			foreach (float f in errors)
				total += f;

			return total / errors.Length;
		}

		public float GetCorrelationCoefficient()
		{
			float productSum = 0f;
			float productSquares = 0f;

			for (int a = 0; a < ios[0].Length; a++)
			{
				productSum += ios[0][a] * ios[1][a];
				productSquares += ios[0][a] * ios[0][a];
			}

			float xSqrt = (float)Math.Sqrt((ios[0].Length * productSquares) - Math.Pow(MathUtils.Sum(ios[0]), 2));
			float ySqrt = (float)Math.Sqrt((ios[1].Length * productSquares) - Math.Pow(MathUtils.Sum(ios[1]), 2));



			float dividend = (ios[0].Length * productSum) - (MathUtils.Sum(ios[0]) * MathUtils.Sum(ios[1]));
			float divisor = xSqrt * ySqrt;

			return dividend / divisor;
		}
	}
}