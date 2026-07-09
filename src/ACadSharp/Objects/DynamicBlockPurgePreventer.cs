using ACadSharp.Attributes;

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
public class DynamicBlockPurgePreventer : NonGraphicalObject
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
}