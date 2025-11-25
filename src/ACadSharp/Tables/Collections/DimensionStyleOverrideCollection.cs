using System;
using System.Collections;
using System.Collections.Generic;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStyleOverrideCollection : IDictionary<DimensionStyleOverrideType, DimensionStyleOverride>
	{
		private readonly Dictionary<DimensionStyleOverrideType, DimensionStyleOverride> _dimensionStyleOverrides = new();
        
        public event EventHandler<DimensionStyleOverrideChangedEventArgs> BeforeAdd;
        public event EventHandler<DimensionStyleOverrideChangedEventArgs> BeforeRemove;
        public event EventHandler<DimensionStyleOverrideChangedEventArgs> OnAdd;
        public event EventHandler<DimensionStyleOverrideChangedEventArgs> OnRemove;

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
            BeforeAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Type, item));
			this._dimensionStyleOverrides.Add(item.Type, item);
            OnAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Type, item));
		}

		public void Add(KeyValuePair<DimensionStyleOverrideType, DimensionStyleOverride> item)
		{
            BeforeAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Key, item.Value));
			this._dimensionStyleOverrides.Add(item.Key, item.Value);
            OnAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Key, item.Value));
		}
        
        public void Add(DimensionStyleOverrideType key, DimensionStyleOverride value)
        {
            BeforeAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, value));
            this._dimensionStyleOverrides.Add(key, value);
            OnAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, value));
        }
        
        public void Add(DimensionStyleOverrideType type, object value)
        {
            var dimStyleOverride = new DimensionStyleOverride(type, value);
            BeforeAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(type, dimStyleOverride));
            this._dimensionStyleOverrides.Add(type, dimStyleOverride);
            OnAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(type, dimStyleOverride));
        }

		public void Clear()
		{
			// Raise remove events for each existing item so listeners (e.g., Dimension) can clean XData
			if (this._dimensionStyleOverrides.Count == 0)
				return;

			var keys = new List<DimensionStyleOverrideType>(this._dimensionStyleOverrides.Keys);
			foreach (var key in keys)
			{
				if (!this._dimensionStyleOverrides.TryGetValue(key, out var existing))
					continue;

				BeforeRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
				this._dimensionStyleOverrides.Remove(key);
				OnRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
			}
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

            BeforeRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Key, existing));
            bool removed = this._dimensionStyleOverrides.Remove(item.Key);

            if (removed)
            {
                OnRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(item.Key, existing));
            }

            return removed;
        }
        
      		public bool Remove(DimensionStyleOverrideType key)
      		{
      			if (!this._dimensionStyleOverrides.TryGetValue(key, out var existing))
      				return false;

      			BeforeRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
      			bool removed = this._dimensionStyleOverrides.Remove(key);

      			if (removed)
      			{
      				OnRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
      			}

      			return removed;
      		}

		public int Count => this._dimensionStyleOverrides.Count;
		public bool IsReadOnly => false;

		public bool ContainsKey(DimensionStyleOverrideType key)
		{
			return this._dimensionStyleOverrides.ContainsKey(key);
		}

		public bool TryGetValue(DimensionStyleOverrideType key, out DimensionStyleOverride value)
		{
			return this._dimensionStyleOverrides.TryGetValue(key, out value);
		}

        public DimensionStyleOverride this[DimensionStyleOverrideType key]
        {
            get => this._dimensionStyleOverrides[key];
            set
            {
                bool hadExisting = this._dimensionStyleOverrides.TryGetValue(key, out var existing);

                BeforeAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, value));
                if(hadExisting) BeforeRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
                this._dimensionStyleOverrides[key] = value;

                if (hadExisting)
                {
                    // Treat as remove + add; or create a separate OnReplace if you want
                    OnRemove?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, existing));
                }

                OnAdd?.Invoke(this, new DimensionStyleOverrideChangedEventArgs(key, value));
            }
        }

		public ICollection<DimensionStyleOverrideType> Keys => this._dimensionStyleOverrides.Keys;
		public ICollection<DimensionStyleOverride> Values => this._dimensionStyleOverrides.Values;
	}
}