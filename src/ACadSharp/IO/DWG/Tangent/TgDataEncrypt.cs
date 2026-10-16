using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Decoder for the layer Tianzheng puts between its numeric fields and the file.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A scalar field is not stored as a number. It is stored as a string whose characters are
	/// drawn from a small alphabet, and each pair of characters carries one character of the
	/// decimal text the value would have been printed as. <c>TgDataEncrypt::TDReadDouble</c> reads
	/// the string, decodes it and hands the result to <c>_wtof</c>.
	/// </para>
	/// <para>
	/// One pair decodes to <c>(c1 - K) * 26 + (c2 - K)</c>, where <c>K</c> comes from a table
	/// indexed by the tag and the context the object carries. For the drawings available here the
	/// layout name is empty, so the context is 0, the tag is 17 and <c>K</c> is <c>0xDF</c>. That
	/// the pair values land in <c>0..25</c> for every string in the corpus is what confirms the
	/// key rather than merely fitting it.
	/// </para>
	/// <para>
	/// The pair value then maps to a character through <see cref="Alphabet"/>, which is a table
	/// rather than a shift: the digits are not in value order. The table was recovered by decoding
	/// every payload in the corpus and reading the results as the dimensions they are — the
	/// column in the sample decodes to a height of 2600 next to the 600 by 600 section its outline
	/// gives, and those two come from different parts of the format, so they confirm each other.
	/// </para>
	/// </remarks>
	public static class TgDataEncrypt
	{
		/// <summary>
		/// The constant the character pairs are biased by.
		/// </summary>
		public const int DefaultKey = 0xDF;

		/// <summary>
		/// The value the decoded characters are exclusive-ored with, for a nested reader.
		/// </summary>
		/// <remarks>
		/// The reader derives it from the context it is handed: the high byte of context + 0x9D2.
		/// A class passing its own context lands at 0x0A, and a nested helper passing zero at 0x09,
		/// which is what makes the same bytes read as different characters.
		/// </remarks>
		public const byte NestedXor = 0x09;

		/// <inheritdoc cref="NestedXor"/>
		public const byte ObjectXor = 0x0A;

		/// <summary>
		/// The exclusive-or value that applies to a reader given this context.
		/// </summary>
		public static byte XorFor(int context)
		{
			return (byte)(((context + 0x9D2) >> 8) & 0xFF);
		}

		/// <summary>
		/// Decodes a string written through the obfuscation layer.
		/// </summary>
		/// <param name="cipher">The characters as the filer stored them.</param>
		/// <param name="xor">The value for the context the field was read in.</param>
		/// <param name="key">The pair bias.</param>
		/// <remarks>
		/// Three steps, and missing either of the last two is silent rather than loud.
		/// <para>
		/// Character pairs are read as one value and then exclusive-ored, so the same pair decodes
		/// to different characters in different contexts. A fixed pair-to-character table cannot
		/// express that, which is how an earlier version of this class came to hold a table that
		/// was self-consistent and wrong.
		/// </para>
		/// <para>
		/// The reader then deletes the characters at indices 0 and 2 of what it decoded — the
		/// second deletion is of index 1 <i>after</i> the first, which is the same as index 2 of
		/// the original — so the result is the first character plus everything from the fourth on.
		/// A number written by this layer carries two characters that are not part of it.
		/// </para>
		/// <para>
		/// The two deletions apply whatever the length, and that matters most for the shortest
		/// strings: a one-digit value is written as six characters and decodes to three, of which
		/// the middle one is the value. Passing the three through undecoded reads <c>254</c> where
		/// the object wrote <c>5</c>, which is a number and not a failure, so nothing complains.
		/// </para>
		/// </remarks>
		public static string Decode(string cipher, byte xor, int key = DefaultKey)
		{
			if (cipher == null)
			{
				throw new ArgumentNullException(nameof(cipher));
			}

			StringBuilder text = new StringBuilder(cipher.Length / 2);

			for (int i = 0; i + 1 < cipher.Length; i += 2)
			{
				int value = ((cipher[i] & 0xFF) - key) * 26 + ((cipher[i + 1] & 0xFF) - key);

				text.Append((char)(value ^ xor));
			}

			if (text.Length == 0)
			{
				return string.Empty;
			}

			//Deleting index 0 shifts the character that was at index 2 down to index 1, so the
			//second deletion is of the original index 2. What survives is the character that was at
			//index 1 and everything from index 3 on, and for a text of fewer than four characters
			//that is the single character at index 1.
			return text.Length < 4
				? text[1].ToString()
				: text[1] + text.ToString(3, text.Length - 3);
		}

		/// <summary>
		/// Whether a string is the ciphertext of a number rather than text.
		/// </summary>
		/// <remarks>
		/// Every character of an obfuscated pair carries a low byte of 0xC0 or more, which is a
		/// property of how the writer biases them. Text never does, so the shape separates the two
		/// without needing to know which field was being read.
		/// </remarks>
		public static bool IsCiphertext(string value)
		{
			if (string.IsNullOrEmpty(value) || value.Length < 2 || value.Length % 2 != 0)
			{
				return false;
			}

			foreach (char c in value)
			{
				if ((c & 0xFF) < 0xC0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Reads the number a field holds, for a nested reader.
		/// </summary>
		public static bool TryDecode(string cipher, out double value, byte xor, int key = DefaultKey)
		{
			value = 0;

			if (string.IsNullOrEmpty(cipher) || cipher.Length < 2 || cipher.Length % 2 != 0)
			{
				return false;
			}

			string text;

			try
			{
				text = Decode(cipher, xor, key);
			}
			catch (FormatException)
			{
				return false;
			}

			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

	}
}
