using CSMath;
using CSUtilities.Converters;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
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
			get => _defaultReader.Encoding; set
			{
				_defaultReader.Encoding = value;
				_textHandler.Encoding = value;
				_referenceHander.Encoding = value;
			}
		}
		public Stream Stream => throw new InvalidOperationException();
		public int BitShift { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		public long Position { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		public bool IsEmpty { get; } = false;

		private IDwgStreamReader _defaultReader;
		private IDwgStreamReader _textHandler;
		private IDwgStreamReader _referenceHander;

		public DwgMergedReader(IDwgStreamReader defaultHandler, IDwgStreamReader textHandler, IDwgStreamReader referenceHandler)
		{
			_defaultReader = defaultHandler;
			_textHandler = textHandler;
			_referenceHander = referenceHandler;
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
			return _referenceHander.HandleReference();
		}

		public ulong HandleReference(ulong referenceHandle)
		{
			return _referenceHander.HandleReference(referenceHandle);
		}

		public ulong HandleReference(ulong referenceHandle, out DwgReferenceType reference)
		{
			return _referenceHander.HandleReference(referenceHandle, out reference);
		}

		public long PositionInBits()
		{
			throw new InvalidOperationException();
		}

		public byte Read2Bits()
		{
			return _defaultReader.Read2Bits();
		}

		public XY Read2RawDouble()
		{
			return _defaultReader.Read2RawDouble();
		}

		public XYZ Read3BitDouble()
		{
			return _defaultReader.Read3BitDouble();
		}

		public bool ReadBit()
		{
			return _defaultReader.ReadBit();
		}

		public short ReadBitAsShort()
		{
			return _defaultReader.ReadBitAsShort();
		}

		public double ReadBitDouble()
		{
			return _defaultReader.ReadBitDouble();
		}

		public XY Read2BitDouble()
		{
			return _defaultReader.Read2BitDouble();
		}

		public int ReadBitLong()
		{
			return _defaultReader.ReadBitLong();
		}

		public long ReadBitLongLong()
		{
			return _defaultReader.ReadBitLongLong();
		}

		public short ReadBitShort()
		{
			return _defaultReader.ReadBitShort();
		}

		public bool ReadBitShortAsBool()
		{
			return _defaultReader.ReadBitShortAsBool();
		}

		public byte ReadByte()
		{
			return _defaultReader.ReadByte();
		}

		public byte[] ReadBytes(int length)
		{
			throw new NotImplementedException();
		}

		public XY Read2BitDoubleWithDefault(XY defValues)
		{
			return _defaultReader.Read2BitDoubleWithDefault(defValues);
		}

		public XYZ Read3BitDoubleWithDefault(XYZ defValues)
		{
			return _defaultReader.Read3BitDoubleWithDefault(defValues);
		}

		public Color ReadCmColor()
		{
			if (!(_defaultReader is DwgStreamReaderAC18))
				return _defaultReader.ReadCmColor();

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

			return new Color(colorIndex);
		}

		public Color ReadEnColor(out Transparency transparency, out bool flag)
		{
			return _defaultReader.ReadEnColor(out transparency, out flag);
		}

		public DateTime Read8BitJulianDate()
		{
			throw new NotImplementedException();
		}

		public DateTime ReadDateTime()
		{
			return _defaultReader.ReadDateTime();
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

		public Color ReadColorByIndex()
		{
			return new Color(this.ReadBitShort());
		}

		public ObjectType ReadObjectType()
		{
			throw new NotImplementedException();
		}

		public XYZ ReadBitExtrusion()
		{
			return _defaultReader.ReadBitExtrusion();
		}

		public double ReadBitDoubleWithDefault(double def)
		{
			return _defaultReader.ReadBitDoubleWithDefault(def);
		}

		public double ReadBitThickness()
		{
			return _defaultReader.ReadBitThickness();
		}

		public char ReadRawChar()
		{
			return _defaultReader.ReadRawChar();
		}

		public long ReadRawLong()
		{
			throw new NotImplementedException();
		}

		public byte[] ReadSentinel()
		{
			return _defaultReader.ReadSentinel();
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
			if (_textHandler.IsEmpty)
				return string.Empty;

			return _textHandler.ReadTextUnicode();
		}

		public TimeSpan ReadTimeSpan()
		{
			return _defaultReader.ReadTimeSpan();
		}

		public uint ReadUInt()
		{
			throw new NotImplementedException();
		}

		public string ReadVariableText()
		{
			//Handle the text section if is empty
			if (_textHandler.IsEmpty)
				return string.Empty;

			return _textHandler.ReadVariableText();
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
