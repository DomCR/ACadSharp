using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Face3D"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.Entity3DFace"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Face3d"/>
	/// </remarks>
	[DxfName(DxfFileToken.Entity3DFace)]
	[DxfSubClass(DxfSubclassMarker.Face3d)]
	public class Face3D : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.FACE3D;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.Entity3DFace;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Face3d;

		/// <summary>
		/// First corner(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ FirstCorner { get; set; }

		/// <summary>
		/// Second corner(in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ SecondCorner { get; set; }

		/// <summary>
		/// Third corner(in WCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ ThirdCorner { get; set; }

		/// <summary>
		/// Fourth corner(in WCS).
		/// </summary>
		/// <remarks>
		/// If only three corners are entered, this is the same as the third corner
		/// </remarks>
		[DxfCodeValue(13, 23, 33)]
		public XYZ FourthCorner { get; set; }

		/// <summary>
		/// Invisible edge flags
		/// </summary>
		[DxfCodeValue(70)]
		public InvisibleEdgeFlags Flags { get; set; }

		public Face3D() : base() { }
	}
}
