using ACadSharp.Blocks;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgBlockCtrlObjectTemplate : CadTableTemplate<BlockRecord>
	{
		public ulong? ModelSpaceHandle { get; set; }

		public ulong? PaperSpaceHandle { get; set; }

		public DwgBlockCtrlObjectTemplate(BlockRecordsTable blocks) : base(blocks) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject<BlockRecord>(this.ModelSpaceHandle, out BlockRecord modelSpace))
			{
				this.CadObject.Add(modelSpace);
			}

			if (builder.TryGetCadObject<BlockRecord>(this.PaperSpaceHandle, out BlockRecord paperSpace))
			{
				this.CadObject.Add(paperSpace);
			}
		}
	}
}
