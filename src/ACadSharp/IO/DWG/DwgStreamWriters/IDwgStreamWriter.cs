using CSMath;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

/// <summary>
/// Writer equivalent to reader <see cref="IDwgStreamReader"/>
/// </summary>
internal interface IDwgStreamWriter
{
	Encoding Encoding { get; }

	IDwgStreamWriter Main { get; }

	long PositionInBits { get; }

	long SavedPositionInBits { get; }

	Stream Stream { get; }

	void HandleReference(IHandledCadObject cadObject);

	void HandleReference(DwgReferenceType type, IHandledCadObject cadObject);

	void HandleReference(ulong handle);

	void HandleReference(DwgReferenceType type, ulong handle);

	void ResetStream();

	void SavePositonForSize();

	void SetPositionByFlag(long pos);

	void SetPositionInBits(long posInBits);

	void Write2BitDouble(XY value);

	void Write2BitDoubleWithDefault(XY def, XY value);

	void Write2Bits(byte value);

	void Write2RawDouble(XY value);

	void Write3BitDouble(XYZ value);

	void Write3BitDoubleWithDefault(XYZ def, XYZ value);

	void Write8BitJulianDate(DateTime value);

	void WriteBit(bool value);

	void WriteBitDouble(double value);

	void WriteBitDoubleWithDefault(double def, double value);

	void WriteBitExtrusion(XYZ normal);

	void WriteBitLong(int value);

	void WriteBitLongLong(long value);

	void WriteBitShort(short value);

	void WriteBitThickness(double thickness);

	void WriteByte(byte value);

	void WriteBytes(byte[] bytes);

	void WriteBytes(byte[] bytes, int offset, int length);

	void WriteCmColor(Color value, bool useTextStream = false);

	void WriteDateTime(DateTime value);

	void WriteEnColor(Color color, Transparency transparency);

	void WriteEnColor(Color color, Transparency transparency, bool isBookColor);

	void WriteInt(int value);

	void WriteObjectType(short value);

	void WriteObjectType(ObjectType value);

	void WriteRawDouble(double value);

	void WriteRawLong(long value);

	void WriteRawShort(short value);

	void WriteRawShort(ushort value);

	void WriteShiftValue();

	void WriteSpearShift();

	void WriteTextUnicode(string value);

	void WriteTimeSpan(TimeSpan value);

	void WriteUInt(uint value);

	void WriteVariableText(string value);
}