using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSUtilities.Extensions;

namespace ACadSharp.IO.Templates
{
	internal class CadEntityTemplate : CadTemplate<Entity>
	{
		public byte EntityMode { get; set; }

		public byte? LtypeFlags { get; set; }

		public ulong? LayerHandle { get; set; }

		public string LayerName { get; set; }

		public ulong? LineTypeHandle { get; set; }

		public string LineTypeName { get; set; }

		public ulong? PrevEntity { get; set; }

		public ulong? NextEntity { get; set; }

		public ulong? ColorHandle { get; set; }

		public string BookColorName { get; set; }

		public ulong? MaterialHandle { get; set; }

		public CadEntityTemplate(Entity entity) : base(entity) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			if (this.getTableReference(builder, this.LayerHandle, this.LayerName, out Layer layer))
			{
				this.CadObject.Layer = layer;
			}

			switch (this.LtypeFlags)
			{
				case 0:
					//Get the linetype by layer
					this.LineTypeName = LineType.ByLayerName;
					break;
				case 1:
					//Get the linetype by block
					this.LineTypeName = LineType.ByBlockName;
					break;
				case 2:
					//Get the linetype by continuous
					this.LineTypeName = LineType.ContinuousName;
					break;
			}

			if (this.getTableReference<LineType>(builder, this.LineTypeHandle, this.LineTypeName, out LineType ltype))
			{
				this.CadObject.LineType = ltype;
			}

			BookColor color;
			if (builder.TryGetCadObject(this.ColorHandle, out color))
			{
				this.CadObject.BookColor = color;
			}
			else if (!this.BookColorName.IsNullOrEmpty() &&
				builder.DocumentToBuild != null &&
				builder.DocumentToBuild.Colors != null &&
				builder.DocumentToBuild.Colors.TryGetValue(this.BookColorName, out color))
			{
				this.CadObject.BookColor = color;
			}
		}

		public void SetUnlinkedReferences()
		{
			if (!string.IsNullOrEmpty(this.LayerName))
			{
				this.CadObject.Layer = new Layer(this.LayerName);
			}

			if (!string.IsNullOrEmpty(this.LineTypeName))
			{
				this.CadObject.LineType = new LineType(this.LineTypeName);
			}
		}
	}

	internal class CadEntityTemplate<T> : CadEntityTemplate
			where T : Entity, new()
	{
		public new T CadObject { get { return (T)base.CadObject; } set { base.CadObject = value; } }

		public CadEntityTemplate() : base(new T()) { }

		public CadEntityTemplate(T entity) : base(entity)
		{
		}
	}
}
