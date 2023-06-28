using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadLayoutTemplate : CadTemplate<Layout>
	{
		public ulong? PaperSpaceBlockHandle { get; set; }

		public ulong? ActiveViewportHandle { get; set; }

		public ulong? BaseUcsHandle { get; set; }

		public ulong? NamesUcsHandle { get; set; }

		public ulong? LasActiveViewportHandle { get; set; }

		public List<ulong> ViewportHandles { get; set; } = new List<ulong>();

		public CadLayoutTemplate() : base(new Layout()) { }

		public CadLayoutTemplate(Layout layout) : base(layout) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.PaperSpaceBlockHandle, out BlockRecord record))
			{
				this.CadObject.AssociatedBlock = record;
			}

			if (builder.TryGetCadObject(this.ActiveViewportHandle, out Viewport viewport))
			{
				this.CadObject.Viewport = viewport;
			}

			if (builder.TryGetCadObject(this.BaseUcsHandle, out UCS ucs))
			{
				this.CadObject.UCS = ucs;
			}

			if (builder.TryGetCadObject(this.NamesUcsHandle, out UCS nameducs))
			{
				this.CadObject.UCS = nameducs;
			}

			foreach (var handle in this.ViewportHandles)
			{
				if (builder.TryGetCadObject<Viewport>(handle, out Viewport vp))
				{
					//Is repeated, the viewports are already in the entities list in the BLOCK_RECORD
					// if(this.CadObject.AssociatedBlock.Viewports.Contains(vp))
				}
			}
		}
	}
}
