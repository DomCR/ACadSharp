using ACadSharp.Attributes;
using CSUtilities.Extensions;
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

		#region Root dictionary entries

		/// <summary>
		/// ACAD_COLOR dictionary entry
		/// </summary>
		public const string AcadColor = "ACAD_COLOR";

		/// <summary>
		/// ACAD_GROUP dictionary entry
		/// </summary>
		public const string AcadGroup = "ACAD_GROUP";

		/// <summary>
		/// ACAD_LAYOUT dictionary entry
		/// </summary>
		public const string AcadLayout = "ACAD_LAYOUT";

		/// <summary>
		/// ACAD_MATERIAL dictionary entry
		/// </summary>
		public const string AcadMaterial = "ACAD_MATERIAL";

		/// <summary>
		/// ACAD_SORTENTS dictionary entry
		/// </summary>
		public const string AcadSortEnts = "ACAD_SORTENTS";

		/// <summary>
		/// ACAD_MLEADERSTYLE dictionary entry
		/// </summary>
		public const string AcadMLeaderStyle = "ACAD_MLEADERSTYLE";

		/// <summary>
		/// ACAD_MLINESTYLE dictionary entry
		/// </summary>
		public const string AcadMLineStyle = "ACAD_MLINESTYLE";

		/// <summary>
		/// ACAD_TABLESTYLE dictionary entry
		/// </summary>
		public const string AcadTableStyle = "ACAD_TABLESTYLE";

		/// <summary>
		/// ACAD_PLOTSETTINGS dictionary entry
		/// </summary>
		public const string AcadPlotSettings = "ACAD_PLOTSETTINGS";

		/// <summary>
		/// AcDbVariableDictionary dictionary entry
		/// </summary>
		public const string VariableDictionary = "AcDbVariableDictionary";

		/// <summary>
		/// ACAD_PLOTSTYLENAME dictionary entry
		/// </summary>
		public const string AcadPlotStyleName = "ACAD_PLOTSTYLENAME";

		/// <summary>
		/// scales dictionary
		/// </summary>
		public const string AcadScaleList = "ACAD_SCALELIST";

		/// <summary>
		/// ACAD_VISUALSTYLE dictionary entry
		/// </summary>
		public const string AcadVisualStyle = "ACAD_VISUALSTYLE";

		#endregion

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

		/// <summary>
		/// Entry names
		/// </summary>
		[DxfCodeValue(3)]
		public string[] EntryNames { get { return this._entries.Keys.ToArray(); } }

		/// <summary>
		/// Soft-owner ID/handle to entry object
		/// </summary>
		[DxfCodeValue(350)]
		public ulong[] EntryHandles { get { return this._entries.Values.Select(c => c.Handle).ToArray(); } }

		public CadObject this[string entry] { get { return this._entries[entry]; } }

		private Dictionary<string, CadObject> _entries { get; } = new Dictionary<string, CadObject>();    //TODO: Transform into an objservable collection

		/// <summary>
		/// Creates the root dictionary with the default entries
		/// </summary>
		/// <returns></returns>
		public static CadDictionary CreateRoot()
		{
			CadDictionary root = new CadDictionary();

			root.Add(CadDictionary.AcadLayout, new CadDictionary());

			return root;
		}

		/// <summary>
		/// Add a <see cref="CadObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <exception cref="ArgumentException"></exception>
		public void Add(string key, CadObject value)
		{
			if (_entries.Values.Contains(value))
				throw new ArgumentException($"Dictionary already contains {value.GetType().FullName}", nameof(value));

			this._entries.Add(key, value);
			value.Owner = this;

			OnAdd?.Invoke(this, new ReferenceChangedEventArgs(value));
		}

		/// <summary>
		/// Removes a <see cref="CadObject"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="key"></param>
		/// <returns>The removed <see cref="CadObject"/></returns>
		public CadObject Remove(string key)
		{
			if (this._entries.Remove(key, out CadObject item))
			{
				item.Owner = null;
				OnRemove?.Invoke(this, new ReferenceChangedEventArgs(item));
				return item;
			}

			return null;
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
