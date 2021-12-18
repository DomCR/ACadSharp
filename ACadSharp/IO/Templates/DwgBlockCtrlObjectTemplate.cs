using ACadSharp.Blocks;
using ACadSharp.IO.DWG;
using ACadSharp.Tables.Collections;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgBlockCtrlObjectTemplate : DwgTableTemplate<Block>
	{
		public ulong ModelSpaceHandle { get; set; }
		public ulong PaperSpaceHandle { get; set; }
		public List<ulong> Handles { get; set; } = new List<ulong>();
		public DwgBlockCtrlObjectTemplate(BlockRecordsTable blocks) : base(blocks) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Block modelSpace = builder.GetCadObject<Block>(this.ModelSpaceHandle);
			Block paperSpace = builder.GetCadObject<Block>(this.PaperSpaceHandle);

			this.CadObject.Add(modelSpace);
			this.CadObject.Add(paperSpace);
		}
	}
}
