using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgLayoutTemplate : DwgTemplate<Layout>
	{
		public ulong PaperSpaceBlockHandle { get; set; }
		public ulong ActiveViewportHandle { get; set; }
		public ulong BaseUcsHandle { get; set; }
		public ulong NamesUcsHandle { get; set; }
		public List<ulong> ViewportHandles { get; set; } = new List<ulong>();
		public DwgLayoutTemplate(Layout layout) : base(layout) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			this.CadObject.AssociatedBlock = builder.GetCadObject<Block>(this.PaperSpaceBlockHandle);

			this.CadObject.Viewport = builder.GetCadObject<Viewport>(this.ActiveViewportHandle);

			if (this.BaseUcsHandle > 0UL)
			{
				this.CadObject.UCS = builder.GetCadObject<UCS>(this.BaseUcsHandle);
			}

			if (this.NamesUcsHandle > 0UL)
			{
				this.CadObject.UCS = builder.GetCadObject<UCS>(this.NamesUcsHandle);
			}

			foreach (var handle in this.ViewportHandles)
			{
				this.CadObject.Viewports.Add(builder.GetCadObject<Viewport>(handle));
			}

			builder.DocumentToBuild.Layouts.Add(this.CadObject);
		}
	}
}
