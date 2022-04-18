using ACadSharp.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="CadDictionary"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionary"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dictionary"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionary)]
	[DxfSubClass(DxfSubclassMarker.Dictionary)]
	public class CadDictionary : CadObject, IObservableCollection<CadObject>
	{
		public event EventHandler<ReferenceChangedEventArgs> OnAdd;
		public event EventHandler<ReferenceChangedEventArgs> OnRemove;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DICTIONARY;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDictionary;

		/// <summary>
		/// indicates that elements of the dictionary are to be treated as hard-owned.
		/// </summary>
		[DxfCodeValue(280)]
		public bool HardOwnerFlag { get; set; }

		/// <summary>
		/// Duplicate record cloning flag (determines how to merge duplicate entries)
		/// </summary>
		[DxfCodeValue(281)]
		public DictionaryCloningFlags ClonningFlags { get; set; }

		//3	Entry name(one for each entry) (optional)
		[DxfCodeValue(3)]
		public string[] EntryNames { get; }

		[DxfCodeValue(350)]
		public ulong[] EntryHandle { get; }

		//350	Soft-owner ID/handle to entry object (one for each entry) (optional)
		private Dictionary<string, CadObject> _entries { get; } = new Dictionary<string, CadObject>();    //TODO: Transform into an objservable collection

		public void Add(string key, CadObject value)
		{
			if (_entries.Values.Contains(value))
				throw new ArgumentException($"Dictionary already contains {value.GetType().FullName}", nameof(value));

			this._entries.Add(key, value);

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(value));
		}

		public IEnumerator<CadObject> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}
	}
}
