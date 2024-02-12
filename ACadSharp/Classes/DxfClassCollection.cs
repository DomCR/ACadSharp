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
				CppClassName = "AcDbDictionaryWithDefault",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)22,
				DxfName = "ACDBDICTIONARYWDFLT",
				ItemClassId = 499,
				MaintenanceVersion = 42,
				ProxyFlags = ProxyFlags.R13FormatProxy,
				WasZombie = false,
			});

			//AcDbPlaceHolder
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbPlaceHolder",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)0,
				DxfName = "ACDBPLACEHOLDER",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbLayout
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbLayout",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)0,
				DxfName = "LAYOUT",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbDictionaryVar
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbDictionaryVar",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = (ACadVersion)20,
				DxfName = "DICTIONARYVAR",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbTableStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbTableStyle",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = "TABLESTYLE",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbMaterial
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbMaterial",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "MATERIAL",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbVisualStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbVisualStyle",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = "VISUALSTYLE",
				ItemClassId = 499,
				MaintenanceVersion = 0,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbScale
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbScale",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = "SCALE",
				ItemClassId = 499,
				MaintenanceVersion = 1,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbMLeaderStyle
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbMLeaderStyle",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = "MLEADERSTYLE",
				ItemClassId = 499,
				MaintenanceVersion = 25,
				ProxyFlags = (ProxyFlags)4095,
				WasZombie = false,
			});

			//AcDbCellStyleMap
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbCellStyleMap",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1021,
				DxfName = "CELLSTYLEMAP",
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
				CppClassName = "AcDbSubDMesh",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = 0,
				DxfName = "MESH",
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.None,
				WasZombie = false,
			});

			//AcDbSortentsTable
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbSortentsTable",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1014,
				DxfName = "SORTENTSTABLE",
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
				CppClassName = "AcDbWipeout",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1015,
				DxfName = "WIPEOUT",
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
				CppClassName = "AcDbTable",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = "ACAD_TABLE",
				ItemClassId = 498,
				MaintenanceVersion = 0,
				ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
				WasZombie = false,
			});

			//AcDbTableContent
			doc.Classes.AddOrUpdate(new DxfClass
			{
				CppClassName = "AcDbTableContent",
				ClassNumber = (short)(500 + doc.Classes.Count),
				DwgVersion = ACadVersion.AC1018,
				DxfName = "TABLECONTENT",
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
			if (_entries.TryGetValue(item.DxfName, out DxfClass result))
			{
				result.InstanceCount = item.InstanceCount;
			}
			else
			{
				_entries.Add(item.DxfName, item);
			}
		}

		/// <summary>
		/// Get by <see cref="DxfClass.DxfName"/>
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
		public void Clear()
		{
			_entries.Clear();
		}

		/// <summary>
		/// Determines whether the Collection contains a specific <see cref="DxfClass.DxfName"/>
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
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public IEnumerator<DxfClass> GetEnumerator()
		{
			return _entries.Values.GetEnumerator();
		}

		/// <inheritdoc/>
		public bool Remove(DxfClass item)
		{
			return this._entries.Remove(item.DxfName);
		}

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this._entries.Values.GetEnumerator();
		}
	}
}
