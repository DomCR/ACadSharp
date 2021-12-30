using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfDocumentBuilder : CadDocumentBuilder
	{
		public Dictionary<string, DwgBlockTemplate> BlockRecords { get; } = new Dictionary<string, DwgBlockTemplate>();

		public DxfDocumentBuilder(CadDocument document, NotificationEventHandler notification = null) : base(document, notification)
		{
		}
	}
}
