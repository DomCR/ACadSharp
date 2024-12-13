using System;

namespace CSMath.Geometry
{
	public struct Line2D : ILine<XY>, IEquatable<Line2D>
	{
		/// <inheritdoc/>
		public XY Origin { get; set; }

		/// <inheritdoc/>
		public XY Direction { get; set; }

		public Line2D(XY origin, XY direction)
		{
			this.Origin = origin; 
			this.Direction = direction;
		}

		public bool Equals(Line2D other)
		{
			throw new NotImplementedException();
		}
	}
}