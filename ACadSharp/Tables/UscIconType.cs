namespace ACadSharp.Tables
{
	/// <summary>
	/// Display Ucs icon
	/// </summary>
	public enum UscIconType : short
	{
		/// <summary>
		/// Off; No icon is displayed
		/// </summary>
		Off,

		/// <summary>
		/// On; the icon is displayed only in the lower-left corner of the current viewport or layout
		/// </summary>
		OnLower,

		/// <summary>
		/// Off; if the icon is turned on, it is displayed at the UCS origin, if possible
		/// </summary>
		OffOrigin,

		/// <summary>
		/// On; Displays the UCS icon at the origin, if possible.
		/// </summary>
		OnOrigin
	}
}
