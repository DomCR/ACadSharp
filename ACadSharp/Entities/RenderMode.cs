namespace ACadSharp.Entities
{
	/// <summary>
	/// Viewport render mode
	/// </summary>
	public enum RenderMode : byte
	{
		/// <summary>
		/// Classic 2D
		/// </summary>
		Optimized2D,
		/// <summary>
		/// Wire frame
		/// </summary>
		Wireframe,
		/// <summary>
		/// Hidden line
		/// </summary>
		HiddenLine,
		/// <summary>
		/// Flat shaded
		/// </summary>
		FlatShaded,
		/// <summary>
		/// Gouraud shaded
		/// </summary>
		GouraudShaded,
		/// <summary>
		/// Flat shaded with wire frame
		/// </summary>
		FlatShadedWithWireframe,
		/// <summary>
		/// Gouraud shaded with wireframe
		/// </summary>
		GouraudShadedWithWireframe,
	}
}
