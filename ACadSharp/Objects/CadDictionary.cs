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

		//350	Soft-owner ID/handle to entry object (one for each entry) (optional)
		[DxfCodeValue(350)]
		public ulong[] EntryHandle { get; }

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

		public void Add(string key, CadObject value)
		{
			if (_entries.Values.Contains(value))
				throw new ArgumentException($"Dictionary already contains {value.GetType().FullName}", nameof(value));

			this._entries.Add(key, value);
			value.Owner = this;

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
