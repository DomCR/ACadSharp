using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKLOOKUPPARAMETER object, used in AutoCAD to drive block geometry from
	/// a lookup table in the Block Properties Table (the "Lookup" rows of the Customize tab of
	/// a dynamic block).
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockLookupParameter"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockLookupParameter"/> <br/>
	/// <br/>
	/// Minimal implementation: only the common block-element prefix (which carries
	/// <see cref="EvaluationExpression.Id"/>, code 90 — the key found in ACAD_ENHANCEDBLOCKDATA)
	/// and the display name are decoded. The lookup table itself is not decoded.
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockLookupParameter)]
	[DxfSubClass(DxfSubclassMarker.BlockLookupParameter)]
	public class BlockLookupParameter : Block1PtParameter
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockLookupParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockLookupParameter;

		/// <summary>
		/// Display name of the lookup parameter as shown in the AutoCAD Properties palette
		/// </summary>
		[DxfCodeValue(301)]
		public string Name { get; set; } = string.Empty;
	}
}


