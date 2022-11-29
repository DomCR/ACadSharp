using CSMath;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgmMergedStreamWriter : IDwgStreamWriter
	{
		public IDwgStreamWriter Main { get; }

		public IDwgStreamWriter TextWriter { get; }

		public IDwgStreamWriter HandleWriter { get; }

		public Stream Stream { get; }

		public long SavedPositionInBits { get; private set; }

		public long PositionInBits { get; private set; }

		protected bool _savedPosition;

		public DwgmMergedStreamWriter(Stream stream, IDwgStreamWriter main, IDwgStreamWriter textwriter, IDwgStreamWriter handlewriter)
		{
			this.Stream = stream;
			this.Main = main;
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
			this.Main.ResetStream();
			this.TextWriter.ResetStream();
			this.HandleWriter.ResetStream();
		}

		public void SavePositonForSize()
		{
			this._savedPosition = true;
			this.PositionInBits = this.Main.PositionInBits;
			//Save this position for the size in bits
			this.Main.WriteRawLong(0);
		}

		public void Write2RawDouble(XY value)
		{
			this.Main.Write2RawDouble(value);
		}

		public void Write3BitDouble(XYZ value)
		{
			this.Main.Write3BitDouble(value);
		}

		public void WriteBit(bool value)
		{
			this.Main.WriteBit(value);
		}

		public void Write2Bits(byte value)
		{
			this.Main.Write2Bits(value);
		}

		public void WriteBitDouble(double value)
		{
			this.Main.WriteBitDouble(value);
		}

		public void WriteBitDoubleWithDefault(double def, double value)
		{
			this.Main.WriteBitDoubleWithDefault(def, value);
		}

		public void WriteBitExtrusion(XYZ value)
		{
			this.Main.WriteBitExtrusion(value);
		}

		public void WriteBitLong(int value)
		{
			this.Main.WriteBitLong(value);
		}

		public void WriteBitLongLong(long value)
		{
			this.Main.WriteBitLongLong(value);
		}

		public void WriteBitShort(short value)
		{
			this.Main.WriteBitShort(value);
		}

		public void WriteBitThickness(double value)
		{
			this.Main.WriteBitThickness(value);
		}

		public void WriteByte(byte value)
		{
			this.Main.WriteByte(value);
		}

		public void WriteBytes(byte[] bytes)
		{
			this.Main.WriteBytes(bytes);
		}

		public void WriteCmColor(Color value)
		{
			this.Main.WriteCmColor(value);
		}

		public void WriteDateTime(DateTime value)
		{
			this.Main.WriteDateTime(value);
		}

		public void Write8BitJulianDate(DateTime value)
		{
			this.Main.Write8BitJulianDate(value);
		}

		public void WriteInt(int value)
		{
			this.Main.WriteInt(value);
		}

		public void WriteObjectType(ObjectType value)
		{
			this.Main.WriteObjectType(value);
		}

		public void WriteRawDouble(double value)
		{
			this.Main.WriteRawDouble(value);
		}

		public void WriteRawLong(long value)
		{
			this.Main.WriteRawLong(value);
		}

		public void WriteRawShort(short value)
		{
			this.Main.WriteRawShort(value);
		}

		public void WriteRawShort(ushort value)
		{
			this.Main.WriteRawShort(value);
		}

		public virtual void WriteSpearShift()
		{
			long mainSizeBits = this.Main.PositionInBits;
			long textSizeBits = this.TextWriter.PositionInBits;

			this.Main.WriteSpearShift();

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

				this.Main.SetPositionInBits(this.PositionInBits);
				//Write the total size in bits
				this.Main.WriteRawLong(mainTextTotalBits);
				this.Main.WriteShiftValue();
			}

			this.Main.SetPositionInBits(mainSizeBits);

			if (textSizeBits > 0)
			{
				this.TextWriter.WriteSpearShift();
				this.Main.WriteBytes(((MemoryStream)this.TextWriter.Stream).GetBuffer());
				this.Main.WriteSpearShift();
				this.Main.SetPositionInBits(mainSizeBits + textSizeBits);
				this.Main.SetPositionByFlag(textSizeBits);
				this.Main.WriteBit(true);
			}
			else
			{
				this.Main.WriteBit(false);
			}

			this.HandleWriter.WriteSpearShift();
			this.SavedPositionInBits = this.Main.PositionInBits;
			this.Main.WriteBytes(((MemoryStream)this.HandleWriter.Stream).GetBuffer());
			this.Main.WriteSpearShift();
		}

		public void WriteTimeSpan(TimeSpan value)
		{
			this.Main.WriteTimeSpan(value);
		}

		public void WriteVariableText(string value)
		{
			this.TextWriter.WriteVariableText(value);
		}

		public void WriteTextUnicode(string value)
		{
			this.TextWriter.WriteTextUnicode(value);
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
