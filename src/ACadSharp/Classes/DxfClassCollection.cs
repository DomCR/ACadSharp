using CSUtilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Classes;

/// <summary>
/// Represents a collection of <see cref="DxfClass"/> objects.
/// </summary>
public class DxfClassCollection : ICollection<DxfClass>
{
	/// <inheritdoc/>
	public int Count { get { return this._entries.Count; } }

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	private readonly CadDocument _document;

	private readonly Dictionary<string, DxfClass> _entries = new Dictionary<string, DxfClass>(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="DxfClassCollection"/> class.
	/// </summary>
	/// <param name="document">The CAD document associated with this collection.</param>
	public DxfClassCollection(CadDocument document)
	{
		this._document = document;
	}

	/// <summary>
	/// Add a dxf class to the collection if the <see cref="DxfClass.DxfName"/> is not present.
	/// </summary>
	/// <param name="item">The dxf class to add.</param>
	public void Add(DxfClass item)
	{
		this._entries.Add(item.DxfName, item);
	}

	public bool TryAdd(DxfClass item)
	{
		return this._entries.TryAdd(item.DxfName, item);
	}

	public void IncreaseInstanceCount(DxfClass dxfClass)
	{
		if (this._entries.TryGetValue(dxfClass.DxfName, out DxfClass result))
		{
			result.InstanceCount++;
		}
		else
		{
			this.Add(dxfClass);
			dxfClass.InstanceCount = 1;
		}
	}

	/// <summary>
	/// Add a dxf class to the collection or updates the existing one if the <see cref="DxfClass.DxfName"/> is already in the collection.
	/// </summary>
	/// <param name="item">The dxf class to add or update.</param>
	public void AddOrUpdate(DxfClass item)
	{
		if (this._entries.TryGetValue(item.DxfName, out DxfClass result))
		{
			result.InstanceCount = this._document.GetInstanceCount(item.DxfName);
		}
		else
		{
			this.Add(item);
		}
	}

	/// <inheritdoc/>
	public void Clear()
	{
		this._entries.Clear();
	}

	/// <summary>
	/// Determines whether the Collection contains a specific <see cref="DxfClass.DxfName"/>.
	/// </summary>
	/// <param name="dxfname">The name of the dxf class to check.</param>
	/// <returns>true if the Collection contains an element with the specified name; otherwise, false.</returns>
	public bool Contains(string dxfname)
	{
		return this._entries.ContainsKey(dxfname);
	}

	/// <inheritdoc/>
	public bool Contains(DxfClass item)
	{
		return this._entries.Values.Contains(item);
	}

	/// <inheritdoc/>
	public void CopyTo(DxfClass[] array, int arrayIndex)
	{
		this._entries.Values.CopyTo(array, arrayIndex);
	}

	/// <summary>
	/// Gets the dxf class associated with <see cref="DxfClass.ClassNumber"/>.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	public DxfClass GetByClassNumber(short id)
	{
		return this._entries.Values.FirstOrDefault(c => c.ClassNumber == id);
	}

	/// <summary>
	/// Gets the dxf class associated with <see cref="DxfClass.DxfName"/>.
	/// </summary>
	/// <param name="dxfname"></param>
	/// <returns></returns>
	public DxfClass GetByName(string dxfname)
	{
		if (this._entries.TryGetValue(dxfname, out DxfClass result))
		{
			return result;
		}
		else
		{
			return null;
		}
	}

	/// <inheritdoc/>
	public IEnumerator<DxfClass> GetEnumerator()
	{
		return this._entries.Values.GetEnumerator();
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this._entries.Values.GetEnumerator();
	}

	/// <inheritdoc/>
	public bool Remove(DxfClass item)
	{
		return this._entries.Remove(item.DxfName);
	}

	/// <summary>
	/// Gets the dxf class associated with <see cref="DxfClass.ClassNumber"/>.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	public bool TryGetByClassNumber(short id, out DxfClass result)
	{
		result = this._entries.Values.FirstOrDefault(c => c.ClassNumber == id);
		return result != null;
	}

	/// <summary>
	/// Gets the dxf class associated with <see cref="DxfClass.DxfName"/>.
	/// </summary>
	/// <param name="dxfname"></param>
	/// <param name="result"></param>
	/// <returns>true if the Collection contains an element with the specified key; otherwise, false.</returns>
	public bool TryGetByName(string dxfname, out DxfClass result)
	{
		return this._entries.TryGetValue(dxfname, out result);
	}

	/// <summary>
	/// Updates the DXF class collection in the document with a predefined set of class definitions and resets class
	/// numbers.
	/// </summary>
	[Obsolete]
	public void UpdateDxfClasses()
	{
		this.ResetClassNumbers();

		//AcDbDictionaryWithDefault

		//AcDbPlaceHolder

		//AcDbLayout

		//AcDbDictionaryVar

		//AcDbTableStyle

		//AcDbMaterial

		//AcDbVisualStyle

		//AcDbScale

		//AcDbMLeaderStyle

		//AcDbCellStyleMap
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.CellStyleMap,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.ObjectCellStyleMap,
			ItemClassId = 499,
			MaintenanceVersion = 25,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount(DxfFileToken.ObjectCellStyleMap),
		});

		//ExAcXREFPanelObject
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "ExAcXREFPanelObject",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "EXACXREFPANELOBJECT",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("EXACXREFPANELOBJECT"),
		});

		//AcDbImpNonPersistentObjectsCollection
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbImpNonPersistentObjectsCollection",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "NPOCOLLECTION",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("NPOCOLLECTION"),
		});

		//AcDbLayerIndex
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbLayerIndex",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "LAYER_INDEX",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("LAYER_INDEX"),
		});

		//AcDbSpatialIndex
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbSpatialIndex",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "SPATIAL_INDEX",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("SPATIAL_INDEX"),
		});

		//AcDbIdBuffer
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbIdBuffer",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1014,
			DxfName = "IDBUFFER",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.R13FormatProxy,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("IDBUFFER"),
		});

		//AcDbSectionViewStyle
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbSectionViewStyle",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "ACDBSECTIONVIEWSTYLE",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("ACDBSECTIONVIEWSTYLE"),
		});

		//AcDbDetailViewStyle
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbDetailViewStyle",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "ACDBDETAILVIEWSTYLE",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("ACDBDETAILVIEWSTYLE"),
		});

		//AcDbSubDMesh

		//AcDbSortentsTable

		//AcDbTextObjectContextData
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbTextObjectContextData",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "ACDB_TEXTOBJECTCONTEXTDATA_CLASS",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("ACDB_TEXTOBJECTCONTEXTDATA_CLASS"),
		});

		//AcDbWipeout

		//AcDbWipeoutVariables
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "WipeOut",
			CppClassName = "AcDbWipeoutVariables",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1015,
			DxfName = "WIPEOUTVARIABLES",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.R13FormatProxy,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("WIPEOUTVARIABLES"),
		});

		//AcDbDimAssoc

		//AcDbArcDimension

		//AcDbTable

		//AcDbTableContent

		//AcDbTableGeometry
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = "AcDbTableGeometry",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "TABLEGEOMETRY",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount("TABLEGEOMETRY"),
		});

		//AcDbRasterImage

		//AcDbRasterImageDef

		////AcDbRasterImageDefReactor

		//AcDbColor

		//AcDbGeoData

		//AcDbMLeader

		//AcDbPdfReference

		//AcDbPdfDefinition

		//AcDbRasterVariables
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "ISM",
			CppClassName = DxfSubclassMarker.RasterVariables,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.ObjectRasterVariables,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount(DxfFileToken.ObjectRasterVariables),
		});

		//AcDbSpatialFilter

		//AcDbMLeaderObjectContextData

		//AcDbPlotSettings

		//AcDbField

		//AcDbFieldList

		//AcDbMTextAttributeObjectContextData

		//AcDbBlkRefObjectContextData
	}

	public void ResetClassNumbers()
	{
		var arr = this._entries.Values.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i].ClassNumber = (short)(500 + i);
		}
	}
}