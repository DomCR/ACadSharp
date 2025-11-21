using ACadSharp.Attributes;
using CSMath;
using System.IO;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Ole2Frame"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityOle2Frame"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ole2Frame"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityOle2Frame)]
	[DxfSubClass(DxfSubclassMarker.Ole2Frame)]
	public class Ole2Frame : Entity
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityOle2Frame;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.OLE2FRAME;

		/// <summary>
		/// OLE version number.
		/// </summary>
		[DxfCodeValue(70)]
		public short Version { get; internal set; } = 2;

		//3

		//Length of binary data

		/// <summary>
		/// Gets or sets the upper-left corner of the object in world coordinate system (WCS).
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ UpperLeftCorner { get; set; } = XYZ.Zero;

		/// <summary>
		/// Gets or sets the lower-right corner of the object in world coordinate system (WCS).
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ LowerRightCorner { get; set; } = XYZ.Zero;

		/// <summary>
		/// Gets or sets the type of the OLE (Object Linking and Embedding) object.
		/// </summary>
		/// <remarks>The <see cref="OleObjectType"/> property specifies the behavior and storage type of the OLE object: 
		/// <list type="bullet"> 
		/// <item> <description><see cref="OleObjectType.Link"/>: The object is linked to an external source.</description>
		/// </item> <item> <description><see cref="OleObjectType.Embedded"/>: The object is embedded within the current document.</description> </item> 
		/// <item> <description><see cref="OleObjectType.Static"/>: The object is a static representation.</description>
		/// </item>
		/// </list>
		/// </remarks>
		[DxfCodeValue(71)]
		public OleObjectType OleObjectType { get; set; } = OleObjectType.Embedded;

		/// <summary>
		/// Gets or sets a value indicating whether the object resides in paper space.
		/// </summary>
		[DxfCodeValue(72)]
		public bool IsPaperSpace { get; set; } = false;

		//90
		//Length of binary data
		//310
		//Binary data(multiple lines)
		[DxfCodeValue(310)]
		public Stream BinaryData { get; set; }

		//1
		//End of OLE data(the string “OLE”)

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			throw new System.NotImplementedException();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			throw new System.NotImplementedException();
		}
	}
}