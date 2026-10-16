using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// The parts of reading a Tianzheng class that more than one class does the same way.
	/// </summary>
	/// <remarks>
	/// Reading is mechanical and deriving is not, so only the reading lives here. A class says which
	/// of its fields the stream holds and in what order; the walking is shared, because getting it
	/// wrong is a silent error and every class would get it wrong in the same way.
	/// </remarks>
	internal static class TchFieldMap
	{
		/// <summary>
		/// How many strings in the stream belong to readers above a class.
		/// </summary>
		/// <remarks>
		/// A class reads its base's fields before its own, and the stream is in write order, so a
		/// class's fields start after everything its chain wrote. Counting them is how the fields are
		/// found without an offset for any of them. Each value is measured against its own class.
		/// </remarks>
		public const int MountRoofSkippedStrings = 15;

		/// <inheritdoc cref="MountRoofSkippedStrings"/>
		public const int OpeningSkippedStrings = 11;

		/// <inheritdoc cref="MountRoofSkippedStrings"/>
		public const int ElevationSkippedStrings = 8;

		/// <inheritdoc cref="MountRoofSkippedStrings"/>
		public const int DimensionSkippedStrings = 9;

		/// <summary>
		/// Moves past one string in the stream.
		/// </summary>
		/// <remarks>
		/// Reading a string is what advances a reader, so this is only for stepping over one without
		/// looking at it. Doing both — reading a value and then skipping — walks past the value just
		/// read and then off the end of the payload, which loses the whole entity.
		/// </remarks>
		public static TchStreamReader SkipString(TchStreamReader reader)
		{
			int length;

			try
			{
				length = reader.ReadBitShort();
			}
			catch (InvalidOperationException)
			{
				return reader;
			}

			long next = reader.PositionInBits + (length * 16L);

			//A reader parked at the end reads nothing more, which a class reports as the values it
			//could not read rather than as a failed object.
			return next >= reader.LengthInBits
				? new TchStreamReader(reader.Payload, reader.LengthInBits)
				: new TchStreamReader(reader.Payload, next);
		}

		/// <summary>
		/// Reads the next double from the stream.
		/// </summary>
		/// <remarks>
		/// A field that does not read is left as not-a-number rather than thrown on: these classes are
		/// read without a specification, so a value that surprises the reader is expected, and losing
		/// a whole entity over one field loses the geometry that did read.
		/// </remarks>
		public static double ReadDouble(TchStreamReader reader, byte xor)
		{
			return TchEntity.ReadStreamedDouble(reader, xor);
		}

		/// <summary>
		/// Reads the position a class stores inline.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="offset">Where the point begins.</param>
		/// <remarks>
		/// A class reads its position before any other field, but not at a fixed place in the object:
		/// it follows a version byte and a tag for each class in the chain, so a longer chain starts
		/// later. The offset is therefore per class, and measured against that class's own drawing.
		/// <para>
		/// It is not searched for. A search looks for a run of three doubles that names somewhere in a
		/// drawing, and the numbers that follow a position — lengths, angles, elevations — produce
		/// runs that look like one often enough that the first match is usually not the position.
		/// </para>
		/// <para>
		/// The format writes a point as (y, z, x), so the third value is the first coordinate.
		/// </para>
		/// </remarks>
		public static XYZ ReadInlinePosition(byte[] payload, long offset)
		{
			TchStreamReader reader = new TchStreamReader(payload, offset);

			double first = reader.ReadBitDouble();
			double second = reader.ReadBitDouble();
			double third = reader.ReadBitDouble();

			return new XYZ(third, first, second);
		}

		/// <summary>
		/// Reads a position a class stores as plain consecutive doubles.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="offset">Where the point begins.</param>
		/// <remarks>
		/// Not every class writes its point the same way. One that goes through the filer's point
		/// reader gets a length prefix on each coordinate, and one that reads the coordinates itself
		/// gets none — so the same three numbers sit at different distances apart depending on which
		/// class wrote them. Both are read here rather than one being assumed.
		/// <para>
		/// The pair is written (y, x).
		/// </para>
		/// </remarks>
		public static XYZ ReadInlineRawPosition(byte[] payload, long offset)
		{
			TchStreamReader reader = new TchStreamReader(payload, offset);

			double y = reader.ReadRawDouble();
			double x = reader.ReadRawDouble();

			return new XYZ(x, y, 0.0);
		}

		/// <summary>
		/// Reads one number a class stores as a plain double.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="offset">Where the number begins.</param>
		/// <remarks>
		/// Most of a class's numbers go through the obfuscation layer into the string stream, but not
		/// all of them: a value the class reads with the filer's own double reader is written plainly,
		/// and which values those are differs by class. This is the plain one.
		/// </remarks>
		public static double ReadInlineRawDouble(byte[] payload, long offset)
		{
			return new TchStreamReader(payload, offset).ReadRawDouble();
		}

		/// <summary>
		/// Finds a run of points that ends exactly where the object's string stream begins.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="count">How many points the run holds.</param>
		/// <param name="endBit">Where the run has to end, which is the start of the stream.</param>
		/// <param name="points">The points, when a run was found.</param>
		/// <returns>True when exactly one run fits.</returns>
		/// <remarks>
		/// A class writes its points last among the things it writes inline, so the end of the run is
		/// known and only its start is not. Searching for the start is therefore a check rather than a
		/// guess: a candidate is right only when the points it describes fill the space up to the
		/// stream exactly. That is a stronger test than it sounds, because a point is three values
		/// whose length varies with the value — a coordinate of zero costs two bits and any other
		/// costs sixty six — so a wrong start lands somewhere other than the end.
		/// <para>
		/// Counting is not an alternative. What precedes the run is a fixed set of class fields
		/// followed by the object's property block, and that block carries only the properties that
		/// differ from their defaults, so it is a different length in every object. Ending the search
		/// at the stream is what makes the reading independent of it.
		/// </para>
		/// <para>
		/// More than one run fitting would make the reading a guess, so it is refused rather than
		/// resolved. Nothing in the corpus does, and a class for which it happened would need a
		/// different rule rather than the first match.
		/// </para>
		/// </remarks>
		public static bool TryReadInlinePath(byte[] payload, int count, long endBit, out XYZ[] points)
		{
			points = null;

			if (payload == null || count < 1 || endBit <= 0 || endBit > (long)payload.Length * 8L)
			{
				return false;
			}

			XYZ[] found = null;

			for (long start = 16; start < endBit; start++)
			{
				TchStreamReader reader = new TchStreamReader(payload, start);
				XYZ[] candidate = new XYZ[count];
				bool readable = true;

				for (int i = 0; i < count && readable; i++)
				{
					try
					{
						double y = reader.ReadBitDouble();
						double z = reader.ReadBitDouble();
						double x = reader.ReadBitDouble();

						if (!isPlausible(y) || !isPlausible(z) || !isPlausible(x))
						{
							readable = false;
							break;
						}

						candidate[i] = new XYZ(x, y, z);
					}
					catch (InvalidOperationException)
					{
						readable = false;
					}
				}

				if (!readable || reader.PositionInBits != endBit)
				{
					continue;
				}

				if (found != null)
				{
					//A second fit means the end position does not identify the run, and taking either
					//would be taking the first of two answers.
					return false;
				}

				found = candidate;
			}

			points = found;

			return found != null;
		}

		/// <summary>
		/// Whether a coordinate read out of a payload is one a drawing could hold.
		/// </summary>
		/// <remarks>
		/// Used to reject a candidate rather than to describe a drawing: a run of bits read from the
		/// wrong place usually produces a number of no particular size, and one beyond this has never
		/// been a coordinate.
		/// </remarks>
		private static bool isPlausible(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value) && Math.Abs(value) < 1e8;
		}

		/// <summary>
		/// Positions a reader at the first string after the object's property block.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="streamOffset">Where the string stream begins.</param>
		/// <param name="fallbackSkip">How many strings to step over when there is no block to find.</param>
		/// <remarks>
		/// The property block carries only the properties that differ from their defaults — a wall's
		/// construction, an opening's usage — so it is a different length in every object, and
		/// everything written after it moves with it. Anchoring a class's own fields to the block's
		/// closing marker is what keeps them where the class put them, and it is why a class reads
		/// from there rather than from a count measured off the start of the stream.
		/// <para>
		/// The fallback is for an object that carries no block at all. Every object in the corpus has
		/// one, so it is a guard rather than a path anything is expected to take.
		/// </para>
		/// </remarks>
		public static TchStreamReader SkipPropertyBlock(byte[] payload, long streamOffset, int fallbackSkip)
		{
			TchStreamReader reader = new TchStreamReader(payload, streamOffset);

			for (int i = 0; i < 512 && reader.PositionInBits < reader.LengthInBits - 16; i++)
			{
				string value;

				try
				{
					value = reader.ReadCipherString();
				}
				catch (InvalidOperationException)
				{
					break;
				}

				if (string.Equals(value, TchDataMap.BlockEndMarker, StringComparison.Ordinal))
				{
					return reader;
				}
			}

			reader = new TchStreamReader(payload, streamOffset);

			for (int i = 0; i < fallbackSkip; i++)
			{
				reader = SkipString(reader);
			}

			return reader;
		}

		/// <summary>
		/// Finds the text a class writes among its numbers.
		/// </summary>
		/// <param name="reader">A reader at the start of the stream.</param>
		/// <param name="limit">How many strings to look at before giving up.</param>
		/// <remarks>
		/// Text and numbers share the stream, and which is which is a matter of shape rather than of
		/// position: a number is written as ciphertext and its characters say so. Markers and class
		/// identifiers are text as well, so they are passed over by name.
		/// <para>
		/// A property is written as its key and then its value, and it is the value that says what the
		/// object is. Taking the first text found takes the key, which is the same for every instance
		/// and therefore says nothing.
		/// </para>
		/// </remarks>
		public static string FindText(TchStreamReader reader, int limit)
		{
			for (int i = 0; i < limit; i++)
			{
				string value = reader.ReadCipherString();

				if (!isContent(value))
				{
					continue;
				}

				string following = reader.ReadCipherString();

				return isContent(following) ? following : value;
			}

			return null;
		}

		/// <summary>
		/// Finds the level a marker writes beside it.
		/// </summary>
		/// <remarks>
		/// The level is text, and unlike the usage an opening carries it is written plainly rather
		/// than through the obfuscation layer, so it is the one string in the stream that reads as a
		/// number without being ciphertext. That is what identifies it — and it is also why an earlier
		/// reading of this class concluded the level was not in the object at all: it was looked for as
		/// a decoded number, and it had never been encoded.
		/// </remarks>
		public static string FindLevel(byte[] payload, long streamOffset)
		{
			TchStreamReader reader = new TchStreamReader(payload, streamOffset);

			for (int i = 0; i < 24 && reader.PositionInBits < (long)payload.Length * 8L; i++)
			{
				string value = reader.ReadCipherString();

				if (value.Length > 1
					&& !TgDataEncrypt.IsCiphertext(value)
					&& double.TryParse(value, System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture, out _))
				{
					return value;
				}
			}

			return null;
		}

		private static bool isContent(string value)
		{
			return value != null
				&& value.Length > 1
				&& !TgDataEncrypt.IsCiphertext(value)
				&& !value.StartsWith("TDBXDATAMAP", StringComparison.Ordinal)
				&& !value.StartsWith("T20", StringComparison.Ordinal)
				&& value[0] != '{';
		}
	}
}
