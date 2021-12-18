using ACadSharp.IO.DWG;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class DwgLayerTemplate : DwgTableEntryTemplate<Layer>
	{
		public ulong LayerControlHandle { get; set; }
		public ulong PlotStyleHandle { get; set; }
		public ulong MaterialHandle { get; set; }
		public ulong LineTypeHandle { get; set; }

		public DwgLayerTemplate(Layer entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			//TODO: finish the build for the layer

			base.Build(builder);

			var a = builder.GetCadObject(LayerControlHandle);

			//this.CadObject.PlotStyleName = builder.GetCadObject(PlotStyleHandle);

			var c = builder.GetCadObject(MaterialHandle);

			this.CadObject.LineType = builder.GetCadObject<LineType>(LineTypeHandle);
		}
	}
}
