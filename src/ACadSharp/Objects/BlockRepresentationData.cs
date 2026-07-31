using ACadSharp.Attributes;
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
public class BlockRepresentationData : NonGraphicalObject
{
	[DxfCodeValue(DxfReferenceType.Handle, 340)]
	public BlockRecord Block { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockRepresentationData;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockRepresentationData;

	[DxfCodeValue(70)]
	public short Version { get; set; }
}