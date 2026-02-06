using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents an AcAecBinRecord object used by AEC (Architecture, Engineering & Construction) entities.
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
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBinRecord;

		public override string SubclassMarker => DxfSubclassMarker.BinRecord;

		/// <summary>
		/// Version number of the binary record format.
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// Binary data containing the actual AEC object properties.
		/// The format is proprietary and undocumented.
		/// </summary>
		public byte[] BinaryData { get; set; } = new byte[0];

		/// <summary>
		/// Optional name/identifier for this binary record.
		/// </summary>
		public string Name { get; set; }
	}
}