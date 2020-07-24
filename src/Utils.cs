using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomEvents
{
	public static class Utils
	{
		private static System.Random rng = new System.Random();

		public static SDateTime RemoveDateTime(SDateTime old, int months)
		{
			return old - new SDateTime(months, 0);
		}

		public static void ShuffleList<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static string UpperFirstLetters(string str)
		{
			string[] arr = str.Split('_');
			string result = "";

			foreach(string word in arr)
			{
				if(result == "")
					result = word.First().ToString().ToUpper() + word.Substring(1);
				else
					result += " " + word.First().ToString().ToUpper() + word.Substring(1);
			}
			return result;
		}

		public static float GetPercentage(float value, float percentage, bool subtract = true)
		{
			float perc = (percentage / 100) * value;
			if (subtract)
				value -= perc;
			else
				value += perc;
			return value;
		}

		public static string ReplaceValues(string str, string[] values, string[] replacements)
		{
			if (values.Length != replacements.Length)
				return "{VALUES != REPLACEMENTS}";

			for(int i = 0; i < values.Length; i++)
			{
				str.Replace(values[i], replacements[i]);
			}
			return str;
		}

		public static string ReplaceValues(string str, string values, string replacements)
		{
			return ReplaceValues(str, values.Split('|'), replacements.Split('|'));
		}
	}
}
