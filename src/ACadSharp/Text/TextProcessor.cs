using System;
using System.Text;

namespace ACadSharp.Text
{
	public static class TextProcessor
	{
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

		private static void processFont(string text, int start, out int end)
		{
			//Example:
			//- Font: {\\fCalibri|b0|i0|c0|p34;Calibri\\Fcdm|c0; CDM \\fConsolas|b0|i0|c0|p49;Consolas\\P}
			end = text.IndexOf(';', start);
			end += 1;
			var data = text.Substring(start, end - start).Split('|');

			FontData fontData = new FontData
			{
				Name = data[0],
			};
		}

		private static void processColor(string text, int start, out int end)
		{
			//Example:
			//- Color text {\\C3;green}, {\\C5;blue}, {\\C1;red}, ByLayer, {\\C0;ByBlock}, {\\C21;\\c5872631;TrueColor}
			end = text.IndexOf(';', start);
			end += 1;
		}
	}

	internal struct FontData
	{
		public string Name { get; set; }
	}
}
