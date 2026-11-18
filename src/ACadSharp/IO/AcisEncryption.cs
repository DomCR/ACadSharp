using System;
using System.Text;

namespace ACadSharp.IO;

/// <summary>
/// Provides encoding, decoding, and binary data utilities for ACIS data.
/// </summary>
/// <remarks>Supports ACIS text encoding and decoding, binary signature detection, byte swapping, and trimming
/// data at ACIS end markers.</remarks>
public class AcisEncryption
{
	private static readonly byte[] _binarySignature = Encoding.ASCII.GetBytes("ACIS BinaryFile");

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

	/// <summary>
	/// Decodes ACIS text that was encoded with the DXF/DWG character substitution.
	/// </summary>
	/// <param name="text">The text to decode.</param>
	/// <returns>
	/// The decoded text, or the original <paramref name="text"/> if it is <see langword="null"/>, empty or not encoded.
	/// </returns>
	/// <seealso cref="IsEncoded(string)"/>
	public static string Decode(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}

		if (!IsEncoded(text))
		{
			return text;
		}

		int length = text.Length;
		StringBuilder sb = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (c == ' ' || c < ' ')
			{
				sb.Append(c);
			}
			else if (c == '^' || c >= '?' && c <= '_')
			{
				sb.Append((char)(0x9F - c));
			}
			else
			{
				sb.Append((char)(c ^ 0x5Fu));
			}
		}
		return sb.ToString();
	}

	/// <summary>
	/// Encodes plain ACIS text using the DXF/DWG character substitution.
	/// </summary>
	/// <remarks>
	/// Printable characters are mirrored (<c>0x9F - c</c>), tabs are converted to spaces and line breaks are preserved.
	/// </remarks>
	/// <param name="text">The text to encode.</param>
	/// <returns>
	/// The encoded text, or the original <paramref name="text"/> if it is <see langword="null"/> or already encoded.
	/// </returns>
	/// <seealso cref="IsEncoded(string)"/>
	public static string Encode(string text)
	{
		if (text == null)
		{
			return text;
		}

		if (IsEncoded(text))
		{
			return text;
		}

		int length = text.Length;
		StringBuilder sb = new StringBuilder(length + 8);
		for (int i = 0; i < length; i++)
		{
			char c = text[i];
			if (c > ' ' && c < '\u00a0')
			{
				c = (char)(0x9F - c);
			}
			else if (c == '\t')
			{
				c = ' ';
			}

			if (c == '\r' || c == '\n')
			{
				sb.Append(c);
			}
			else if (c < ' ')
			{
				sb.Append('^');
				sb.Append(c + 0x40);
			}
			else if (c == '^')
			{
				sb.Append('^');
			}
			else
			{
				sb.Append(c);
			}
		}
		return sb.ToString();
	}

	/// <summary>
	/// Determines whether the data starts with the ACIS binary file signature (<c>ACIS BinaryFile</c>).
	/// </summary>
	/// <param name="data">The data to inspect.</param>
	/// <returns>
	/// <see langword="true"/> if the data begins with the binary signature; otherwise <see langword="false"/>,
	/// including when <paramref name="data"/> is <see langword="null"/> or too short.
	/// </returns>
	public static bool IsBinary(byte[] data)
	{
		if (data == null || data.Length < _binarySignature.Length)
		{
			return false;
		}

		for (int i = 0; i < _binarySignature.Length; i++)
		{
			if (data[i] != _binarySignature[i])
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Determines whether the text is encoded.
	/// </summary>
	/// <remarks>
	/// Plain ACIS text starts with a number, so the text is considered encoded when its first
	/// non-whitespace character is not a digit.
	/// </remarks>
	/// <param name="text">The text to inspect.</param>
	/// <returns>
	/// <see langword="true"/> if the text is encoded; otherwise <see langword="false"/>, including when
	/// <paramref name="text"/> is <see langword="null"/>, empty or contains only whitespace.
	/// </returns>
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

	/// <summary>
	/// Swaps the bytes of the ACIS binary data using the <c>0x9F - b</c> substitution.
	/// </summary>
	/// <remarks>
	/// Only bytes in the range <c>0x21</c> to <c>0x9E</c> are modified. The operation is symmetric,
	/// applying it twice returns the original data.
	/// </remarks>
	/// <param name="data">The data to swap.</param>
	/// <returns>
	/// A new array with the swapped bytes, or the original <paramref name="data"/> if it is <see langword="null"/> or empty.
	/// </returns>
	public static byte[] Swap(byte[] data)
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
	/// Cuts the data right after the first ACIS end marker (<c>End-of-ACIS-data</c> or <c>End-of-ASM-data</c>,
	/// in text or binary form).
	/// </summary>
	/// <param name="data">The data to trim.</param>
	/// <param name="remaining">
	/// The bytes that follow the end marker, or an empty array if no marker is found or nothing follows it.
	/// </param>
	/// <returns>
	/// The data up to and including the end marker, or the original <paramref name="data"/> if no marker is found.
	/// </returns>
	public static byte[] TrimAtAcisEnd(byte[] data, out byte[] remaining)
	{
		remaining = Array.Empty<byte>();
		if (data == null || data.Length == 0)
		{
			return data;
		}

		foreach (byte[] marker in _endMarkers)
		{
			int index = indexOf(data, marker, 0);
			if (index >= 0)
			{
				return cutAfter(data, index + marker.Length, out remaining);
			}
		}

		return data;
	}

	private static byte[] cutAfter(byte[] data, int end, out byte[] remain)
	{
		if (end >= data.Length)
		{
			remain = Array.Empty<byte>();
			return data;
		}

		byte[] result = new byte[end];
		remain = new byte[data.Length - end];

		Array.Copy(data, result, end);
		Array.Copy(data, end, remain, 0, remain.Length);
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
}