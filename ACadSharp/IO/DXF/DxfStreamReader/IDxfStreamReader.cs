using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamReader
	{
		bool SectionEndFound { get; }
		DxfCode LastDxfCode { get; }
		int LastCode { get; }
		object LastValue { get; }
		int Line { get; }

		string LastValueAsString { get; }
		bool LastValueAsBool { get; }
		short LastValueAsShort { get; }
		int LastValueAsInt { get; }
		long LastValueAsLong { get; }
		double LastValueAsDouble { get; }
		ulong LastValueAsHandle { get; }
		byte[] LastValueAsBinaryChunk { get; }

		/// <summary>
		/// Find a dxf entry in the file.
		/// </summary>
		/// <param name="dxfEntry"></param>
		void Find(string dxfEntry);
		Tuple<DxfCode, object> ReadNext();
	}
}
