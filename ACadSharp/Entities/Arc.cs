using ACadSharp.Attributes;
using CSMath;
using System;
using static System.Net.Mime.MediaTypeNames;

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

		/// <summary>
		/// The start angle in radians.
		/// </summary>
		[DxfCodeValue(50)]
		public double StartAngle { get; set; } = 0.0;

		/// <summary>
		/// The end angle in radians. 
		/// </summary>
		/// <remarks>
		/// Use 6.28 radians to specify a closed circle or ellipse.
		/// </remarks>
		[DxfCodeValue(51)]
		public double EndAngle { get; set; } = Math.PI;

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
			if (bulge > 0)
			{
				startAngle = p2.Substract(center).GetAngle();
				endAngle = p1.Substract(center).GetAngle();
			}
			else
			{
				startAngle = p1.Substract(center).GetAngle();
				endAngle = p2.Substract(center).GetAngle();
			}

			return new Arc
			{
				Center = new XYZ(center.X, center.Y, 0),
				Radius = r,
				StartAngle = startAngle,
				EndAngle = endAngle,
			};
		}
	}
}
