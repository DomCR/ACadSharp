using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	public partial class Hatch
	{
		public partial class BoundaryPath
		{
			public class Line : Edge
			{
				/// <summary>
				/// Endpoint (in OCS)
				/// </summary>
				[DxfCodeValue(11, 21)]
				public XY End { get; set; }

				/// <summary>
				/// Start point (in OCS)
				/// </summary>
				[DxfCodeValue(10, 20)]
				public XY Start { get; set; }

				public override EdgeType Type => EdgeType.Line;

				/// <inheritdoc/>
				public override void ApplyTransform(Transform transform)
				{
					this.Start = transform.ApplyTransform(this.Start.Convert<XYZ>()).Convert<XY>();
					this.End = transform.ApplyTransform(this.End.Convert<XYZ>()).Convert<XY>();
				}

				/// <inheritdoc/>
				public override BoundingBox GetBoundingBox()
				{
					return BoundingBox.FromPoints([(XYZ)this.Start, (XYZ)this.End]);
				}

				/// <inheritdoc/>
				public override Entity ToEntity()
				{
					return new Entities.Line(this.Start, this.End);
				}
			}
		}
	}
}