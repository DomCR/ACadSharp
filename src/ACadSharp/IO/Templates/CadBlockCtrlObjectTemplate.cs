using ACadSharp.Tables;
using ACadSharp.Tables.Collections;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockCtrlObjectTemplate : CadTableTemplate<BlockRecord>
	{
		public ulong? ModelSpaceHandle { get; set; }

		public ulong? PaperSpaceHandle { get; set; }

		public CadBlockCtrlObjectTemplate(BlockRecordsTable blocks) : base(blocks) { }

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
