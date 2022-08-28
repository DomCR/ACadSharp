using ACadSharp.Entities;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadEntityTemplate : CadTemplate<Entity>
	{
		public byte EntityMode { get; set; }

		public byte? LtypeFlags { get; set; }

		public ulong? LayerHandle { get; set; }

		public string LayerName { get; set; }

		public ulong? LineTypeHandle { get; set; }

		public string LtypeName { get; set; }

		public ulong? PrevEntity { get; set; }

		public ulong? NextEntity { get; set; }

		public ulong? ColorHandle { get; set; }

		public ulong? MaterialHandle { get; set; }

		public CadEntityTemplate(Entity entity) : base(entity) { }

		public override bool CheckDxfCode(int dxfcode, object value)
		{
			switch (dxfcode)
			{
				//Absent or zero indicates entity is in model space.
				//1 indicates entity is in paper space (optional).
				case 67:
					return true;
				default:
					return false;
			}
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
			}

			return value;
		}

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 6:
					this.LtypeName = name;
					value = true;
					break;
				case 8:
					this.LayerName = name;
					value = true;
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.LayerHandle.HasValue && builder.TryGetCadObject<Layer>(this.LayerHandle.Value, out Layer layer))
				this.CadObject.Layer = layer;

			//Handle the line type for this entity
			if (this.LtypeFlags.HasValue)
			{
				switch (this.LtypeFlags)
				{
					case 0:
						//Get the linetype by layer
						this.CadObject.LineType = builder.LineTypes["ByLayer"];
						break;
					case 1:
						//Get the linetype by block
						this.CadObject.LineType = builder.LineTypes["ByBlock"];
						break;
					case 2:
						//Get the linetype by continuous
						this.CadObject.LineType = builder.LineTypes["Continuous"];
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
			else
			{
				//TODO: Dxf sets the linetype by name
				// this.CadObject.LineType = builder.LineTypes["ByLayer"];
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
		}

		private void applyLineType(CadDocumentBuilder builder)
		{
			this.CadObject.LineType = builder.GetCadObject<LineType>(this.LineTypeHandle.Value);
		}
	}

	internal class CadVertexTemplate : CadEntityTemplate
	{
		public CadVertexTemplate() : base(new VertexPlaceholder())
		{
		}

		internal void SetVertexObject(Vertex vertex)
		{
			vertex.Handle = this.CadObject.Handle;
			vertex.Owner = this.CadObject.Owner;

			vertex.XDictionary = this.CadObject.XDictionary;

			//polyLine.Reactors = this.CadObject.Reactors;
			//polyLine.ExtendedData = this.CadObject.ExtendedData;

			vertex.Color = this.CadObject.Color;
			vertex.Lineweight = this.CadObject.Lineweight;
			vertex.LinetypeScale = this.CadObject.LinetypeScale;
			vertex.IsInvisible = this.CadObject.IsInvisible;
			vertex.Transparency = this.CadObject.Transparency;

			VertexPlaceholder placeholder = this.CadObject as VertexPlaceholder;

			vertex.Location = placeholder.Location;
			vertex.StartWidth = placeholder.StartWidth;
			vertex.EndWidth = placeholder.EndWidth;
			vertex.Bulge = placeholder.Bulge;
			vertex.Flags = placeholder.Flags;
			vertex.CurveTangent = placeholder.CurveTangent;
			vertex.Id = placeholder.Id;

			this.CadObject = vertex;
		}

		public class VertexPlaceholder : Vertex
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}
	}
}
