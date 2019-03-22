using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
	public static class TextHelpers
	{
		/// <summary>
		/// Trims the input string to the required length
		/// </summary>
		/// <param name="input">Input string</param>
		/// <param name="maxLength">Maximum length of the string</param>
		/// <returns>Trimmed string</returns>
		/// <remarks>
		/// This is an extension method so it can be called upon a string object.
		/// Example: s.SafeTrim(10);
		/// </remarks>
		public static string SafeTrim(this string input, int maxLength)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(input))
				{
					return string.Empty;
				}
				input = input.TrimEnd();
				if (input.Length > maxLength)
				{
					return input.Substring(0, maxLength);
				}
				return input;
			}
			catch (Exception ex)
			{
				string location = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Namespace + "." + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
				Console.WriteLine("An unexpected error occurred in {0}: {1}", location, ex.Message);
				throw ex;
			}
		}

		/// <summary>
		/// Convert the specified enum value to a mixed-case string with white space. 
		/// Add a space before every capital letter in the string, except if:
		///   - The capital letter is the first in the string
		///   - The capital letter is already preceded by white space
		///   - The capital letter is preceded by another capital letter
		/// Also add a space before the last capital letter in a sequence of
		/// capital letters, when the next letter is lower-case.
		/// </summary>
		/// <param name="source">Enum value to convert</param>
		/// <returns>The resultant string</returns>
		/// <remarks>
		/// This is an extension method so it can be called upon an enum object.
		/// Example: someEnum.WordBreakMixedCase();
		/// </remarks>
		public static string WordBreakMixedCase(this Enum source)
		{
			return(WordBreakMixedCase(source.ToString()));
		}

		/// <summary>
		/// Convert the specified string to a mixed-case string with white space. 
		/// Add a space before every capital letter in the specified string,
		/// except if:
		/// 
		///      - The capital letter is the first in the string
		///      - The capital letter is already preceded by white space
		///      - The capital letter is preceded by another capital letter
		///
		/// Also add a space before the last capital letter in a sequence of
		/// capital letters, when the next letter is lower-case.      
		/// </summary>
		/// <param name="source">The string to modify</param>
		/// <returns>The modified string</returns>
		/// <remarks>
		/// This is an extension method so it can be called upon a string object.
		/// Example: s.WordBreakMixedCase();
		/// </remarks>
		public static string WordBreakMixedCase(this string source)
		{
			StringBuilder result = new StringBuilder();
			int length = source.Length;

			for (int i = 0; i < length; i++)
			{
				if (0 == i || length - 1 == i)
				{
					result.Append(source[i]);
				}
				else if (!char.IsWhiteSpace(source[i]) && !char.IsUpper(source[i]))
				{
					int len = result.Length;

					if (char.IsUpper(source[i + 1]))
					{
						result.Append(source[i]).Append(' ');
					}
					else
					{
						result.Append(source[i]);
					}

					if (2 <= i && Char.IsUpper(source[i - 1]) && Char.IsUpper(source[i - 2]))
					{
						result.Insert(len - 1, ' ');
					}
				}
				else
				{
					result.Append(source[i]);
				}
			}
			return result.ToString();
		}


	}
}
