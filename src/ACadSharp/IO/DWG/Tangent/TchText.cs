using ACadSharp.IO.DWG;
using CSMath;
using System;
using System.IO;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Position of a Tianzheng text object (<c>TCH_TEXT</c> / <c>TCH_MTEXT</c>).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The offsets are relative to the start of the class specific data, which is what
	/// <see cref="Entities.UnknownEntity.RawData"/> holds. They were verified on a drawing with
	/// eleven <c>TCH_TEXT</c> objects: the extracted points match the insertion points of the
	/// corresponding text entities in Tianzheng's own flattened export of the same drawing exactly,
	/// with no offset at all. That export is produced by a different tool, so the check is
	/// independent rather than a re-reading of the same assumption.
	/// </para>
	/// <para>
	/// The layout is fixed for these classes because everything in front of it is constant:
	/// </para>
	/// <code>
	/// bit   0 ..   7    base class version, 10 in the sample drawings
	/// bit   8 ..  15    tag, 17 in the sample drawings
	/// bit  16 ..  17    layout name, empty
	/// bit  18 ..  83    base class fields covered by the obfuscation layer
	/// bit  84 .. 147    Y coordinate, IEEE double, little endian
	/// bit 148 .. 151    gap
	/// bit 152 .. 215    X coordinate, IEEE double, little endian
	/// bit 216 ..        class id, property block and the remaining fields
	/// </code>
	/// <para>
	/// Only the position is exposed. The text content is stored further along, interleaved with
	/// values that use the same string encoding, and telling them apart needs the field walk to know
	/// where the class's own fields end. A heuristic scan of the entries cannot do it reliably, so
	/// no attempt is made here.
	/// </para>
	/// </remarks>
	public sealed class TchText
	{
		/// <summary>Bit offset of the Y coordinate.</summary>
		public const int YOffset = 84;

		/// <summary>Bit offset of the X coordinate.</summary>
		public const int XOffset = 152;

		private const int RequiredBits = XOffset + 64;

		private TchText()
		{
		}

		/// <summary>Gets the insertion point of the text, in drawing coordinates.</summary>
		public XYZ InsertionPoint { get; private set; } = XYZ.Zero;

		/// <summary>Gets a value indicating whether the payload carried a readable position.</summary>
		public bool HasPosition { get; private set; }

		/// <summary>
		/// Reads the position of a <c>TCH_TEXT</c> or <c>TCH_MTEXT</c> object.
		/// </summary>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the string encoding.</param>
		/// <returns>The parsed position; never null.</returns>
		public static TchText Parse(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			TchText text = new TchText();

			if ((long)payload.Length * 8L < RequiredBits)
			{
				return text;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);

				reader.SetPositionInBits(YOffset);
				double y = reader.ReadDouble();

				reader.SetPositionInBits(XOffset);
				double x = reader.ReadDouble();

				text.InsertionPoint = new XYZ(x, y, 0.0);
				text.HasPosition = true;
			}

			return text;
		}
	}
}
