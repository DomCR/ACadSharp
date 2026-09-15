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
		/// Decodes (or encodes) a buffer of character-swapped SAT text.
		/// </summary>
		public static byte[] Decode(byte[] data)
		{
			if (data == null || data.Length == 0)
			{
				return data;
			}

			byte[] result = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				byte b = data[i];
				result[i] = b > 0x20 && b < 0x9F ? (byte)(0x9F - b) : b;
			}

			return result;
		}

		/// <summary>
		/// Trims an ACIS payload at its end marker.
		/// </summary>
		/// <remarks>
		/// The DWG entity stream gives no length for the payload, so the caller
		/// reads up to the end of the object data and this method cuts the result
		/// right after the End-of-ACIS-data marker. SAT text and single-tag SAB
		/// records spell the marker as plain ASCII; older SAB writers emit it as a
		/// compound entity name split in tagged chunks. The payload is returned
		/// unchanged when no marker is found.
		/// </remarks>
		public static byte[] TrimAtAcisEnd(byte[] data)
		{
			if (data == null || data.Length == 0)
			{
				return data;
			}

			foreach (byte[] marker in _endMarkers)
			{
				int index = indexOf(data, marker, 0);
				if (index >= 0)
				{
					return cutAfter(data, index + marker.Length);
				}
			}

			return data;
		}

		//End-of-ACIS-data / End-of-ASM-data markers: the name depends on the kernel
		//that wrote the payload, spelled as plain ASCII (SAT text and single-tag SAB
		//records) or as a compound SAB entity name split in tagged chunks.
		private static readonly byte[][] _endMarkers = new byte[][]
		{
			Encoding.ASCII.GetBytes("End-of-ACIS-data"),
			Encoding.ASCII.GetBytes("End-of-ASM-data"),
			new byte[]
			{
				0x0E, 0x03, (byte)'E', (byte)'n', (byte)'d',
				0x0E, 0x02, (byte)'o', (byte)'f',
				0x0E, 0x04, (byte)'A', (byte)'C', (byte)'I', (byte)'S',
				0x0D, 0x04, (byte)'d', (byte)'a', (byte)'t', (byte)'a'
			},
			new byte[]
			{
				0x0E, 0x03, (byte)'E', (byte)'n', (byte)'d',
				0x0E, 0x02, (byte)'o', (byte)'f',
				0x0E, 0x03, (byte)'A', (byte)'S', (byte)'M',
				0x0D, 0x04, (byte)'d', (byte)'a', (byte)'t', (byte)'a'
			}
		};

		private static byte[] cutAfter(byte[] data, int end)
		{
			if (end >= data.Length)
			{
				return data;
			}

			byte[] result = new byte[end];
			System.Array.Copy(data, result, end);
			return result;
		}

		private static int indexOf(byte[] haystack, byte[] needle, int start)
		{
			for (int i = start; i <= haystack.Length - needle.Length; i++)
			{
				bool match = true;
				for (int j = 0; j < needle.Length; j++)
				{
					if (haystack[i + j] != needle[j])
					{
						match = false;
						break;
					}
				}

				if (match)
				{
					return i;
				}
			}

			return -1;
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
