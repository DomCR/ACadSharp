using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects;

public partial class MultiLeaderObjectContextData
{
	/// <summary>
	/// Start/end point pairs
	/// 3BD	11	Start Point
	/// 3BD	12	End point
	/// </summary>
	public struct StartEndPointPair
	{
		/// <summary>
		/// Break end point
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ EndPoint { get; private set; }

		/// <summary>
		/// Break start point
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ StartPoint { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StartEndPointPair"/> struct with the specified start and end points.
		/// </summary>
		/// <param name="startPoint">The start point of the pair.</param>
		/// <param name="endPoint">The end point of the pair.</param>
		public StartEndPointPair(XYZ startPoint, XYZ endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <summary>
		/// Creates a copy of the current <see cref="StartEndPointPair"/> instance.
		/// </summary>
		/// <returns>A new <see cref="StartEndPointPair"/> instance with the same start and end points.</returns>
		public StartEndPointPair Clone()
		{
			return new StartEndPointPair(this.StartPoint, this.EndPoint);
		}
	}
}