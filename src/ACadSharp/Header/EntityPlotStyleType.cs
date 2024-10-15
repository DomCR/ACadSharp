namespace ACadSharp.Header
{
	/// <summary>
	/// Plot style type of new objects
	/// </summary>
	public enum EntityPlotStyleType : short
	{
		/// <summary>
		/// Plot style by layer
		/// </summary>
		ByLayer = 0,
		/// <summary>
		/// Plot style by block
		/// </summary>
		ByBlock = 1,
		/// <summary>
		/// Plot style by dictionary default
		/// </summary>
		ByDictionaryDefault = 2,
		/// <summary>
		/// Plot style by object ID/handle
		/// </summary>
		ByObjectId = 3
	}
}