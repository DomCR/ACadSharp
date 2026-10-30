using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="AcdbPlaceHolder"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectPlaceholder"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.AcDbPlaceHolder"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectPlaceholder)]
[DxfSubClass(DxfSubclassMarker.AcDbPlaceHolder)]
public class AcdbPlaceHolder : NonGraphicalObject, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectPlaceholder;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.ACDBPLACEHOLDER; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.AcDbPlaceHolder;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.AcDbPlaceHolder,
			DwgVersion = (ACadVersion)0,
			DxfName = DxfFileToken.ObjectPlaceholder,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}