using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Tables;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="BlockRepresentationData"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectBlockRepresentationData"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.BlockRepresentationData"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectBlockRepresentationData)]
[DxfSubClass(DxfSubclassMarker.BlockRepresentationData)]
public class BlockRepresentationData : NonGraphicalObject, IDxfClassDefined
{
	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public BlockRecord Block { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRepresentationData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRepresentationData;

	[DxfCodeValue(70)]
	public short Version { get; set; }

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.BlockRepresentationData,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectBlockRepresentationData,
			ItemClassId = 499,
			MaintenanceVersion = 58,
			ProxyFlags = ACadSharp.Classes.ProxyFlags.EraseAllowed | ACadSharp.Classes.ProxyFlags.CloningAllowed | ACadSharp.Classes.ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}
}