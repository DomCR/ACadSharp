using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="ImageDefinitionReactor"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectImageDefinitionReactor"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterImageDefReactor"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectImageDefinitionReactor)]
	[DxfSubClass(DxfSubclassMarker.RasterImageDefReactor)]
	public class ImageDefinitionReactor : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNDEFINED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectImageDefinitionReactor;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterImageDefReactor;

		/// <summary>
		/// Class version 2.
		/// </summary>
		[DxfCodeValue(90)]
		public int ClassVersion { get; set; }

		/// <summary>
		/// Object ID for associated image object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public ImageDefinition Definition { get; set; }
	}
}
