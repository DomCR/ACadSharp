using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKGRIPLOCATIONCOMPONENT object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockGripLocationComponent"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockGripExpression"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockGripLocationComponent)]
	[DxfSubClass(DxfSubclassMarker.BlockGripExpression)]
	public class BlockGripExpression : EvaluationExpression
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockGripLocationComponent;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockGripExpression;

		[DxfCodeValue(300)]
		public string Value300 { get; set; }

		[DxfCodeValue(91)]
		public int Value91 { get; set; }
	}
}