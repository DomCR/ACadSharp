using CSMath;
using CSUtilities.Converters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
			get => this._mainReader.Encoding; set
			{
				this._mainReader.Encoding = value;
				this._textReader.Encoding = value;
				this._handleReader.Encoding = value;
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
			this._mainReader = manReader;
			this._textReader = textReader;
			this._handleReader = handleReader;
		}

		public void Advance(int offset)
		{
			_mainReader.Advance(offset);
		}

		public void AdvanceByte()
		{
			throw new InvalidOperationException();
		}

		public ulong HandleReference()
		{
			return this._handleReader.HandleReference();
		}

		public ulong HandleReference(ulong referenceHandle)
		{
			return this._handleReader.HandleReference(referenceHandle);
		}

		public ulong HandleReference(ulong referenceHandle, out DwgReferenceType reference)
		{
			return this._handleReader.HandleReference(referenceHandle, out reference);
		}

		public long PositionInBits()
		{
			return this._mainReader.PositionInBits();
		}

		public byte Read2Bits()
		{
			return this._mainReader.Read2Bits();
		}

		public XY Read2RawDouble()
		{
			return this._mainReader.Read2RawDouble();
		}

		public XYZ Read3BitDouble()
		{
			return this._mainReader.Read3BitDouble();
		}

		public bool ReadBit()
		{
			return this._mainReader.ReadBit();
		}

		public short ReadBitAsShort()
		{
			return this._mainReader.ReadBitAsShort();
		}

		public double ReadBitDouble()
		{
			return this._mainReader.ReadBitDouble();
		}

		public XY Read2BitDouble()
		{
			return this._mainReader.Read2BitDouble();
		}

		public int ReadBitLong()
		{
			return this._mainReader.ReadBitLong();
		}

		public long ReadBitLongLong()
		{
			return this._mainReader.ReadBitLongLong();
		}

		public short ReadBitShort()
		{
			return this._mainReader.ReadBitShort();
		}

		public bool ReadBitShortAsBool()
		{
			return this._mainReader.ReadBitShortAsBool();
		}

		public byte ReadByte()
		{
			return this._mainReader.ReadByte();
		}

		public byte[] ReadBytes(int length)
		{
			throw new NotImplementedException();
		}

		public XY Read2BitDoubleWithDefault(XY defValues)
		{
			return this._mainReader.Read2BitDoubleWithDefault(defValues);
		}

		public XYZ Read3BitDoubleWithDefault(XYZ defValues)
		{
			return this._mainReader.Read3BitDoubleWithDefault(defValues);
		}

		public Color ReadCmColor()
		{
			if (!(this._mainReader is DwgStreamReaderAC18))
				return this._mainReader.ReadCmColor();

			Color color = default;

			//To read a color in this version file needs to access to the
			//string data section at the same time.

			//CMC:
			//BS: color index(always 0)
			short colorIndex = this.ReadBitShort();
			//BL: RGB value
			//Always negative
			uint rgb = (uint)this.ReadBitLong();
			byte[] arr = LittleEndianConverter.Instance.GetBytes(rgb);

			if ((rgb & 0b0000_0001_0000_0000_0000_0000_0000_0000) != 0)
			{
				//Indexed color
				color = new Color(arr[0]);
			}
			else
			{
				//CECOLOR:
				//3221225472
				//0b11000000000000000000000000000000
				//0b1100_0000_0000_0000_0000_0000_0000_0000 --> this should be ByLayer
				//0xC0000000

				//True color
				color = new Color(arr[0], arr[1], arr[2]);
			}

			//RC : Color Byte
			byte id = this.ReadByte();

			string colorName = string.Empty;
			//RC: Color Byte(&1 => color name follows(TV),
			if ((id & 1) == 1)
			{
				colorName = this.ReadVariableText();
			}

			string bookName = string.Empty;
			//&2 => book name follows(TV))
			if ((id & 2) == 2)
			{
				bookName = this.ReadVariableText();
			}

			return color;
		}

		public Color ReadEnColor(out Transparency transparency, out bool flag)
		{
			return this._mainReader.ReadEnColor(out transparency, out flag);
		}

		public DateTime Read8BitJulianDate()
		{
			throw new NotImplementedException();
		}

		public DateTime ReadDateTime()
		{
			return this._mainReader.ReadDateTime();
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
			return new Color((byte)this.ReadBitShort());
		}

		public ObjectType ReadObjectType()
		{
			throw new NotImplementedException();
		}

		public XYZ ReadBitExtrusion()
		{
			return this._mainReader.ReadBitExtrusion();
		}

		public double ReadBitDoubleWithDefault(double def)
		{
			return this._mainReader.ReadBitDoubleWithDefault(def);
		}

		public double ReadBitThickness()
		{
			return this._mainReader.ReadBitThickness();
		}

		public char ReadRawChar()
		{
			return this._mainReader.ReadRawChar();
		}

		public long ReadRawLong()
		{
			throw new NotImplementedException();
		}

		public byte[] ReadSentinel()
		{
			return this._mainReader.ReadSentinel();
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
			if (this._textReader.IsEmpty)
				return string.Empty;

			return this._textReader.ReadTextUnicode();
		}

		public TimeSpan ReadTimeSpan()
		{
			return this._mainReader.ReadTimeSpan();
		}

		public uint ReadUInt()
		{
			throw new NotImplementedException();
		}

		public string ReadVariableText()
		{
			//Handle the text section if is empty
			if (this._textReader.IsEmpty)
				return string.Empty;

			return this._textReader.ReadVariableText();
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
