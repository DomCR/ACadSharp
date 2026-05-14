using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	[DxfSubClass(DxfSubclassMarker.BlockElement)]
	public abstract class BlockElement : EvaluationExpression
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockElement;

		/// <summary>
		/// Block element name.
		/// </summary>
		[DxfCodeValue(300)]
		public string ElementName { get; set; }

		[DxfCodeValue(1071)]
		public int Value1071 { get; set; }
	}
}
