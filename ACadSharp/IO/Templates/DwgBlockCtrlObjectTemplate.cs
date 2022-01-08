using ACadSharp.Blocks;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgBlockCtrlObjectTemplate : DwgTableTemplate<BlockRecord>
	{
		public ulong ModelSpaceHandle { get; set; }
		public ulong PaperSpaceHandle { get; set; }
		public DwgBlockCtrlObjectTemplate(BlockRecordsTable blocks) : base(blocks) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			BlockRecord modelSpace = builder.GetCadObject<BlockRecord>(this.ModelSpaceHandle);
			BlockRecord paperSpace = builder.GetCadObject<BlockRecord>(this.PaperSpaceHandle);

			this.CadObject.Add(modelSpace);
			this.CadObject.Add(paperSpace);
		}
	}
}
