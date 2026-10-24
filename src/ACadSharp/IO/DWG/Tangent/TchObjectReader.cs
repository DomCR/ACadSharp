using System;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Reading the parts of a Tianzheng object that every class writes the same way.
	/// </summary>
	/// <remarks>
	/// A Tianzheng object is <c>[what the class writes inline][a stream of strings][the handle
	/// section]</c>, and the class writes its numbers into the middle part as text rather than
	/// inline — which is why scanning the object's own bytes for a value never finds one. Locating
	/// that stream and reading out of it is shared, because getting it wrong is silent and every
	/// class would get it wrong in the same way.
	/// </remarks>
	internal static class TchObjectReader
	{
		/// <summary>
		/// Where the position a class stores inline begins, as a bit offset into the object's data.
		/// </summary>
		/// <remarks>
		/// A class writes its position before its string stream, and what precedes it is a version
		/// byte and one tag per class in the chain, none of which varies. Counting rather than
		/// searching is safe here for that reason.
		/// </remarks>
		public const long PositionOffset = 66;

		/// <summary>
		/// Where the position begins for a class whose version prefix is a different length.
		/// </summary>
		/// <remarks>
		/// The prefix is a version byte and a tag per class in the chain, so a class that reads fewer
		/// of them starts earlier. Each value here is measured against the class's own drawing rather
		/// than derived, and the comment says which measurement it came from.
		/// </remarks>
		public const long ShortPositionOffset = 52;

		/// <summary>
		/// How far below the handle section the end of the string stream is looked for.
		/// </summary>
		/// <remarks>
		/// Only a flag and the size sit between the two, so a handful of bits is enough when the
		/// handle section's start is known — which it is while an object is being read. An object
		/// that owns many handles has a handle section long enough to push the stream outside a
		/// narrower window. Widening it cannot change an answer that was already found, because the
		/// search runs downwards and stops at the first fit.
		/// </remarks>
		private const long StreamSearchBits = 4096;

		/// <summary>
		/// Finds where an object's string stream begins, given where its handle section does.
		/// </summary>
		/// <param name="payload">The object's bytes.</param>
		/// <param name="handleOffset">The handle section's start, as a bit offset into the payload.</param>
		/// <returns>The bit offset of the first string, or -1 when the object has no string stream.</returns>
		/// <remarks>
		/// The stream is described from its end rather than its start, because its length is known
		/// before its contents are: the bit before the handle section says whether there is one, and
		/// the sixteen bits below that hold its size in the modular form this format uses for
		/// lengths, where the top bit means a second word follows. The start is then that size back
		/// from where the size was read. No class needs to know its own offsets, which is the point.
		/// </remarks>
		public static long FindStringStream(byte[] payload, long handleOffset)
		{
			if (payload == null || handleOffset <= 0)
			{
				return -1;
			}

			long bits = (long)payload.Length * 8L;

			//The end of the string stream is at or just before the handle section, and where exactly
			//is not something the handle reader knows: it marks the end of the object's data, and the
			//stream ends a little before that. Searching for it is cheap and self-checking, because a
			//candidate is only right if the strings it describes fill the stream exactly: walking
			//them has to land on the size field and not before or after it.
			for (long endbit = Math.Min(handleOffset, bits - 1); endbit > handleOffset - StreamSearchBits && endbit > 32; endbit--)
			{
				TchStreamReader probe = new TchStreamReader(payload, endbit);

				if (!probe.ReadBit())
				{
					continue;
				}

				long sizeAt = endbit - 16;

				if (sizeAt < 0)
				{
					continue;
				}

				TchStreamReader sizes = new TchStreamReader(payload, sizeAt);

				int size = sizes.ReadUShort();

				if ((size & 0x8000) != 0)
				{
					long hi = sizeAt - 16;

					if (hi < 0)
					{
						continue;
					}

					TchStreamReader high = new TchStreamReader(payload, hi);

					size = (size & 0x7FFF) | (high.ReadUShort() << 15);
					sizeAt = hi;
				}

				long start = sizeAt - size;

				if (start < 0 || start >= sizeAt)
				{
					continue;
				}

				if (fillsStream(payload, start, sizeAt))
				{
					return start;
				}
			}

			return -1;
		}

		/// <summary>
		/// Whether the strings from a position fill the space up to another, exactly.
		/// </summary>
		/// <remarks>
		/// This is what makes the search for the stream's end a check rather than a guess. A wrong
		/// candidate describes a run of strings that stops somewhere other than where it should,
		/// and that happens for every position but the right one.
		/// </remarks>
		private static bool fillsStream(byte[] payload, long start, long end)
		{
			TchStreamReader reader = new TchStreamReader(payload, start);

			while (reader.PositionInBits < end)
			{
				int length = reader.ReadBitShort();

				if (length < 0 || length > 2048)
				{
					return false;
				}

				long next = reader.PositionInBits + (length * 16L);

				if (next > end)
				{
					return false;
				}

				reader = new TchStreamReader(payload, next);
			}

			return reader.PositionInBits == end;
		}

		/// <summary>
		/// Reads the number at a position in the string stream.
		/// </summary>
		/// <param name="reader">The stream, positioned at the number.</param>
		/// <param name="xor">The value for the context the class reads its fields in.</param>
		/// <remarks>
		/// A field that does not read is left as not-a-number rather than thrown on, and that covers
		/// running off the end of the stream as much as it covers a value that does not decode. These
		/// classes are read without a specification, so a value that surprises the reader is
		/// expected; a throw here costs the whole object, because the reader above drops an entity
		/// whose class threw and says only that it could not be read.
		/// </remarks>
		public static double ReadStreamedDouble(TchStreamReader reader, byte xor)
		{
			string cipher;

			try
			{
				cipher = reader.ReadCipherString();
			}
			catch (InvalidOperationException)
			{
				return double.NaN;
			}

			return TgDataEncrypt.TryDecode(cipher, out double value, xor) ? value : double.NaN;
		}

		/// <summary>
		/// Moves past one string in the stream.
		/// </summary>
		/// <remarks>
		/// Reading a string is what advances a reader, so this is only for stepping over one without
		/// looking at it.
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

			return next >= reader.LengthInBits
				? new TchStreamReader(reader.Payload, reader.LengthInBits)
				: new TchStreamReader(reader.Payload, next);
		}
	}
}
