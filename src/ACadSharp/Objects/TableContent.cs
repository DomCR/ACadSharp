using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="TableContent"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectTableContent"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.TableContent"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectTableContent)]
[DxfSubClass(DxfSubclassMarker.TableContent)]
public class TableContent : FormattedTableData, IDxfClassDefined
{
	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectTableContent;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.TableContent;

	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public TableStyle Style { get; set; } = TableStyle.Default;

	public TableStyle StyleOverride { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableContent,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectTableContent,
			ItemClassId = 499,
			MaintenanceVersion = 21,
			ProxyFlags = ProxyFlags.CloningAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}
