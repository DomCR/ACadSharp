using CSMath;
using System;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamWriter : IDisposable
	{
		bool WriteOptional { get; set; }

		void Close();

		void Flush();

		void Write(DxfCode code, object value, DxfClassMap map = null);

		void Write(DxfCode code, IVector value, DxfClassMap map = null);

		void Write(int code, object value, DxfClassMap map = null);

		void Write(int code, IVector value, DxfClassMap map = null);

		void WriteCmColor(int code, Color color, DxfClassMap map = null);

		void WriteHandle(int code, IHandledCadObject value, DxfClassMap map = null);

		void WriteName(int code, INamedCadObject value, DxfClassMap map = null);

		void WriteTrueColor(int code, Color color, DxfClassMap map = null);
	}
}