using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class DwgEntityTemplate : DwgTemplate<Entity>
	{
		public byte EntityMode { get; set; }
		public byte? LtypeFlags { get; set; }
		public ulong LayerHandle { get; set; }
		public ulong? LineTypeHandle { get; set; }
		public ulong? PrevEntity { get; set; }
		public ulong? NextEntity { get; set; }
		public ulong? ColorHandle { get; set; }

		public DwgEntityTemplate(Entity entity) : base(entity) { }

		public override void Build(DwgDocumentBuilder builder)
		{
			base.Build(builder);

			this.CadObject.Layer = builder.GetCadObject<Layer>(this.LayerHandle);

			//Handle the line type for this entity
			if (this.LtypeFlags.HasValue)
			{
				switch (this.LtypeFlags)
				{
					case 0:
						//Get the linetype by layer
						this.CadObject.LineType = builder.DocumentToBuild.LineTypes["ByLayer"];
						break;
					case 1:
						//Get the linetype by block
						this.CadObject.LineType = builder.DocumentToBuild.LineTypes["ByBlock"];
						break;
					case 2:
						//Get the linetype by continuous
						this.CadObject.LineType = builder.DocumentToBuild.LineTypes["Continuous"];
						break;
					case 3:
						if (this.LineTypeHandle.HasValue)
						{
							applyLineType(builder);
						}
						break;
				}
			}
			else if (this.LineTypeHandle.HasValue)
			{
				applyLineType(builder);
			}
			//TODO: Dxf sets the linetype by name
			else
			{
				this.CadObject.LineType = builder.DocumentToBuild.LineTypes["ByLayer"];
			}

			if (this.ColorHandle.HasValue)
			{
				var dwgColor = builder.GetCadObject<DwgColorTemplate.DwgColor>(this.ColorHandle.Value);

				if (dwgColor != null)
					this.CadObject.Color = dwgColor.Color;
			}
			else
			{
				//TODO: Set color by name, only for dxf?
			}

			//Add the entity to the document
			builder.DocumentToBuild.AddEntity(CadObject);
		}

		private void applyLineType(DwgDocumentBuilder builder)
		{
			this.CadObject.LineType = builder.GetCadObject<LineType>(this.LineTypeHandle.Value);
		}
	}
}
