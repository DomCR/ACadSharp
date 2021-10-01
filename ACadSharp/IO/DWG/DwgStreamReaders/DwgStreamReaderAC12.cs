using ACadSharp.Geometry;
using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgStreamReaderAC12 : DwgStreamReader
	{
		public DwgStreamReaderAC12(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override string ReadVariableText()
		{
			short length = ReadBitShort();
			string str;
			if (length > 0)
			{
				str = ReadString(length, Encoding).Replace("\0", "");
			}
			else
				str = string.Empty;
			return str;
		}
	}
	internal class DwgStreamReaderAC15 : DwgStreamReaderAC12
	{
		public DwgStreamReaderAC15(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override XYZ ReadBitExtrusion()
		{
			//For R2000, this is a single bit, followed optionally by 3BD.
			//If the single bit is 1, 
			//the extrusion value is assumed to be 0,0,1 and no explicit extrusion is stored.
			//If the single bit is 0, 
			//then it will be followed by 3BD.
			return ReadBit() ? XYZ.AxisZ : Read3BitDouble();
		}
		public override double ReadBitThickness()
		{
			//For R2000+, this is a single bit followed optionally by a BD. 
			//If the bit is one, the thickness value is assumed to be 0.0. 
			//If the bit is 0, then a BD that represents the thickness follows.
			return ReadBit() ? 0.0 : ReadBitDouble();
		}
	}
	internal class DwgStreamReaderAC18 : DwgStreamReaderAC15
	{
		public DwgStreamReaderAC18(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override string ReadTextUnicode()
		{
			short textLength = ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				value = ReadString(textLength,
					TextEncoding.GetListedEncoding(CodePage.Windows1252))
					.Replace("\0", "");
			}
			return value;
		}
		/// <inheritdoc/>
		public override Color ReadCmColor()
		{
			//CMC:
			//BS: color index(always 0)
			short colorIndex = ReadBitShort();
			//BL: RGB value
			int rgb = ReadBitLong();

			byte id = ReadByte();

			string colorName = string.Empty;
			//RC: Color Byte(&1 => color name follows(TV),
			if ((id & 1) == 1)
				colorName = ReadVariableText();

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
				bookName = ReadVariableText();

			//TODO: Finish the color implementation
			return new Color();
		}
		public override Color ReadEnColor(out Transparency transparency, out bool flag)
		{
			Color color = new Color();
			transparency = Transparency.ByLayer;
			flag = false;

			//BS : color number: flags + color index
			short size = ReadBitShort();

			if (size != 0)
			{
				//color flags: first byte of the bitshort.
				ushort flags = (ushort)((ushort)size & 0b1111111100000000);
				//0x4000: has AcDbColor reference (0x8000 is also set in this case).
				if ((flags & 0x4000) > 0)
				{
					color = Color.ByBlock;
					//The handle to the color is written in the handle stream.
					flag = true;
				}
				//0x8000: complex color (rgb).
				else if ((flags & 0x8000) > 0)
				{
					//Next value is a BS containing the RGB value(last 24 bits).
					color = new Color();
					color.Index = (short)ReadBitLong();
				}
				else
				{
					color = new Color();
					//Color index: if no flags were set, the color is looked up by the color number (ACI color).
					color.Index = (short)(size & 0b111111111111);
				}

				//0x2000: color is followed by a transparency BL.
				transparency = (flags & 0x2000U) <= 0U ? Transparency.ByLayer : new Transparency((short)ReadBitLong());
			}
			else
			{
				color = Color.ByBlock;
				transparency = Transparency.Opaque;
			}

			return color;
		}
	}
	internal class DwgStreamReaderAC21 : DwgStreamReaderAC18
	{
		public DwgStreamReaderAC21(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override string ReadTextUnicode()
		{
			short textLength = ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Correct the text length by shifting 1 bit
				short length = (short)(textLength << 1);
				//Read the string and get rid of the empty bytes
				value = ReadString(length, Encoding.Unicode).Replace("\0", "");
			}
			return value;
		}
		public override string ReadVariableText()
		{
			int textLength = ReadBitShort();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Correct the text length by shifting 1 bit
				short length = (short)(textLength << 1);
				//Read the string and get rid of the empty bytes
				value = ReadString(length, Encoding.Unicode).Replace("\0", "");
			}
			return value;
		}
	}
	internal class DwgStreamReaderAC24 : DwgStreamReaderAC21
	{
		public DwgStreamReaderAC24(Stream stream, bool resetPosition) : base(stream, resetPosition) { }
		public override ObjectType ReadObjectType()
		{
			//A bit pair, followed by either 1 or 2 bytes, depending on the bit pair value:
			byte pair = Read2Bits();
			short value = 0;

			switch (pair)
			{
				//Read the following byte
				case 0:
					value = ReadByte();
					break;
				//Read following byte and add 0x1f0.
				case 1:
					value = (short)(0x1F0 + ReadByte());
					break;
				//Read the following two bytes (raw short)
				case 2:
					value = ReadShort();
					break;
				//The value 3 should never occur, but interpret the same as 2 nevertheless.
				case 3:
					value = ReadShort();
					break;
			}

			return (ObjectType)value;
		}
	}
	/// <summary>
	/// Enables to handle the different offsets for the objects, text and handles.
	/// The versions <see cref="ACadVersion.AC1021"/> and above, have different offsets for 
	/// the main data like system variables and handles or text, which are separated in a 
	/// "handle-section" and "string-data-section".
	/// This class makes it easier to handle the different stream offsets.
	/// </summary>
	internal class DwgMergedReader : IDwgStreamReader
	{
		public Encoding Encoding
		{
			get => m_defaultHandler.Encoding; set
			{
				m_defaultHandler.Encoding = value;
				m_textHandler.Encoding = value;
				m_referenceHander.Encoding = value;
			}
		}
		public Stream Stream => throw new InvalidOperationException();
		public int BitShift { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		public long Position { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		public bool IsEmpty { get; } = false;

		private IDwgStreamReader m_defaultHandler;
		private IDwgStreamReader m_textHandler;
		private IDwgStreamReader m_referenceHander;

		public DwgMergedReader(IDwgStreamReader defaultHandler, IDwgStreamReader textHandler, IDwgStreamReader referenceHandler)
		{
			m_defaultHandler = defaultHandler;
			m_textHandler = textHandler;
			m_referenceHander = referenceHandler;
		}

		public void Advance(int offset)
		{
			throw new InvalidOperationException();
		}

		public void AdvanceByte()
		{
			throw new InvalidOperationException();
		}

		public ulong HandleReference()
		{
			return m_referenceHander.HandleReference();
		}

		public ulong HandleReference(ulong referenceHandle)
		{
			return m_referenceHander.HandleReference(referenceHandle);
		}

		public ulong HandleReference(ulong referenceHandle, out ReferenceType reference)
		{
			return m_referenceHander.HandleReference(referenceHandle, out reference);
		}

		public long PositionInBits()
		{
			throw new InvalidOperationException();
		}

		public byte Read2Bits()
		{
			return m_defaultHandler.Read2Bits();
		}

		public XY Read2RawDouble()
		{
			return m_defaultHandler.Read2RawDouble();
		}

		public XYZ Read3BitDouble()
		{
			return m_defaultHandler.Read3BitDouble();
		}

		public bool ReadBit()
		{
			return m_defaultHandler.ReadBit();
		}

		public short ReadBitAsShort()
		{
			return m_defaultHandler.ReadBitAsShort();
		}

		public double ReadBitDouble()
		{
			return m_defaultHandler.ReadBitDouble();
		}

		public XY Read2BitDouble()
		{
			return m_defaultHandler.Read2BitDouble();
		}

		public int ReadBitLong()
		{
			return m_defaultHandler.ReadBitLong();
		}

		public long ReadBitLongLong()
		{
			return m_defaultHandler.ReadBitLongLong();
		}

		public short ReadBitShort()
		{
			return m_defaultHandler.ReadBitShort();
		}

		public bool ReadBitShortAsBool()
		{
			return m_defaultHandler.ReadBitShortAsBool();
		}

		public byte ReadByte()
		{
			return m_defaultHandler.ReadByte();
		}

		public byte[] ReadBytes(int length)
		{
			throw new NotImplementedException();
		}
		public XYZ Read3BitDoubleWithDefault(XYZ defValues)
		{
			return m_defaultHandler.Read3BitDoubleWithDefault(defValues);
		}
		public Color ReadCmColor()
		{
			//To read a color in this version file needs to access to the
			//string data section at the same time.

			//CMC:
			//BS: color index(always 0)
			short colorIndex = ReadBitShort();
			//BL: RGB value
			int rgb = ReadBitLong();

			//RC : Color Byte
			byte id = ReadByte();

			string colorName = string.Empty;
			//RC: Color Byte(&1 => color name follows(TV),
			if ((id & 1) == 1)
				colorName = ReadVariableText();

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
				bookName = ReadVariableText();

			return new Color();
		}

		public Color ReadEnColor(out Transparency transparency, out bool flag)
		{
			return m_defaultHandler.ReadEnColor(out transparency, out flag);
		}

		public DateTime Read8BitJulianDate()
		{
			throw new NotImplementedException();
		}

		public DateTime ReadDateTime()
		{
			return m_defaultHandler.ReadDateTime();
		}

		public double ReadDouble()
		{
			throw new NotImplementedException();
		}

		public int ReadInt()
		{
			throw new NotImplementedException();
		}
		public ulong ReadModularChar()
		{
			throw new NotImplementedException();
		}
		public int ReadSignedModularChar()
		{
			throw new NotImplementedException();
		}
		public int ReadModularShort()
		{
			throw new NotImplementedException();
		}

		public ObjectType ReadObjectType()
		{
			throw new NotImplementedException();
		}

		public XYZ ReadBitExtrusion()
		{
			return m_defaultHandler.ReadBitExtrusion();
		}

		public double ReadBitDoubleWithDefault(double def)
		{
			return m_defaultHandler.ReadBitDoubleWithDefault(def);
		}

		public double ReadBitThickness()
		{
			return m_defaultHandler.ReadBitThickness();
		}

		public char ReadRawChar()
		{
			return m_defaultHandler.ReadRawChar();
		}

		public long ReadRawLong()
		{
			throw new NotImplementedException();
		}

		public byte[] ReadSentinel()
		{
			return m_defaultHandler.ReadSentinel();
		}

		public short ReadShort()
		{
			throw new NotImplementedException();
		}

		public short ReadShort<T>() where T : IEndianConverter, new()
		{
			throw new NotImplementedException();
		}

		public string ReadTextUnicode()
		{
			//Handle the text section if is empty
			if (m_textHandler.IsEmpty)
				return string.Empty;

			return m_textHandler.ReadTextUnicode();
		}

		public TimeSpan ReadTimeSpan()
		{
			return m_defaultHandler.ReadTimeSpan();
		}

		public uint ReadUInt()
		{
			throw new NotImplementedException();
		}

		public string ReadVariableText()
		{
			//Handle the text section if is empty
			if (m_textHandler.IsEmpty)
				return string.Empty;

			return m_textHandler.ReadVariableText();
		}

		public ushort ResetShift()
		{
			throw new InvalidOperationException();
		}

		public void SetPositionInBits(long positon)
		{
			throw new InvalidOperationException();
		}

		public long SetPositionByFlag(long position)
		{
			throw new InvalidOperationException();
		}
	}
}
