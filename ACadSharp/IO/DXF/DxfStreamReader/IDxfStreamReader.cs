using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamReader
	{
		DxfCode LastDxfCode { get; }

		GroupCodeValueType LastGroupCodeValue { get; }
		
		int LastCode { get; }
		
		object LastValue { get; }

		/// <summary>
		/// Current line or offset in the file
		/// </summary>
		int Line { get; }

		/// <summary>
		/// Last value read in the dxf file without any transformation
		/// </summary>
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
