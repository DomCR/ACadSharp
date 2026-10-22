using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="DynamicBlockPurgePreventer"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectDynamicBlockPurgePreventer"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.AcDbDynamicBlockPurgePreventer"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectDynamicBlockPurgePreventer)]
[DxfSubClass(DxfSubclassMarker.AcDbDynamicBlockPurgePreventer)]
public class DynamicBlockPurgePreventer : NonGraphicalObject, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectDynamicBlockPurgePreventer;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AcDbDynamicBlockPurgePreventer;

	/// <summary>
	/// Gets or sets the version of the dynamic block purge preventer.
	/// </summary>
	[DxfCodeValue(70)]
	public short Version { get; set; }

	// Not present in dxf
	internal BlockRecord Block { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.AcDbDynamicBlockPurgePreventer,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectDynamicBlockPurgePreventer,
			ItemClassId = 499,
			MaintenanceVersion = 61,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}