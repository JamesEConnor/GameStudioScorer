using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameStudioScorer.Wiki;

namespace GameStudioScorer.Extensions
{
	public static class Extensions
	{
		public static string GetString(this int[] array)
		{
			string result = "{";
			foreach (int el in array)
				result += el.ToString() + "|";
			return result.Remove(result.Length - 1) + "}";
		}

		public static int[] LoadGameYears(string str)
		{
			str = str.Substring(1);
			str.Remove(str.Length - 1);

			string[] split = str.Split('|');
			int[] result = new int[split.Length];
			for (int a = 0; a < result.Length; a++)
				result[a] = int.Parse(split[a]);

			return result;
		}

		public static int GetEmployeeCount(string companyName)
		{
			Dictionary<string, string> dict = WikipediaRetriever.GetWikiInfo(companyName);
			int employeeCount = 0;

			foreach (var key in dict.Keys)
			{
				if (key.Contains("employee"))
				{
					employeeCount = int.Parse(Regex.Replace(dict[key].Split(new char[] { '_', ' ' })[0], "[^0-9]", String.Empty));
				}
			}

			return employeeCount;
		}
	}
}
