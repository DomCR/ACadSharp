using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamWriter : IDisposable
	{
		void Write(DxfCode code, object value);

		void Write(int code, object value);
	}
}
