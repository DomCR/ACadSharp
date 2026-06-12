using System;
using System.Linq;
using ACadSharp.Tables;

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

	public override void Add(Layout entry)
	{
		if (!isDefaultLayout(entry))
			entry.AssociatedBlock.Name = findSmallestFreePaperSpaceName(this._dictionary.Document);

		base.Add(entry);
	}

	public override bool Remove(string name, out Layout entry)
	{
		if (!this.TryGet(name, out entry))
			return false;

		if (isDefaultLayout(entry))
		{
			throw new ArgumentException($"The Layout {name} cannot be removed.");
		}

		return base.Remove(name, out entry);
	}

	private static bool isDefaultLayout(Layout layout)
	{
		return layout.AssociatedBlock.Name.Equals(BlockRecord.PaperSpaceName, StringComparison.InvariantCultureIgnoreCase) ||
		       layout.AssociatedBlock.Name.Equals(BlockRecord.ModelSpaceName, StringComparison.InvariantCultureIgnoreCase);
	}

	private static string findSmallestFreePaperSpaceName(CadDocument doc)
	{
		int n = 0;
		while (true)
		{
			string candidate = Tables.BlockRecord.PaperSpaceName + n.ToString(System.Globalization.CultureInfo.InvariantCulture);
			if (!doc.BlockRecords.Contains(candidate))
			{
				return candidate;
			}
			n++;
		}
	}
}
