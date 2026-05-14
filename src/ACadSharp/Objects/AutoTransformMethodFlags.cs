namespace ACadSharp.Objects
{
	[System.Flags]
	public enum AutoTransformMethodFlags : byte
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0,

		/// <summary>
		/// No auto transform.
		/// </summary>
		NoAutoTransform = 1,

		/// <summary>
		/// Scale mapper to current entity extents; translate mapper to entity origin.
		/// </summary>
		ScaleMapper = 2,

		/// <summary>
		/// Include current block transform in mapper transform.
		/// </summary>
		IncludeCurrentBlock = 4
	}
}