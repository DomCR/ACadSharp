using ACadSharp.Classes;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A Tianzheng text object.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Covers the classes whose object is text: <c>TCH_TEXT</c>, <c>TCH_MTEXT</c> and
	/// <c>TCH_DRAWINGNAME</c>. They are one type because they are read the same way — the payload is
	/// scanned for the string that is content rather than a field — and
	/// <see cref="TchEntity.DxfClass"/> still says which of them it was.
	/// </para>
	/// <para>
	/// It is not a <see cref="TextEntity"/>. The bytes were written by <c>TDbText::dwgInFields</c>,
	/// not by the <c>TEXT</c> reader, and treating it as one would mean the writers produced a
	/// <c>TEXT</c> that was never in the drawing. <see cref="ToTextEntity"/> is how a caller asks for
	/// one deliberately.
	/// </para>
	/// <para>
	/// The class is heavier than it looks because the payload has to be searched: a Tianzheng object
	/// writes its fields with no delimiter and its position at an offset that depends on what the
	/// base class wrote before it. The search is what the measurements in
	/// <see cref="TchGeometry.FindPositions"/> and the alphabet of <see cref="TgDataEncrypt"/> make
	/// possible; neither was known when this type was a fixed pair of offsets.
	/// </para>
	/// </remarks>
	public class TchText : TchEntity
	{
		/// <summary>Bit offset of the Y coordinate in the sample drawings.</summary>
		public const int YOffset = 84;

		/// <summary>Bit offset of the X coordinate in the sample drawings.</summary>
		public const int XOffset = 152;

		internal TchText(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the insertion point of the text, in drawing coordinates.
		/// </summary>
		/// <remarks>
		/// Zero when <see cref="HasPosition"/> is false, which happens when the payload is too short
		/// or holds nothing that reads as a position.
		/// </remarks>
		public XYZ InsertionPoint { get; internal set; } = XYZ.Zero;

		/// <summary>
		/// Gets a value indicating whether a position was found.
		/// </summary>
		public bool HasPosition { get; internal set; }

		/// <summary>
		/// Gets the text content, or null when none could be told apart from the object's fields.
		/// </summary>
		/// <remarks>
		/// A multiline text keeps its line breaks, so the value can contain <c>\r\n</c>.
		/// </remarks>
		public string Value { get; internal set; }

		/// <summary>
		/// Builds the standard text this object stands for.
		/// </summary>
		/// <remarks>
		/// The result is a new <see cref="TextEntity"/>; this object is left alone. Height is left at
		/// its default because the height is one of the obfuscated scalars and which one has not been
		/// established for these classes.
		/// </remarks>
		/// <returns>The text entity.</returns>
		public TextEntity ToTextEntity()
		{
			return new TextEntity
			{
				InsertPoint = this.InsertionPoint,
				Value = this.Value ?? string.Empty,
				Layer = this.Layer,
				Color = this.Color,
			};
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			yield return this.ToTextEntity();
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.InsertionPoint = transform.ApplyTransform(this.InsertionPoint);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// A text with no position has no extent, so the box is null rather than empty.
		/// </remarks>
		public override BoundingBox GetBoundingBox()
		{
			if (!this.HasPosition)
			{
				return BoundingBox.Null;
			}

			return new BoundingBox(this.InsertionPoint, this.InsertionPoint);
		}

		/// <summary>
		/// Reads a Tianzheng text from the class specific data of an object.
		/// </summary>
		/// <param name="dxfClass">Class the object was read from.</param>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		/// <returns>The text, with whatever could be read filled in.</returns>
		public static TchText Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchText text = new TchText(dxfClass)
			{
				RawData = payload,
			};

			text.Fill(payload, version);
			return text;
		}

		/// <summary>
		/// Fills this text from the class specific data of an object.
		/// </summary>
		/// <remarks>
		/// Separate from <see cref="Parse"/> because a reader has to create the entity before it can
		/// consume the common entity data, and the class specific data only follows that.
		/// </remarks>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				return;
			}

			readPosition(this, payload, version);

			//Only the classes that write text take content; for anything else the longest readable
			//string is a property name, which is not content.
			if (WritesText(this.DxfClass?.DxfName))
			{
				this.Value = readContent(payload, version);
			}
		}

		/// <summary>
		/// Says whether a Tianzheng class writes text, and so is read as a
		/// <see cref="TchText"/> rather than as an unlisted entity.
		/// </summary>
		/// <param name="dxfName">DXF name of the class.</param>
		/// <returns>True when the class writes text.</returns>
		internal static bool WritesText(string dxfName)
		{
			return dxfName == "TCH_TEXT" || dxfName == "TCH_MTEXT" || dxfName == "TCH_DRAWINGNAME";
		}

		private static void readPosition(TchText text, byte[] payload, ACadVersion version)
		{
			int required = XOffset + 64;

			if (payload.Length * 8 < required)
			{
				return;
			}

			try
			{
				using (MemoryStream stream = new MemoryStream(payload, writable: false))
				{
					IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);

					reader.SetPositionInBits(YOffset);
					double y = reader.ReadDouble();

					reader.SetPositionInBits(XOffset);
					double x = reader.ReadDouble();

					if (double.IsNaN(x) || double.IsNaN(y) || double.IsInfinity(x) || double.IsInfinity(y))
					{
						return;
					}

					text.InsertionPoint = new XYZ(x, y, 0.0);
					text.HasPosition = true;
				}
			}
			catch (Exception)
			{
				//A payload that cannot be walked simply has no position.
			}
		}

		/// <summary>
		/// Finds the string in a payload that is content rather than a field.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Three kinds of string are interleaved: the class id and block markers, which are
		/// recognisable by shape; the obfuscated scalars, which are recognisable because they decode
		/// through <see cref="TgDataEncrypt"/>; and the text, which is what is left. The third could
		/// not be told from the second until the alphabet was known.
		/// </para>
		/// <para>
		/// Misaligned reads of the other kinds survive that as short strings of unusual characters,
		/// so the longest candidate is taken.
		/// </para>
		/// </remarks>
		private static string readContent(byte[] payload, ACadVersion version)
		{
			string best = null;

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				for (long offset = 0; offset < limit; offset++)
				{
					string value;

					try
					{
						reader.SetPositionInBits(offset);
						value = reader.ReadVariableText();
					}
					catch (Exception)
					{
						continue;
					}

					if (isContent(value) && (best == null || value.Length > best.Length))
					{
						best = value;
					}
				}
			}

			return best;
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

			//The stream a payload carries holds the class's strings and the text it writes, and a
			//number written through the obfuscation layer sits among them as ciphertext. The two
			//are told apart by shape: every character of a ciphertext pair has a low byte of 0xC0
			//or more, which no text does.
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
	}
}
