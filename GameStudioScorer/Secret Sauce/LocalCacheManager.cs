using System.IO;
using System.Text;
using GameStudioScorer.IGDB;
using GameStudioScorer.Extensions;
using System;
using GameStudioScorer.Utils;

namespace GameStudioScorer
{
	public class LocalCacheManager
	{
		/// <summary>
		/// Gets Studio information from the local cache.
		/// </summary>
		/// <returns>The cached Studio info.</returns>
		/// <param name="studioName">The name of the Studio, as stored in the cache.</param>
		public static StudioInfo GetCachedInfo(string studioName)
		{
			//Formatting stuff so it can be stored in a CSV properly.
			string formattedStudioName = studioName.Replace(",", "-").ToLower();

			//Logging
			if (Logger.VERBOSE)
				Logger.Log(studioName + " formatted to: " + formattedStudioName);

			if (File.Exists("cache.csv"))
			{
				//Get lines from the cache.
				string[] lines = File.ReadAllLines("cache.csv");

				//Check all lines for the specific one containing the info for the studio.
				string line = "";
				foreach (string l in lines)
				{
					try
					{
						if (l.Split(',')[1] == formattedStudioName)
							line = l;
					}
					catch
					{
						continue;
					}
				}

				if (line != "")
				{
					//Split and get the info.
					string[] split = line.Split(',');

					if (Logger.VERBOSE)
						Logger.Log(formattedStudioName + " (" + studioName + ") found in LocalCache.");

					return new StudioInfo
					{
						id = split[0],
						name = studioName,
						employeeCount = int.Parse(split[2]),
						GameYears = Extensions.Extensions.LoadGameYears(split[3]),
						GenreScore = float.Parse(split[4]),
						ReviewScore = float.Parse(split[5]),
						ConsScore = float.Parse(split[6]),
						aliases = Extensions.Extensions.CreateAliasList(studioName)
					};
				}
			}
			else
			{
				//If the cache file doesn't exist, create and clean up.
				FileStream fs = File.Create("cache.csv");
				fs.Close();
				fs.Dispose();
			}

			//If the cache doesn't exist or the Studio wasn't found, return the Studio
			//equivalent of 'null'
			return new StudioInfo
			{
				id = "-1"
			};
		}

		/// <summary>
		/// Saves Studio information to the cache.
		/// </summary>
		/// <returns><c>true</c> if the cached info was saved successfully, <c>false</c> otherwise.</returns>
		/// <param name="si">Si.</param>
		public static bool SaveCachedInfo(string saveAs, StudioInfo si)
		{
			//Read the contents and create the new value for the Studio.
			string[] contents = File.ReadAllLines("cache.csv");

			string newValue = 		si.id.Replace(",", "-")					+ "," +
			                        saveAs.Replace(',', '-').ToLower()		+ "," +
									si.employeeCount 						+ "," +
									si.GameYears.GetString() 				+ "," +
									si.GenreScore 							+ "," +
									si.ReviewScore							+"," +
									si.ConsScore;

			string toAdd = "";
			bool saved = false;
			for (int a = 0; a < contents.Length; a++)
			{
				//Check and see if the cache already has information. If it does,
				//we're going to update it instead of add it.
				string[] split = contents[a].Split(',');
				if (split[0] == si.id.ToString())
				{
					contents[a] = newValue;

					saved = true;
				}

				//No matter what, keep the rest of the contents for the cache.
				toAdd += contents[a] + "\n";
			}

			//If it didn't exist, append the new values to the end of the cache.
			if (!saved)
				toAdd += newValue;

			//Open the File, set the Encoding, and write to it.
			FileStream stream = File.Open("cache.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			Encoding enc = Encoding.UTF8;

			stream.Write(enc.GetBytes(toAdd), 0, enc.GetByteCount(toAdd));
			stream.Close();
			stream.Dispose();

			return true;
		}
	}
}
