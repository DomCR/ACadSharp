using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ACadSharp;

internal class CadObjectReferenceHandler<TKey, TValue>
{
	private readonly Dictionary<TKey, HashSet<ReferenceHolder>> _references = new();

	private readonly Dictionary<CadObject, ReferenceHolder> _referencesByOwner = new();

	public CadObjectReferenceHandler()
	{
	}

	public void AddReference(TKey key, CadObject owner, Action<TValue> assignTo)
	{
		if (!this._references.TryGetValue(key, out HashSet<ReferenceHolder> holders))
		{
			holders = new HashSet<ReferenceHolder>();
			this._references[key] = holders;
		}

		var holder = new ReferenceHolder(owner, assignTo);
		holders.Add(holder);
		this._referencesByOwner[owner] = holder;
	}

	public void RemoveReference(TKey key, CadObject owner)
	{
		if (this._references.TryGetValue(key, out HashSet<ReferenceHolder> holders)
			&& this._referencesByOwner.TryGetValue(owner, out ReferenceHolder holder))
		{
			holders.Remove(holder);
			if (holders.Count == 0)
			{
				this._references.Remove(key);
			}
		}
	}

	public void ChangeKey(TKey current, TKey newKey)
	{
		if (this._references.Remove(current, out HashSet<ReferenceHolder> holders))
		{
			this._references[newKey] = holders;
		}
	}

	public IEnumerable<CadObject> GetReferences(TKey key)
	{
		if (this._references.TryGetValue(key, out HashSet<ReferenceHolder> holders))
		{
			return holders.Select(h => h.Owner);
		}

		return Enumerable.Empty<CadObject>();
	}

	public void RemoveReference(TKey key, TValue value)
	{
		if (this._references.Remove(key, out HashSet<ReferenceHolder> holders))
		{
			foreach (var holder in holders)
			{
				holder.AssignTo(value);
			}
		}
	}

	private sealed class ReferenceHolder
	{
		public Action<TValue> AssignTo { get; }

		public CadObject Owner { get; }

		public ReferenceHolder(CadObject owner, Action<TValue> assignTo)
		{
			this.Owner = owner;
			this.AssignTo = assignTo;
		}
	}
}