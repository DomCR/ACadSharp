namespace ACadSharp.Entities
{
	public enum OleObjectType
	{
		/// <summary>
		/// The object is linked to an external source.
		/// </summary>
		Link = 1,
		/// <summary>
		/// The object is embedded within the current document.
		/// </summary>
		Embedded = 2,
		/// <summary>
		/// The object is a static representation.
		/// </summary>
		Static = 3
	}
}