using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.DXF
{
	internal class DxfDocumentBuilder : CadDocumentBuilder
	{
		public DxfDocumentBuilder(CadDocument document, NotificationEventHandler notification = null) : base(document, notification) { }

		public override void BuildDocument()
		{
			base.BuildDocument();
		}
	}
}
