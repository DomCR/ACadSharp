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
		/// The pair value that stands for each character of the decoded text.
		/// </summary>
		/// <remarks>
		/// Twelve entries, being the ten digits, the decimal point and the sign. The digits are
		/// deliberately out of order: 6, 9 and 8 and 7 sit below 0.
		/// </remarks>
		public static readonly IReadOnlyDictionary<int, char> Alphabet = new Dictionary<int, char>
		{
			{ 39, '-' },
			{ 36, '.' },
			{ 50, '6' },
			{ 51, '9' },
			{ 56, '8' },
			{ 57, '7' },
			{ 58, '0' },
			{ 59, '1' },
			{ 60, '2' },
			{ 61, '3' },
			{ 62, '4' },
			{ 63, '5' },
		};

		/// <summary>
		/// The key that applies to the objects in the sample drawings.
		/// </summary>
		public const int DefaultKey = 0xDF;

		/// <summary>
		/// Reads the number stored in a string written through the obfuscation layer.
		/// </summary>
		/// <param name="cipher">Ciphertext as it was read from the file.</param>
		/// <param name="key">Key for the object, <see cref="DefaultKey"/> for the samples.</param>
		/// <returns>The decoded text.</returns>
		/// <exception cref="FormatException">A pair decodes to a value outside the alphabet.</exception>
		public static string Decode(string cipher, int key = DefaultKey)
		{
			if (cipher == null)
			{
				throw new ArgumentNullException(nameof(cipher));
			}

			StringBuilder text = new StringBuilder(cipher.Length / 2);

			for (int i = 0; i + 1 < cipher.Length; i += 2)
			{
				int value = ((cipher[i] & 0xFF) - key) * 26 + ((cipher[i + 1] & 0xFF) - key);

				if (!Alphabet.TryGetValue(value, out char character))
				{
					throw new FormatException($"Pair at {i} decodes to {value}, which is not in the alphabet");
				}

				text.Append(character);
			}

			return text.ToString();
		}

		/// <summary>
		/// Reads every obfuscated number in a class specific payload.
		/// </summary>
		/// <remarks>
		/// The scalar fields of a class sit in its payload with nothing to delimit them, so they
		/// are found the way the other readers find their data: by looking for what they are. A
		/// ciphertext string is recognisable because every character of it has a low byte of
		/// <c>0xC0</c> or more, which no ordinary text and no class id does, and because it decodes
		/// to a number. Strings that fail either test are skipped.
		/// </remarks>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="key">Key for the object, <see cref="DefaultKey"/> for the samples.</param>
		/// <returns>The values in the order they appear; never null.</returns>
		public static IReadOnlyList<double> ReadAllFrom(byte[] payload, ACadVersion version, int key = DefaultKey)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			List<double> values = new List<double>();

			if (payload.Length < 16)
			{
				return values;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				while (reader.PositionInBits() < limit)
				{
					long start = reader.PositionInBits();
					string cipher;

					try
					{
						cipher = reader.ReadVariableText();
					}
					catch (Exception)
					{
						reader.SetPositionInBits(start + 1);
						continue;
					}

					if (!isCiphertext(cipher))
					{
						//Step one bit and try again: the string may start at a different alignment.
						reader.SetPositionInBits(start + 1);
						continue;
					}

					if (TryDecode(cipher, out double value, key))
					{
						values.Add(value);
					}
				}
			}

			return values;
		}

		private static bool isCiphertext(string value)
		{
			if (value == null || value.Length < 2 || value.Length > 120 || value.Length % 2 != 0)
			{
				return false;
			}

			foreach (char c in value)
			{
				//Every character of a ciphertext string sits in the high half of Latin-1, which is
				//what makes it distinguishable from a class id, a property name and a marker.
				if ((c & 0xFF) < 0xC0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Tries to read the number stored in a string written through the obfuscation layer.
		/// </summary>
		/// <param name="cipher">Ciphertext as it was read from the file.</param>
		/// <param name="value">Receives the decoded number.</param>
		/// <param name="key">Key for the object, <see cref="DefaultKey"/> for the samples.</param>
		/// <returns>True when the string decoded to a number.</returns>
		public static bool TryDecode(string cipher, out double value, int key = DefaultKey)
		{
			value = 0.0;

			if (cipher == null || cipher.Length < 2 || cipher.Length % 2 != 0)
			{
				return false;
			}

			string text;
			try
			{
				text = Decode(cipher, key);
			}
			catch (FormatException)
			{
				return false;
			}

			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}
	}
}
