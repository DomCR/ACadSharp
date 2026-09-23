using System.Collections.Generic;
using ACadSharp.Attributes;

namespace ACadSharp.Objects;

public partial class MultiLeaderObjectContextData
{
	/// <summary>
	/// Information about a break in a <see cref="MultiLeaderObjectContextData"/> object.
	/// </summary>
	public class BreakInfo
	{
		/// <summary>
		/// The index of the segment that is broken.
		/// </summary>
		[DxfCodeValue(90)]
		public int SegmentIndex { get; set; }

		/// <summary>
		/// The start and end points of the broken segment.
		/// </summary>
		public IList<StartEndPointPair> StartEndPoints { get; private set; } = new List<StartEndPointPair>();

		/// <summary>
		/// Creates a new instance of the <see cref="BreakInfo"/> class.
		/// </summary>
		/// <returns>A new <see cref="BreakInfo"/> instance with the same segment index and start/end points.</returns>
		public BreakInfo Clone()
		{
			BreakInfo clone = new BreakInfo
			{
				SegmentIndex = this.SegmentIndex
			};

			foreach (var startEndPoint in this.StartEndPoints)
			{
				clone.StartEndPoints.Add(startEndPoint.Clone());
			}

			return clone;
		}
	}
}