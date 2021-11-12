using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.Geometry;
using ACadSharp.IO.Templates;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class Insert : Entity
	{
		public override ObjectType ObjectType => ObjectType.INSERT;
		public override string ObjectName => DxfFileToken.EntityInsert;

		//66	Variable attributes-follow flag(optional; default = 0); 
		//		if the value of attributes-follow flag is 1, a series of 
		//		attribute entities is expected to follow the insert, terminated by a seqend entity
		[DxfCodeValue(66)]
		public List<Attribute> Attributes { get; set; } = new List<Attribute>();

		/// <summary>
		/// Specifies the name of the object.
		/// </summary>
		[DxfCodeValue(2)]
		public string BlockName { get { return this.Block.Name; } }

		/// <summary>
		///  Gets the insert block definition
		/// </summary>
		public Block Block { get; set; }

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

		[DxfCodeValue(DxfCode.Int16)]
		public ushort ColumnCount { get; set; } = 1;
		[DxfCodeValue(DxfCode.RowCount)]
		public ushort RowCount { get; set; } = 1;
		[DxfCodeValue(DxfCode.ShapeXOffset)]
		public double ColumnSpacing { get; set; } = 0;
		[DxfCodeValue(DxfCode.ShapeYOffset)]
		public double RowSpacing { get; set; } = 0;

		public Insert() : base() { }

		internal Insert(DxfEntityTemplate template) : base(template) { }
	}
}
