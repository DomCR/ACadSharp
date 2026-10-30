using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockRepresentationDataTemplate : CadTemplate<BlockRepresentationData>
	{
		public ulong? BlockHandle { get; set; }

		public CadBlockRepresentationDataTemplate() : base(new BlockRepresentationData())
		{
		}

		public CadBlockRepresentationDataTemplate(BlockRepresentationData representation) : base(representation)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if(this.getTableReference(builder, this.BlockHandle, null, out BlockRecord record))
			{
				this.CadObject.Block = record;
			}
		}
	}
}