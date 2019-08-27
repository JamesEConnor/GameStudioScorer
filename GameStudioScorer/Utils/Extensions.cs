using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using GameStudioScorer.Wiki;

namespace GameStudioScorer.Extensions
{
	//Various extension methods.
	public static class Extensions
	{
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

			//If there was no Wikipedia entry, return a default of 100 employees.
			int employeeCount = 100;

			//Search through each fact until one is found containing the word 'employee'
			//(as in, "number of employees", "employees", or "employee count")
			foreach (var key in dict.Keys)
			{
				if (key.Contains("employee"))
				{
					//Use some Regex to remove the html stuff.
					string val = Regex.Replace(dict[key], "<.+?>.+<\\/.+?>", "");
					//Use some Regex to remove everything but the numbers.
					string result = Regex.Replace(val, "[^\\d]+", "");

					//If the result is parsable, the number of employees has been found!
					if (int.TryParse(result, out employeeCount))
						break;

					//employeeCount = int.Parse(Regex.Replace(dict[key].Split(new char[] { '_', ' ' })[0], "[^0-9]", String.Empty));
				}
			}

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
	}
}
