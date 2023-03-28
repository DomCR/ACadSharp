using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Writer equivalent to reader <see cref="DwgStreamReaderBase"/>
	/// </summary>
	internal abstract class DwgStreamWriterBase : StreamIO, IDwgStreamWriter
	{
		public IDwgStreamWriter Main { get { return this; } }

		public long PositionInBits { get { return this.Position * 8 + this.BitShift; } }

		public long SavedPositionInBits { get; } = 0;

		public Encoding Encoding { get; }

		public int BitShift { get; private set; } = 0;

		private byte _lastByte;

		public DwgStreamWriterBase(Stream stream, Encoding encoding) : base(stream)
		{
			this.Encoding = encoding;
		}

		public static IDwgStreamWriter GetStreamHandler(ACadVersion version, Stream stream, Encoding encoding)
		{
			switch (version)
			{
				case ACadVersion.Unknown:
					throw new Exception();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new NotSupportedException($"Dwg version not supported: {version}");
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
					return new DwgStreamWriterAC12(stream, encoding);
				case ACadVersion.AC1015:
					return new DwgStreamWriterAC15(stream, encoding);
				case ACadVersion.AC1018:
					return new DwgStreamWriterAC18(stream, encoding);
				case ACadVersion.AC1021:
					return new DwgStreamWriterAC21(stream, encoding);
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					return new DwgStreamWriterAC24(stream, encoding);
				default:
					throw new NotSupportedException($"Dwg version not supported: {version}");
			}
		}

		public static IDwgStreamWriter GetMergedWriter(ACadVersion version, Stream stream, Encoding encoding)
		{
			switch (version)
			{
				case ACadVersion.Unknown:
					throw new Exception();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new NotSupportedException($"Dwg version not supported: {version}");
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
					return new DwgmMergedStreamWriterAC14(
						stream,
						new DwgStreamWriterAC12(stream, encoding),
						new DwgStreamWriterAC12(new MemoryStream(), encoding));
				case ACadVersion.AC1015:
					return new DwgmMergedStreamWriterAC14(
							stream,
							new DwgStreamWriterAC15(stream, encoding),
							new DwgStreamWriterAC15(new MemoryStream(), encoding));
				case ACadVersion.AC1018:
					return new DwgmMergedStreamWriterAC14(
							stream,
							new DwgStreamWriterAC18(stream, encoding),
							new DwgStreamWriterAC18(new MemoryStream(), encoding));
				case ACadVersion.AC1021:
					return new DwgmMergedStreamWriter(
							stream,
							new DwgStreamWriterAC21(stream, encoding),
							new DwgStreamWriterAC21(new MemoryStream(), encoding),
							new DwgStreamWriterAC21(new MemoryStream(), encoding));
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					return new DwgmMergedStreamWriter(
							stream,
							new DwgStreamWriterAC24(stream, encoding),
							new DwgStreamWriterAC24(new MemoryStream(), encoding),
							new DwgStreamWriterAC24(new MemoryStream(), encoding));
				default:
					throw new NotSupportedException($"Dwg version not supported: {version}");
			}
		}

		public void WriteInt(int value)
		{
			this.Write(value, LittleEndianConverter.Instance);
		}

		public virtual void WriteObjectType(ObjectType value)
		{
			this.WriteBitShort((short)value);
		}

		public void WriteRawLong(long value)
		{
			this.WriteBytes(LittleEndianConverter.Instance.GetBytes((int)value));
		}

		public override void WriteBytes(byte[] arr)
		{
			if (this.BitShift == 0)
			{
				for (int i = 0; i < arr.Length; i++)
				{
					this.Stream.WriteByte(arr[i]);
				}
				return;
			}

			int num = 8 - this.BitShift;
			foreach (byte b in arr)
			{
				this.Stream.WriteByte((byte)(this._lastByte | (b >> this.BitShift)));
				this._lastByte = (byte)(b << num);
			}
		}

		public void WriteBytes(byte[] arr, int initialIndex, int length)
		{
			if (this.BitShift == 0)
			{
				for (int i = 0, j = initialIndex; i < length; i++, j++)
				{
					this.WriteByte(arr[j]);
				}

				return;
			}

			int num = 8 - this.BitShift;
			for (int i = 0, j = initialIndex; i < length; i++, j++)
			{
				byte b = arr[j];
				this.WriteByte((byte)(this._lastByte | (b >> this.BitShift)));
				this._lastByte = (byte)(b << num);
			}
		}

		public void WriteBitShort(short value)
		{
			if (value == 0)
			{
				this.Write2Bits(2);
			}
			else if (value > 0 && value < 256)
			{
				this.Write2Bits(1);
				this.WriteByte((byte)value);
			}
			else if (value == 256)
			{
				this.Write2Bits(3);
			}
			else
			{
				this.Write2Bits(0);
				this.WriteByte((byte)value);
				this.WriteByte((byte)(value >> 8));
			}
		}

		public void WriteBitDouble(double value)
		{
			if (value == 0.0)
			{
				this.Write2Bits(2);
				return;
			}

			if (value == 1.0)
			{
				this.Write2Bits(1);
				return;
			}

			this.Write2Bits(0);
			this.WriteBytes(LittleEndianConverter.Instance.GetBytes(value));
		}

		public void WriteBitLong(int value)
		{
			if (value == 0)
			{
				this.Write2Bits(2);
				return;
			}

			if (value > 0 && value < 256)
			{
				this.Write2Bits(1);
				this.WriteByte((byte)value);
				return;
			}

			this.Write2Bits(0);
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
			this.WriteByte((byte)(value >> 16));
			this.WriteByte((byte)(value >> 24));
		}

		public void WriteBitLongLong(long value)
		{
			byte size = 0;
			ulong unsignedValue = (ulong)value;

			ulong hold = unsignedValue;
			while (hold != 0)
			{
				hold >>= 8;
				size = (byte)(size + 1);
			}

			this.write3Bits(size);

			hold = unsignedValue;
			for (int i = 0; i < size; i++)
			{
				this.WriteByte((byte)(hold & 0xFF));
				hold >>= 8;
			}
		}

		public virtual void WriteVariableText(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				this.WriteBitShort(0);
				return;
			}

			byte[] bytes = this.Encoding.GetBytes(value);
			this.WriteBitShort((short)bytes.Length);
			this.WriteBytes(bytes);
		}

		public virtual void WriteTextUnicode(string value)
		{
			byte[] bytes = this.Encoding.GetBytes(string.IsNullOrEmpty(value) ? string.Empty : value);
			this.WriteRawShort((ushort)(bytes.Length + 1));
			this._stream.Write(bytes, 0, bytes.Length);
			this._stream.WriteByte(0);
		}

		public void Write2Bits(byte value)
		{
			if (this.BitShift < 6)
			{
				this._lastByte |= (byte)(value << 6 - this.BitShift);
				this.BitShift += 2;
			}
			else if (this.BitShift == 6)
			{
				this._lastByte |= value;
				this.Stream.WriteByte(this._lastByte);
				this.resetShift();
			}
			else
			{
				this._lastByte |= (byte)(value >> 1);
				this.Stream.WriteByte(this._lastByte);
				this._lastByte = (byte)(value << 7);
				this.BitShift = 1;
			}
		}

		public void WriteBit(bool value)
		{
			if (this.BitShift < 7)
			{
				if (value)
				{
					this._lastByte |= (byte)(1 << 7 - this.BitShift);
				}
				this.BitShift++;
				return;
			}

			if (value)
			{
				this._lastByte |= 1;
			}

			this.Stream.WriteByte(this._lastByte);
			this.resetShift();
		}

		public void WriteByte(byte value)
		{
			if (this.BitShift == 0)
			{
				this.Stream.WriteByte(value);
				return;
			}

			int shift = 8 - this.BitShift;
			this.Stream.WriteByte((byte)(this._lastByte | (value >> this.BitShift)));
			this._lastByte = (byte)(value << shift);
		}

		private void resetShift()
		{
			this.BitShift = 0;
			this._lastByte = 0;
		}

		public void WriteDateTime(DateTime value)
		{
			CadUtils.DateToJulian(value, out int jdate, out int miliseconds);

			this.WriteBitLong(jdate);
			this.WriteBitLong(miliseconds);
		}

		public void WriteTimeSpan(TimeSpan value)
		{
			this.WriteBitLong(value.Days);
			this.WriteBitLong(value.Milliseconds);
		}

		public void Write8BitJulianDate(DateTime value)
		{
			CadUtils.DateToJulian(value, out int jdate, out int miliseconds);
			this.WriteRawLong(jdate);
			this.WriteRawLong(miliseconds);
		}

		public virtual void WriteCmColor(Color value)
		{
			//R15 and earlier: BS color index
			this.WriteBitShort(value.Index);
		}

		public void Write3BitDouble(XYZ value)
		{
			this.WriteBitDouble(value.X);
			this.WriteBitDouble(value.Y);
			this.WriteBitDouble(value.Z);
		}

		public void Write2RawDouble(XY value)
		{
			this.WriteRawDouble(value.X);
			this.WriteRawDouble(value.Y);
		}

		public void WriteRawShort(short value)
		{
			this.WriteBytes(LittleEndianConverter.Instance.GetBytes(value));
		}

		public void WriteRawShort(ushort value)
		{
			this.WriteBytes(LittleEndianConverter.Instance.GetBytes(value));
		}

		public void WriteRawDouble(double value)
		{
			this.WriteBytes(LittleEndianConverter.Instance.GetBytes(value));
		}

		public void HandleReference(CadObject cadObject)
		{
			this.HandleReference(DwgReferenceType.Undefined, cadObject);
		}

		public void HandleReference(DwgReferenceType type, CadObject cadObject)
		{
			if (cadObject == null)
			{
				this.HandleReference(type, 0uL);
			}
			else
			{
				this.HandleReference(type, cadObject.Handle);
			}
		}

		public void HandleReference(ulong handle)
		{
			this.HandleReference(DwgReferenceType.Undefined, handle);
		}

		public void HandleReference(DwgReferenceType type, ulong handle)
		{
			byte b = (byte)((uint)type << 4);

			if (handle == 0)
			{
				this.WriteByte(b);
			}
			else if (handle < 0x100)
			{
				this.WriteByte((byte)(b | 1u));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x10000)
			{
				this.WriteByte((byte)(b | 2u));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x1000000)
			{
				this.WriteByte((byte)(b | 3u));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x100000000L)
			{
				this.WriteByte((byte)(b | 4u));
				this.WriteByte((byte)(handle >> 24));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x10000000000L)
			{
				this.WriteByte((byte)(b | 5u));
				this.WriteByte((byte)(handle >> 32));
				this.WriteByte((byte)(handle >> 24));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x1000000000000L)
			{
				this.WriteByte((byte)(b | 6u));
				this.WriteByte((byte)(handle >> 40));
				this.WriteByte((byte)(handle >> 32));
				this.WriteByte((byte)(handle >> 24));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else if (handle < 0x100000000000000L)
			{
				this.WriteByte((byte)(b | 7u));
				this.WriteByte((byte)(handle >> 48));
				this.WriteByte((byte)(handle >> 40));
				this.WriteByte((byte)(handle >> 32));
				this.WriteByte((byte)(handle >> 24));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
			else
			{
				this.WriteByte((byte)(b | 8u));
				this.WriteByte((byte)(handle >> 56));
				this.WriteByte((byte)(handle >> 48));
				this.WriteByte((byte)(handle >> 40));
				this.WriteByte((byte)(handle >> 32));
				this.WriteByte((byte)(handle >> 24));
				this.WriteByte((byte)(handle >> 16));
				this.WriteByte((byte)(handle >> 8));
				this.WriteByte((byte)handle);
			}
		}

		public void WriteSpearShift()
		{
			if (this.BitShift > 0)
			{
				for (int i = this.BitShift; i < 8; i++)
				{
					this.WriteBit(value: false);
				}
			}
		}

		public virtual void WriteBitThickness(double thickness)
		{
			//For R13-R14, this is a BD.
			this.WriteBitDouble(thickness);
		}

		public virtual void WriteBitExtrusion(XYZ normal)
		{
			//For R13-R14 this is 3BD.
			this.Write3BitDouble(normal);
		}

		public void WriteBitDoubleWithDefault(double def, double value)
		{
			if (def == value)
			{
				//00 No more data present, use the value of the default double.
				this.Write2Bits(0);
				return;
			}

			byte[] defBytes = LittleEndianConverter.Instance.GetBytes(def);
			byte[] valueBytes = LittleEndianConverter.Instance.GetBytes(value);

			//Compare the 2 sets of bytes by it's simetry
			int first = 0;
			int last = 7;
			while (last >= 0 && defBytes[last] == valueBytes[last])
			{
				first++;
				last--;
			}

			if (first >= 4)
			{
				//01 4 bytes of data are present. The result is the default double, with the 4 data bytes patched in
				//replacing the first 4 bytes of the default double(assuming little endian).
				this.Write2Bits(1);
				this.WriteBytes(defBytes, 0, 4);
			}
			else if (first >= 2)
			{
				//10 6 bytes of data are present. The result is the default double, with the first 2 data bytes patched in
				//replacing bytes 5 and 6 of the default double, and the last 4 data bytes patched in replacing the first 4
				//bytes of the default double(assuming little endian).
				this.Write2Bits(2);
				this.WriteByte(defBytes[4]);
				this.WriteByte(defBytes[5]);
				this.WriteByte(defBytes[0]);
				this.WriteByte(defBytes[1]);
				this.WriteByte(defBytes[2]);
				this.WriteByte(defBytes[3]);
			}
			else
			{
				//11 A full RD follows.
				this.Write2Bits(3);
				this.WriteBytes(defBytes);
			}
		}

		public void ResetStream()
		{
			this._stream.Position = 0L;
			this.resetShift();
			this._stream.SetLength(0L);
		}

		public void SavePositonForSize()
		{
			this.WriteRawLong(0);
		}

		public void SetPositionByFlag(long pos)
		{
			if (pos >= 0x8000)
			{
				if (pos >= 0x40000000)
				{
					this.WriteBytes(LittleEndianConverter.Instance.GetBytes((ushort)((pos >> 30) & 0xFFFF)));
					this.WriteBytes(LittleEndianConverter.Instance.GetBytes((ushort)(((pos >> 15) & 0x7FFF) | 0x8000)));
				}
				else
				{
					this.WriteBytes(LittleEndianConverter.Instance.GetBytes((ushort)((pos >> 15) & 0xFFFF)));
				}

				this.WriteBytes(LittleEndianConverter.Instance.GetBytes((ushort)((pos & 0x7FFF) | 0x8000)));
			}
			else
			{
				this.WriteBytes(LittleEndianConverter.Instance.GetBytes((ushort)pos));
			}
		}

		public void SetPositionInBits(long posInBits)
		{
			long position = posInBits / 8;
			this.BitShift = (int)(posInBits % 8);
			this._stream.Position = position;

			if (this.BitShift > 0)
			{
				int value = this._stream.ReadByte();
				if (value < 0)
				{
					throw new EndOfStreamException();
				}
				this._lastByte = (byte)value;
			}
			else
			{
				this._lastByte = 0;
			}

			this._stream.Position = position;
		}

		public void WriteShiftValue()
		{
			if (this.BitShift > 0)
			{
				long position = this._stream.Position;
				int lastValue = this._stream.ReadByte();
				byte currValue = (byte)(this._lastByte | ((byte)lastValue & (0b11111111 >> this.BitShift)));
				this._stream.Position = position;
				this._stream.WriteByte(currValue);
			}
		}

		private void write3Bits(byte value)
		{
			this.WriteBit((value & 4) != 0);
			this.WriteBit((value & 2) != 0);
			this.WriteBit((value & 1) != 0);
		}
	}
}
