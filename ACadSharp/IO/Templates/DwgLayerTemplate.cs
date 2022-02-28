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

		public string LineTypeName { get; set; }

		public DwgLayerTemplate(Layer entry) : base(entry) { }

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 6:
					LineTypeName = name;
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
					MaterialHandle = handle;
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

			var a = builder.GetCadObject(LayerControlHandle);

			//this.CadObject.PlotStyleName = builder.GetCadObject(PlotStyleHandle);

			var c = builder.GetCadObject(MaterialHandle);

			this.CadObject.LineType = builder.GetCadObject<LineType>(LineTypeHandle);
		}
	}
}
