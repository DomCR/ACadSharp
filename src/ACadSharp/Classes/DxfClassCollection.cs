using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Classes;

public class DxfClassCollection : ICollection<DxfClass>
{
	/// <inheritdoc/>
	public int Count { get { return this._entries.Count; } }

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	private readonly CadDocument _document;

	private readonly Dictionary<string, DxfClass> _entries = new Dictionary<string, DxfClass>(StringComparer.OrdinalIgnoreCase);

	public DxfClassCollection(CadDocument document)
	{
		this._document = document;
	}

	/// <summary>
	/// Add a dxf class to the collection if the <see cref="DxfClass.DxfName"/> is not present
	/// </summary>
	/// <param name="item"></param>
	public void Add(DxfClass item)
	{
		this._entries.Add(item.DxfName, item);
	}

	/// <summary>
	/// Add a dxf class to the collection or updates the existing one if the <see cref="DxfClass.DxfName"/> is already in the collection
	/// </summary>
	/// <param name="item"></param>
	public void AddOrUpdate(DxfClass item)
	{
		if (this._entries.TryGetValue(item.DxfName, out DxfClass result))
		{
			result.InstanceCount = result.InstanceCount;
		}
		else
		{
			this.Add(item);
		}
	}

	/// <inheritdoc/>
	public void Clear()
	{
		_entries.Clear();
	}

	/// <summary>
	/// Determines whether the Collection contains a specific <see cref="DxfClass.DxfName"/>.
	/// </summary>
	/// <param name="dxfname"></param>
	/// <returns></returns>
	public bool Contains(string dxfname)
	{
		return this._entries.ContainsKey(dxfname);
	}

	/// <inheritdoc/>
	public bool Contains(DxfClass item)
	{
		return _entries.Values.Contains(item);
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
		return _entries.Values.GetEnumerator();
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
	public void UpdateDxfClasses()
	{
		this.resetClassNumbers();

		//AcDbDictionaryWithDefault
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.DictionaryWithDefault,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)22,
			DxfName = DxfFileToken.ObjectDictionaryWithDefault,
			ItemClassId = 499,
			MaintenanceVersion = 42,
			ProxyFlags = ProxyFlags.R13FormatProxy,
			WasZombie = false,
			InstanceCount = this._document.GetInstanceCount(DxfFileToken.ObjectDictionaryWithDefault),
		});

		//AcDbPlaceHolder
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.AcDbPlaceHolder,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)0,
			DxfName = DxfFileToken.ObjectPlaceholder,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbLayout
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.Layout,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)0,
			DxfName = DxfFileToken.ObjectLayout,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbDictionaryVar
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.DictionaryVar,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.ObjectDictionaryVar,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbTableStyle
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableStyle,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectTableStyle,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		});

		//AcDbMaterial
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.Material,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = DxfFileToken.ObjectMaterial,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbVisualStyle
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.VisualStyle,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.ObjectVisualStyle,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		});

		//AcDbScale
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.Scale,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.ObjectScale,
			ItemClassId = 499,
			MaintenanceVersion = 1,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbMLeaderStyle
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.MLeaderStyle,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.ObjectMLeaderStyle,
			ItemClassId = 499,
			MaintenanceVersion = 25,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		});

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
		});

		//AcDbSubDMesh
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.Mesh,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = DxfFileToken.EntityMesh,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbSortentsTable
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.SortentsTable,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1014,
			DxfName = DxfFileToken.ObjectSortEntsTable,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

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
		});

		//AcDbWipeout
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "WipeOut",
			CppClassName = DxfSubclassMarker.Wipeout,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1015,
			DxfName = DxfFileToken.EntityWipeout,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
			WasZombie = false,
		});

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
		});

		//AcDbDimAssoc
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "AcDbDimAssoc",
			CppClassName = "AcDbDimAssoc",
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = 0,
			DxfName = "DIMASSOC",
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbTable
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableEntity,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.EntityTable,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbTableContent
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableContent,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectTableContent,
			ItemClassId = 499,
			MaintenanceVersion = 21,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

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
		});

		//AcDbRasterImage
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "ISM",
			CppClassName = DxfSubclassMarker.RasterImage,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.EntityImage,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
			WasZombie = false,
		});

		//AcDbRasterImageDef
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "ISM",
			CppClassName = DxfSubclassMarker.RasterImageDef,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.ObjectImageDefinition,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		////AcDbRasterImageDefReactor
		this.AddOrUpdate(new DxfClass
		{
			ApplicationName = "ISM",
			CppClassName = DxfSubclassMarker.RasterImageDefReactor,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.ObjectImageDefinitionReactor,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed,
			WasZombie = false,
		});

		//AcDbColor
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.DbColor,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1015,
			DxfName = DxfFileToken.ObjectDBColor,
			ItemClassId = 499,
			MaintenanceVersion = 14,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbGeoData
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.GeoData,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.ObjectGeoData,
			ItemClassId = 499,
			MaintenanceVersion = 45,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		});

		//AcDbMLeader
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.MultiLeader,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.MC0_0,
			DxfName = DxfFileToken.EntityMultiLeader,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbPdfReference
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.PdfReference,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.EntityPdfUnderlay,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		});

		//AcDbPdfDefinition
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.PdfDefinition,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)26,
			DxfName = DxfFileToken.ObjectPdfDefinition,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

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
		});

		//AcDbSpatialFilter
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.SpatialFilter,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = (ACadVersion)20,
			DxfName = DxfFileToken.ObjectSpatialFilter,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbMLeaderObjectContextData
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.MultiLeaderObjectContextData,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.MC0_0,
			DxfName = DxfFileToken.ObjectMLeaderContextData,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbPlotSettings
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.PlotSettings,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1015,
			DxfName = DxfFileToken.ObjectPlotSettings,
			ItemClassId = 499,
			MaintenanceVersion = 42,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		});

		//AcDbField
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.Field,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectField,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbFieldList
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.FieldList,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectFieldList,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbMTextAttributeObjectContextData
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.MTextAttributeObjectContextData,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.MTextAttributeObjectContextData,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});

		//AcDbBlkRefObjectContextData
		this.AddOrUpdate(new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlkRefObjectContextData,
			ClassNumber = (short)(500 + this.Count),
			DwgVersion = ACadVersion.AC1021,
			DxfName = DxfFileToken.BlkRefObjectContextData,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		});
	}

	private void resetClassNumbers()
	{
		var arr = this._entries.Values.ToArray();
		for (int i = 0; i < arr.Length; i++)
		{
			arr[i].ClassNumber = (short)(500 + i);
		}
	}
}