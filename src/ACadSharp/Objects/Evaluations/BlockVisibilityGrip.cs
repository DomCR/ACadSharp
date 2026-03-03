using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKVISIBILITYGRIP object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockVisibilityGrip"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockVisibilityGrip"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockVisibilityGrip)]
	[DxfSubClass(DxfSubclassMarker.BlockVisibilityGrip)]
	public class BlockVisibilityGrip : BlockGrip
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockVisibilityGrip;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityGrip;
	}
}
