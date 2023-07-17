using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamWriter : IDisposable
	{
		void Write(DxfCode code, object value);

		void Write(DxfCode code, object value, DxfClassMap map);

		void Write(int code, object value);

		void Write(int code, object value, DxfClassMap map);

		void WriteHandle(int code, CadObject value, DxfClassMap map);

		void Flush();

		void Close();
	}
}
