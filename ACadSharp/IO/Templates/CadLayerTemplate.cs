using ACadSharp.IO.DWG;
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

		public CadLayerTemplate(Layer entry) : base(entry) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			bool found = base.CheckDxfCode(dxfcode, value);
			if (found)
				return found;

			switch (dxfcode)
			{
				case 348:
					// Code not documented
					found = true;
					break;
			}

			return found;
		}

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 6:
					this.LineTypeName = name;
					value = true;
					break;
				default:
					break;
			}

			return value;
		}

		public override bool AddHandle(int dxfcode, ulong handle)
		{
			bool value = base.AddHandle(dxfcode, handle);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 347:
					this.MaterialHandle = handle;
					value = true;
					break;
				case 390:
					//Hard-pointer ID/handle of PlotStyleName object
					value = true;
					break;
				default:
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			//TODO: finish the build for the layer

			base.Build(builder);

			//this.CadObject.PlotStyleName = builder.GetCadObject(PlotStyleHandle);

			if (builder.TryGetCadObject(this.MaterialHandle, out Material material))
			{

			}
			else
			{
				// builder.Notify($"Linetype with handle {this.LineTypeHandle} could not be found for layer {this.CadObject.Name}", NotificationType.Warning);
			}

			LineType lineType;
			if (builder.TryGetCadObject(this.LineTypeHandle, out lineType))
			{
				this.CadObject.LineType = lineType;
			}
			else if (!string.IsNullOrEmpty(this.LineTypeName) && builder.DocumentToBuild.LineTypes.TryGetValue(this.LineTypeName, out lineType))
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
