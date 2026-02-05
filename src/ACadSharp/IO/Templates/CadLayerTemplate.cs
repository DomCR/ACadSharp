using ACadSharp.Objects;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadLayerTemplate : CadTableEntryTemplate<Layer>
	{
		public ulong LayerControlHandle { get; set; }

		public ulong PlotStyleHandle { get; set; }

		public ulong MaterialHandle { get; set; }

		public ulong? LineTypeHandle { get; set; }

		public string LineTypeName { get; set; }

		//TODO: Implement the color name for true color, dxf code 430
		public string TrueColorName { get; set; }

		public CadLayerTemplate() : base(new Layer()) { }

		public CadLayerTemplate(Layer entry) : base(entry) { }

		protected override void build(CadDocumentBuilder builder)
		{
			//TODO: finish the build for the layer

			base.build(builder);

			//this.CadObject.PlotStyleName = builder.GetCadObject(PlotStyleHandle);

			if (builder.TryGetCadObject(this.MaterialHandle, out Material material))
			{

			}
			else
			{
				// builder.Notify($"Linetype with handle {this.LineTypeHandle} could not be found for layer {this.CadObject.Name}", NotificationType.Warning);
			}

			if (this.getTableReference(builder, LineTypeHandle, LineTypeName, out LineType lineType))
			{
				this.CadObject.LineType = lineType;
			}
			else
			{
				builder.Notify($"Linetype with handle {this.LineTypeHandle} could not be found for layer {this.CadObject.Name}", NotificationType.Warning);
			}
		}
	}
}
