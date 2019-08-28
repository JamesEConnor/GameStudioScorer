using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using CommandLine;
using GameStudioScorer.Crunch;
using GameStudioScorer.Extensions;
using GameStudioScorer.Regression;

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
		public static string[] DEBUG_MODE = { };

		//'p' = Print. This will simply print the values. No logistic regression applied.
		//'s' = Save. This will perform the same operations as print, but save them to a file.
		//'l' = Learn. This will take in values from a file and create a model based off of them.
		//'m' = Model. This will perform print, but then apply the values to a logistic regression model.

		public static void Main(string[] args)
		{
			//Uses the CommandLineParser library to parse command line arguments.
			Options options = new Options();
			Parser.Default.ParseArguments<Options>(args)
				  .WithParsed(o => options = o);

			if (options.RegressionType == 'p' ||
			   options.RegressionType == 's' ||
			   options.RegressionType == 'm')
			{
				string[] lines = File.ReadAllLines("Logistic Regression Model/sets/" + options.setName + ".txt");

				List<KeyValuePair<string, float[]>> scores = GetScores(lines[0].Split(new string[] { ", " }, StringSplitOptions.None));

				//Print the values out.
				if (options.RegressionType == 'p')
				{
					//Print data.
					foreach (KeyValuePair<string, float[]> s in scores)
					{
						Console.WriteLine("{0, -20}: {1, -5}, {2, 10}, {3, 25}", s.Key, s.Value[0], s.Value[1], s.Value[2]);
					}
				}
				else if (options.RegressionType == 's')
				{
					//Get the scores for the second line of the set.
					List<KeyValuePair<string, float[]>> noCrunchScores = GetScores(lines[1].Split(new string[] { ", " }, StringSplitOptions.None));

					//Save data to file.
					LRegression.SaveToDataFile(scores, noCrunchScores, options.fileName);
				}
				else if (options.RegressionType == 'm')
				{
					//Model based off of learned weights.
					LRegression.Model(scores, options.modelName);
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

		public static List<KeyValuePair<string, float[]>> GetScores(string[] studios)
		{
			Dictionary<string, float[]> dict = new Dictionary<string, float[]>();
			foreach (string studio in studios)
			{
				try
				{
					//Gets a StudioInfo object, containing all sorts of goodies.
					StudioInfo si = Giantbomb.GiantBombInterfacer.GetStudio(studio, DEBUG_MODE.Contains(studio));

					//TODO Eventually need to make this use logarithmic regression
					//Add the different values to the dictionary.
					dict.Add(studio, new float[]{
						si.CrunchOvertimeScore, si.GenreScore, si.ReviewScore
					});

					//If we force-calculated new values or if it doesn't already exist,
					//cache the Studio.
					if (DEBUG_MODE.Contains(studio) || LocalCacheManager.GetCachedInfo(studio).id == "-1")
						LocalCacheManager.SaveCachedInfo(si);
				}
				catch(Exception e)
				{
					Console.WriteLine("ERROR: " + e.Message + ". Occurred during \"" + studio + "\"");
				}
			}

			//Order things so it looks nice.
			return dict.OrderByDescending(x => x.Value[0]).ToList();
		}
	}
}