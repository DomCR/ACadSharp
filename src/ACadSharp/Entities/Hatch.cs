using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Hatch"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityHatch"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Hatch"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityHatch)]
	[DxfSubClass(DxfSubclassMarker.Hatch)]
	public partial class Hatch : Entity
	{
		/// <summary>
		/// The current elevation of the object.
		/// </summary>
		[DxfCodeValue(30)]
		public double Elevation { get; set; }

		/// <summary>
		/// Gradient color pattern, if exists.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 470)]
		public HatchGradientPattern GradientColor { get; set; } = new HatchGradientPattern();

		/// <summary>
		/// Associativity flag.
		/// </summary>
		[DxfCodeValue(71)]
		public bool IsAssociative { get; set; }

		/// <summary>
		/// Hatch pattern double flag (pattern fill only)
		/// </summary>
		[DxfCodeValue(77)]
		public bool IsDouble { get; set; }

		/// <summary>
		/// Solid fill flag
		/// </summary>
		[DxfCodeValue(70)]
		public bool IsSolid { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityHatch;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.HATCH;

		/// <summary>
		/// Boundary paths (loops).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<BoundaryPath> Paths { get; set; } = new List<BoundaryPath>();

		/// <summary>
		/// Pattern of this hatch.
		/// </summary>
		/// <value>
		/// Default value: SOLID pattern.
		/// </value>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public HatchPattern Pattern { get; set; } = HatchPattern.Solid;

		/// <summary>
		/// Hatch pattern angle (pattern fill only).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 52)]
		public double PatternAngle
		{
			get
			{
				return this._patternAngle;
			}

			set
			{
				this._patternAngle = value;
				this.Pattern?.Update(XY.Zero, this._patternAngle, 1);
			}
		}

		/// <summary>
		/// Hatch pattern scale or spacing(pattern fill only).
		/// </summary>
		[DxfCodeValue(41)]
		public double PatternScale
		{
			get
			{
				return this._patternScale;
			}

			set
			{
				this._patternScale = value;
				this.Pattern?.Update(XY.Zero, 0, this._patternScale);
			}
		}

		/// <summary>
		/// Hatch pattern type
		/// </summary>
		[DxfCodeValue(76)]
		public HatchPatternType PatternType { get; set; }

		/// <summary>
		/// Pixel size used to determine the density to perform various intersection and ray casting operations in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
		/// </summary>
		[DxfCodeValue(47)]
		public double PixelSize { get; set; }

		/// <summary>
		/// Seed points codes (in OCS)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 98)]
		[DxfCollectionCodeValue(10, 20)]
		public List<XY> SeedPoints { get; set; } = new List<XY>();

		//63	For MPolygon, pattern fill color as the ACI
		/// <summary>
		/// Hatch style.
		/// </summary>
		[DxfCodeValue(75)]
		public HatchStyleType Style { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Hatch;

		private double _patternAngle;

		private double _patternScale;

		//73	For MPolygon, boundary annotation flag:
		//0 = boundary is not an annotated boundary
		//1 = boundary is an annotated boundary
		//78	Number of pattern definition lines
		//varies
		//Pattern line data.Repeats number of times specified by code 78. See Pattern Data
		//11	For MPolygon, offset vector

		//99	For MPolygon, number of degenerate boundary paths(loops), where a degenerate boundary path is a border that is ignored by the hatch
		/// <inheritdoc/>
		public Hatch() : base() { }

		public override void ApplyTransform(Transform transform)
		{
			if (this.IsAssociative)
			{
			}

			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Hatch clone = base.Clone() as Hatch;

			clone.GradientColor = this.GradientColor?.Clone();
			clone.Pattern = this.Pattern?.Clone();

			clone.Paths = new List<BoundaryPath>();
			foreach (BoundaryPath item in this.Paths)
			{
				clone.Paths.Add(item.Clone());
			}

			return clone;
		}

		/// <summary>
		/// Explode the hatch edges into the equivalent entities.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<Entity> Explode()
		{
			List<Entity> entities = new List<Entity>();

			foreach (BoundaryPath b in this.Paths)
			{
				foreach (BoundaryPath.Edge e in b.Edges)
				{
					entities.Add(e.ToEntity());
				}
			}

			return entities;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			BoundingBox box = BoundingBox.Null;

			foreach (BoundaryPath bp in this.Paths)
			{
				box = box.Merge(bp.GetBoundingBox());
			}

			return box;
		}
	}
}