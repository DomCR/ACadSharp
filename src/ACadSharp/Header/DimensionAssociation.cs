namespace ACadSharp.Header
{
	/// <summary>
	/// Controls the associativity of dimension objects.
	/// </summary>
	public enum DimensionAssociation : short
	{
		/// <summary>
		/// Creates exploded dimensions; there is no association between elements of the dimension, and the lines, arcs, arrowheads, and text of a dimension are drawn as separate objects
		/// </summary>
		CreateExplodedDimensions = 0,
		/// <summary>
		/// Creates non-associative dimension objects; the elements of the dimension are formed into a single object, and if the definition point on the object moves, then the dimension value is updated
		/// </summary>
		CreateNonAssociativeDimensions = 1,
		/// <summary>
		/// Creates associative dimension objects; the elements of the dimension are formed into a single object and one or more definition points of the dimension are coupled with association points on geometric objects
		/// </summary>
		CreateAssociativeDimensions = 2
	}
}
