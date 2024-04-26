using System;

namespace ACadSharp
{
	/// <summary>
	/// Defines a CadObject with a unique name
	/// </summary>
	public interface INamedCadObject
	{
		event EventHandler<OnNameChangedArgs> OnNameChanged;

		/// <summary>
		/// Name identifier for this object
		/// </summary>
		string Name { get; }
	}
}
