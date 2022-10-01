using ACadSharp.IO.DWG;
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

			var a = builder.GetCadObject(this.LayerControlHandle);

			//this.CadObject.PlotStyleName = builder.GetCadObject(PlotStyleHandle);

			var c = builder.GetCadObject(this.MaterialHandle);

			if (builder.TryGetCadObject<LineType>(this.LineTypeHandle, out LineType lineType))
			{
				this.CadObject.LineType = lineType;
			}
			else
			{

			}
		}
	}
}
