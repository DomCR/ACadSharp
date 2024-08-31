namespace ACadSharp.Header
{
	/// <summary>
	/// Edge shape type
	/// </summary>
	public enum ShadeEdgeType : short
	{
		/// <summary>
		/// Faces shaded, edges not highlighted
		/// </summary>
		FacesShadedEdgesNotHighlighted,
		/// <summary>
		/// Faces shaded, edges highlighted in black
		/// </summary>
		FacesShadedEdgesHighlightedInBlack,
		/// <summary>
		/// Faces not filled, edges in entity color
		/// </summary>
		FacesNotFilledEdgesInEntityColor,
		/// <summary>
		/// Faces in entity color, edges in black
		/// </summary>
		FacesInEntityColorEdgesInBlack,
	}
}
