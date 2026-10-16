using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// Base class for every entity that comes from a Tianzheng drawing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Tianzheng is a Chinese architectural package built on ObjectARX. Its entities are ordinary
	/// custom objects, so the common entity data — layer, colour, handle — is read the usual way;
	/// what differs is everything the class writes for itself, which is obfuscated and uses a
	/// layout that is not documented anywhere.
	/// </para>
	/// <para>
	/// Because that reading process differs, the result is a distinct type rather than something
	/// reshaped into an existing one. A Tianzheng text carries what a <see cref="TextEntity"/>
	/// carries, but it was written by <c>TDbText::dwgInFields</c> and not by the <c>TEXT</c> reader,
	/// so it is a <see cref="TchText"/> and
	/// <see cref="TchText.ToTextEntity"/> is how it becomes one when that is what a caller wants.
	/// </para>
	/// <para>
	/// The base class exists so that the writers can leave all of these alone with a single check.
	/// ACadSharp cannot write a Tianzheng class correctly — it has no <c>dwgOutFields</c> for one —
	/// so they are skipped, and one case covers every class now and every class added later.
	/// </para>
	/// </remarks>
	public abstract class TchEntity : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <summary>
		/// Gets the class this entity was read from.
		/// </summary>
		/// <remarks>
		/// The entity type says how the object was read; this says which Tianzheng class it is.
		/// Two objects of different classes share a type when they are read the same way, which is
		/// why the class is kept rather than encoded in the type.
		/// </remarks>
		public DxfClass DxfClass { get; }

		/// <summary>
		/// Where the position a class stores inline begins, as a bit offset into the payload.
		/// </summary>
		/// <remarks>
		/// A class writes its position before its string stream, at a fixed place: a version byte, a
		/// tag, and one short per class in the chain, none of which varies. Counting rather than
		/// searching is safe here for that reason.
		/// </remarks>
		public const long PositionOffset = 66;

		/// <summary>
		/// Gets the part of the object data that lies beyond the common entity data, as it is stored
		/// in the file.
		/// </summary>
		/// <remarks>
		/// The bytes written by the class's own <c>dwgOutFields</c>, kept verbatim. Only populated
		/// when the reader is configured to keep unknown entities.
		/// </remarks>
		public byte[] RawData { get; internal set; }

		/// <summary>
		/// Gets the property blocks the object carries, if it carries any.
		/// </summary>
		public TchDataMap Properties { get; internal set; }

		/// <summary>
		/// Gets where the object's string stream begins, as a bit offset into <see cref="RawData"/>.
		/// </summary>
		/// <remarks>
		/// A Tianzheng class writes its numbers as text into the object's string stream rather than
		/// inline, which is why scanning the object's own bytes for a value never finds one. The
		/// stream is located from the handle section, and only the object reader knows where that
		/// starts, so the offset is recorded while reading rather than derived afterwards.
		/// </remarks>
		public long StringStreamOffset { get; internal set; } = -1;


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
		internal static long FindStringStream(byte[] payload, long handleOffset)
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
			for (long endbit = Math.Min(handleOffset, bits - 1); endbit > handleOffset - 256 && endbit > 32; endbit--)
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
		internal static double ReadStreamedDouble(TchStreamReader reader, byte xor)
		{
			string cipher = reader.ReadCipherString();

			//A field that does not read is left as not-a-number rather than thrown on. These classes
			//are read without a specification, so a value that surprises the reader is expected, and
			//losing the whole entity over one of them loses the geometry that did read.
			return TgDataEncrypt.TryDecode(cipher, out double value, xor) ? value : double.NaN;
		}

		/// <inheritdoc/>
		public override string ObjectName => this.DxfClass?.DxfName ?? "TCH_UNKNOWN";

		/// <inheritdoc/>
		public override string SubclassMarker => this.DxfClass?.CppClassName ?? DxfSubclassMarker.Entity;

		internal TchEntity(DxfClass dxfClass)
		{
			this.DxfClass = dxfClass;
		}

		/// <summary>
		/// Reads whatever this type knows how to read from the class specific data.
		/// </summary>
		/// <remarks>
		/// Empty on the base: a type that has worked out how to read its class overrides this, and
		/// one that has not keeps only the raw bytes. The reader calls it without knowing which type
		/// it has.
		/// </remarks>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		internal virtual void Fill(byte[] payload, ACadVersion version)
		{
		}

		/// <summary>
		/// Converts this object into the standard entities it stands for.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Some Tianzheng classes store the shape they draw, and conversion is a direct mapping.
		/// Others store parameters from which the shape is derived — a wall keeps a centre line and
		/// a thickness and its two side lines are computed — and for those this needs the drawing
		/// logic of the class.
		/// </para>
		/// <para>
		/// Classes that have neither give an empty result rather than throwing, so a document can
		/// be converted without checking every object first.
		/// </para>
		/// </remarks>
		/// <returns>The standard entities, or empty when there is nothing to convert.</returns>
		public abstract IEnumerable<Entity> ToStandardEntities();
	}
}
