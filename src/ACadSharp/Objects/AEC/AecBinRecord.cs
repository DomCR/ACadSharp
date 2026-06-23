using ACadSharp.Attributes;

namespace ACadSharp.Objects.AEC;

/// <summary>
/// Represents an AcAecBinRecord object used by AEC (Architecture, Engineering and Construction) entities.
/// </summary>
/// <remarks>
/// AcAecBinRecord objects store the actual geometry and property data for AEC entities
/// like AEC_WALL, AEC_DOOR, AEC_WINDOW, etc. The parent AEC entity contains a reference
/// to this object which holds the actual data in a proprietary binary format.
/// </remarks>
[DxfName(DxfFileToken.ObjectBinRecord)]
[DxfSubClass(DxfSubclassMarker.BinRecord)]
public class AecBinRecord : NonGraphicalObject
{
	/// <summary>
	/// Binary data containing the actual AEC object properties.
	/// The format is proprietary and undocumented.
	/// </summary>
	public byte[] BinaryData { get; set; } = new byte[0];

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBinRecord;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	public override string SubclassMarker => DxfSubclassMarker.BinRecord;

	/// <summary>
	/// Version number of the binary record format.
	/// </summary>
	public int Version { get; set; }

	/// <inheritdoc/>
	public AecBinRecord()
	{
	}

	/// <inheritdoc/>
	public AecBinRecord(string name) : base(name)
	{
	}
}