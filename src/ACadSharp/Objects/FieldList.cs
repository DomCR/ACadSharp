using ACadSharp.Attributes;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="FieldList"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectFieldList"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.FieldList"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectFieldList)]
[DxfSubClass(DxfSubclassMarker.FieldList)]
public class FieldList : NonGraphicalObject
{
	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectFieldList;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.FieldList;
}
