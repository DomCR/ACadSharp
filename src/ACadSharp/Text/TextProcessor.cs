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
			while (index < text.Length)
			{
				int currIndex = text.IndexOf(@"\", index);
				if (currIndex <= 0)
				{
					break;
				}

				char prev = text[currIndex - 1];
				char next = text[currIndex + 1];

				if (prev == '{')
				{
					currIndex--;
				}

				if (currIndex > index)
				{
					sb.Append(text.Substring(index, currIndex - index));
				}

				switch (next)
				{
					case 'f':
					case 'F':
						//Process font
						processFont(text, currIndex, out int f);
						currIndex = f;
						var a = text.Substring(currIndex, f - currIndex);
						break;
					case 'c':
					case 'C':
						break;
					case 'P':
					case 'n':
						sb.Append(Environment.NewLine);
						currIndex += 1;
						break;
					case '{':
					case '}':
					case '\\':
					default:
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
	}

	internal struct FontData
	{
		public string Name { get; set; }
	}
}
