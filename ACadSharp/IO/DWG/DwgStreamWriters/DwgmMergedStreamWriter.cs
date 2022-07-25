using CSMath;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgmMergedStreamWriter : IDwgStreamWriter
	{
		public IDwgStreamWriter Main { get { return this.MainWriter; } }

		public Stream Stream { get; }

		public long PositionInBits { get; private set; }

		public IDwgStreamWriter MainWriter { get; }

		public IDwgStreamWriter TextWriter { get; }

		public IDwgStreamWriter HandleWriter { get; }

		private bool _savedPosition;

		public DwgmMergedStreamWriter(Stream stream, IDwgStreamWriter main, IDwgStreamWriter textwriter, IDwgStreamWriter handlewriter)
		{
			this.Stream = stream;
			this.MainWriter = main;
			this.TextWriter = textwriter;
			this.HandleWriter = handlewriter;
		}

		public void HandleReference(CadObject cadObject)
		{
			this.HandleWriter.HandleReference(cadObject);
		}

		public void HandleReference(DwgReferenceType type, CadObject cadObject)
		{
			this.HandleWriter.HandleReference(type, cadObject);
		}

		public void HandleReference(ulong handle)
		{
			this.HandleWriter.HandleReference(handle);
		}

		public void HandleReference(DwgReferenceType type, ulong handle)
		{
			this.HandleWriter.HandleReference(type, handle);
		}

		public void ResetStream()
		{
			this.MainWriter.ResetStream();
			this.TextWriter.ResetStream();
			this.HandleWriter.ResetStream();
		}

		public void SavePositonForSize()
		{
			this._savedPosition = true;
			this.PositionInBits = this.MainWriter.PositionInBits;
			//Save this position for the size in bits
			this.WriteRawLong(0);
		}

		public void Write2RawDouble(XY value)
		{
			this.MainWriter.Write2RawDouble(value);
		}

		public void Write3BitDouble(XYZ value)
		{
			this.MainWriter.Write3BitDouble(value);
		}

		public void WriteBit(bool value)
		{
			this.MainWriter.WriteBit(value);
		}

		public void WriteBitDouble(double value)
		{
			this.MainWriter.WriteBitDouble(value);
		}

		public void WriteBitDoubleWithDefault(double def, double value)
		{
			this.MainWriter.WriteBitDoubleWithDefault(def, value);
		}

		public void WriteBitExtrusion(XYZ value)
		{
			this.MainWriter.WriteBitExtrusion(value);
		}

		public void WriteBitLong(int value)
		{
			this.MainWriter.WriteBitLong(value);
		}

		public void WriteBitLongLong(long value)
		{
			this.MainWriter.WriteBitLongLong(value);
		}

		public void WriteBitShort(short value)
		{
			this.MainWriter.WriteBitShort(value);
		}

		public void WriteBitThickness(double value)
		{
			this.MainWriter.WriteBitThickness(value);
		}

		public void WriteByte(byte value)
		{
			this.MainWriter.WriteByte(value);
		}

		public void WriteBytes(byte[] bytes)
		{
			this.MainWriter.WriteBytes(bytes);
		}

		public void WriteCmColor(Color value)
		{
			this.MainWriter.WriteCmColor(value);
		}

		public void WriteDateTime(DateTime value)
		{
			this.MainWriter.WriteDateTime(value);
		}

		public void WriteInt(int value)
		{
			this.MainWriter.WriteInt(value);
		}

		public void WriteObjectType(ObjectType value)
		{
			this.MainWriter.WriteObjectType(value);
		}

		public void WriteRawDouble(double value)
		{
			this.MainWriter.WriteRawDouble(value);
		}

		public void WriteRawLong(long value)
		{
			this.MainWriter.WriteRawLong(value);
		}

		public void WriteRawShort(ushort value)
		{
			this.MainWriter.WriteRawShort(value);
		}

		public void WriteSpearShift()
		{
			long mainSizeBits = this.MainWriter.PositionInBits;
			long textSizeBits = this.TextWriter.PositionInBits;

			this.MainWriter.WriteSpearShift();

			if (this._savedPosition)
			{
				int mainTextTotalBits = (int)(mainSizeBits + textSizeBits + 1);
				if (textSizeBits > 0)
				{
					mainTextTotalBits += 16;
					if (textSizeBits >= 0x8000)
					{
						mainTextTotalBits += 16;
						if (textSizeBits >= 0x40000000)
						{
							mainTextTotalBits += 16;
						}
					}
				}

				this.MainWriter.SetPositionInBits(0);
				//Write the total size in bits
				this.MainWriter.WriteRawLong(mainTextTotalBits);
				this.MainWriter.WriteShiftValue();
			}

			this.MainWriter.SetPositionInBits(mainSizeBits);

			if (textSizeBits > 0)
			{
				this.TextWriter.WriteSpearShift();
				this.MainWriter.WriteBytes(((MemoryStream)this.TextWriter.Stream).GetBuffer());
				this.MainWriter.WriteSpearShift();
				this.MainWriter.SetPositionInBits(mainSizeBits + textSizeBits);
				this.MainWriter.SetPositionByFlag(textSizeBits);
				this.MainWriter.WriteBit(value: true);
			}
			else
			{
				this.MainWriter.WriteBit(value: false);
			}

			this.HandleWriter.WriteSpearShift();
			this.MainWriter.WriteBytes(((MemoryStream)this.HandleWriter.Stream).GetBuffer());
			this.MainWriter.WriteSpearShift();
		}

		public void WriteTimeSpan(TimeSpan value)
		{
			this.MainWriter.WriteTimeSpan(value);
		}

		public void WriteVariableText(string value)
		{
			this.TextWriter.WriteVariableText(value);
		}

		public void SetPositionInBits(long posInBits)
		{
			throw new NotImplementedException();
		}

		public void SetPositionByFlag(long pos)
		{
			throw new NotImplementedException();
		}

		public void WriteShiftValue()
		{
			throw new NotImplementedException();
		}
	}
}
