using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents an AcAecCleanupGroup object used by AEC Wall entities.
	/// </summary>
	/// <remarks>
	/// AcAecCleanupGroup objects manage wall cleanup operations, which control
	/// how walls join and interact with each other at intersections and endpoints.
	/// Object name: AEC_CLEANUP_GROUP
	/// </remarks>
	[DxfName("AEC_CLEANUP_GROUP")]
	[DxfSubClass("AecDbCleanupGroup")]
	public class AecCleanupGroup : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectAecCleanupGroupDef;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AecDbCleanupGroupDef;

		/// <summary>
		/// AEC cleanup group version number.
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// Description of the cleanup group.
		/// </summary>
		public string Description { get; set; } = string.Empty;
		public byte[] RawData { get; internal set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public AecCleanupGroup() : base()
		{
		}

		/// <summary>
		/// Initialize an <see cref="AecCleanupGroup"/> with a specific name.
		/// </summary>
		/// <param name="name">Name of the cleanup group.</param>
		public AecCleanupGroup(string name) : base(name)
		{
		}
	}
}