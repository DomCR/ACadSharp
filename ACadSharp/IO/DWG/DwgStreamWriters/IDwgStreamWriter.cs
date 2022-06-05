using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal interface IDwgStreamWriter
	{
		Stream Stream { get; }

		void WriteBytes(byte[] bytes);

		public void WriteInt(int value);

		public void WriteRawLong(long value);
	}

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
			this.Write(value, LittleEndianConverter.Instance);
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
	}

	internal class DwgStreamWriterAC18 : DwgStreamWriter
	{
		public DwgStreamWriterAC18(Stream stream, Encoding encoding) : base(stream, encoding)
		{
		}
	}
}
