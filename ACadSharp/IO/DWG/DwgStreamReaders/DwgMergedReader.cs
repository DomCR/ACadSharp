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
			get => _mainReader.Encoding; set
			{
				_mainReader.Encoding = value;
				_textReader.Encoding = value;
				_handleReader.Encoding = value;
			}
		}
		
		public Stream Stream => throw new InvalidOperationException();
		
		public int BitShift { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
		
		public long Position { get => this._mainReader.Position; set => throw new InvalidOperationException(); }
		
		public bool IsEmpty { get; } = false;

		private IDwgStreamReader _mainReader;
	
		private IDwgStreamReader _textReader;
		
		private IDwgStreamReader _handleReader;

		public DwgMergedReader(IDwgStreamReader manReader, IDwgStreamReader textReader, IDwgStreamReader handleReader)
		{
			_mainReader = manReader;
			_textReader = textReader;
			_handleReader = handleReader;
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
			return _handleReader.HandleReference();
		}

		public ulong HandleReference(ulong referenceHandle)
		{
			return _handleReader.HandleReference(referenceHandle);
		}

		public ulong HandleReference(ulong referenceHandle, out DwgReferenceType reference)
		{
			return _handleReader.HandleReference(referenceHandle, out reference);
		}

		public long PositionInBits()
		{
			return _mainReader.PositionInBits();
		}

		public byte Read2Bits()
		{
			return _mainReader.Read2Bits();
		}

		public XY Read2RawDouble()
		{
			return _mainReader.Read2RawDouble();
		}

		public XYZ Read3BitDouble()
		{
			return _mainReader.Read3BitDouble();
		}

		public bool ReadBit()
		{
			return _mainReader.ReadBit();
		}

		public short ReadBitAsShort()
		{
			return _mainReader.ReadBitAsShort();
		}

		public double ReadBitDouble()
		{
			return _mainReader.ReadBitDouble();
		}

		public XY Read2BitDouble()
		{
			return _mainReader.Read2BitDouble();
		}

		public int ReadBitLong()
		{
			return _mainReader.ReadBitLong();
		}

		public long ReadBitLongLong()
		{
			return _mainReader.ReadBitLongLong();
		}

		public short ReadBitShort()
		{
			return _mainReader.ReadBitShort();
		}

		public bool ReadBitShortAsBool()
		{
			return _mainReader.ReadBitShortAsBool();
		}

		public byte ReadByte()
		{
			return _mainReader.ReadByte();
		}

		public byte[] ReadBytes(int length)
		{
			throw new NotImplementedException();
		}

		public XY Read2BitDoubleWithDefault(XY defValues)
		{
			return _mainReader.Read2BitDoubleWithDefault(defValues);
		}

		public XYZ Read3BitDoubleWithDefault(XYZ defValues)
		{
			return _mainReader.Read3BitDoubleWithDefault(defValues);
		}

		public Color ReadCmColor()
		{
			if (!(_mainReader is DwgStreamReaderAC18))
				return _mainReader.ReadCmColor();

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
			return _mainReader.ReadEnColor(out transparency, out flag);
		}

		public DateTime Read8BitJulianDate()
		{
			throw new NotImplementedException();
		}

		public DateTime ReadDateTime()
		{
			return _mainReader.ReadDateTime();
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
			return _mainReader.ReadBitExtrusion();
		}

		public double ReadBitDoubleWithDefault(double def)
		{
			return _mainReader.ReadBitDoubleWithDefault(def);
		}

		public double ReadBitThickness()
		{
			return _mainReader.ReadBitThickness();
		}

		public char ReadRawChar()
		{
			return _mainReader.ReadRawChar();
		}

		public long ReadRawLong()
		{
			throw new NotImplementedException();
		}

		public byte[] ReadSentinel()
		{
			return _mainReader.ReadSentinel();
		}

		public short ReadShort()
		{
			return this._mainReader.ReadShort();
		}

		public short ReadShort<T>() where T : IEndianConverter, new()
		{
			throw new NotImplementedException();
		}

		public string ReadTextUnicode()
		{
			//Handle the text section if is empty
			if (_textReader.IsEmpty)
				return string.Empty;

			return _textReader.ReadTextUnicode();
		}

		public TimeSpan ReadTimeSpan()
		{
			return _mainReader.ReadTimeSpan();
		}

		public uint ReadUInt()
		{
			throw new NotImplementedException();
		}

		public string ReadVariableText()
		{
			//Handle the text section if is empty
			if (_textReader.IsEmpty)
				return string.Empty;

			return _textReader.ReadVariableText();
		}

		public ushort ResetShift()
		{
			return this._mainReader.ResetShift();
		}

		public void SetPositionInBits(long positon)
		{
			this._mainReader.SetPositionInBits(positon);
		}

		public long SetPositionByFlag(long position)
		{
			throw new InvalidOperationException();
		}
	}
}
