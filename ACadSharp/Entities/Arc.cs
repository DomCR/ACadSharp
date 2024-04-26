using ACadSharp.Attributes;
using CSMath;
using System;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Arc"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityArc"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Arc"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityArc)]
	[DxfSubClass(DxfSubclassMarker.Arc)]
	public class Arc : Circle
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.ARC;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityArc;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Arc;

		/// <summary>
		/// The start angle in radians.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.IsAngle, 50)]
		public double StartAngle { get; set; } = 0.0;

		/// <summary>
		/// The end angle in radians. 
		/// </summary>
		/// <remarks>
		/// Use 6.28 radians to specify a closed circle or ellipse.
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.IsAngle, 51)]
		public double EndAngle { get; set; } = Math.PI;

		/// <summary>
		/// Default constructor
		/// </summary>
		public Arc() : base() { }

		/// <summary>
		/// Creates an arc using 2 points and a bulge
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <param name="bulge"></param>
		/// <returns></returns>
		public static Arc CreateFromBulge(XY p1, XY p2, double bulge)
		{
			XY center = MathUtils.GetCenter(p1, p2, bulge, out double r);

			double startAngle;
			double endAngle;
			if (bulge < 0)
			{
				startAngle = p2.Subtract(center).GetAngle();
				endAngle = p1.Subtract(center).GetAngle();
			}
			else
			{
				startAngle = p1.Subtract(center).GetAngle();
				endAngle = p2.Subtract(center).GetAngle();
			}

			return new Arc
			{
				Center = new XYZ(center.X, center.Y, 0),
				Radius = r,
				StartAngle = startAngle,
				EndAngle = endAngle,
			};
		}

		/// <summary>
		/// Process the 2 points limiting the arc segment
		/// </summary>
		/// <param name="start">Start point of the arc segment</param>
		/// <param name="end">End point of the arc segment</param>
		public void GetEndVertices(out XYZ start, out XYZ end)
		{
			if (this.Normal != XYZ.AxisZ)
			{
				throw new NotImplementedException("GetBoundPoints box for not aligned Normal is not implemented");
			}

			double tmpEndAngle = this.EndAngle;

			if (this.EndAngle < this.StartAngle)
			{
				tmpEndAngle += 2 * Math.PI;
			}

			double delta = tmpEndAngle - this.StartAngle;

			double angle = this.StartAngle + delta;
			double startX = this.Radius * Math.Sin(angle);
			double startY = this.Radius * Math.Cos(angle);

			startX = MathUtils.IsZero(startX) ? 0 : startX;
			startY = MathUtils.IsZero(startY) ? 0 : startY;

			start = new XYZ(startX, startY, 0);

			double angle2 = this.StartAngle + delta * 2;
			double endX = (this.Radius * Math.Sin(angle2));
			double endY = (this.Radius * Math.Cos(angle2));

			endX = MathUtils.IsZero(endX) ? 0 : endX;
			endY = MathUtils.IsZero(endY) ? 0 : endY;

			end = new XYZ(endX, endY, 0);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			throw new NotImplementedException();
		}
	}
}
