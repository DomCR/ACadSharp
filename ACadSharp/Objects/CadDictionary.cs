using ACadSharp.Attributes;
using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public class CadDictionary : NonGraphicalObject, IObservableCollection<NonGraphicalObject>
	{
		public event EventHandler<CollectionChangedEventArgs> OnAdd;
		public event EventHandler<CollectionChangedEventArgs> OnRemove;

		#region Root dictionary entries

		/// <summary>
		/// ROOT dictionary, only used in the top level dictionary
		/// </summary>
		public const string Root = "ROOT";

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

		/// <summary>
		/// ACAD_FIELDLIST dictionary entry
		/// </summary>
		public const string AcadFieldList = "ACAD_FIELDLIST";

		/// <summary>
		/// ACAD_IMAGE_DICT dictionary entry
		/// </summary>
		public const string AcadImageDict = "ACAD_IMAGE_DICT";

		#endregion

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DICTIONARY;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDictionary;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Dictionary;

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

		public CadObject this[string key] { get { return this._entries[key]; } }

		private readonly Dictionary<string, NonGraphicalObject> _entries = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Creates the root dictionary with the default entries.
		/// </summary>
		/// <returns></returns>
		public static CadDictionary CreateRoot()
		{
			CadDictionary root = new CadDictionary(Root);

			root.TryAdd(new CadDictionary(AcadColor));
			root.TryAdd(new CadDictionary(AcadGroup));

			CadDictionary layouts = root.ensureCadDictionaryExist(AcadLayout);
			layouts.TryAdd(Layout.Default);
			layouts.TryAdd(new Layout("Layout1"));

			root.TryAdd(new CadDictionary(AcadMaterial));
			root.TryAdd(new CadDictionary(AcadSortEnts));

			CadDictionary mLeaderStyles = root.ensureCadDictionaryExist(AcadMLeaderStyle);
			mLeaderStyles.TryAdd(MultiLeaderStyle.Default);

			CadDictionary mLineStyles = root.ensureCadDictionaryExist(AcadMLineStyle);
			mLineStyles.TryAdd(MLineStyle.Default);

			root.TryAdd(new CadDictionary(AcadTableStyle));
			root.TryAdd(new CadDictionary(AcadPlotSettings));
			// { AcadPlotStyleName, new CadDictionaryWithDefault() },	//Add default entry "Normal"	PlaceHolder	??

			root.TryAdd(new CadDictionary(VariableDictionary));
			//DictionaryVars Entry DIMASSOC and HIDETEXT ??

			CadDictionary scales = root.ensureCadDictionaryExist(AcadScaleList);
			scales.TryAdd(new Scale { Name = "A0", PaperUnits = 1.0, DrawingUnits = 1.0, IsUnitScale = true });
			scales.TryAdd(new Scale { Name = "A1", PaperUnits = 1.0, DrawingUnits = 2.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A2", PaperUnits = 1.0, DrawingUnits = 4.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A3", PaperUnits = 1.0, DrawingUnits = 5.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A4", PaperUnits = 1.0, DrawingUnits = 8.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A5", PaperUnits = 1.0, DrawingUnits = 10.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A6", PaperUnits = 1.0, DrawingUnits = 16.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A7", PaperUnits = 1.0, DrawingUnits = 20.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A8", PaperUnits = 1.0, DrawingUnits = 30.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "A9", PaperUnits = 1.0, DrawingUnits = 40.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B0", PaperUnits = 1.0, DrawingUnits = 50.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B1", PaperUnits = 1.0, DrawingUnits = 100.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B2", PaperUnits = 2.0, DrawingUnits = 1.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B3", PaperUnits = 4.0, DrawingUnits = 1.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B4", PaperUnits = 8.0, DrawingUnits = 1.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B5", PaperUnits = 10.0, DrawingUnits = 1.0, IsUnitScale = false });
			scales.TryAdd(new Scale { Name = "B6", PaperUnits = 100.0, DrawingUnits = 1.0, IsUnitScale = false });

			root.TryAdd(new CadDictionary(AcadVisualStyle));
			root.TryAdd(new CadDictionary(AcadFieldList));
			root.TryAdd(new CadDictionary(AcadImageDict));

			return root;
		}

		/// <summary>
		/// Default constructor.
		/// </summary>
		public CadDictionary() { }

		/// <summary>
		/// Constructor for a named dictionary.
		/// </summary>
		/// <param name="name">Dictionary name.</param>
		public CadDictionary(string name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Add a <see cref="NonGraphicalObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="key">key for the entry in the dictionary</param>
		/// <param name="value"></param>
		public void Add(string key, NonGraphicalObject value)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(nameof(value), $"NonGraphicalObject [{this.GetType().FullName}] must have a name");
			}

			this._entries.Add(key, value);
			value.Owner = this;

			OnAdd?.Invoke(this, new CollectionChangedEventArgs(value));
		}

		/// <summary>
		/// Add a <see cref="NonGraphicalObject"/> to the collection, this method triggers <see cref="OnAdd"/>
		/// </summary>
		/// <param name="value">the name of the NonGraphicalObject will be used as a key for the dictionary</param>
		/// <exception cref="ArgumentException"></exception>
		public void Add(NonGraphicalObject value)
		{
			this.Add(value.Name, value);
		}

		/// <summary>
		/// Tries to add the <see cref="NonGraphicalObject"/> entry if the key doesn't exits.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns>true if the element is successfully added; otherwise, false.</returns>
		public bool TryAdd(string key, NonGraphicalObject value)
		{
			if (!this._entries.ContainsKey(key))
			{
				this.Add(key, value);
			}

			return false;
		}

		/// <summary>
		/// Tries to add the <see cref="NonGraphicalObject"/> entry using the name as key.
		/// </summary>
		/// <param name="value"></param>
		/// <returns>true if the element is successfully added; otherwise, false.</returns>
		public bool TryAdd(NonGraphicalObject value)
		{
			return this.TryAdd(value.Name, value);
		}

		/// <summary>
		/// Removes a <see cref="NonGraphicalObject"/> from the collection, this method triggers <see cref="OnRemove"/>
		/// </summary>
		/// <param name="key"></param>
		/// <param name="item"></param>
		/// <returns>true if the element is successfully removed; otherwise, false.</returns>
		public bool Remove(string key, out NonGraphicalObject item)
		{
			if (this._entries.Remove(key, out item))
			{
				item.Owner = null;
				OnRemove?.Invoke(this, new CollectionChangedEventArgs(item));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the value associated with the specific key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns>The value with Type T or null if not found or different type</returns>
		public T GetEntry<T>(string name)
			where T : NonGraphicalObject
		{
			this.TryGetEntry<T>(name, out T value);
			return value;
		}

		/// <summary>
		/// Gets the value associated with the specific key
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns>true if the value is found or false if not found or different type</returns>
		public bool TryGetEntry<T>(string name, out T value)
			where T : NonGraphicalObject
		{
			if (this._entries.TryGetValue(name, out NonGraphicalObject obj))
			{
				if (obj is T t)
				{
					value = t;
					return true;
				}
			}

			value = null;
			return false;
		}

		/// <inheritdoc/>
		public IEnumerator<NonGraphicalObject> GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}

		private CadDictionary ensureCadDictionaryExist(string name)
		{
			if (!this.TryGetEntry(name, out CadDictionary entry))
			{
				entry = new CadDictionary(name);
				this.Add(entry);
			}

			return entry;
		}
	}
}
