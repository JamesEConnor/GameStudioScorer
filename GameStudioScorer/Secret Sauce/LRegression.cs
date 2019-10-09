using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameStudioScorer.Extensions;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Fitting;
using GameStudioScorer.Utils;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;

namespace GameStudioScorer.Regression
{
	public class LRegression
	{
		/// <summary>
		/// Saves scores to a data file for Regression training.
		/// </summary>
		/// <param name="crunchScores">The scores of studios that crunch calculated by MainClass.GetScores().</param>
		/// <param name="noCrunchScores">The scores of studios that don't crunch calculated by MainClass.GetScores().</param>
		/// <param name="saveTo">The name of the file to save the data to.</param>
		public static void SaveToDataFile(List<KeyValuePair<string, float[]>> crunchScores, List<KeyValuePair<string, float[]>> noCrunchScores, string saveTo)
		{
			//Create the file if it doesn't exist.
			if (!File.Exists("Logistic Regression Model/data/" + saveTo + ".txt"))
			{
				FileStream fs = File.Create("Logistic Regression Model/data/" + saveTo + ".txt");
				fs.Close();
				fs.Dispose();
			}

			//Setup
			string[] lines = File.ReadAllLines("Logistic Regression Model/data/" + saveTo + ".txt");
			StreamWriter writer = new StreamWriter("Logistic Regression Model/data/" + saveTo + ".txt");

			//Loop through and make sure the scores haven't already been recorded.
			foreach (KeyValuePair<string, float[]> studio in crunchScores)
			{
				//If the score already has been recorded, we'll just re-write the same line.
				string existingLine = "";
				foreach (string line in lines)
					if (line.Split(':')[0] == studio.Key)
						existingLine = line;

				if (existingLine == "")
				{
					string writeLine = studio.Key + ":";
					foreach (float f in studio.Value)
						writeLine += f + "&";
					writeLine = writeLine.Substring(0, writeLine.Length - 1) + ":1";
					writer.WriteLine(writeLine);
				}
				else
					writer.WriteLine(existingLine);
			}

			foreach (KeyValuePair<string, float[]> studio in noCrunchScores)
			{
				bool canWrite = true;
				foreach (string line in lines)
					if (line.Split(':')[0] == studio.Key)
						canWrite = false;

				if (canWrite)
				{
					string writeLine = studio.Key + ":";
					foreach (float f in studio.Value)
						writeLine += f + "&";
					writeLine = writeLine.Substring(0, writeLine.Length - 1) + ":0";
					writer.WriteLine(writeLine);
				}
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

			Dictionary<string, double> probabilities = new Dictionary<string, double>();

			//Predict and print.
			foreach (KeyValuePair<string, float[]> score in scores)
			{
				double[] inputs = new double[score.Value.Length];
				for (int a = 0; a < score.Value.Length; a++)
					inputs[a] = score.Value[a];

				//Get the probability
				double prob = regression.Probability(inputs);

				probabilities.Add(score.Key, prob);
			}

			//Print in order
			List<KeyValuePair<string, double>> probList = probabilities.ToList();
			probList.Sort((x, y) => x.Value.CompareTo(y.Value));

			foreach (KeyValuePair<string, double> score in probList)
			{
				string confidence =
					((Math.Abs(score.Value - 0.5f) <= 0.3f) ? "LOW CONFIDENCE" : "HIGH CONFIDENCE") +
					" OF " +
					((score.Value > 0.5f) ? "CRUNCHING" : "NOT CRUNCHING");
				Console.WriteLine("{0, -40}: {1, -20} {2, 15}", score.Key, score.Value, confidence);
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
			string[] lines = File.ReadAllLines("Logistic Regression Model/data/" + fileName + ".txt");
			double[][] inputs = new double[lines.Length][];
			int[] outputs = new int[lines.Length];

			for (int a = 0; a < lines.Length; a++)
			{
				string[] split = lines[a].Split(':');

				//Dynamically get variables from file.
				string[] scores = split[1].Split('&');
				inputs[a] = new double[scores.Length];
				for (int b = 0; b < scores.Length; b++)
					inputs[a][b] = double.Parse(scores[b]);

				outputs[a] = int.Parse(split[2]);
			}

			//Set up Accord.NET learner.
			IterativeReweightedLeastSquares<LogisticRegression> learner = new IterativeReweightedLeastSquares<LogisticRegression>()
			{
				Tolerance = 1e-4,
				MaxIterations = 100,
				Regularization = 1e-10
			};

			//Shuffle the input and output pairs to eliminate some inherent bias from
			//training data.
			Dictionary<double[], int> map = inputs.Zip(outputs, (arg1, arg2) => new { arg1, arg2}).ToDictionary(x => x.arg1, x => x.arg2);
			map.Shuffle();
			inputs = map.Keys.ToArray();
			outputs = map.Values.ToArray();


			//Train Regression
			LogisticRegression regression = learner.Learn(inputs, outputs.ToBoolArray());



			//Save to a Model file.
			int counter = 0;
			while (File.Exists("Logistic Regression Model/models/Model-" + counter + ".txt"))
				counter++;

			//Create a file writer
			FileStream fs = File.Create("Logistic Regression Model/models/Model-" + counter + ".txt");
			StreamWriter writer = new StreamWriter(fs);

			//Print the weights
			string result = "Weights: " + regression.Weights.GetString() + "\n";

			//Write lines.
			writer.WriteLine(regression.Weights.GetString());
			for (int c = 0; c < regression.Weights.Length; c++)
			{
				writer.WriteLine(regression.GetOddsRatio(c));
				result += "Odds Ratio " + c + ": " + regression.GetOddsRatio(c) + "\n";
			}

			//Get Loss values.
			double[] actual = new double[inputs.Length];
			double[] expected = new double[outputs.Length];
			for (int a = 0; a < actual.Length; a++)
			{
				actual[a] = regression.Probability(inputs[a]);
				expected[a] = outputs[a];
			}

			//Calculate and print square loss.
			string loss = "Loss: " + new SquareLoss(expected)
			{
				Mean = true,
				Root = true
			}.Loss(actual);
			result += loss + "\n";
			writer.WriteLine(loss);

			Console.WriteLine("\n\n" + loss);


			//Calculate and print R-squared Loss
			string r2 = "R2: " + new RSquaredLoss(inputs[0].Length, expected).Loss(actual);
			result += r2;
			writer.WriteLine(r2);

			//Cleanup
			writer.Close();
			writer.Dispose();

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Model trained successfully!");
			Console.WriteLine("\nEvaluating...\n");

			//Get the VIFs
			float[] VIFs = CalculateVIFs(inputs);

			//Log it
			for (int a = 0; a < VIFs.Length; a++)
				Logger.Log("Variance Inflation Factor #" + a + ": " + VIFs[a]);

			return result;
		}

		/// <summary>
		/// Evaluates a model by calculating it's loss from a set.
		/// </summary>
		/// <returns>An array of measurements. (SME, False Positive Rate, Accuracy)</returns>
		/// <param name="crunchingScores">The scores for crunching studios.</param>
		/// <param name="nonCrunchingScores">The scores for non-crunching studios.</param>
		/// <param name="modelName">The file name of the model.</param>
		public static float[] EvaluateModel(List<KeyValuePair<string, float[]>> crunchingScores, List<KeyValuePair<string, float[]>> nonCrunchingScores, string modelName)
		{
			//Get the model file and deserialize the weights.
			if (!File.Exists("Logistic Regression Model/models/" + modelName + ".txt"))
				throw new Exception("Model doesn't exist. Check your spelling and try again.");

			string line = File.ReadAllLines("Logistic Regression Model/models/" + modelName + ".txt")[0];
			double[] weights = Extensions.Extensions.LoadWeights(line);

			//Create a regression model.
			LogisticRegression regression = new LogisticRegression();
			regression.Weights = weights;

			//Get the actual and expected values from the scores.
			double[] actual = new double[crunchingScores.Count + nonCrunchingScores.Count];
			double[] expected = new double[crunchingScores.Count + nonCrunchingScores.Count];

			int[] actualClass = new int[crunchingScores.Count + nonCrunchingScores.Count];
			int[] expectedClass = new int[crunchingScores.Count + nonCrunchingScores.Count];

			//Get expected and actual values for crunching studios.
			for (int a = 0; a < crunchingScores.Count; a++)
			{
				actual[a] = regression.Probability(crunchingScores[a].Value.Select((arg) => (double)arg).ToArray());
				expected[a] = 1;

				actualClass[a] = (actual[a] > 0.5f) ? 1 : 0;
				expectedClass[a] = 1;
			}

			//Get expected and actual values for non-crunching studios.
			for (int b = 0; b < nonCrunchingScores.Count; b++)
			{
				actual[b + crunchingScores.Count] = regression.Probability(nonCrunchingScores[b].Value.Select((arg) => (double)arg).ToArray());
				expected[b + crunchingScores.Count] = 0;

				actualClass[b + crunchingScores.Count] = (actual[b + crunchingScores.Count] > 0.5f) ? 1 : 0;
				expectedClass[b + crunchingScores.Count] = 0;
			}

			//Calculate and return the loss.
			float loss = (float)new SquareLoss(expected)
			{
				Mean = true,
				Root = true
			}.Loss(actual);

			//Get R-squared loss
			float r2 = (float)new RSquaredLoss(crunchingScores.First().Value.Length, expected).Loss(actual);

			//Calculate confusion matrix
			ConfusionMatrix gcm = new ConfusionMatrix(actualClass.ToBoolArray(), expectedClass.ToBoolArray());

			//Calculate false negative and positive rates
			float falsePR = (float)gcm.FalsePositiveRate;

			//Calculate overall accuracy
			float accuracy = (float)gcm.Accuracy;

			//Calculate the high confidence rate
			//(how many are correct and have a probability within 0.2 of either extreme)
			int correctHighConfidence = 0, highConfidence = 0;
			for (int a = 0; a < actual.Length; a++)
			{
				//If it's in the range of 0 - 0.2 or 0.8 - 1, it's considered 'high confidence'
				if (Math.Abs(actual[a] - 0.5f) > 0.3f)
				{
					//Is it correct?
					if (actualClass[a] == expectedClass[a])
						correctHighConfidence++;

					highConfidence++;
				}
			}

			float correctHC = ((float)correctHighConfidence) / highConfidence;

			return new float[] { loss, r2, falsePR, accuracy, correctHC };
		}

		/// <summary>
		/// Calculates the Variance Inflation Factors (VIFs) for the different coefficients.
		/// </summary>
		/// <returns>An array containing corresponding VIFs.</returns>
		/// <param name="inputs">The inputs that a model was trained on.</param>
		public static float[] CalculateVIFs(double[][] inputs)
		{
			//Rotate array and create resultant array.
			inputs = MathUtils.RotateArray(inputs);
			float[] VIFs = new float[inputs.Length];

			//Loop through each variable
			for (int a = 0; a < inputs.Length; a++)
			{
				//The inputs/outputs for the regression models.
				double[][] regressionInputs = new double[inputs[0].Length][];
				double[] regressionOutput = new double[inputs[0].Length];

				//Loop through and assign all of the independent variables as IVs,
				//except inputs[a], which becomes the dependent variable.
				for (int b = 0; b < inputs[0].Length; b++)
				{
					regressionInputs[b] = new double[inputs.Length - 1];

					for (int c = 0, d = 0; c < inputs.Length; c++)
					{
						if (a == c)
						{
							regressionOutput[b] = inputs[a][b];
						}
						else
						{
							regressionInputs[b][d] = inputs[c][b];
							d++;
						}
					}
				}

				//Perform regression
				OrdinaryLeastSquares ols = new OrdinaryLeastSquares()
				{
					UseIntercept = true
				};

				MultipleLinearRegression regression = ols.Learn(regressionInputs, regressionOutput);

				//Make predictions
				double[] predictions = regression.Transform(regressionInputs);

				//Calculate the loss
				double r2 = (new RSquaredLoss(inputs.Length - 1, regressionOutput)).Loss(predictions);

				//Calculate the VIF
				VIFs[a] = (float)(1.0f / (1.0f - r2));
			}

			return VIFs;
		}
	}
}
