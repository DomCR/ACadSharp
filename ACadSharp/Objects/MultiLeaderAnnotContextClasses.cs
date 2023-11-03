using System;
using System.Collections.Generic;

using ACadSharp.Attributes;
using ACadSharp.Tables;

using CSMath;


namespace ACadSharp.Objects
{

	/// <summary>
	/// Nested classes in MultiLeaderAnnotContext
	/// </summary>
	public partial class MultiLeaderAnnotContext : CadObject
	{
		/// <summary>
		/// Represents a leader root
		/// </summary>
		/// <remarks>
		/// Appears in DXF as 302 DXF: “LEADER“
		/// </remarks>
		public class LeaderRoot : ICloneable
		{
			public LeaderRoot() { }

			/// <summary>
			/// Is content valid (ODA writes true)
			/// </summary>
			public bool ContentValid { get; set; }

			/// <summary>
			/// Unknown (ODA writes true)
			/// </summary>
			public bool Unknown { get; set; }

			/// <summary>
			/// Connection point
			/// </summary>
			public XYZ ConnectionPoint { get; set; }

			/// <summary>
			/// Direction
			/// </summary>
			public XYZ Direction { get; set; }

			/// <summary>
			/// Gets a list of <see cref="StartEndPointPair" />.
			/// </summary>
			public IList<StartEndPointPair> BreakStartEndPointsPairs { get; } = new List<StartEndPointPair>();

			/// <summary>
			/// Leader index
			/// </summary>
			public int LeaderIndex { get; set; }

			/// <summary>
			/// Landing distance
			/// </summary>
			public double LandingDistance { get; set; }

			/// <summary>
			/// Gets a list of <see cref="LeaderLine"/> objects representing
			/// representing leader lines starting from the landing point
			/// of the multi leader.
			/// </summary>
			public IList<LeaderLine> Lines { get; } = new List<LeaderLine>();

			//R2010
			/// <summary>
			/// Attachment direction
			/// </summary>
			public TextAttachmentDirectionType AttachmentDirection { get; internal set; }

			public object Clone()
			{
				LeaderRoot clone = (LeaderRoot)this.MemberwiseClone();

				foreach (var breakStartEndPoint in BreakStartEndPointsPairs)
				{
					clone.BreakStartEndPointsPairs.Add((StartEndPointPair)breakStartEndPoint.Clone());
				}

				foreach (var line in Lines)
				{
					clone.Lines.Add((LeaderLine)line.Clone());
				}

				return clone;
			}
		}

		/// <summary>
		/// Start/end point pairs
		/// 3BD	11	Start Point
		/// 3BD	12	End point
		/// </summary>
		public struct StartEndPointPair : ICloneable
		{
			public StartEndPointPair(XYZ startPoint, XYZ endPoint) {
				StartPoint = startPoint;
				EndPoint = endPoint;
			}

			/// <summary>
			/// Break start point
			/// </summary>
			[DxfCodeValue(12)]
			public XYZ StartPoint { get; private set; }

			/// <summary>
			/// Break end point
			/// </summary>
			[DxfCodeValue(13)]
			public XYZ EndPoint { get; private set; }

			public object Clone()
			{
				return this.MemberwiseClone();
			}
		}


		/// <summary>
		///	Represents a leader line
		/// </summary>
		/// <remarks>
		/// Appears as 304	DXF: “LEADER_LINE“
		/// </remarks>
		public class LeaderLine : ICloneable
		{
			public LeaderLine() { }

			/// <summary>
			/// Points of leader line
			/// </summary>
			public IList<XYZ> Points { get; set; } = new List<XYZ>();

			/// <summary>
			/// Break info count
			/// </summary>
			public int BreakInfoCount { get; set; }

			/// <summary>
			/// Segment index
			/// </summary>
			public int SegmentIndex { get; set; }

			/// <summary>
			/// Start/end point pairs
			/// </summary>
			public IList<StartEndPointPair> StartEndPoints { get; set; }

			/// <summary>
			/// Leader line index.
			/// </summary>
			public int Index { get; set; }

			//R2010
			/// <summary>
			/// Leader type
			/// </summary>
			public MultiLeaderPathType PathType { get; set; }

			/// <summary>
			/// Line color
			/// </summary>
			public Color LineColor { get; set; }

			/// <summary>
			/// Line type
			/// </summary>
			public LineType LineType { get; set; }

			/// <summary>
			/// Line weight
			/// </summary>
			public LineweightType LineWeight { get; set; }

			/// <summary>
			/// Arrowhead size
			/// </summary>
			public double ArrowheadSize { get; set; }

			/// <summary>
			/// Gets or sets a <see cref="BlockRecord"/> containig elements
			/// to be dawn as arrow symbol.
			/// </summary>
			public BlockRecord Arrowhead { get; set; }

			/// <summary>
			/// Override flags
			/// </summary>
			public LeaderLinePropertOverrideFlags OverrideFlags { get; set; }

			public object Clone()
			{
				LeaderLine clone = (LeaderLine)this.MemberwiseClone();

				foreach (var point in Points)
				{
					clone.Points.Add(point);
				}

				foreach (var startEndPoint in StartEndPoints)
				{
					clone.StartEndPoints.Add((StartEndPointPair)startEndPoint.Clone());
				}

				return clone;
			}
		}
	}
}