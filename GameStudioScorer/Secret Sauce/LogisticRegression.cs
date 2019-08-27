using System;
using System.Collections.Generic;
using System.IO;

namespace GameStudioScorer.Regression
{
	public class LogisticRegression
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

		public static void Learn(string fileName)
		{

		}
	}
}
