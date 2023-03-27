using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamWriter : IDisposable
	{
		void Write(DxfCode code, object value);

		void Write(int code, object value);

		void Flush();

		void Close();
	}
}
