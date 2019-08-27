using System.IO;
using System.Text;
using GameStudioScorer.IGDB;
using GameStudioScorer.Extensions;

namespace GameStudioScorer
{
	public class LocalCacheManager
	{
		public static StudioInfo GetCachedInfo(string studioName)
		{
			studioName = studioName.Replace(",", "-");

			if (File.Exists("cache.csv"))
			{
				string[] lines = File.ReadAllLines("cache.csv");

				string line = "";
				foreach (string l in lines)
				{
					try
					{
						if (l.Split(',')[1] == studioName)
							line = l;
					}
					catch
					{
						continue;
					}
				}

				if (line != "")
				{
					string[] split = line.Split(',');
					return new StudioInfo
					{
						id = split[0],
						name = split[1],
						employeeCount = int.Parse(split[2]),
						GameYears = Extensions.Extensions.LoadGameYears(split[3]),
						GenreScore = float.Parse(split[4]),
						ReviewScore = float.Parse(split[5])
					};
				}
			}

			return new StudioInfo
			{
				id = "-1"
			};
		}

		public static bool SaveCachedInfo(StudioInfo si)
		{
			string[] contents = File.ReadAllLines("cache.csv");

			string newValue = 		si.id.Replace(",", "-")		+ "," +
			                        si.alias.Replace(",", "-") 	+ "," +
									si.employeeCount 			+ "," +
									si.GameYears.GetString() 	+ "," +
									si.GenreScore 				+ "," +
									si.ReviewScore;

			string toAdd = "";
			bool saved = false;
			for (int a = 0; a < contents.Length; a++)
			{
				string[] split = contents[a].Split(',');
				if (split[0] == si.id.ToString())
				{
					contents[a] = newValue;

					saved = true;
				}

				toAdd += contents[a] + "\n";
			}

			if (!saved)
				toAdd += newValue;

			FileStream stream = File.Open("cache.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			Encoding enc = Encoding.UTF8;

			stream.Write(enc.GetBytes(toAdd), 0, enc.GetByteCount(toAdd));
			stream.Close();
			stream.Dispose();

			return true;
		}
	}
}
