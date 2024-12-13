using System;
using System.Collections.Generic;
using System.Linq;

namespace CSUtilities.Extensions
{
	/// <summary>
	/// String utility extensions.
	/// </summary>
#if PUBLIC
	public
#else
	internal
#endif
	static class StringExtensions
	{
		/// <summary>
		/// Indicates if the specified string is null
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns></returns>
		public static bool IsNull(this string str)
		{
			return str == null;
		}

		/// <summary>
		/// Indicates whether the specified string is null or an empty string ("").
		/// </summary>
		/// <param name="str">The string to test.</param>
		/// <returns>true if the value parameter is null or an empty string (""); otherwise, false.</returns>
		public static bool IsNullOrEmpty(this string str)
		{
			return string.IsNullOrEmpty(str);
		}

		/// <summary>
		/// Indicates whether a specified string is null, empty, or consists only of white-space characters.
		/// </summary>
		/// <param name="str"></param>
		/// <returns>true if the value parameter is null or System.String.Empty, or if value consists exclusively of white-space characters.</returns>
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return string.IsNullOrWhiteSpace(str);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void TrowIfNullOrEmpty(this string str)
		{
			str.TrowIfNullOrEmpty("String cannot be null or empty");
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <param name="message"></param>
		/// <exception cref="ArgumentException"></exception>
		public static void TrowIfNullOrEmpty(this string str, string message)
		{
			if (string.IsNullOrEmpty(str))
			{
				throw new ArgumentException(message);
			}
		}

		/// <summary>
		/// Return an array with all the lines.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string[] GetLines(this string str)
		{
			//Guard
			if (str == null)
				return null;

			str = str.Replace("\r\n", "\n");

			string[] lines = str.Split('\n');

			if (string.IsNullOrEmpty(lines.Last()))
			{
				//Delete the last empty line
				lines = lines.Take(lines.Length - 1).ToArray();
			}

			return lines;
		}

		/// <summary>
		/// Return if the string is numeric.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsNumeric(this string s)
		{
			return double.TryParse(s, out _);
		}

		/// <summary>
		/// Gets a string and returns an array of bytes.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static byte[] ToByteArray(this string str)
		{
			return Enumerable.Range(0, str.Length)
				.Where(x => x % 2 == 0)
				.Select(x => Convert.ToByte(str.Substring(x, 2), 16))
				.ToArray();
		}

		/// <summary>
		/// Returns the first string between 2 characters.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="keepTokens"></param>
		/// <exception cref="FormatException">The line is not closed by the 2 characters.</exception>
		/// <returns></returns>
		public static string ReadBetween(this string str, char start, char end, bool keepTokens = false)
		{
			if (str.TryReadBetween(start, end, out string group, keepTokens))
			{
				return group;
			}
			else
			{
				throw new FormatException("Closing character not found, this is an open line.");
			}
		}

		/// <summary>
		/// Reads between 2 characters, but returns a value even if the group is not closed.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="group"></param>
		/// <param name="keepTokens"></param>
		/// <returns></returns>
		public static bool TryReadBetween(this string s, char start, char end, out string group, bool keepTokens = false)
		{
			Stack<int> stack = new Stack<int>();
			bool isFirst = true;
			group = "";

			for (int i = 0; i < s.Length; i++)
			{
				//Check if is the ending token
				if (s[i] == end)
				{
					//Check if the sequence contains an open
					if (!isFirst)
					{
						stack.Pop();

						//Closing character found
						if (!stack.Any())
						{
							if (keepTokens)
								//Keep the token in the group
								group += s[i];

							return true;
						}
					}
				}

				if (s[i] == start)
				{
					//Save the index of the open character
					stack.Push(i);

					//Save the index of the first open char
					if (isFirst)
					{
						if (keepTokens)
							//Keep the token in the group
							group += s[i];

						isFirst = false;
						continue;
					}
				}

				//If the first open character have been found, start reading the string
				if (!isFirst)
				{
					group += s[i];
				}
			}

			return false;
		}

		/// <summary>
		/// Reads a string until it finds a character.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="c">Character to find.</param>
		/// <returns></returns>
		public static string ReadUntil(this string str, char c)
		{
			return str.ReadUntil(c, out _);
		}

		/// <summary>
		/// Reads a string until it finds a character.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="c"></param>
		/// <param name="residual">The last part of the string.</param>
		/// <returns></returns>
		public static string ReadUntil(this string str, char c, out string residual)
		{
			string value = "";
			residual = "";

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == c)
				{
					residual += str.Substring(i);
					break;
				}
				else
					value += str[i];
			}

			return value;

		}

		/// <summary>
		/// Remove all the first whitespaces in a string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveStartWhitespaces(this string str)
		{
			while (str.StartsWith(" "))
			{
				str = str.Remove(0, 1);
			}

			return str;
		}

		/// <summary>
		/// Remove the last character of a string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string RemoveLast(this string str)
		{
			return str.Remove(str.Length - 1);
		}

		/// <summary>
		/// Find the first character in the list of tokens.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="characters"></param>
		/// <returns></returns>
		public static char? FirstEqual(this string str, IEnumerable<char> characters)
		{
			return FirstEqual(str, characters, out _);
		}

		/// <summary>
		/// Find the first character in the list of tokens.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="characters"></param>
		/// <param name="index">Index of the found character</param>
		/// <returns></returns>
		public static char? FirstEqual(this string str, IEnumerable<char> characters, out int? index)
		{
			char? token = null;
			index = null;

			foreach (char item in characters)
			{
				//string tmp = m_buffer.Substring(m_currIndex);
				int curr = str.IndexOf(item);

				//Get the next token in the buffer
				if ((index == null || curr < index) && curr > -1 /*&& curr >= m_currIndex*/)
				{
					//pos = m_buffer.IndexOf(item);
					index = curr;
					token = item;
				}
			}

			return token;
		}

		/// <summary>
		/// Split an string by spaces and substrings between collons.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="keepCollons"></param>
		/// <param name="ignoreEmpty"></param>
		/// <returns></returns>
		public static string[] ToArgs(this string str, bool keepCollons = false, bool ignoreEmpty = true)
		{
			return str.ToArgs(' ', '"', keepCollons, ignoreEmpty);
		}

		/// <summary>
		/// Split an string by spaces and substrings between collons.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="separator"></param>
		/// <param name="keepCollons"></param>
		/// <param name="ignoreEmpty"></param>
		/// <returns></returns>
		public static string[] ToArgs(this string str, char separator, bool keepCollons = false, bool ignoreEmpty = true)
		{
			return str.ToArgs(separator, '"', keepCollons, ignoreEmpty);
		}

		public static string[] ToArgs(this string str, char separator, char stringDelimitier, bool keepCollons = false, bool ignoreEmpty = true)
		{
			List<string> args = new List<string>();
			string word = "";
			bool isReading = false;

			foreach (char c in str)
			{
				//Open or close the string
				if (c == stringDelimitier)
				{
					isReading = !isReading;

					if (keepCollons)
						word += c;

					continue;
				}

				//Ignore the separator outside the strings
				if (c == separator && !isReading)
				{
					//Avoid empty words (multiple spaces)
					if (String.IsNullOrEmpty(word) && ignoreEmpty)
						continue;

					args.Add(word);
					word = "";
				}
				else
				{
					word += c;
				}
			}

			//Add the last word
			if (String.IsNullOrEmpty(word))
			{
				if (!ignoreEmpty)
					args.Add(word);
			}
			else
				args.Add(word);

			return args.ToArray();
		}

		public static string[] ToArgs(this string str, char separator, IDictionary<char, char> groupDelimitiers, bool keepTokens = false)
		{
			List<string> args = new List<string>();
			List<char> tokens = new List<char>(groupDelimitiers.Keys);
			tokens.Add(separator);

			for (int i = 0; i < str.Length; i++)
			{
				string sub = str.Substring(i);
				char? token = str.Substring(i).FirstEqual(tokens, out int? pos);

				if (token == null)
				{
					string arg = str.Substring(i);

					args.Add(arg);
					i += arg.Length;
				}
				else if (token == separator)
				{
					string arg = str.SubstringByIndex(i, i + pos.Value);

					//Check if the argument is not empty
					if (!string.IsNullOrEmpty(arg))
					{
						args.Add(arg);
						i += arg.Length;
					}
				}
				else if (groupDelimitiers.ContainsKey(token.Value))
				{
					string group = str.Substring(i + pos.Value).ReadBetween(token.Value, groupDelimitiers[token.Value], keepTokens);
					args.Add(str.Substring(i).ReadUntil(token.Value) + group);

					//Jump the last group character
					i += group.Length;
				}
			}

			return args.ToArray();
		}

		public static string SubstringByIndex(this string s, int startIndex, int endIndex)
		{
			return s.Remove(endIndex).Substring(startIndex);
		}
	}
}
