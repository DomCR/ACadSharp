using System;
using System.IO;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamWriter : IDisposable
	{
		Stream Stream { get; }

		void Write(DxfCode code, object value);

		void Write(int code, object value);
	}
}
