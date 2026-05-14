using System;
using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects
{
	public partial class MultiLeaderObjectContextData
	{
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
			[DxfCodeValue(12, 22, 32)]
			public XYZ StartPoint { get; private set; }

			/// <summary>
			/// Break end point
			/// </summary>
			[DxfCodeValue(13, 23, 33)]
			public XYZ EndPoint { get; private set; }

			public object Clone()
			{
				return this.MemberwiseClone();
			}
		}
	}
}