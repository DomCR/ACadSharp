using ACadSharp.IO.DWG;
using CSMath;
using System;
using System.Collections.Generic;
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
		/// Gets the text content, or null when the payload carried none that could be told apart from
		/// the object's own fields.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The entries in a payload interleave three kinds of string and only the last is content.
		/// The class id and the block markers are recognisable by shape, and the scalars are
		/// recognisable because they decode through the alphabet of §4.2.2 — that is what makes this
		/// readable now, where an earlier attempt failed. What is left is text.
		/// </para>
		/// <para>
		/// Misaligned reads of the other kinds survive that filter as short strings of unusual
		/// characters, so the longest candidate is taken. On the sample drawing that gives the right
		/// content for every text object: <c>Wall &amp; Window &amp; Door</c>, <c>Square Column</c>,
		/// <c>Elevation</c>, <c>Breakline</c>, <c>U Stair</c>, <c>Line Stair</c>,
		/// <c>Multi-flight Staircase</c>, <c>Callout Title</c> and <c>axial grid with Dimension</c>.
		/// </para>
		/// </remarks>
		public string Value { get; private set; }

		/// <summary>
		/// Reads the position of a <c>TCH_TEXT</c> or <c>TCH_MTEXT</c> object.
		/// </summary>
		/// <param name="dxfName">DXF name of the class. Content is only taken from the classes this
		/// reader is for; the payload of any other class holds property names that would otherwise
		/// be mistaken for text.</param>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the string encoding.</param>
		/// <returns>The parsed position; never null.</returns>
		public static TchText Parse(string dxfName, byte[] payload, ACadVersion version)
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

			//Only these two classes write text; for anything else the longest readable string is a
			//property name, which is not content.
			if (dxfName == "TCH_TEXT" || dxfName == "TCH_MTEXT")
			{
				readContent(text, payload, version);
			}

			return text;
		}
		private static void readContent(TchText text, byte[] payload, ACadVersion version)
		{
			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				while (reader.PositionInBits() < limit)
				{
					long start = reader.PositionInBits();
					string value;

					try
					{
						value = reader.ReadVariableText();
					}
					catch (Exception)
					{
						reader.SetPositionInBits(start + 1);
						continue;
					}

					if (isContent(value) && (text.Value == null || value.Length > text.Value.Length))
					{
						text.Value = value;
					}

					reader.SetPositionInBits(start + 1);
				}
			}
		}

		private static bool isContent(string value)
		{
			if (value == null || value.Length < 2 || value.Length > 400)
			{
				return false;
			}

			//Markers, field descriptors and the class id are all recognisable by shape.
			if (value.StartsWith("TDBXDATAMAP", StringComparison.Ordinal) || value.StartsWith("T20", StringComparison.Ordinal))
			{
				return false;
			}

			if (value[0] == '{' || value[value.Length - 1] == '}')
			{
				return false;
			}

			//A scalar decodes through the alphabet, so it is not content.
			if (TgDataEncrypt.TryDecode(value, out _))
			{
				return false;
			}

			int core = 0;
			foreach (char c in value)
			{
				int o = c;

				//The blocks a drawing's text actually uses: ASCII, CJK ideographs, CJK punctuation,
				//fullwidth forms, and a few typographic marks.
				bool allowed = (o >= 0x20 && o <= 0x7E)
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
	}
}