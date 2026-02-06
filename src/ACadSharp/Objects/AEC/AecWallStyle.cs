using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents an AcAecWallStyle object used by AEC Wall entities.
	/// </summary>
	/// <remarks>
	/// AcAecWallStyle objects define wall style properties such as components, 
	/// materials, and other wall construction parameters used by AEC_WALL entities.
	/// These objects are stored in the DWG file and referenced by wall entities.
	/// Object name: AEC_WALL_STYLE
	/// </remarks>
	[DxfName("AEC_WALL_STYLE")]
	[DxfSubClass("AecDbWallStyle")]
	public class AecWallStyle : NonGraphicalObject
	{

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectAecWallStyle;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AecDbWallStyle;

		/// <summary>
		/// AEC wall style version number.
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// Description of the wall style.
		/// </summary>
		public string Description { get; set; } = string.Empty;

		public byte[] RawData;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public AecWallStyle() : base()
		{
		}

		/// <summary>
		/// Initialize an <see cref="AecWallStyle"/> with a specific name.
		/// </summary>
		/// <param name="name">Name of the wall style.</param>
		public AecWallStyle(string name) : base(name)
		{
		}
	}
}