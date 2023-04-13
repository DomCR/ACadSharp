using ACadSharp.Attributes;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Shape"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityShape"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Shape"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityShape)]
	[DxfSubClass(DxfSubclassMarker.Shape)]
	public class Shape : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.SHAPE;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityShape;

		/// <summary>
		/// Thickness
		/// </summary>
		[DxfCodeValue(39)]
		public double Thickness { get; set; } = 0.0;

		/// <summary>
		/// Insertion point (in WCS)
		/// </summary>
		[DxfCodeValue(10,20,30)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// Size
		/// </summary>
		[DxfCodeValue(40)]
		public double Size { get; set; } = 0.0;

		/// <summary>
		/// Shape name
		/// </summary>
		[DxfCodeValue(2)]
		public string Name { get; set; }

		/// <summary>
		/// Rotation angle
		/// </summary>
		[DxfCodeValue(50)]
		public double Rotation { get; set; } = 0;

		/// <summary>
		/// Relative X scale factor
		/// </summary>
		[DxfCodeValue(41)]
		public double RelativeXScale { get; set; } = 1;

		/// <summary>
		/// Oblique angle
		/// </summary>
		[DxfCodeValue(51)]
		public double ObliqueAngle { get; set; } = 0;

		/// <summary>
		/// Extrusion direction
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Extrusion { get; set; } = XYZ.AxisZ;

		public Shape() : base() { }

		public override object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
