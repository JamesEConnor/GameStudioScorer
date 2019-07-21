using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using GameStudioScorer.Wiki;

namespace GameStudioScorer.Extensions
{
	public static class Extensions
	{
		public static string GetString<T>(this T[] array)
		{
			string result = "{";
			foreach (T el in array)
				result += el.ToString() + "|";
			return result.Remove(result.Length - 1) + "}";
		}

		public static int MaxIndex<T>(this IEnumerable<T> array)
			where T : IComparable<T>
		{
			int maxIndex = -1;
			T maxValue = default(T);

			int index = 0;
			foreach (T element in array)
			{
				if (element.CompareTo(maxValue) > 0 || maxIndex == -1)
				{
					maxIndex = index;
					maxValue = element;
				}
				index++;
			}

			return maxIndex;
		}

		public static int[] LoadGameYears(string str)
		{
			str = str.Substring(1);
			str = str.Remove(str.Length - 1);

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
					string val = Regex.Replace(dict[key], "<.+?>.+<\\/.+?>", "");
					string result = Regex.Replace(val, "[^\\d]+", "");
					if (int.TryParse(result, out employeeCount))
						break;

					//employeeCount = int.Parse(Regex.Replace(dict[key].Split(new char[] { '_', ' ' })[0], "[^0-9]", String.Empty));
				}
			}

			return employeeCount;
		}

		public static XmlNode[] ConvertToList(XmlNodeList list)
		{
			XmlNode[] result = new XmlNode[list.Count];
			for (int a = 0; a < result.Length; a++)
				result[a] = list[a];

			return result;
		}
	}
}
