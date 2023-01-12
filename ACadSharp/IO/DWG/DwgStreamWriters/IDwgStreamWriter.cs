using CSMath;
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Writer equivalent to reader <see cref="IDwgStreamReader"/>
	/// </summary>
	internal interface IDwgStreamWriter
	{
		IDwgStreamWriter Main { get; }

		Stream Stream { get; }

		long PositionInBits { get; }

		long SavedPositionInBits { get; }

		void WriteBytes(byte[] bytes);

		void WriteInt(int value);

		void WriteObjectType(ObjectType value);

		void WriteRawLong(long value);

		void WriteBitDouble(double value);

		void WriteBitLong(int value);

		void WriteBitLongLong(long value);

		void WriteVariableText(string value);

		void WriteTextUnicode(string value);

		void WriteBit(bool value);

		void Write2Bits(byte value);

		void WriteBitShort(short value);

		void WriteDateTime(DateTime value);

		void Write8BitJulianDate(DateTime value);

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

		void WriteRawShort(short value);

		void WriteRawShort(ushort value);

		void WriteRawDouble(double value);

		void WriteBitThickness(double thickness);

		void WriteBitExtrusion(XYZ normal);

		void WriteBitDoubleWithDefault(double def, double value);

		void ResetStream();

		void SavePositonForSize();

		void SetPositionInBits(long posInBits);

		void SetPositionByFlag(long pos);

		void WriteShiftValue();
	}
}
