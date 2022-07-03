using ACadSharp.Attributes;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Leader creation type
	/// </summary>
	public enum LeaderCreationType : short
	{
		/// <summary>
		/// Created with text annotation
		/// </summary>
		CreatedWithTextAnnotation = 0,

		/// <summary>
		/// Created with tolerance annotation
		/// </summary>
		CreatedWithToleranceAnnotation = 1,

		/// <summary>
		/// Created with block reference annotation
		/// </summary>
		CreatedWithBlockReferenceAnnotation = 2,

		/// <summary>
		/// Created without any annotation
		/// </summary>
		CreatedWithoutAnnotation = 3
	}

	/// <summary>
	/// Controls the way the leader is drawn.
	/// </summary>
	public enum LeaderPathType
	{
		/// <summary>
		/// Draws the leader line as a set of straight line segments
		/// </summary>
		StraightLineSegments = 0,

		/// <summary>
		/// Draws the leader line as a spline
		/// </summary>
		Spline = 1
	}

	/// <summary>
	/// Represents a <see cref="Leader"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityLeader"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Leader"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityLeader)]
	[DxfSubClass(DxfSubclassMarker.Leader)]
	public class Leader : Entity
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LEADER;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityLeader;

		//3	Dimension style name
		[DxfCodeValue(3)]
		public DimensionStyle Style { get; set; } = new DimensionStyle();

		/// <summary>
		/// Arrowhead flag
		/// </summary>
		[DxfCodeValue(71)]
		public bool ArrowHeadEnabled { get; set; }

		/// <summary>
		/// Leader creation flag
		/// </summary>
		[DxfCodeValue(72)]
		public LeaderPathType PathType { get; set; }

		/// <summary>
		/// Leader creation flag
		/// </summary>
		[DxfCodeValue(73)]
		public LeaderCreationType CreationType { get; set; } = LeaderCreationType.CreatedWithoutAnnotation;

		/// <summary>
		/// Hookline direction flag
		/// </summary>
		/// <value>
		/// 0 = Hookline(or end of tangent for a splined leader) is the opposite direction from the horizontal vector <br/>
		/// 1 = Hookline(or end of tangent for a splined leader) is the same direction as horizontal vector(see code 75)
		/// </value>
		[DxfCodeValue(74)]
		public bool HookLineDirection { get; set; }

		/// <summary>
		/// Hookline flag
		/// </summary>
		[DxfCodeValue(75)]
		public bool HasHookline { get; set; }

		/// <summary>
		/// Text annotation height
		/// </summary>
		[DxfCodeValue(40)]
		public double TextHeight { get; set; }

		/// <summary>
		/// Text annotation width
		/// </summary>
		[DxfCodeValue(41)]
		public double TextWidth { get; set; }

		/// <summary>
		/// Vertices in leader
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 76)]
		[DxfCollectionCodeValue(10, 20, 30)]
		public List<XYZ> Vertices { get; set; } = new List<XYZ>();

		//77	Color to use if leader's DIMCLRD = BYBLOCK

		//340	Hard reference to associated annotation(mtext, tolerance, or insert entity)

		/// <summary>
		/// Normal vector
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Normal { get; set; } = XYZ.AxisZ;

		/// <summary>
		/// Horizontal direction for leader
		/// </summary>
		[DxfCodeValue(211, 221, 231)]
		public XYZ HorizontalDirection { get; set; } = XYZ.AxisX;

		/// <summary>
		/// Offset of last leader vertex from block reference insertion point
		/// </summary>
		[DxfCodeValue(212, 222, 232)]
		public XYZ BlockOffset { get; set; } = XYZ.Zero;

		/// <summary>
		/// Offset of last leader vertex from annotation placement point
		/// </summary>
		[DxfCodeValue(213, 223, 233)]
		public XYZ AnnotationOffset { get; set; } = XYZ.Zero;
	}
}
