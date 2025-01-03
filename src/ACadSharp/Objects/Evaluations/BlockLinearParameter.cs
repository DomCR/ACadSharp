using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// 
	/// </summary>
	[DxfName(DxfFileToken.ObjectBlockLinearParameter)]
	[DxfSubClass(DxfSubclassMarker.BlockLinearParameter)]
	public class BlockLinearParameter : Block2PtParameter
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockLinearParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockLinearParameter;

		/// <summary>
		/// Label text.
		/// </summary>
		[DxfCodeValue(305)]
		public string Label { get; set; }

		[DxfCodeValue(306)]
		public string Description { get; set; }

		[DxfCodeValue(140)]
		public double LabelOffset { get; set; }
	}
}
