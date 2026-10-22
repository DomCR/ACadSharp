using System;

namespace ACadSharp.Objects.Collections;

/// <summary>
/// Represents a collection of named <see cref="Layout"/> definitions stored in a CAD dictionary.
/// </summary>
/// <remarks>
/// This collection provides access to individual <see cref="Layout"/> definitions by name and supports enumeration of
/// all layouts in the associated CAD dictionary. Modifications to the collection are reflected in the underlying
/// dictionary.
/// </remarks>
public class LayoutCollection : ObjectDictionaryCollection<Layout>
{
	/// <inheritdoc/>
	public LayoutCollection(CadDictionary dictionary) : base(dictionary)
	{
		this._dictionary = dictionary;
	}

	/// <inheritdoc/>
	public override bool Remove(string name, out Layout entry)
	{
		if (name.Equals(Layout.ModelLayoutName, StringComparison.InvariantCultureIgnoreCase)
			|| name.Equals(Layout.PaperLayoutName, StringComparison.InvariantCultureIgnoreCase))
		{
			throw new ArgumentException($"The Layout {name} cannot be removed.");
		}

		return base.Remove(name, out entry);
	}
}
