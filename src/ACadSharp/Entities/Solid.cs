using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Solid"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntitySolid"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Solid"/>
/// </remarks>
[DxfName(DxfFileToken.EntitySolid)]
[DxfSubClass(DxfSubclassMarker.Solid)]
public class Solid : Entity
{
	/// <summary>
	/// First corner.
	/// </summary>
	[DxfCodeValue(10, 20, 30)]
	public XYZ FirstCorner { get; set; }

	/// <summary>
	/// Fourth corner.
	/// </summary>
	/// <remarks>
	/// If only three corners are entered to define the SOLID, then the fourth corner coordinate is the same as the third.
	/// </remarks>
	[DxfCodeValue(13, 23, 33)]
	public XYZ FourthCorner { get; set; }

	/// <summary>
	/// Specifies the three-dimensional normal unit vector for the object.
	/// </summary>
	[DxfCodeValue(210, 220, 230)]
	public XYZ Normal { get; set; } = XYZ.AxisZ;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntitySolid;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.SOLID;

	/// <summary>
	/// Second corner.
	/// </summary>
	[DxfCodeValue(11, 21, 31)]
	public XYZ SecondCorner { get; set; }

	//Replaces also TRACE
	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Solid;

	/// <summary>
	/// Specifies the distance a 2D object is extruded above or below its elevation.
	/// </summary>
	[DxfCodeValue(39)]
	public double Thickness { get; set; } = 0.0;

	/// <summary>
	/// Third corner.
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ ThirdCorner { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Solid"/> class.
	/// </summary>
	public Solid()
	{ }

	/// <summary>
	/// Initializes a new instance of the Solid class using the specified corner points.
	/// </summary>
	/// <remarks>The four corner points define the base of the solid in 3D space. The order of the points determines
	/// the orientation of the solid.</remarks>
	/// <param name="firstCorner">The first corner point of the solid. Defines one vertex of the quadrilateral base.</param>
	/// <param name="secondCorner">The second corner point of the solid. Must be distinct from the other corners.</param>
	/// <param name="thirdCorner">The third corner point of the solid. Must be distinct from the other corners.</param>
	/// <param name="fourthCorner">The fourth corner point of the solid. Must be distinct from the other corners.</param>
	public Solid(XYZ firstCorner, XYZ secondCorner, XYZ thirdCorner, XYZ fourthCorner)
	{
		this.FirstCorner = firstCorner;
		this.SecondCorner = secondCorner;
		this.ThirdCorner = thirdCorner;
		this.FourthCorner = fourthCorner;
	}

	/// <summary>
	/// Initializes a new instance of the Solid class using the specified three corner points.
	/// </summary>
	/// <remarks>The three corner points must be specified to define the base of the solid. The order and position
	/// of the points affect the orientation and shape of the resulting solid.</remarks>
	/// <param name="firstCorner">The first corner point of the solid. Defines one vertex of the solid's base.</param>
	/// <param name="secondCorner">The second corner point of the solid. Defines a second vertex of the solid's base.</param>
	/// <param name="thirdCorner">The third corner point of the solid. Defines a third vertex of the solid's base.</param>
	public Solid(XYZ firstCorner, XYZ secondCorner, XYZ thirdCorner)
		: this(firstCorner, secondCorner, thirdCorner, thirdCorner)
	{
	}

	/// <inheritdoc/>
	public override void ApplyTransform(Transform transform)
	{
		this.FirstCorner = transform.ApplyTransform(this.FirstCorner);
		this.SecondCorner = transform.ApplyTransform(this.SecondCorner);
		this.ThirdCorner = transform.ApplyTransform(this.ThirdCorner);
		this.FourthCorner = transform.ApplyTransform(this.FourthCorner);
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		return BoundingBox.FromPoints(new[] { this.FirstCorner, this.SecondCorner, this.ThirdCorner, this.FourthCorner });
	}
}