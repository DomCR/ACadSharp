using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgDocumentBuilder : CadDocumentBuilder
	{
		public DwgHeaderHandlesCollection HeaderHandles { get; set; }

		public DwgDocumentBuilder(CadDocument document, NotificationEventHandler notification = null)
			: base(document, notification)
		{
		}

		public override void BuildDocument()
		{
			if (this.HeaderHandles.BYLAYER.HasValue && this.TryGetCadObject(this.HeaderHandles.BYLAYER.Value, out LineType lineType))
				this.DocumentToBuild.LineTypes.Add(lineType);
			else
				throw new NotImplementedException();

			if (this.HeaderHandles.BYBLOCK.HasValue && this.TryGetCadObject(this.HeaderHandles.BYBLOCK.Value, out LineType byBlock))
				this.DocumentToBuild.LineTypes.Add(byBlock);
			else
				throw new NotImplementedException();

			if (this.HeaderHandles.CONTINUOUS.HasValue && this.TryGetCadObject(this.HeaderHandles.CONTINUOUS.Value, out LineType continuous))
				this.DocumentToBuild.LineTypes.Add(continuous);
			else
				throw new NotImplementedException();

			base.BuildDocument();
		}
	}
}
