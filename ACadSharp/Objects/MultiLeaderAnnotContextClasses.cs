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
		/// 302	DXF: “LEADER“
		/// </summary>
		public class LeaderRoot : ICloneable
		{
			public LeaderRoot() { }

			/// <summary>
			/// B	290	Is content valid (ODA writes true)
			/// </summary>
			public bool ContentValid { get; set; }

			/// <summary>
			/// B	291	Unknown (ODA writes true)
			/// </summary>
			public bool Unknown { get; set; }

			/// <summary>
			/// 3BD	10	Connection point
			/// </summary>
			public XYZ ConnectionPoint { get; set; }

			/// <summary>
			/// 3BD	11	Direction
			/// </summary>
			public XYZ Direction { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public IList<StartEndPointPair> BreakStartEndPointsPairs { get; set; } = new List<StartEndPointPair>();

			/// <summary>
			/// BL	90	Leader index
			/// </summary>
			public int LeaderIndex { get; set; }

			/// <summary>
			/// BD	40	Landing distance
			/// </summary>
			public double LandingDistance { get; set; }

			/// <summary>
			/// 
			/// </summary>
			public IList<LeaderLine> Lines { get; } = new List<LeaderLine>();

			//R2010
			/// <summary>
			/// BS	271	Attachment direction
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
			/// 3BD	12	Break start point
			/// </summary>
			[DxfCodeValue(12)]
			public XYZ StartPoint { get; private set; }

			/// <summary>
			/// 3BD	13	Break end point
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
		///	304	DXF: “LEADER_LINE“
		/// </summary>
		public class LeaderLine : ICloneable
		{
			public LeaderLine() { }

			/// <summary>
			/// Points of leader line
			///	3BD	10	Point
			/// </summary>
			public IList<XYZ> Points { get; set; } = new List<XYZ>();

			/// <summary>
			/// BL	Break info count
			/// </summary>
			public int BreakInfoCount { get; set; }

			/// <summary>
			/// BL 90 Segment index
			/// </summary>
			public int SegmentIndex { get; set; }

			/// <summary>
			/// Start/end point pairs
			/// </summary>
			public IList<StartEndPointPair> StartEndPoints { get; set; }

			/// <summary>
			/// BL 91 Leader line index.
			/// </summary>
			public int Index { get; set; }

			//R2010
			/// <summary>
			/// BS 170 Leader type
			/// </summary>
			public MultiLeaderPathType PathType { get; set; }

			/// <summary>
			/// CMC 92 Line color
			/// </summary>
			public Color LineColor { get; set; }

			/// <summary>
			/// H 340 Line type handle (hard pointer)
			/// </summary>
			public LineType LineType { get; set; }

			/// <summary>
			/// BL 171 Line weight
			/// </summary>
			public LineweightType LineWeight { get; set; }

			/// <summary>
			/// BD 40 Arrow size
			/// </summary>
			public double ArrowheadSize { get; set; }

			/// <summary>
			/// H 341 Arrow symbol handle (hard pointer)
			/// </summary>
			public BlockRecord Arrowhead { get; set; }

			/// <summary>
			/// BL 93 Override flags
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