using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Classes
{
	public class DxfClassCollection : ICollection<DxfClass>
	{
		/// <inheritdoc/>
		public int Count { get { return this._entries.Count; } }

		/// <inheritdoc/>
		public bool IsReadOnly => false;

		public Dictionary<string, DxfClass> _entries = new Dictionary<string, DxfClass>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Adds or updates the classes in a specific document
		/// </summary>
		/// <param name="doc"></param>
		public static void UpdateDxfClasses(CadDocument doc)
		{
			//AcDbDictionaryWithDefault
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.DictionaryWithDefault,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)22,
				DxfName = DxfFileToken.ObjectDictionaryWithDefault,
				ItemClassId = 499,
				MaintenanceVersion = 42,
				ProxyFlags = ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbPlaceHolder
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.AcDbPlaceHolder,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)0,
				DxfName = DxfFileToken.ObjectPlaceholder,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbLayout
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.Layout,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)0,
				DxfName = DxfFileToken.ObjectLayout,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbDictionaryVar
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.DictionaryVar,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = DxfFileToken.ObjectDictionaryVar,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbTableStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.TableStyle,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = DxfFileToken.ObjectTableStyle,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbMaterial
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.Material,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = DxfFileToken.ObjectMaterial,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbVisualStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.VisualStyle,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = DxfFileToken.ObjectVisualStyle,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbScale
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.Scale,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = DxfFileToken.ObjectScale,
				ItemClassId = 499,
				MaintenanceVersion = 1,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbMLeaderStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.MLeaderStyle,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = DxfFileToken.ObjectMLeaderStyle,
				ItemClassId = 499,
				MaintenanceVersion = 25,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbCellStyleMap
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.CellStyleMap,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = DxfFileToken.ObjectCellStyleMap,
				ItemClassId = 499,
				MaintenanceVersion = 25,
				ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//ExAcXREFPanelObject
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "ExAcXREFPanelObject",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "EXACXREFPANELOBJECT",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbImpNonPersistentObjectsCollection
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbImpNonPersistentObjectsCollection",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "NPOCOLLECTION",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbLayerIndex
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbLayerIndex",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "LAYER_INDEX",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbSpatialIndex
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbSpatialIndex",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "SPATIAL_INDEX",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbIdBuffer
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbIdBuffer",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1014,
				DxfName = "IDBUFFER",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbSectionViewStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbSectionViewStyle",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "ACDBSECTIONVIEWSTYLE",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbDetailViewStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbDetailViewStyle",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "ACDBDETAILVIEWSTYLE",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbSubDMesh
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.Mesh,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = DxfFileToken.EntityMesh,
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbSortentsTable
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.SortentsTable,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1014,
				DxfName = DxfFileToken.ObjectSortEntsTable,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbTextObjectContextData
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbTextObjectContextData",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "ACDB_TEXTOBJECTCONTEXTDATA_CLASS",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbWipeout
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "WipeOut",
				CppClassName = DxfSubclassMarker.Wipeout,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1015,
				DxfName = DxfFileToken.EntityWipeout,
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbWipeoutVariables
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "WipeOut",
				CppClassName = "AcDbWipeoutVariables",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1015,
				DxfName = "WIPEOUTVARIABLES",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbDimAssoc
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "AcDbDimAssoc",
				CppClassName = "AcDbDimAssoc",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "DIMASSOC",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbTable
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.TableEntity,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = DxfFileToken.EntityTable,
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbTableContent
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.TableContent,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = DxfFileToken.ObjectTableContent,
				ItemClassId = 499,
				MaintenanceVersion = 21,
				ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbTableGeometry
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbTableGeometry",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "TABLEGEOMETRY",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbRasterImage
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "ISM",
				CppClassName = DxfSubclassMarker.RasterImage,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = DxfFileToken.EntityImage,
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbRasterImageDef
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "ISM",
				CppClassName = DxfSubclassMarker.RasterImageDef,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = DxfFileToken.ObjectImageDefinition,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbColor
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.DbColor,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1015,
				DxfName = DxfFileToken.ObjectDBColor,
				ItemClassId = 499,
				MaintenanceVersion = 14,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbGeoData
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.GeoData,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = DxfFileToken.ObjectGeoData,
				ItemClassId = 499,
				MaintenanceVersion = 45,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbMLeader
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.MultiLeader,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.MC0_0,
				DxfName = DxfFileToken.EntityMultiLeader,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbPdfReference
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.PdfReference,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)26,
				DxfName = DxfFileToken.EntityPdfUnderlay,
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbPdfDefinition
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.PdfDefinition,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)26,
				DxfName = DxfFileToken.ObjectPdfDefinition,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbRasterVariables
			doc.Classes.AddOrUpdate(new DxfClass
			{
				ApplicationName = "ISM",
				CppClassName = DxfSubclassMarker.RasterVariables,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = DxfFileToken.ObjectRasterVariables,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbSpatialFilter
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = DxfSubclassMarker.SpatialFilter,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = DxfFileToken.ObjectSpatialFilter,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
				IsAnEntity = false,
			});

			//AcDbMLeaderObjectContextData
			doc.Classes.AddOrUpdate(new DxfClass {
				CppClassName = DxfSubclassMarker.MultiLeaderObjectContextData,
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.MC0_0,
				DxfName = DxfFileToken.ObjectMLeaderContextData,
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});
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
				result.InstanceCount = item.InstanceCount;
			}
			else
			{
				this._entries.Add(item.DxfName, item);
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
	}
}