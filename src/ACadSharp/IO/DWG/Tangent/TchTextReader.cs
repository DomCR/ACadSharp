using ACadSharp.Entities.Tangent;
using CSMath;
using System;

namespace ACadSharp.IO.DWG.Tangent;

/// <summary>
/// Reading a Tianzheng class whose object is text.
/// </summary>
/// <remarks>
/// <para>
/// The object's own data is gone over here rather than walked by the filer, because the fields
/// in it are not at offsets the filer can reach: the string stream that holds them is located
/// from the end of the object rather than stepped to, and the position is found by trying both
/// of the alignments these classes use. Nothing of that reaches the entity, which is handed the
/// fields rather than the bytes.
/// </para>
/// <para>
/// This covers the classes whose object is text — <c>TCH_TEXT</c>, <c>TCH_MTEXT</c> and
/// <c>TCH_DRAWINGNAME</c>. They are read the same way, which is why they are one reader.
/// </para>
/// </remarks>
public static class TchTextReader
{
	private const long _textXOffset = 152;

	/// <summary>
	/// Where each coordinate of the position a text class stores begins, as a bit offset.
	/// </summary>
	/// <remarks>
	/// Two offsets rather than one, because the coordinates are not written as a pair: each is a
	/// plain double with no length prefix, sixty eight bits apart. Reading them as a consecutive
	/// pair finds a number for each and puts every text in the wrong place, which is what an
	/// earlier reading of this class did.
	/// </remarks>
	private const long _textYOffset = 84;

	/// <summary>
	/// Fills a text from the object's own data.
	/// </summary>
	/// <param name="text">The entity to fill.</param>
	/// <param name="payload">The object's bytes.</param>
	/// <param name="handleOffset">Where the handle section begins, as a bit offset.</param>
	public static void Read(TchText text, byte[] payload, long handleOffset)
	{
		if (text == null || payload == null || payload.Length == 0)
		{
			return;
		}

		long streamOffset = TchObjectReader.FindStringStream(payload, handleOffset);

		readPosition(text, payload);
		readContent(text, payload, streamOffset);

		if (streamOffset >= 0)
		{
			text.Height = readHeight(payload, streamOffset);
		}
	}

	/// <summary>
	/// Where the longest content string in an object ends.
	/// </summary>
	private static long findTextEnd(byte[] payload, long streamOffset)
	{
		long start = streamOffset >= 0 ? streamOffset : 0L;
		long limit = (long)payload.Length * 8L;

		string best = null;
		long bestEnd = -1;

		for (long offset = start; offset < limit - 8; offset++)
		{
			try
			{
				TchStreamReader reader = new TchStreamReader(payload, offset);
				string value = reader.ReadCipherString();

				if (isContent(value) && (best == null || value.Length > best.Length))
				{
					best = value;
					bestEnd = reader.PositionInBits;
				}
			}
			catch (InvalidOperationException)
			{
				continue;
			}
		}

		return bestEnd;
	}

	/// <summary>
	/// Whether a string read out of an object is the text rather than a field.
	/// </summary>
	private static bool isContent(string value)
	{
		if (value == null || value.Length < 2 || value.Length > 400)
		{
			return false;
		}

		//Markers, field descriptors and the class identifier are all recognisable by shape.
		if (value.StartsWith("TDBXDATAMAP", StringComparison.Ordinal)
			|| value.StartsWith("T20", StringComparison.Ordinal))
		{
			return false;
		}

		if (value[0] == '{' || value[value.Length - 1] == '}')
		{
			return false;
		}

		//A number written through the obfuscation layer sits among the strings as ciphertext, and
		//the two are told apart by shape: every character of a ciphertext pair has a low byte of
		//0xC0 or more, which no text does.
		if (TgDataEncrypt.IsCiphertext(value))
		{
			return false;
		}

		int core = 0;

		foreach (char c in value)
		{
			int o = c;

			//A multiline text carries its line breaks, so the control characters a drawing
			//legitimately contains are allowed. Leaving them out rejected every multiline text
			//while every single line one still read, which is why the gap went unnoticed.
			bool allowed = (o >= 0x20 && o <= 0x7E)
				|| o == '\t' || o == '\n' || o == '\r' || o == '\f' || o == '\v'
				|| (o >= 0x4E00 && o <= 0x9FFF)
				|| (o >= 0x3000 && o <= 0x303F)
				|| (o >= 0xFF00 && o <= 0xFFEF)
				|| o == 0x2011 || o == 0x2013 || o == 0x2014
				|| o == 0x2018 || o == 0x2019 || o == 0x201C || o == 0x201D
				|| o == 0x2026 || o == 0x00B0 || o == 0x00D7 || o == 0x00B1;

			if (!allowed)
			{
				return false;
			}

			if (char.IsLetterOrDigit(c))
			{
				core++;
			}
		}

		return core >= 2;
	}

	/// <summary>
	/// Finds the string in an object that is content rather than a field.
	/// </summary>
	/// <remarks>
	/// Three kinds of string are interleaved: the class identifier and the property block's
	/// markers, which are recognisable by shape; the numbers, which are recognisable because they
	/// decode through <see cref="TgDataEncrypt"/>; and the text, which is what is left.
	/// Misaligned reads of the other kinds survive that as short strings of unusual characters,
	/// so the longest candidate is taken.
	/// </remarks>
	private static void readContent(TchText text, byte[] payload, long streamOffset)
	{
		long start = streamOffset >= 0 ? streamOffset : 0L;
		long limit = (long)payload.Length * 8L;

		string best = null;
		long bestEnd = -1;

		for (long offset = start; offset < limit - 8; offset++)
		{
			string value;

			try
			{
				TchStreamReader reader = new TchStreamReader(payload, offset);
				value = reader.ReadCipherString();

				if (isContent(value) && (best == null || value.Length > best.Length))
				{
					best = value;
					bestEnd = reader.PositionInBits;
				}
			}
			catch (InvalidOperationException)
			{
				continue;
			}
		}

		text.Value = best;
	}

	/// <summary>
	/// Reads the height the text is drawn at.
	/// </summary>
	/// <param name="payload">The object's bytes.</param>
	/// <param name="streamOffset">Where the string stream begins.</param>
	/// <returns>The height in drawing units, or not-a-number when it does not read.</returns>
	/// <remarks>
	/// The value written immediately after the text is the height in the drawing's own scale, so
	/// it is the scale that turns it into a length — the sample's texts all carry 3.5 and the
	/// export draws every one of them at 350, the drawing being at 1:100. The scale is the second
	/// string of the stream, after the class identifier, and is the same in every object of a
	/// drawing.
	/// </remarks>
	private static double readHeight(byte[] payload, long streamOffset)
	{
		double scale = TchObjectReader.ReadStreamedDouble(
			TchObjectReader.SkipString(new TchStreamReader(payload, streamOffset)),
			TgDataEncrypt.ObjectXor);

		if (double.IsNaN(scale) || scale <= 0.0)
		{
			return double.NaN;
		}

		//The text's own string is the one the content search took, and what follows it is the
		//height. Finding that string again rather than carrying its end out of the search keeps
		//the two readings independent of each other.
		long end = findTextEnd(payload, streamOffset);

		if (end < 0 || end >= (long)payload.Length * 8L)
		{
			return double.NaN;
		}

		double height = TchObjectReader.ReadStreamedDouble(
			new TchStreamReader(payload, end), TgDataEncrypt.ObjectXor);

		return double.IsNaN(height) || height <= 0.0 ? double.NaN : height * scale;
	}

	/// <summary>
	/// Reads the point a text class stores inline.
	/// </summary>
	private static void readPosition(TchText text, byte[] payload)
	{
		if (payload == null || (long)payload.Length * 8L < _textXOffset + 64L)
		{
			return;
		}

		try
		{
			double y = new TchStreamReader(payload, _textYOffset).ReadRawDouble();
			double x = new TchStreamReader(payload, _textXOffset).ReadRawDouble();

			if (double.IsNaN(x) || double.IsNaN(y)
				|| double.IsInfinity(x) || double.IsInfinity(y)
				|| Math.Abs(x) > 1e8 || Math.Abs(y) > 1e8)
			{
				return;
			}

			text.InsertionPoint = new XYZ(x, y, 0.0);
		}
		catch (InvalidOperationException)
		{
			//A payload that cannot be walked that far simply has no position.
		}
	}
}