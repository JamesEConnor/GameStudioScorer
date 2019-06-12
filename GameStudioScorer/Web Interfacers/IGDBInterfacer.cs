using System.Net;
using System.IO;
using GameStudioScorer.Extensions;

namespace GameStudioScorer.IGDB
{
	public class IGDBInterfacer
	{
		public static StudioInfo GetStudio(string name)
		{
			return new StudioInfo
			{
				id = "-1"
			};

			/*StudioInfo si = LocalCacheManager.GetCachedInfo(name);
			if (si.id != "-1")
				return si;

			int employeeCount = Extensions.Extensions.GetEmployeeCount(name);
			int[] gameInfo = GetIGDBInfo(name);
			int[] gameYears = new int[gameInfo.Length - 1];

			for (int a = 0; a < gameInfo.Length - 1; a++)
				gameYears[a] = gameInfo[a];

			si = new StudioInfo
			{
				id = gameInfo[gameInfo.Length - 1],
				name = name,
				employeeCount = employeeCount,
				GameYears = gameYears
			};

			LocalCacheManager.SaveCachedInfo(si);
			return si;*/
		}

		public static int[] GetIGDBInfo(string name)
		{
			//Unirest.
			return null;
		}
	}
}

namespace GameStudioScorer
{
	public struct StudioInfo
	{
		public string id;
		public string name;
		public int[] GameYears;
		public int employeeCount;
	}
}