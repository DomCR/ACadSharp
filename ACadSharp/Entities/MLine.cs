using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="MLine"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityMLine"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.MLine"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityMLine)]
	[DxfSubClass(DxfSubclassMarker.MLine)]
	public partial class MLine : Entity
	{
		public override ObjectType ObjectType => ObjectType.MLINE;

		public override string ObjectName => DxfFileToken.EntityMLine;

		/// <summary>
		/// String of up to 32 characters.The name of the style used for this mline.An entry for this style must exist in the MLINESTYLE dictionary.
		/// </summary>
		/// <remarks>
		/// Do not modify this field without also updating the associated entry in the MLINESTYLE dictionary
		/// </remarks>
		[DxfCodeValue(DxfReferenceType.Name, 2)]
		public MLStyle MlStyleName { get { return this.MLStyle; } set{ this.MLStyle = value; } }    //TODO: Fix duplicated MLStyle

		/// <summary>
		/// MLINESTYLE object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public MLStyle MLStyle { get; set; }

		/// <summary>
		/// Scale factor
		/// </summary>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Justification
		/// </summary>
		[DxfCodeValue(70)]
		public MLineJustification Justification { get; set; }

		/// <summary>
		/// Flags
		/// </summary>
		[DxfCodeValue(71)]
		public MLineFlags Flags { get; set; }

		/// <summary>
		/// Start point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; }

		/// <summary>
		/// Extrusion direction
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Extrusion { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Vertices in the MLine
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 72)]
		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		public MLine() : base() { }
	}
}
