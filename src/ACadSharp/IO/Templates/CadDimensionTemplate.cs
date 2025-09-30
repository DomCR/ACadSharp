using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionTemplate : CadEntityTemplate
	{
		public ulong? StyleHandle { get; set; }

		public ulong? BlockHandle { get; set; }

		public string BlockName { get; set; }

		public string StyleName { get; set; }

		public CadDimensionTemplate() : base(new DimensionPlaceholder()) { }

		public CadDimensionTemplate(Dimension dimension) : base(dimension) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			Dimension dimension = this.CadObject as Dimension;

			if (this.getTableReference(builder, this.StyleHandle, this.StyleName, out DimensionStyle style))
			{
				dimension.Style = style;
			}

			if (this.getTableReference(builder, this.BlockHandle, this.BlockName, out BlockRecord block))
			{
				dimension.Block = block;
			}
		}

		public class DimensionPlaceholder : Dimension
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }

			public override double Measurement { get; }

			public DimensionPlaceholder() : base(DimensionType.Linear) { }

			public override BoundingBox GetBoundingBox()
			{
				throw new System.InvalidOperationException();
			}

			public override void ApplyTransform(Transform transform)
			{
				throw new System.NotImplementedException();
			}

			public override void UpdateBlock()
			{
				throw new System.NotImplementedException();
			}
		}

		public void SetDimensionFlags(DimensionType flags)
		{
			Dimension dimension = this.CadObject as Dimension;
			dimension.Flags = flags;
		}

		public void SetDimensionObject(Dimension dimension)
		{
			dimension.Handle = this.CadObject.Handle;
			dimension.Owner = this.CadObject.Owner;

			dimension.XDictionary = this.CadObject.XDictionary;
			//dimensionAligned.Reactors = this.CadObject.Reactors;
			//dimensionAligned.ExtendedData = this.CadObject.ExtendedData;

			dimension.Color = this.CadObject.Color;
			dimension.LineWeight = this.CadObject.LineWeight;
			dimension.LineTypeScale = this.CadObject.LineTypeScale;
			dimension.IsInvisible = this.CadObject.IsInvisible;
			dimension.Transparency = this.CadObject.Transparency;

			Dimension source = this.CadObject as Dimension;

			dimension.Version = source.Version;
			dimension.DefinitionPoint = source.DefinitionPoint;
			dimension.TextMiddlePoint = source.TextMiddlePoint;
			dimension.InsertionPoint = source.InsertionPoint;
			dimension.Normal = source.Normal;
			dimension.IsTextUserDefinedLocation = source.IsTextUserDefinedLocation;
			dimension.AttachmentPoint = source.AttachmentPoint;
			dimension.LineSpacingStyle = source.LineSpacingStyle;
			dimension.LineSpacingFactor = source.LineSpacingFactor;
			//dimensionAligned.Measurement = dimension.Measurement;
			dimension.Text = source.Text;
			dimension.TextRotation = source.TextRotation;
			dimension.HorizontalDirection = source.HorizontalDirection;

			dimension.Flags = source.Flags;

			if (this.CadObject is DimensionAligned aligned &&
				dimension is DimensionLinear linear)
			{
				linear.FirstPoint = aligned.FirstPoint;
				linear.SecondPoint = aligned.SecondPoint;
				linear.ExtLineRotation = aligned.ExtLineRotation;
			}

			this.CadObject = dimension;
		}
	}
}
