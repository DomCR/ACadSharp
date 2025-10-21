using System;
using System.Collections.Generic;
using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects
{
	public partial class MultiLeaderObjectContextData
	{
		/// <summary>
		/// Represents a leader root
		/// </summary>
		/// <remarks>
		/// Appears in DXF as 302 DXF: “LEADER“
		/// </remarks>
		public class LeaderRoot : ICloneable
		{
			/// <summary>
			/// Gets a list of <see cref="StartEndPointPair" />.
			/// </summary>
			public IList<StartEndPointPair> BreakStartEndPointsPairs { get; private set; } = new List<StartEndPointPair>();

			/// <summary>
			/// Connection point
			/// </summary>
			[DxfCodeValue(10, 20, 30)]
			public XYZ ConnectionPoint { get; set; }

			/// <summary>
			/// Is content valid (ODA writes true)
			/// </summary>
			[DxfCodeValue(290)]
			public bool ContentValid { get; set; }

			/// <summary>
			/// Direction
			/// </summary>
			[DxfCodeValue(11, 21, 31)]
			public XYZ Direction { get; set; }

			/// <summary>
			/// Landing distance
			/// </summary>
			[DxfCodeValue(40)]
			public double LandingDistance { get; set; }

			/// <summary>
			/// Leader index
			/// </summary>
			[DxfCodeValue(90)]
			public int LeaderIndex { get; set; }

			/// <summary>
			/// Gets a list of <see cref="LeaderLine"/> objects representing
			/// leader lines starting from the landing point
			/// of the multi leader.
			/// </summary>
			public IList<LeaderLine> Lines { get; private set; } = new List<LeaderLine>();

			//R2010
			/// <summary>
			/// Attachment direction
			/// </summary>
			[DxfCodeValue(271)]
			public TextAttachmentDirectionType TextAttachmentDirection { get; set; }

			/// <summary>
			/// Unknown (ODA writes true)
			/// </summary>
			[DxfCodeValue(291)]
			public bool Unknown { get; set; }

			public LeaderRoot() { }

			public object Clone()
			{
				LeaderRoot clone = (LeaderRoot)base.MemberwiseClone();

				clone.BreakStartEndPointsPairs = new List<StartEndPointPair>();
				foreach (var breakStartEndPoint in this.BreakStartEndPointsPairs)
				{
					clone.BreakStartEndPointsPairs.Add((StartEndPointPair)breakStartEndPoint.Clone());
				}

				clone.Lines = new List<LeaderLine>();
				foreach (var line in this.Lines)
				{
					clone.Lines.Add((LeaderLine)line.Clone());
				}

				return clone;
			}
		}
	}
}