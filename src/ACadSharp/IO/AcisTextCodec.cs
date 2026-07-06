using System.Text;

namespace ACadSharp.IO
{
	/// <summary>
	/// Codec for the character swap applied to the SAT text carried by the
	/// modeler geometry entities (3DSOLID, REGION, BODY) in pre-2013 files.
	/// </summary>
	/// <remarks>
	/// Each character above the space is stored as 0x9F minus its code, the rest
	/// pass through unchanged. The transformation is an involution: applying it
	/// twice returns the original text, so the same method encodes and decodes.
	/// </remarks>
	internal static class AcisTextCodec
	{
		/// <summary>
		/// Decodes (or encodes) a chunk of character-swapped SAT text.
		/// </summary>
		public static string Decode(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}

			StringBuilder sb = new StringBuilder(text.Length);
			foreach (char c in text)
			{
				if (c > ' ' && c < (char)0x9F)
				{
					sb.Append((char)(0x9F - c));
				}
				else
				{
					sb.Append(c);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Detects if a SAT chunk is character-swapped.
		/// </summary>
		/// <remarks>
		/// Plain SAT starts with the numeric header line (for example "400 0 1 0"),
		/// so a first printable character that is not a digit marks a swapped payload.
		/// </remarks>
		public static bool IsEncoded(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}

			foreach (char c in text)
			{
				if (c <= ' ')
				{
					continue;
				}

				return !char.IsDigit(c);
			}

			return false;
		}
	}
}
