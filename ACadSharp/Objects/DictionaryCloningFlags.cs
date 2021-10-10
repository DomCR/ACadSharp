using System;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Duplicate record cloning flag (determines how to merge duplicate entries).
	/// </summary>
	public enum DictionaryCloningFlags : short
	{
		/// <summary>
		/// Not applicable.
		/// </summary>
		NotApplicable = 0,
		/// <summary>
		/// Keep existing.
		/// </summary>
		KeepExisting = 1,
		/// <summary>
		/// Use clone.
		/// </summary>
		UseClone = 2,
		/// <summary>
		/// External reference name.
		/// </summary>
		XrefName = 3,
		/// <summary>
		/// Name.
		/// </summary>
		Name = 4,
		/// <summary>
		/// Unmangle name.
		/// </summary>
		UnmangleName = 5
	}
}
