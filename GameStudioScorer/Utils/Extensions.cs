using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using GameStudioScorer.Wiki;

namespace GameStudioScorer.Extensions
{
	//Various extension methods.
	public static class Extensions
	{
		//Used for removing company suffixes from names.
		//https://en.wikipedia.org/wiki/List_of_legal_entity_types_by_country
		static readonly string[] COMPANY_SUFFIXES = {
			"lp", "llp", "lllp", "llc", "lc", "ltd", "co", "pllc", "corp", "pc", "cic", "plc", "cyf", "ccc", "inc", "ent", "ltd co", "coop", "gp"
		};

		/// <summary>
		/// Turns an array into a string. Good for debugging.
		/// </summary>
		/// <returns>The array in string form.</returns>
		/// <param name="array">An array.</param>
		/// <typeparam name="T">The array type.</typeparam>
		public static string GetString<T>(this T[] array)
		{
			string result = "{";
			foreach (T el in array)
				result += el.ToString() + "|";
			return result.Remove(result.Length - 1) + "}";

			//Returns an array in the format: {1|2|3|4|5}
		}

		/// <summary>
		/// Gets which index has the highest value.
		/// </summary>
		/// <returns>The index with the highest value.</returns>
		/// <param name="array">The array of numbers.</param>
		/// <typeparam name="T">The array type (must be an ienumerable).</typeparam>
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

		/// <summary>
		/// Converts the string of game years in the local cache to an array of integers.
		/// </summary>
		/// <returns>The game years, in an array of integers.</returns>
		/// <param name="str">The game years as a string, in the form that they're stored
		/// in the local cache.</param>
		public static int[] LoadGameYears(string str)
		{
			//Remove the opening and closing brackets.
			str = str.Substring(1);
			str = str.Remove(str.Length - 1);

			//Split between the pipes and parse.
			string[] split = str.Split('|');
			int[] result = new int[split.Length];
			for (int a = 0; a < result.Length; a++)
				result[a] = int.Parse(split[a]);

			return result;
		}

		/// <summary>
		/// Converts the string of weights from a model file to an array of doubles.
		/// </summary>
		/// <returns>The weights, in an array of doubles.</returns>
		/// <param name="str">The model weights as a string, in the form that they're stored
		/// in the model file.</param>
		public static double[] LoadWeights(string str)
		{
			//Remove the opening and closing brackets.
			str = str.Substring(1);
			str = str.Remove(str.Length - 1);

			//Split between the pipes and parse.
			string[] split = str.Split('|');
			double[] result = new double[split.Length];
			for (int a = 0; a < result.Length; a++)
				result[a] = double.Parse(split[a]);

			return result;
		}

		/// <summary>
		/// Gets the number of employees for a company from Wikipedia.
		/// </summary>
		/// <returns>The number of employees.</returns>
		/// <param name="companyName">The company name.</param>
		public static int GetEmployeeCount(string companyName)
		{
			//Get all information from the quick facts sidebar.
			Dictionary<string, string> dict = WikipediaRetriever.GetWikiInfo(companyName);

			int employeeCount = 0;

			//Search through each fact until one is found containing the word 'employee'
			//(as in, "number of employees", "employees", or "employee count")
			foreach (var key in dict.Keys)
			{
				if (key.Contains("employee"))
				{
					//Use some Regex to remove the html stuff.
					string val = Regex.Replace(dict[key], "<.+?>.+<\\/.+?>", "");
					//Use some Regex to remove everything but the numbers.

					if (val.Contains('-'))
						val = val.Substring(0, val.IndexOf('-'));

					string result = Regex.Replace(val, "[^\\d]+", "");

					//If the result is parsable, the number of employees has been found!
					if (int.TryParse(result, out employeeCount))
						break;

					//employeeCount = int.Parse(Regex.Replace(dict[key].Split(new char[] { '_', ' ' })[0], "[^0-9]", String.Empty));
				}
			}

			//If there was no Wikipedia entry, return a default of 100 employees.
			if (employeeCount == 0)
				employeeCount = 100;

			//Return it.
			return employeeCount;
		}

		/// <summary>
		/// Converts an XML Node List to an array of XML Nodes.
		/// </summary>
		/// <returns>The array of XML Nodes.</returns>
		/// <param name="list">The XML Node List.</param>
		public static XmlNode[] ConvertToList(XmlNodeList list)
		{
			XmlNode[] result = new XmlNode[list.Count];
			for (int a = 0; a < result.Length; a++)
				result[a] = list[a];

			return result;
		}

		/// <summary>
		/// Computes the Levenshtein distance between two strings.
		/// </summary>
		/// <param name="s">The first string.</param>
		/// <param name="t">The second string.</param>
		/// Source from here: https://stackoverflow.com/questions/13793560/find-closest-match-to-input-string-in-a-list-of-strings
		/// Learn more here: http://en.wikipedia.org/wiki/Levenshtein_distance
		public static int LevenshteinDistance(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}

		/// <summary>
		/// Removes the company suffix from a company name.
		/// </summary>
		/// <returns>The name, without a company suffix.</returns>
		/// <param name="name">A company name.</param>
		public static string removeCompanySuffix(this string name)
		{
			name = name.Trim();

			//If the name is only one word, it doesn't have a company suffix.
			if (!name.Contains(" "))
				return name;

			//Gets last 'word' in name, which could be a company suffix.
			string suffix = name.Substring(name.LastIndexOf(' ') + 1);
			suffix = suffix.Replace(".", "").Replace(",", "");

			//Is the suffix a company suffix?
			if (Array.IndexOf(COMPANY_SUFFIXES, suffix.ToLower()) > -1)
				return name.Substring(0, name.LastIndexOf(' ')).TrimEnd(new char[] { ',' });

			//If it's not a company suffix, return the name.
			return name;
		}

		/// <summary>
		/// Creates a list of possible alternate names for a company.
		/// </summary>
		/// <returns>A list of alternative studio names.</returns>
		/// <param name="name">The name of the studio.</param>
		public static List<string> CreateAliasList(string name)
		{
			List<string> result = new List<string>();
			result.Add(name);

			//Add name without "Games", to handle certain edge cases.
			if (name.EndsWith("Games", StringComparison.CurrentCultureIgnoreCase))
				result.Add(Regex.Replace(name, "Games", "", RegexOptions.IgnoreCase).Trim());
			else
				result.Add(name.Trim() + " Games");

			//Add name without "Studios", to handle certain edge cases.
			if(name.EndsWith("Studios", StringComparison.CurrentCultureIgnoreCase))
				result.Add(Regex.Replace(name, "Studios", "", RegexOptions.IgnoreCase).Trim());
			else
				result.Add(name.Trim() + " Studios");

			//Add name without "Studio", to handle certain edge cases.
			if (name.EndsWith("Studio", StringComparison.CurrentCultureIgnoreCase))
				result.Add(Regex.Replace(name, "Studio", "", RegexOptions.IgnoreCase).Trim());
			else
				result.Add(name.Trim() + " Studio");


			return result;
		}

		//Random for shuffling lists.
		private static Random rng = new Random();

		/// <summary>
		/// Suffles a provided Dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to shuffle.</param>
		public static void Shuffle<T1, T2>(this Dictionary<T1, T2> dict)
		{
			dict.OrderBy(x => rng.Next())
  				.ToDictionary(item => item.Key, item => item.Value);
		}

		/// <summary>
		/// Converts an array of binary integers (0's and 1's) to an array of booleans.
		/// </summary>
		/// <returns>An array of booleans.</returns>
		/// <param name="array">This array.</param>
		public static bool[] ToBoolArray(this int[] array)
		{
			bool[] result = new bool[array.Length];
			for (int a = 0; a < array.Length; a++)
				result[a] = (array[a] == 1);

			return result;
		}
	}
}
