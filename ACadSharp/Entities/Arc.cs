using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System.Collections.Generic;
using System.Text;

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
		/// The end angle in radians. Use 6.28 radians to specify a closed circle or ellipse.
		/// </summary>
		[DxfCodeValue(51)]
		public double EndAngle { get; set; } = 180.0;

		public Arc() : base() { }

		protected override void createCopy(CadObject copy)
		{
			base.createCopy(copy);

			Arc c = copy as Arc;

			c.StartAngle = this.StartAngle;
			c.EndAngle = this.EndAngle;
		}
	}
}
