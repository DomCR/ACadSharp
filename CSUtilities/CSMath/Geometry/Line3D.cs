namespace CSMath.Geometry
{
	public struct Line3D : ILine<XYZ>
	{
		/// <inheritdoc/>
		public XYZ Origin { get; set; }

		/// <inheritdoc/>
		public XYZ Direction { get; set; }

		public Line3D(XYZ origin, XYZ direction)
		{
			this.Origin = origin;
			this.Direction = direction;
		}
	}
}