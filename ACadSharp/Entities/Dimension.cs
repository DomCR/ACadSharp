using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Dimension"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityDimension"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dimension"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityDimension)]
	[DxfSubClass(DxfSubclassMarker.Dimension)]
	public abstract class Dimension : Entity
	{
		/// <summary>
		/// Version number
		/// </summary>
		[DxfCodeValue(280)]
		public byte Version { get; set; }

		/// <summary>
		/// Block that contains the entities that make up the dimension picture
		/// </summary>
		[DxfCodeValue(2)]
		public Block Block { get; set; }

		/// <summary>
		/// Definition point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ DefinitionPoint { get; set; }

		/// <summary>
		/// Middle point of dimension text(in OCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ TextMiddlePoint { get; set; }

		/// <summary>
		/// Insertion point for clones of a dimension-Baseline and Continue(in OCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ InsertionPoint { get; set; }

		/// <summary>
		/// Specifies the three-dimensional normal unit vector for the object.
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Dimension type
		/// </summary>
		[DxfCodeValue(70)]
		public DimensionType DimensionType { get; set; }

		/// <summary>
		/// Attachment point
		/// </summary>
		[DxfCodeValue(71)]
		public AttachmentPointType AttachmentPoint { get; set; }

		/// <summary>
		/// Dimension text line-spacing style
		/// </summary>
		/// <remarks>
		/// optional
		/// </remarks>
		[DxfCodeValue(72)]
		public LineSpacingStyleType LineSpacingStyle { get; set; }

		/// <summary>
		/// Dimension text-line spacing factor
		/// </summary>
		/// <remarks>
		/// (optional) Percentage of default (3-on-5) line spacing to be applied.
		/// </remarks>
		/// <value>
		/// Valid values range from 0.25 to 4.00
		/// </value>
		[DxfCodeValue(41)]
		public double LineSpacingFactor { get; set; }

		/// <summary>
		/// Actual measurement
		/// </summary>
		/// <remarks>
		/// optional; read-only value
		/// </remarks>
		[DxfCodeValue(42)]
		public double Measurement { get; set; }

		/// <summary>
		/// Dimension text explicitly entered by the user
		/// </summary>
		/// <remarks>
		/// Optional; default is the measurement.
		/// If null, the dimension measurement is drawn as the text, 
		/// if ““ (one blank space), the text is suppressed.Anything else is drawn as the text
		/// </remarks>
		[DxfCodeValue(1)]
		public string Text { get; set; }

		/// <summary>
		/// rotation angle of the dimension text away from its default orientation (the direction of the dimension line)
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(53)]
		public double TextRotation { get; set; }

		/// <summary>
		/// All dimension types have an optional 51 group code, which indicates the horizontal direction for the dimension entity.The dimension entity determines the orientation of dimension text and lines for horizontal, vertical, and rotated linear dimensions
		/// This group value is the negative of the angle between the OCS X axis and the UCS X axis. It is always in the XY plane of the OCS
		/// </summary>
		/// <remarks>
		/// Optional
		/// </remarks>
		[DxfCodeValue(51)]
		public double HorizontalDirection { get; set; }

		//This group value is the negative of the angle between the OCS X axis and the UCS X axis.It is always in the XY plane of the OCS

		/// <summary>
		/// Dimension style
		/// </summary>
		[DxfCodeValue(3)]
		public DimensionStyle Style { get; set; }
	}
}
