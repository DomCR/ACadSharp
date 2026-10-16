using ACadSharp.IO.DWG.Tangent;
using CSMath;

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
		/// Reads the position a class stores inline, before the stream begins.
		/// </summary>
		/// <remarks>
		/// The one field of these classes that is not text: a point is written as three doubles where
		/// the class writes everything else, and in the order the format writes points — the second
		/// of the three is the first coordinate.
		/// </remarks>
		public static XYZ ReadInlinePosition(byte[] payload)
		{
			TchStreamReader reader = new TchStreamReader(payload, TchEntity.PositionOffset);

			double first = reader.ReadBitDouble();
			double second = reader.ReadBitDouble();
			double third = reader.ReadBitDouble();

			//The format writes a point as (y, z, x), so the third value is the first coordinate.
			return new XYZ(third, first, second);
		}
	}
}
