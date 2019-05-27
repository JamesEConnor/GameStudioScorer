﻿using System.IO;
using System.Text;
using GameStudioScorer.IGDB;
using GameStudioScorer.Extensions;

namespace GameStudioScorer
{
	public class LocalCacheManager
	{
		public static StudioInfo GetCachedInfo(string studioName)
		{
			if (File.Exists("cache.csv"))
			{
				string[] lines = File.ReadAllLines("cache.csv");

				string line = "";
				foreach (string l in lines)
					if (l.Split(',')[1] == studioName)
						line = l;

				if (line != "")
				{
					string[] split = line.Split(',');
					return new StudioInfo
					{
						id = int.Parse(split[0]),
						name = split[1],
						employeeCount = int.Parse(split[2]),
						GameYears = Extensions.Extensions.LoadGameYears(split[3])
					};
				}
			}

			return new StudioInfo
			{
				id = -1
			};
		}

		public static bool SaveCachedInfo(StudioInfo si)
		{
			FileStream stream = File.Open("cache.csv", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			Encoding enc = Encoding.UTF8;

			string[] contents = File.ReadAllLines("cache.csv");
			string toAdd = "";
			for (int a = 0; a < contents.Length; a++)
			{
				string[] split = contents[a].Split(',');
				if (split[0] == si.id.ToString())
				{
					contents[a] = si.id.ToString() + "," +
									si.name + "," +
									si.employeeCount + "," +
									si.GameYears.GetString();
				}

				toAdd += contents[a] + "\n";
			}

			stream.Write(enc.GetBytes(toAdd), 0, enc.GetByteCount(toAdd));

			return true;
		}
	}
}
