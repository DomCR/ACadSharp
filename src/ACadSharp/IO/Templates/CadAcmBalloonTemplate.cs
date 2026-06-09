using ACadSharp.Entities.Mechanical;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadAcmBalloonTemplate : CadEntityTemplate<AcmBalloon>
	{
		public ulong? BlockHandle { get; set; }

		public CadAcmBalloonTemplate() : base(new AcmBalloon())
		{
		}

		public CadAcmBalloonTemplate(AcmBalloon representation) : base(representation)
		{
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.getTableReference(builder, this.BlockHandle, null, out BlockRecord record))
			{
				this.CadObject.Block = record;
			}
		}
	}
}
