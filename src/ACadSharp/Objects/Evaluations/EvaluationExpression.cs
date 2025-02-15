using ACadSharp.Attributes;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// 
	/// </summary>
	[DxfSubClass(DxfSubclassMarker.EvalGraphExpr)]
	public abstract class EvaluationExpression : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.EvalGraphExpr;

		[DxfCodeValue(90)]
		internal int Value90 { get; set; }

		[DxfCodeValue(98)]
		internal int Value98 { get; set; }

		[DxfCodeValue(99)]
		internal int Value99 { get; set; }
	}
}
