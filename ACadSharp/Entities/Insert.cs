using ACadSharp.Attributes;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;

namespace ACadSharp.Entities
{
	public class Insert : Entity
	{
		public override ObjectType ObjectType => ObjectType.INSERT;
		public override string ObjectName => DxfFileToken.EntityInsert;

		//100	Subclass marker(AcDbBlockReference)

		//66	Variable attributes-follow flag(optional; default = 0); 
		//		if the value of attributes-follow flag is 1, a series of 
		//		attribute entities is expected to follow the insert, terminated by a seqend entity

		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(DxfCode.BlockName)]
		public string BlockName { get; set; } 
		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;
		/// <summary>
		/// Scale factor of this block.
		/// </summary>
		[DxfCodeValue(DxfCode.XScaleFactor, DxfCode.YScaleFactor, DxfCode.ZScaleFactor)]
		public XYZ Scale { get; set; } = new XYZ(1, 1, 1);
		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(DxfCode.Angle)]
		public double Rotation { get; set; } = 0.0;

		//70	Column count(optional; default = 1)
		//71	Row count(optional; default = 1)
		//44	Column spacing(optional; default = 0)
		//45	Row spacing(optional; default = 0)

		public Insert() : base() { }

		internal Insert(DxfEntityTemplate template) : base(template) { }
	}
}
