using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.IO.DWG;
using ACadSharp.Tables;
using System;

namespace ACadSharp.IO.Templates
{
	internal class CadDimensionTemplate : CadEntityTemplate
	{
		public ulong StyleHandle { get; set; }

		public ulong BlockHandle { get; set; }

		public string BlockName { get; set; }

		public string StyleName { get; set; }

		public CadDimensionTemplate() : base(new DimensionPlaceholder()) { }

		public CadDimensionTemplate(Dimension dimension) : base(dimension) { }

		public override bool AddName(int dxfcode, string name)
		{
			bool value = base.AddName(dxfcode, name);
			if (value)
				return value;

			switch (dxfcode)
			{
				case 2:
					this.BlockName = name;
					value = true;
					break;
				case 3:
					this.StyleName = name;
					value = true;
					break;
			}

			return value;
		}

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			Dimension dimension = this.CadObject as Dimension;

			if (builder.TryGetCadObject<DimensionStyle>(this.StyleHandle, out DimensionStyle style))
			{
				dimension.Style = style;
			}

			if (builder.TryGetCadObject<Block>(this.BlockHandle, out Block block))
			{
				dimension.Block = block;
			}
		}

		public class DimensionPlaceholder : Dimension
		{
			public override ObjectType ObjectType { get { return ObjectType.INVALID; } }
		}

		public void SetDimensionObject(Dimension dimensionAligned)
		{
			dimensionAligned.Handle = this.CadObject.Handle;
			dimensionAligned.Owner = this.CadObject.Owner;
			dimensionAligned.XDictionary = this.CadObject.XDictionary;
			//dimensionAligned.Reactors = this.CadObject.Reactors;
			//dimensionAligned.ExtendedData = this.CadObject.ExtendedData;

			dimensionAligned.Color = this.CadObject.Color;
			dimensionAligned.Lineweight = this.CadObject.Lineweight;
			dimensionAligned.LinetypeScale = this.CadObject.LinetypeScale;
			dimensionAligned.IsInvisible = this.CadObject.IsInvisible;
			dimensionAligned.Transparency = this.CadObject.Transparency;

			Dimension dimension = this.CadObject as Dimension;

			dimensionAligned.Version = dimension.Version;
			dimensionAligned.DefinitionPoint = dimension.DefinitionPoint;
			dimensionAligned.TextMiddlePoint = dimension.TextMiddlePoint;
			dimensionAligned.InsertionPoint = dimension.InsertionPoint;
			dimensionAligned.Normal = dimension.Normal;
			dimensionAligned.DimensionType = dimension.DimensionType;
			dimensionAligned.AttachmentPoint = dimension.AttachmentPoint;
			dimensionAligned.LineSpacingStyle = dimension.LineSpacingStyle;
			dimensionAligned.LineSpacingFactor = dimension.LineSpacingFactor;
			dimensionAligned.Measurement = dimension.Measurement;
			dimensionAligned.Text = dimension.Text;
			dimensionAligned.TextRotation = dimension.TextRotation;
			dimensionAligned.HorizontalDirection = dimension.HorizontalDirection;

			this.CadObject = dimensionAligned;
		}
	}
}
