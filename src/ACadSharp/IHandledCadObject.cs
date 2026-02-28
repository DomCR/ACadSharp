namespace ACadSharp
{
	/// <summary>
	/// Defines a CadObject with a unique handle
	/// </summary>
	public interface IHandledCadObject
	{
		/// <summary>
		/// Unique handle for this object in a <see cref="CadDocument"/>.
		/// </summary>
		ulong Handle { get; }
	}
}
