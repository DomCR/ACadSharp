using ACadSharp.Tables;
using System;
using System.Collections.Generic;

namespace ACadSharp.XData
{
	public class ExtendedData
	{
		public List<ExtendedDataRecord> Records { get; } = new();

		public ExtendedData()
		{
		}

		public ExtendedData(IEnumerable<ExtendedDataRecord> records) : this()
		{
			this.Records.AddRange(records);
		}

		public void AddControlStrings()
		{
			throw new NotImplementedException();
		}
	}
}
