using System;
using GameStudioScorer.Utils;

namespace GameStudioScorer.Utils
{
	public class MathUtils
	{
		/// <summary>
		/// Calculate the exponential function of best fit and return it.
		/// </summary>
		/// <returns>The exponential function of best fit.</returns>
		/// <param name="x">The x inputs.</param>
		/// <param name="y">The y values.</param>
		/// For a clearer understanding of exponential regression, check out this
		/// link: http://mathworld.wolfram.com/LeastSquaresFittingExponential.html
		public static BestFit ExpRegression(float[] x, float[] y)
		{
			//Convert the line into a "linear" form. So it changes from
			//y = a * 10^bx to log(y) = log(a) + bx. Solve for the best fit of this "line".
			BestFit bestLin = LinRegression(x, Log(y));

			//Convert to a linear equation. This will allow us to use the slope and
			//y-intercept in calculations.
			LinearEquation eq = (LinearEquation)bestLin.equation;

			//Find line of best fit for actual x-inputs and y-inputs.
			bestLin = LinRegression(x, y);

			//Looking at the equation from earlier, a is the y-intercept of the line.
			float A = ((LinearEquation)bestLin.equation).b;

			//The exponential equation should be in the form y = a * r^x.
			//From earlier, that means r = 10^b, which in this case makes r = 10^m
			float r = (float)Math.Pow(10, eq.m);

			//Return exponential equation.
			ExponentialEquation exp = new ExponentialEquation(A, r);
			return new BestFit(exp, x, y);
		}

		/// <summary>
		/// Calculate line of best fit.
		/// </summary>
		/// <returns>The regression.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// I'm not going to try and explain linear regression in comments.
		/// Check out this link for more info: http://mathworld.wolfram.com/LeastSquaresFitting.html
		public static BestFit LinRegression(float[] x, float[] y)
		{
			if (x.Length != y.Length)
				throw new Exception("Error: x-values and y-values must be of equal length!");

			//m = (n*sum(products) - sum(x's)sum(y's))/(n*sum(x squares) - sum(x's)^2)

			//Calculate:
			//1: The sum of the squares of the inputs.
			//2: The sum of the products of the inputs and outputs.
			float productSum = 0f;
			float productSquares = 0f;

			for (int a = 0; a < x.Length; a++)
			{
				productSum += x[a] * y[a];
				productSquares += x[a] * x[a];
			}

			//Calculus to solve for the line of best fit's slope.
			float dividend = (x.Length * productSum) - (Sum(x) * Sum(y));
			float divisor = (x.Length * productSquares) - (float)Math.Pow(Sum(x), 2.0);
			float m = dividend / divisor;

			//Calculus to solve for the line of best fit's y-intercept.
			float b = (Sum(y) - m * (Sum(x))) / x.Length;

			//Return the result.
			LinearEquation eq = new LinearEquation(m, b);
			return new BestFit(eq, x, y);
		}

		/// <summary>
		/// Calculates the finite integral, or the area beneath a logarithmic curve
		/// between two points.
		/// </summary>
		/// <returns>The area beneath the curve.</returns>
		/// <param name="e">The logarithmic equation.</param>
		/// <param name="c1">The first point of the finite integral.</param>
		/// <param name="c2">The second point of the finite integral.</param>
		public static float LogarithmicIntegral(LogarithmicEquation e, float c1, float c2)
		{
			//Calculate the integral's value at the first point.
			float A = e.c * c1;
			float area1 = (float)((e.a * c1 * (Math.Log(A, Math.E) - 1)) / Math.Log(e.b, Math.E));

			//Calculate the integral's value at the second point.
			A = e.c * c2;
			float area2 = (float)((e.a * c2 * (Math.Log(A, Math.E) - 1)) / Math.Log(e.b, Math.E));

			//Return the difference, which is the area beneath the curve.
			return area2 - area1;
		}

		/// <summary>
		/// Generate an array of ascending values that go to a certain length.
		/// (i.e. length = 4: [1, 2, 3, 4])
		/// </summary>
		/// <returns>An array of ascending numbers of size <paramref name="length"/>.</returns>
		/// <param name="length">How long the array should be.</param>
		public static float[] GenInputs(int length)
		{
			if (length <= 0)
				throw new Exception("Cannot have a negative or empty input array!");

			float[] inputs = new float[length];
			for (int a = 0; a < inputs.Length; a++)
				inputs[a] = a;

			return inputs;
		}

		/// <summary>
		/// Sum the specified array.
		/// </summary>
		/// <returns>The sum of all values in array.</returns>
		/// <param name="array">The array.</param>
		public static float Sum(float[] array)
		{
			float total = 0.0f;

			foreach (float num in array)
				total += num;

			return total;
		}

		/// <summary>
		/// Get the Base-10 Log of every value in the array. Useful for Exp. Regression.
		/// </summary>
		/// <returns>An array of the Base-10 Logarithm of each value in array</returns>
		/// <param name="array">The array.</param>
		public static float[] Log(float[] array)
		{
			float[] result = new float[array.Length];
			for (int a = 0; a < array.Length; a++)
				result[a] = (float)Math.Log(array[a]);

			return result;
		}
	}

	/// <summary>
	/// Equation interface. Used to represent all equations.
	/// </summary>
	public interface Equation
	{
		float GetValue(float x);
	}

	/// <summary>
	/// A linear equation, in the form y = mx + b.
	/// </summary>
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

	/// <summary>
	/// An exponential equation, in the form y = a * r^x.
	/// </summary>
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

	/// <summary>
	/// A logarithmic equation, in the form y = a * log base b of (x * c)
	/// </summary>
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

	/// <summary>
	/// A line representing the best fit of a set of points.
	/// </summary>
	public class BestFit
	{
		//The equation acting as the line/function of best fit.
		public Equation equation;
		//The different points.
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

		/// <summary>
		/// For each point, get the residual error. That is, the difference between
		/// the supplied y-value and the expected y-value.
		/// </summary>
		/// <returns>The residual errors.</returns>
		public float[] GetResidualErrors()
		{
			float[] results = new float[ios[0].Length];
			for (int a = 0; a < results.Length; a++)
				results[a] = Math.Abs(equation.GetValue(ios[0][a]) - ios[1][a]);

			return results;
		}

		/// <summary>
		/// Returns the sum of squared errors.
		/// </summary>
		/// <returns>The average of the residual errors..</returns>
		public float GetSSE()
		{
			float[] errors = GetResidualErrors();
			float total = 0;
			foreach (float f in errors)
				total += f;

			return total / errors.Length;
		}

		/// <summary>
		/// Gets the correlation coefficient. The closer to 1, the better fit the line is.
		/// </summary>
		/// <returns>The correlation coefficient.</returns>
		/// Again, not explaining it. Sorry, but it involves Calc.
		/// Check out this link: https://www.thoughtco.com/how-to-calculate-the-correlation-coefficient-3126228
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