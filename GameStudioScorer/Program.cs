using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using CommandLine;
using GameStudioScorer.Crunch;
using GameStudioScorer.Extensions;
using GameStudioScorer.Regression;
using GameStudioScorer.Utils;

namespace GameStudioScorer
{
	class MainClass
	{
		//The Game Studios to get a score for.
		public static string[] GAME_STUDIOS = {  };

		//Should any of the Game Studios be used in Debug Mode?
		//Copy them from the GAME_STUDIOS array. They *must* be in both.
		//This will cause them to ignore any cached values and forcibly recalculate
		//a score. This is good if they're outdated.
		public static bool DEBUG_MODE;

		//The command line options.
		public static Options options;

		//'p' = Print. This will simply print the values. No logistic regression applied.
		//'s' = Save. This will perform the same operations as print, but save them to a file.
		//'l' = Learn. This will take in values from a file and create a model based off of them.
		//'m' = Model. This will perform print, but then apply the values to a logistic regression model.

		public static void Main(string[] args)
		{
			//Uses the CommandLineParser library to parse command line arguments.
			options = new Options();
			if (Parser.Default.ParseArguments<Options>(args)
				  .WithParsed(o => options = o).Tag == ParserResultType.NotParsed)
				return;

			//Go through the various options.
			Logger.VERBOSE = options.verbose;
			DEBUG_MODE = options.debug;
			if (options.studio != "null")
				options.studio = options.studio.Replace("_", " ");


			if (options.RegressionType == 'p' ||
			   options.RegressionType == 's' ||
			   options.RegressionType == 'm' ||
			   options.RegressionType == 'e')
			{
				if (!File.Exists("Logistic Regression Model/sets/" + options.setName + ".txt") && options.studio == "null")
					throw new Exception("Set file must exist or studio must be specified. Consult the ReadMe for more information.");
				   
				string[] lines = File.ReadAllLines("Logistic Regression Model/sets/" + options.setName + ".txt");

				List<KeyValuePair<string, float[]>> scores;

				//If studio is specified, use it. Otherwise, load from the file.
				if (options.studio == "null")
					scores = GetScores(lines[0].Split(new string[] { ", " }, StringSplitOptions.None), 2);
				else
					scores = GetScores(new string[] { options.studio });

				//Print the values out.
				if (options.RegressionType == 'p')
				{
					//Print data.
					Console.WriteLine("{0, -40}  {1, -15} {4, -8} {2, -15} {4, -10} {3, -15}\n", "STUDIO NAME", "CRUNCH/TIME SCORE", "GENRES SCORE", "REVIEW SCORE", " ");
					foreach (KeyValuePair<string, float[]> s in scores)
					{
						Console.WriteLine("{0, -40}: {1, -15} {4, -10} {2, -15} {4, -10} {3, -15}", s.Key, s.Value[0], s.Value[1], s.Value[2], ",");
					}

					//Print out any additional lines.
					for (int a = 1; a < lines.Length; a++)
					{
						//Get the scores for the second line of the set.
						List<KeyValuePair<string, float[]>> otherScores = GetScores(lines[a].Split(new string[] { ", " }, StringSplitOptions.None));
						foreach (KeyValuePair<string, float[]> s in otherScores)
						{
							Console.WriteLine("{0, -40}: {1, -15} {4, -10} {2, -15} {4, -10} {3, -15}", s.Key, s.Value[0], s.Value[1], s.Value[2], ",");
						}
					}
				}
				else if (options.RegressionType == 's')
				{
					//Get the scores for the second line of the set.
					List<KeyValuePair<string, float[]>> noCrunchScores = (options.studio == "null") ?
						GetScores(lines[1].Split(new string[] { ", " }, StringSplitOptions.None)) :
						new List<KeyValuePair<string, float[]>>();

					//Save data to file.
					LRegression.SaveToDataFile(scores, noCrunchScores, options.fileName);
				}
				else if (options.RegressionType == 'm')
				{
					//Get additional lines.
					if(options.studio == "null")
						for (int c = 1; c < lines.Length; c++)
							scores.AddRange(GetScores(lines[c].Split(new string[] { ", " }, StringSplitOptions.None), 1));

					//Model based off of learned weights.
					LRegression.Model(scores, options.modelName);
				}
				else if (options.RegressionType == 'e')
				{
					//Get the scores for the second line of the set.
					List<KeyValuePair<string, float[]>> noCrunchScores = (options.studio == "null") ?
						GetScores(lines[1].Split(new string[] { ", " }, StringSplitOptions.None)) :
						new List<KeyValuePair<string, float[]>>();

					//Calculate and print the loss
					float[] measurements = LRegression.EvaluateModel(scores, noCrunchScores, options.modelName);
					Console.WriteLine("Loss (SME): " + measurements[0]);
					Console.WriteLine("False Positive Rate: " + measurements[1]);
					Console.WriteLine("Overall Accuracy: " + measurements[2]);
					Console.WriteLine("Correct High Confidence Rate: " + measurements[3]);
				}
			}
			else if (options.RegressionType == 'l')
			{
				//Learn the weights and record as a model.
				Console.WriteLine(LRegression.Learn(options.fileName));
			}
			else
			{
				throw new Exception("Regression type is limited to 'p', 'l', 'm', or 's'. For more info on all of them and what they mean, consult the ReadMe.");
			}
		}

		/// <summary>
		/// Gets the three scores for a list of given studios.
		/// </summary>
		/// <returns>A list of KeyValuePairs in which the provided studios are the keys
		///  and their values are arrays of scores (of length 3).</returns>
		/// <param name="studios">The different studios to get scores for.</param>
		public static List<KeyValuePair<string, float[]>> GetScores(string[] studios, int crunches = 0)
		{
			Dictionary<string, float[]> dict = new Dictionary<string, float[]>();
			foreach (string studio in studios)
			{
				try
				{
					//Gets a StudioInfo object, containing all sorts of goodies.
					StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio, DEBUG_MODE);

					//Add the different values to the dictionary.
					dict.Add(studio + ((crunches == 2) ? " - TRUE" : (crunches == 1) ? " - FALSE" : ""), new float[]{
						si.CrunchOvertimeScore, si.GenreScore, si.ReviewScore, si.ConsScore
					});

					//If we force-calculated new values or if it doesn't already exist,
					//cache the Studio
					if (MainClass.options.force || LocalCacheManager.GetCachedInfo(studio).id == "-1")
						LocalCacheManager.SaveCachedInfo(studio, si);
				}
				catch(Exception e)
				{
					Logger.Log(e.Message + ". Occurred for " + studio, Logger.LogLevel.ERROR, true);
					Logger.Log(e.StackTrace, Logger.LogLevel.CRITICAL, false);
				}
			}

			//Return
			return dict.ToList();
		}
	}
}