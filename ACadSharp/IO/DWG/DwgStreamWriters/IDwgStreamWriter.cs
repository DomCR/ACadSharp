using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Writer equivalent to reader <see cref="IDwgStreamReader"/>
	/// </summary>
	internal interface IDwgStreamWriter
	{
		Stream Stream { get; }

		long PositionInBitsValue { get; }

		void WriteBytes(byte[] bytes);

		void WriteInt(int value);

		void WriteObjectType(ObjectType value);

		void WriteRawLong(long value);

		void WriteBitDouble(double value);

		void WriteBitLong(int value);

		void WriteVariableText(string value);

		void WriteBit(bool value);

		void WriteBitShort(short value);

		void WriteDateTime(DateTime value);

		void WriteTimeSpan(TimeSpan value);

		void WriteCmColor(Color value);

		void Write3BitDouble(XYZ value);

		void Write2RawDouble(XY value);

		void WriteByte(byte value);

		void HandleReference(CadObject cadObject);

		void HandleReference(DwgReferenceType type, CadObject cadObject);

		void HandleReference(ulong handle);

		void HandleReference(DwgReferenceType type, ulong handle);

		void WriteSpearShift();

		void WriteRawShort(ushort value);

		void WriteRawDouble(double value);

		void WriteBitThickness(double thickness);

		void WriteBitExtrusion(XYZ normal);

		void WriteBitDoubleWithDefault(double def, double value);

		void ResetStream();

		void UpdatePositonWriter();
	}

	/*
	internal class DwgMergedWriter : IDwgStreamWriter
	{

	}
	*/

	/// <summary>
	/// Writer equivalent to reader <see cref="DwgStreamReaderBase"/>
	/// </summary>
	internal abstract class DwgStreamWriterBase : StreamIO, IDwgStreamWriter
	{
		public long PositionInBitsValue { get; }

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
					//return new DwgStreamReaderAC24(stream, resetPositon);
					throw new NotSupportedException($"Dwg version not supported: {version}");
				default:
					break;
			}

			return null;
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
			this.Write((int)value, LittleEndianConverter.Instance);
		}

		public void WriteBytes(byte[] arr)
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
			this.dateToJulian(value, out int jdate, out int miliseconds);
			this.WriteBitLong(jdate);
			this.WriteBitLong(miliseconds);
		}

		public void WriteTimeSpan(TimeSpan value)
		{
			this.WriteBitLong(value.Days);
			this.WriteBitLong(value.Milliseconds);
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

		public void UpdatePositonWriter()
		{
			long result = this._stream.Position * 8 + this.BitShift;
			this.WriteRawLong(0);
		}

		private void dateToJulian(DateTime date, out int jdate, out int miliseconds)
		{
			if (date < new DateTime(1, 1, 1, 12, 0, 0))
			{
				jdate = 0;
				miliseconds = 0;
				return;
			}

			date = date.AddHours(-12.0);
			int day = (int)Math.Floor((14.0 - (double)date.Month) / 12.0);
			int year = date.Year + 4800 - day;
			int month = date.Month;
			jdate = date.Day + (int)System.Math.Floor((153.0 * (double)(month + 12 * day - 3) + 2.0) / 5.0) + 365 * year + (int)System.Math.Floor((double)year / 4.0) - (int)System.Math.Floor((double)year / 100.0) + (int)System.Math.Floor((double)year / 400.0) - 32045;
			miliseconds = date.Millisecond + date.Second * 1000 + date.Minute * 60000 + date.Hour * 3600000;
		}
	}

	internal class DwgStreamWriterAC12 : DwgStreamWriterBase
	{
		public DwgStreamWriterAC12(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}
	}

	internal class DwgStreamWriterAC15 : DwgStreamWriterAC12
	{
		public DwgStreamWriterAC15(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteBitExtrusion(XYZ normal)
		{
			//For R2000, this is a single bit, followed optionally by 3BD.
			if (normal == XYZ.AxisZ)
			{
				//If the single bit is 1, 
				//the extrusion value is assumed to be 0,0,1 and no explicit extrusion is stored.
				base.WriteBit(value: true);
				return;
			}

			//If the single bit is 0, 
			base.WriteBit(value: false);
			//then it will be followed by 3BD.
			base.Write3BitDouble(normal);
		}

		public override void WriteBitThickness(double thickness)
		{
			//For R2000+, this is a single bit followed optionally by a BD. 
			//If the bit is one, the thickness value is assumed to be 0.0. 
			//If the bit is 0, then a BD that represents the thickness follows.
			if (thickness == 0.0)
			{
				base.WriteBit(value: true);
				return;
			}

			base.WriteBit(value: false);
			base.WriteBitDouble(thickness);
		}
	}

	internal class DwgStreamWriterAC18 : DwgStreamWriterAC15
	{
		public DwgStreamWriterAC18(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteCmColor(Color value)
		{
			//TODO: Finish writer color implementation

			//CMC:
			//BS: color index(always 0)
			this.WriteBitShort(0);

			//BL: RGB value
			this.WriteBitLong(0);

			//RC: Color Byte
			this.WriteByte(0);

			//(&1 => color name follows(TV),
			//&2 => book name follows(TV))
		}
	}

	internal class DwgStreamWriterAC21: DwgStreamWriterAC18
	{
		public DwgStreamWriterAC21(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}
	}
}
