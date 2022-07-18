using CSMath;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgmMergedStreamWriter : IDwgStreamWriter
	{
		public Stream Stream { get; }

		public long PositionInBitsValue { get; }

		public IDwgStreamWriter MainWriter { get; }

		public IDwgStreamWriter TextWriter { get; }

		public IDwgStreamWriter HandleWriter { get; }

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

		public void UpdatePositonWriter()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		public void WriteTimeSpan(TimeSpan value)
		{
			this.MainWriter.WriteTimeSpan(value);
		}

		public void WriteVariableText(string value)
		{
			this.TextWriter.WriteVariableText(value);
		}
	}
}
