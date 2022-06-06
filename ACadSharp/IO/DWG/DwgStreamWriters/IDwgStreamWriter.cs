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

		void WriteBytes(byte[] bytes);

		public void WriteInt(int value);

		public void WriteRawLong(long value);

		void WriteBitDouble(double value);

		void WriteVariableText(string value);
	}

	/// <summary>
	/// Writer equivalent to reader <see cref="DwgStreamReader"/>
	/// </summary>
	internal abstract class DwgStreamWriter : StreamIO, IDwgStreamWriter
	{
		public Encoding Encoding { get; }

		public int BitShift { get; private set; } = 0;

		private byte _lastByte;

		public DwgStreamWriter(Stream stream, Encoding encoding) : base(stream)
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
				//return new DwgStreamReaderAC12(stream, resetPositon);
				case ACadVersion.AC1015:
					//return new DwgStreamReaderAC15(stream, resetPositon);
					throw new NotSupportedException($"Dwg version not supported: {version}");
				case ACadVersion.AC1018:
					return new DwgStreamWriterAC18(stream, encoding);
				case ACadVersion.AC1021:
				//return new DwgStreamReaderAC21(stream, resetPositon);
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

		public virtual void WriteVariableText(string value)
		{
			throw new NotImplementedException();
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
	}

	internal class DwgStreamWriterAC18 : DwgStreamWriter
	{
		public DwgStreamWriterAC18(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}

		public override void WriteVariableText(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				base.WriteBitShort(0);
				return;
			}

			byte[] bytes = base.Encoding.GetBytes(value);
			base.WriteBitShort((short)bytes.Length);
			base.WriteBytes(bytes);
		}
	}
}
