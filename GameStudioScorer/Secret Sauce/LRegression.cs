using System;
using System.Collections.Generic;
using System.IO;
using GameStudioScorer.Extensions;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;

namespace GameStudioScorer.Regression
{
	public class LRegression
	{
		/// <summary>
		/// Saves scores to a data file for Regression training.
		/// </summary>
		/// <param name="scores">The scores calculated by MainClass.GetScores().</param>
		/// <param name="saveTo">The name of the file to save the data to.</param>
		public static void SaveToDataFile(List<KeyValuePair<string, float[]>> scores, string saveTo)
		{
			//Create the file if it doesn't exist.
			if (!File.Exists("Logistic Regression Model/data.txt"))
			{
				FileStream fs = File.Create("Logistic Regression Model/data.txt");
				fs.Close();
				fs.Dispose();
			}

			//Setup
			string[] lines = File.ReadAllLines("Logistic Regression Model/data/" + saveTo);
			StreamWriter writer = new StreamWriter("Logistic Regression Model/data/" + saveTo);

			//Loop through and make sure the scores haven't already been recorded.
			foreach (KeyValuePair<string, float[]> studio in scores)
			{
				bool canWrite = true;
				foreach (string line in lines)
					if (line.Split(':')[0] == studio.Key)
						canWrite = false;

				if(canWrite)
					writer.WriteLine(studio.Key + ":" + studio.Value[0] + "-" + studio.Value[1] + "-" + studio.Value[2]);
			}

			//Clean up
			writer.Close();
			writer.Dispose();
		}

		/// <summary>
		/// Predicts the probability of a studio crunching based off a model and the studio's scores.
		/// </summary>
		/// <param name="scores">The scores as calculated.</param>
		/// <param name="modelName">The name of the regression model to load weights from..</param>
		public static void Model(List<KeyValuePair<string, float[]>> scores, string modelName)
		{
			//Get the model file and deserialize the weights.
			if (!File.Exists("Logistic Regression Model/models/" + modelName + ".txt"))
				throw new Exception("Model doesn't exist. Check your spelling and try again.");

			string line = File.ReadAllLines("Logistic Regression Model/models/" + modelName + ".txt")[0];
			double[] weights = Extensions.Extensions.LoadWeights(line);

			//Create a regression model.
			LogisticRegression regression = new LogisticRegression();
			regression.Weights = weights;

			Console.WriteLine("Probability of studios crunching:");

			//Predict and print.
			foreach (KeyValuePair<string, float[]> score in scores)
			{
				double[] inputs = new double[score.Value.Length];
				for (int a = 0; a < score.Value.Length; a++)
					inputs[a] = score.Value[a];

				double prob = regression.Probability(inputs);

				Console.WriteLine("{0, -20}: {1, -5}", score.Key, prob);
			}
		}

		/// <summary>
		/// Uses data from <paramref name="fileName">fileName</paramref> to train a logistic regression model./>
		/// </summary>
		/// <param name="fileName">The name of the data file.</param>
		/// <returns>A string to print giving information about the weights and odds ratios.</returns>
		public static string Learn(string fileName)
		{
			//Read all inputs and outputs from training file.
			string[] lines = File.ReadAllLines(fileName);
			double[][] inputs = new double[lines.Length][];
			int[] outputs = new int[lines.Length];

			for (int a = 0; a < lines.Length; a++)
			{
				string[] split = lines[a].Split(':');

				string[] scores = split[1].Split('-');
				inputs[a] = new double[scores.Length];
				for (int b = 0; b < scores.Length; b++)
					inputs[a][b] = double.Parse(scores[b]);

				outputs[a] = int.Parse(split[2]);
			}

			//Set up Accord.NET learner.
			IterativeReweightedLeastSquares learner = new IterativeReweightedLeastSquares()
			{
				Tolerance = 1e-4,
				MaxIterations = 100,
				Regularization = 0
			};

			//Perform learning.
			LogisticRegression regression = (LogisticRegression)learner.Learn(inputs, outputs);



			//Save to a Model file.
			int counter = 0;
			while (File.Exists("Logistic Regression Model/models/Model-" + counter + ".txt"))
				counter++;

			FileStream fs = File.Create("Logistic Regression Model/models/Model-" + counter + ".txt");
			StreamWriter writer = new StreamWriter(fs);

			string result = "Weights: " + regression.Weights.GetString() + "\n";

			//Write lines.
			writer.WriteLine(regression.Weights.GetString());
			for (int c = 0; c < regression.Weights.Length; c++)
			{
				writer.WriteLine(regression.GetOddsRatio(c));
				result += "Odds Ratio " + c + ": " + regression.GetOddsRatio(c) + "\n";
			}

			Console.WriteLine("Model trained successfully!");

			return result;
		}
	}
}
