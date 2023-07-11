namespace ACadSharp.IO.DXF
{
	internal interface IDxfStreamReader
	{
		DxfCode DxfCode { get; }

		GroupCodeValueType GroupCodeValue { get; }
		
		int Code { get; }
		
		object Value { get; }

		/// <summary>
		/// Current line or offset in the file
		/// </summary>
		int Position { get; }

		/// <summary>
		/// Last value read in the dxf file without any transformation
		/// </summary>
		string ValueAsString { get; }

		bool ValueAsBool { get; }

		short ValueAsShort { get; }

		ushort ValueAsUShort { get; }
		
		int ValueAsInt { get; }
		
		long ValueAsLong { get; }
		
		double ValueAsDouble { get; }

		double ValueAsAngle { get; }
		
		ulong ValueAsHandle { get; }
		
		byte[] ValueAsBinaryChunk { get; }

		/// <summary>
		/// Find a dxf entry in the file.
		/// </summary>
		/// <param name="dxfEntry"></param>
		void Find(string dxfEntry);

		void ReadNext();
	}
}
