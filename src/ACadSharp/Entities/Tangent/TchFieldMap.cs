using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// The parts of reading a Tianzheng class that more than one class does the same way.
	/// </summary>
	/// <remarks>
	/// Reading is mechanical and deriving is not, so only the reading lives here. A class declares
	/// which of its fields the stream holds and in what order; the walking is shared, because getting
	/// it wrong is a silent error and every class would get it wrong in the same way.
	/// </remarks>
	internal static class TchFieldMap
	{
		/// <summary>
		/// How many strings in the stream belong to readers above <see cref="TchMountRoof"/>.
		/// </summary>
		/// <remarks>
		/// A class reads its base's fields before its own, and the stream is in write order, so a
		/// class's fields start after everything its chain wrote. Counting them is how the fields are
		/// found without an offset for any of them.
		/// </remarks>
		public const int MountRoofSkippedStrings = 15;

		/// <summary>
		/// Reads the next double from the stream.
		/// </summary>
		/// <remarks>
		/// Reading a string is what advances the reader, so there is nothing to skip afterwards.
		/// Skipping as well walks past the value it just read and then off the end of the payload,
		/// which loses the whole entity rather than one field.
		/// </remarks>
		public static double ReadDouble(TchStreamReader reader, byte xor)
		{
			return TchEntity.ReadStreamedDouble(reader, xor);
		}

		/// <summary>
		/// Moves past one string in the stream.
		/// </summary>
		public static TchStreamReader SkipString(TchStreamReader reader)
		{
			int length = reader.ReadBitShort();

			TchStreamReader next = new TchStreamReader(reader.Payload, reader.PositionInBits + (length * 16L));

			return next;
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
		/// Reading a string is what moves the reader along, so nothing is skipped afterwards. Skipping
		/// as well steps over the next string without looking at it, which drops the one being sought
		/// whenever it follows a marker.
		/// </para>
		/// </remarks>
		public static string FindText(TchStreamReader reader, int limit)
		{
			for (int i = 0; i < limit; i++)
			{
				string value = reader.ReadCipherString();

				if (value.Length > 1
					&& !TgDataEncrypt.IsCiphertext(value)
					&& !value.StartsWith("TDBXDATAMAP", System.StringComparison.Ordinal)
					&& !value.StartsWith("T20", System.StringComparison.Ordinal)
					&& value[0] != '{')
				{
					//A property is written as its key and then its value, and it is the value that
					//says what the object is. Taking the first text found takes the key, which is the
					//same for every instance and therefore says nothing.
					string following = reader.ReadCipherString();

					return following.Length > 0 && !TgDataEncrypt.IsCiphertext(following) ? following : value;
				}
			}

			return null;
		}

		/// <summary>
		/// Reads the position a class stores inline.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="streamOffset">Where the string stream begins, which bounds the search.</param>
		/// <remarks>
		/// A class reads its position first, before any other field, so it is the first point in the
		/// object's own bytes. It is not at a fixed offset: it follows a version byte and a tag for
		/// each class in the chain, and a longer chain starts later.
        /// <para>
		/// Finding it means finding a point, and a class may write more than one — an opening writes
		/// three, and the later ones are offsets from this one rather than the position itself. So the
		/// search runs forward and takes the first run of three doubles that names somewhere in a
		/// drawing, which is what a position is and what the fields after it are not.
		/// </para>
		/// <para>
		/// The format writes a point as (y, z, x), so the third value is the first coordinate.
		/// </para>
		/// </remarks>
		public static XYZ ReadInlinePosition(byte[] payload, long streamOffset)
		{
			for (long at = 16; at + 8 < streamOffset; at++)
			{
				TchStreamReader reader = new TchStreamReader(payload, at);

				double first = reader.ReadBitDouble();
				double second = reader.ReadBitDouble();
				double third = reader.ReadBitDouble();

				//The order is (y, z, x), so the test is on the first and third, which are the plan
				//coordinates. The elevation is deliberately not tested: a window carries one, and
				//requiring a point at ground level rejected every window and accepted a field that
				//follows the position instead.
				if (isCoordinate(first) && isCoordinate(third))
				{
					return new XYZ(third, first, second);
				}
			}

			return XYZ.Zero;
		}

		/// <summary>
		/// Whether a value could be one coordinate of a point in a drawing.
		/// </summary>
		/// <remarks>
		/// Deliberately loose. The point of the test is to tell a position from the numbers that
		/// follow it, and those are lengths and angles: a length near zero and an angle in the first
		/// few radians, neither of which looks like a coordinate in a survey.
		/// </remarks>
		private static bool isCoordinate(double value)
		{
			return Math.Abs(value) > 1000.0 && Math.Abs(value) < 1e7;
		}
	}
}
