namespace ACadSharp.Entities
{
	/// <remarks>
	/// Starting with AutoCAD 2016-based products, this property is obsolete but still supported for backwards compatibility.
	/// </remarks>
	public enum ShadowMode
	{
		CastsAndReceivesShadows = 0,
		CastsShadows = 1,
		ReceivesShadows = 2,
		IgnoresShadows = 3,
	}

}
