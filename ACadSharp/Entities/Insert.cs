using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.IO.Templates;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Insert"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityInsert"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Insert"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityInsert)]
	[DxfSubClass(DxfSubclassMarker.Insert)]
	public class Insert : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.INSERT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityInsert;

		//66	Variable attributes-follow flag(optional; default = 0); 
		//		if the value of attributes-follow flag is 1, a series of 
		//		attribute entities is expected to follow the insert, terminated by a seqend entity
		[DxfCodeValue(DxfReferenceType.Count, 66)]
		public SeqendCollection<AttributeEntity> Attributes { get; set; }

		/// <summary>
		///  Gets the insert block definition
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public BlockRecord Block { get; set; }

		/// <summary>
		/// A 3D WCS coordinate representing the insertion or origin point.
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ InsertPoint { get; set; } = XYZ.Zero;

		/// <summary>
		/// X scale factor 
		/// </summary>
		[DxfCodeValue(41)]
		public double XScale { get; set; } = 1;

		/// <summary>
		/// Y scale factor 
		/// </summary>
		[DxfCodeValue(42)]
		public double YScale { get; set; } = 1;

		/// <summary>
		/// Z scale factor 
		/// </summary>
		[DxfCodeValue(43)]
		public double ZScale { get; set; } = 1;

		/// <summary>
		/// Specifies the rotation angle for the object.
		/// </summary>
		/// <value>
		/// The rotation angle in radians.
		/// </value>
		[DxfCodeValue(50)]
		public double Rotation { get; set; } = 0.0;

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		[DxfCodeValue(70)]
		public ushort ColumnCount { get; set; } = 1;

		[DxfCodeValue(71)]
		public ushort RowCount { get; set; } = 1;

		[DxfCodeValue(44)]
		public double ColumnSpacing { get; set; } = 0;

		[DxfCodeValue(45)]
		public double RowSpacing { get; set; } = 0;

		public Insert() : base()
		{
			this.Attributes = new SeqendCollection<AttributeEntity>(this);
		}
	}
}
