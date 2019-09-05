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

			foreach(KeyValuePair<string, double> score in probList)
				Console.WriteLine("{0, -40}: {1, -20} {2, 15}", score.Key, score.Value, (score.Value >= 0.5f) ? ": TRUE " : ": FALSE");
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

			//Cleanup
			writer.Close();
			writer.Dispose();

			fs.Close();
			fs.Dispose();

			Console.WriteLine("Model trained successfully!");
			Console.WriteLine("\nEvaluating...\n");

			//Evaluate the model and get the average K-score.
			float avrgK = EvaluateModel(learner, inputs, outputs, inputs.Length / 4);

			//Log it.
			Logger.Log("Average K-score: " + avrgK);

			//Get the VIFs
			float[] VIFs = CalculateVIFs(inputs);

			//Log it
			for (int a = 0; a < VIFs.Length; a++)
				Logger.Log("Variance Inflation Factor #" + a + ": " + VIFs[a]);

			return result;
		}

		/// <summary>
		/// Evaluates a Logistic Regression Model using K-Fold Shuffle Cross Validation.
		/// </summary>
		/// <returns>A float representing the average K-score.</returns>
		/// <param name="learner">An instance of a learner to use to train the evaluation models.</param>
		/// <param name="inputs">The inputs.</param>
		/// <param name="outputs">The outputs.</param>
		/// <param name="partitionSize">The size of each Partition.</param>
		public static float EvaluateModel(IterativeReweightedLeastSquares<LogisticRegression> learner, double[][] inputs, int[] outputs, int partitionSize)
		{
			if (inputs.Length < partitionSize)
				throw new Exception("Input Partition Size can not be larger than the number of inputs!");

			//Get to evenly partitionable size.
			List<double[]> inputList = new List<double[]>(inputs);
			List<int> outputList = new List<int>(outputs);
			while (inputList.Count % partitionSize != 0)
			{
				inputList.RemoveAt(0);
				outputList.RemoveAt(0);
			}

			//Allows for shuffling while keeping the inputs/outputs together.
			Dictionary<double[], int> map = inputList.Zip(outputList, (arg1, arg2) => new { arg1, arg2 })
													 .ToDictionary(x => x.arg1, x => x.arg2);

			//K-Fold Shuffle Validation
			int k = inputList.Count / partitionSize;
			float total = 0.0f;

			for (int a = 0; a < k; a++)
			{
				//Shuffle the Dictionary.
				map.Shuffle();

				//The input/output for training.
				inputs = new double[(k - 1) * partitionSize][];
				outputs = new int[(k - 1) * partitionSize];

				//The input/output for validation.
				double[][] testInput = new double[partitionSize][];
				int[] testOutput = new int[partitionSize];

				//Keeps track of the current training data partition.
				int currPartition = 0;

				for (int b = 0; b < k; b++)
				{
					for (int c = 0; c < partitionSize; c++)
					{
						if (a != b)
						{
							//If this isn't the test partition, add to the training
							//data.
							inputs[(currPartition * partitionSize) + c] = map.ElementAt(b * partitionSize + c).Key;
							outputs[(currPartition * partitionSize) + c] = map.ElementAt(b * partitionSize + c).Value;
						}
						else
						{
							//If this is the test partition, add to the test data.
							testInput[c] = map.ElementAt(b * partitionSize + c).Key;
							testOutput[c] = map.ElementAt(b * partitionSize + c).Value;
						}
					}

					//Only increase the current training data partition if the most
					//recent was a training partition.
					if(a != b)
						currPartition++;
				}

				//Train
				LogisticRegression regression = learner.Learn(inputs, outputs.ToBoolArray());

				//Evaluate
				for (int d = 0; d < testInput.Length; d++)
				{
					//Test actual output against expected output.
					double output = regression.Probability(testInput[d]);

					total += (float)Math.Abs(output - testOutput[d]);
				}

				total /= partitionSize;
			}

			//Return average of K-scores.
			return total / k;
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
