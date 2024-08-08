namespace CSMath.Geometry
{
	public interface ILine<T>
		where T : IVector
	{
		/// <summary>
		/// Origin point that the line intersects with
		/// </summary>
		public T Origin { get; set; }

		/// <summary>
		/// Direction fo the line
		/// </summary>
		public T Direction { get; set; }
	}
}