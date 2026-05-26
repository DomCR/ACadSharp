using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKROTATIONGRIP object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockRotationGrip"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockRotationGrip"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockRotationGrip)]
	[DxfSubClass(DxfSubclassMarker.BlockRotationGrip)]
	public class BlockRotationGrip : BlockGrip
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockRotationGrip;
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockRotationGrip;

	}
}