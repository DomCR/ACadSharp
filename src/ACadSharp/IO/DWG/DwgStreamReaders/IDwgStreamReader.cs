using CSMath;
using CSUtilities.Converters;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	/*
	 NOTE: Unless otherwise stated, all data in this manual is in little-endian order,
			with the least significant byte first.

		B : bit (1 or 0)
		BB : special 2 bit code (entmode in entities, for instance)
		3B : bit triplet (1-3 bits) (R24)
		BS : bitshort (16 bits)
		BL : bitlong (32 bits)
		BLL : bitlonglong (64 bits) (R24)
		BD : bitdouble
		2BD : 2D point (2 bitdoubles)
		3BD : 3D point (3 bitdoubles)
		RC : raw char (not compressed)
		RS : raw short (not compressed)
		RD : raw double (not compressed)
		RL : raw long (not compressed)
		2RD : 2 raw doubles
		3RD : 3 raw doubles
		MC : modular char
		MS : modular short
		H : handle reference (see the HANDLE REFERENCES section)
		T : text (bitshort length, followed by the string).
		TU : Unicode text (bitshort character length, followed by Unicode string,
			2 bytes per character). Unicode text is read from the “string stream”
			within the object data, see the main Object description section for details.
		TV : Variable text, T for 2004 and earlier files, TU for 2007+ files.
		X : special form
		U : unknown
		SN : 16 byte sentinel
		BE : BitExtrusion
		DD : BitDouble With Default
		BT : BitThickness
		3DD : 3D point as 3 DD, needing 3 default values
		CMC : CmColor value
		TC : True Color: this is the same format as CMC in R2004+.
		OT : Object type
	 */

	internal interface IDwgStreamReader
	{
		/// <summary>
		/// Shift to perform after reading a single bit.
		/// </summary>
		int BitShift { get; }

		/// <summary>
		/// Encoding used to read the text.
		/// </summary>
		Encoding Encoding { get; set; }

		/// <summary>
		/// Indicates that the handler is empty of information.
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Current stream position.
		/// </summary>
		long Position { get; set; }

		/// <summary>
		/// Stream that will be read.
		/// </summary>
		Stream Stream { get; }

		/// <summary>
		/// Advance an offset of bytes fordward, saves the last byte.
		/// </summary>
		/// <param name="offset"></param>
		void Advance(int offset);

		/// <summary>
		/// Advance one byte fordward, saves the last byte.
		/// </summary>
		void AdvanceByte();

		/// <summary>
		/// H : handle reference(see the HANDLE REFERENCES section)
		/// </summary>
		/// <returns></returns>
		ulong HandleReference();

		/// <summary>
		/// H : handle reference(see the HANDLE REFERENCES section)
		/// </summary>
		/// <param name="referenceHandle"></param>
		/// <returns></returns>
		ulong HandleReference(ulong referenceHandle);

		/// <summary>
		/// H : handle reference(see the HANDLE REFERENCES section)
		/// </summary>
		/// <param name="referenceHandle"></param>
		/// <param name="reference"></param>
		/// <returns></returns>
		ulong HandleReference(ulong referenceHandle, out DwgReferenceType reference);

		/// <summary>
		/// Get the absolute position in the stream in bits.
		/// </summary>
		/// <returns></returns>
		long PositionInBits();

		/// <summary>
		/// 2BD : 2D point (2 bitdoubles)
		/// </summary>
		/// <returns></returns>
		XY Read2BitDouble();

		/// <summary>
		/// 2DD : 2D point as 2DD, needing 2 default values
		/// </summary>
		/// <param name="defValues"></param>
		/// <returns></returns>
		XY Read2BitDoubleWithDefault(XY defValues);

		/// <summary>
		/// BB : special 2 bit code (entmode in entities, for instance)
		/// </summary>
		/// <returns></returns>
		byte Read2Bits();

		/// <summary>
		/// 2RD : 2 raw doubles
		/// </summary>
		/// <returns></returns>
		XY Read2RawDouble();

		/// <summary>
		/// 3BD : 3D point (3 bitdoubles)
		/// </summary>
		/// <returns></returns>
		XYZ Read3BitDouble();

		/// <summary>
		/// 3DD : 3D point as 3 DD, needing 3 default values
		/// </summary>
		/// <returns></returns>
		XYZ Read3BitDoubleWithDefault(XYZ defValues);

		/// <summary>
		/// 3RD : 3 raw doubles
		/// </summary>
		/// <returns></returns>
		XYZ Read3RawDouble();

		/// <summary>
		/// Reads 2 Integers into a DateTime.
		/// </summary>
		/// <returns></returns>
		DateTime Read8BitJulianDate();

		/// <summary>
		/// B : bit (1 or 0)
		/// </summary>
		/// <returns></returns>
		bool ReadBit();

		/// <summary>
		/// <see cref="IDwgStreamReader.ReadBit"/> return the result as short.
		/// </summary>
		/// <returns></returns>
		short ReadBitAsShort();

		/// <summary>
		/// BD : bitdouble
		/// </summary>
		/// <returns></returns>
		double ReadBitDouble();

		/// <summary>
		/// DD : BitDouble With Default
		/// </summary>
		/// <param name="def"></param>
		/// <returns></returns>
		double ReadBitDoubleWithDefault(double def);

		/// <summary>
		/// BE : BitExtrusion
		/// </summary>
		/// <returns></returns>
		XYZ ReadBitExtrusion();

		/// <summary>
		/// BL : bitlong (32 bits)
		/// </summary>
		/// <returns></returns>
		int ReadBitLong();

		/// <summary>
		/// BLL : bitlonglong (64 bits) (R24)
		/// </summary>
		/// <returns></returns>
		long ReadBitLongLong();

		/// <summary>
		/// BS : bitshort (16 bits)
		/// </summary>
		/// <returns></returns>
		short ReadBitShort();

		/// <summary>
		/// <see cref="IDwgStreamReader.ReadBitShort"/> return the result as bool.
		/// </summary>
		/// <returns></returns>
		bool ReadBitShortAsBool();

		/// <summary>
		/// BT : BitThickness
		/// </summary>
		/// <returns></returns>
		double ReadBitThickness();

		/// <summary>
		/// Read a byte and store the value, apply the shift to correct the bit reading.
		/// </summary>
		/// <returns>Value of the last byte.</returns>
		byte ReadByte();

		byte[] ReadBytes(int length);

		/// <summary>
		/// CMC : CmColor value
		/// </summary>
		/// <returns></returns>
		Color ReadCmColor();

		Color ReadColorByIndex();

		/// <summary>
		/// BL: Julian day
		/// BL: Milliseconds into the day
		/// </summary>
		/// <returns></returns>
		DateTime ReadDateTime();

		double ReadDouble();

		/// <summary>
		/// ENC: This color is used by entities: this definition may contain a DBCOLOR reference and optional transparency.
		/// </summary>
		/// <param name="transparency"></param>
		/// <param name="flag">If true the handle to the color is written in the handle stream.</param>
		/// <returns></returns>
		Color ReadEnColor(out Transparency transparency, out bool flag);

		int ReadInt();

		/// <summary>
		/// MC : modular char
		/// </summary>
		/// <returns></returns>
		ulong ReadModularChar();

		/// <summary>
		/// MS : modular short
		/// </summary>
		/// <returns></returns>
		int ReadModularShort();

		/// <summary>
		/// OT : Object type
		/// </summary>
		/// <returns></returns>
		ObjectType ReadObjectType();

		/// <summary>
		/// RC : raw char (not compressed)
		/// </summary>
		/// <returns></returns>
		char ReadRawChar();

		/// <summary>
		/// RL : raw long (not compressed)
		/// </summary>
		/// <returns></returns>
		long ReadRawLong();

		ulong ReadRawULong();

		/// <summary>
		/// SN : 16 byte sentinel
		/// </summary>
		/// <returns></returns>
		byte[] ReadSentinel();

		short ReadShort();

		short ReadShort<T>() where T : IEndianConverter, new();

		/// <summary>
		/// MC : modular char
		/// </summary>
		/// <remarks>
		/// The 4th bit of the final value it will be the sign.
		/// </remarks>
		/// <returns></returns>
		int ReadSignedModularChar();

		/// <summary>
		/// T : text (bitshort length, followed by the string).
		/// </summary>
		/// <returns></returns>
		string ReadTextUnicode();

		/// <summary>
		/// BL: Days
		/// BL: Milliseconds into the day
		/// </summary>
		/// <returns></returns>
		TimeSpan ReadTimeSpan();

		uint ReadUInt();

		/// <summary>
		/// TV : Variable text, T for 2004 and earlier files, TU for 2007+ files.
		/// </summary>
		/// <returns></returns>
		string ReadVariableText();

		/// <summary>
		/// Sets the shift displacement to 0.
		/// </summary>
		/// <returns></returns>
		ushort ResetShift();

		/// <summary>
		/// Find the position of the string stream.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		long SetPositionByFlag(long position);

		/// <summary>
		/// Set the position in the stream by bits.
		/// </summary>
		/// <param name="positon"></param>
		void SetPositionInBits(long positon);
	}
}