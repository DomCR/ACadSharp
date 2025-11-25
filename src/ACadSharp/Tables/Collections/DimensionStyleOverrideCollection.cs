using System.Collections;
using System.Collections.Generic;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStyleOverrideCollection : IDictionary<DimensionStyleOverrideType, DimensionStyleOverride>
	{
		private readonly Dictionary<DimensionStyleOverrideType, DimensionStyleOverride> _dimensionStyleOverrides = new();

		public IEnumerator<KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride>> GetEnumerator()
		{
			return this._dimensionStyleOverrides.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(DimensionStyleOverride item)
		{
			this._dimensionStyleOverrides.Add(item.Type, item);
		}

		public void Add(KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride> item)
		{
			this._dimensionStyleOverrides.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			this._dimensionStyleOverrides.Clear();
		}

		public bool Contains(KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride> item)
		{
			return this._dimensionStyleOverrides.TryGetValue(item.Key, out var existing) &&
			       EqualityComparer<DimensionStyleOverride>.Default.Equals(existing, item.Value);
		}

		public void CopyTo(KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride>>)_dimensionStyleOverrides)
				.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride> item)
		{
			if (!this._dimensionStyleOverrides.TryGetValue(item.Key, out var existing) ||
			    !EqualityComparer<DimensionStyleOverride>.Default.Equals(existing, item.Value))
			{
				return false;
			}

			return this._dimensionStyleOverrides.Remove(item.Key);
		}

		public int Count => this._dimensionStyleOverrides.Count;
		public bool IsReadOnly => false;

		public void Add(DimensionStyleOverrideType key, DimensionStyleOverride value)
		{
			this._dimensionStyleOverrides.Add(key, value);
		}

		public bool ContainsKey(DimensionStyleOverrideType key)
		{
			return this._dimensionStyleOverrides.ContainsKey(key);
		}

		public bool Remove(DimensionStyleOverrideType key)
		{
			return this._dimensionStyleOverrides.Remove(key);
		}

		public bool TryGetValue(DimensionStyleOverrideType key, out DimensionStyleOverride value)
		{
			return this._dimensionStyleOverrides.TryGetValue(key, out value);
		}

		public DimensionStyleOverride this[DimensionStyleOverrideType key]
		{
			get => this._dimensionStyleOverrides[key];
			set => this._dimensionStyleOverrides[key] = value;
		}

		public ICollection<DimensionStyleOverrideType> Keys => this._dimensionStyleOverrides.Keys;
		public ICollection<DimensionStyleOverride> Values => this._dimensionStyleOverrides.Values;
	}
}