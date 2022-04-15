namespace ACadSharp
{
	/// <summary>
	/// Defines a CadObject with a unique name
	/// </summary>
	public interface INamedCadObject
	{
		/// <summary>
		/// Name identifier for this object
		/// </summary>
		string Name { get; }
	}
}
