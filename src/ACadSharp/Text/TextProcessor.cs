using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Text
{
	public static class TextProcessor
	{
		public static string Parse(string text, out List<string> groups)
		{
			groups = new List<string>();
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			StringBuilder sb = new StringBuilder();

			int index = 0;
			bool openGroup = false;
			while (index < text.Length)
			{
				char? prev = text.TryGet(index - 1);
				char? current = text.TryGet(index);
				char? next = text.TryGet(index + 1);

				if (current == '\\' && next.HasValue)
				{
					switch (next)
					{
						case '}':
						case '{':
						case '\\':
							sb.Append(next);
							index += 2;
							break;
						case 'A':
						case 'Q':
						case 'T':
						case 'W':
							jump(text, index, out index);
							break;
						case 'L':
						case 'l':
						case 'O':
						case 'o':
						case 'K':
						case 'k':
							index += 2;
							break;
						case '~':
							sb.Append(' ');
							index += 2;
							break;
						case 'S':
							processStacking(text, index, sb, out index);
							break;
						case 'c':
						case 'C':
							processColor(text, index, out index);
							break;
						case 'f':
						case 'F':
							processFont(text, index, out index);
							break;
						case 'h':
						case 'H':
							processHeight(text, index, out index);
							break;
						case 'p':
							processJustification(text, index, out index);
							break;
						case 'P':
						case 'n':
						case 'N':
							sb.Append(Environment.NewLine);
							index += 2;
							break;
						default:
							index++;
							break;
					}
				}
				else if (current == '{' && prev != '\\')
				{
					openGroup = true;
					index++;
				}
				else if (current == '}' && prev != '\\')
				{
					openGroup = false;
					index++;
				}
				else
				{
					sb.Append(current);
					index++;
				}
			}

			return sb.ToString();
		}


		public static string Unescape(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			StringBuilder sb = new StringBuilder();

			int index = 0;
			bool openGroup = false;
			while (index < text.Length)
			{
				int currIndex = text.IndexOf(@"\", index);
				if (currIndex <= 0)
				{
					string s = text.Substring(index, text.Length - index);
					if (openGroup && s.Contains("}"))
					{
						s = s.Replace("}", string.Empty);
						openGroup = false;
					}
					sb.Append(s);
					break;
				}

				char prev = text[currIndex - 1];
				char current = text[currIndex];
				char next = text[currIndex + 1];

				if (prev == '{')
				{
					currIndex--;
					openGroup = true;
				}

				if (currIndex > index)
				{
					string s = text.Substring(index, currIndex - index);
					if (openGroup && s.Contains("}"))
					{
						s = s.Replace("}", string.Empty);
						openGroup = false;
					}
					sb.Append(s);
				}

				int f;
				switch (next)
				{
					case 'f':
					case 'F':
						processFont(text, currIndex, out f);
						currIndex = f;
						break;
					case 'c':
					case 'C':
						processColor(text, currIndex, out f);
						currIndex = f;
						break;
					case 'P':
					case 'n':
						sb.Append(Environment.NewLine);
						currIndex += 2;
						break;
					case 'r':
						break;
					case '}':
					case '{':
					case '\\':
						sb.Append(next);
						break;
				}

				index = currIndex;
			}

			return sb.ToString();
		}

		private static void processStacking(string text, int start, StringBuilder output, out int end)
		{
			int index = start + 2;
			bool separated = false;
			while (index < text.Length && text[index] != ';')
			{
				char ch = text[index];
				if (ch == '\\' && index + 1 < text.Length)
					ch = text[++index];
				else if (!separated && (ch == '^' || ch == '/' || ch == '#'))
				{
					separated = true;
					ch = '/';
				}
				output.Append(ch);
				index++;
			}
			end = Math.Min(index + 1, text.Length);
		}

		private static void processFont(string text, int start, out int end) => jump(text, start, out end);

		private static void jump(string text, int start, out int end)
		{
			int terminator = text.IndexOf(';', start);
			end = terminator < 0 ? text.Length : terminator + 1;
		}

		private static void processColor(string text, int start, out int end) => jump(text, start, out end);

		private static void processHeight(string text, int start, out int end) => jump(text, start, out end);

		private static void processJustification(string text, int start, out int end) => jump(text, start, out end);

	}

	internal struct FontData
	{
		public string Name { get; set; }
	}
}
