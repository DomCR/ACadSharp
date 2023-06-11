namespace ACadSharp.Entities
{
	/// <summary>
	/// Leader creation type
	/// </summary>
	public enum LeaderCreationType : short
	{
		/// <summary>
		/// Created with text annotation
		/// </summary>
		CreatedWithTextAnnotation = 0,

		/// <summary>
		/// Created with tolerance annotation
		/// </summary>
		CreatedWithToleranceAnnotation = 1,

		/// <summary>
		/// Created with block reference annotation
		/// </summary>
		CreatedWithBlockReferenceAnnotation = 2,

		/// <summary>
		/// Created without any annotation
		/// </summary>
		CreatedWithoutAnnotation = 3
	}
}
