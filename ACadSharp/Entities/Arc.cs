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
		/// 
		/// </summary>
		/// <param name="pt1"></param>
		/// <param name="pt2"></param>
		/// <param name="bulge"></param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		public static Arc CreateFromBulge(XYZ pt1, XYZ pt2, double bulge)
		{
			//Needs a plane of reference in case is in 3D
			throw new System.NotImplementedException();
		}
	}
}
