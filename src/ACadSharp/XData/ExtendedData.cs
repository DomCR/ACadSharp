using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.XData
{
	public class ExtendedData
	{
		public AppId AppId { get; }

		public List<ExtendedDataRecord> Records { get; } = new List<ExtendedDataRecord>();

		public ExtendedData(AppId app)
		{
			this.AppId = app;
		}

		public ExtendedData(AppId app, IEnumerable<ExtendedDataRecord> records) : this(app)
		{
			this.Records.AddRange(records);
		}
	}
}
