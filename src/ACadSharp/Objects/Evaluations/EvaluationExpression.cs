using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Objects.Evaluations
{
	//Tree example:
	//BLOCKLINEARPARAMETER
	//AcDbEvalExpr
	//AcDbBlockElement
	//AcDbBlockParameter
	//AcDbBlock2PtParameter
	//AcDbBlockLinearParameter

	//BLOCKLINEARGRIP
	//AcDbEvalExpr
	//AcDbBlockElement
	//AcDbBlockGrip
	//AcDbBlockLinearGrip
	//

	/// <summary>
	/// 
	/// </summary>
	public abstract class EvaluationExpression : CadObject
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.EvalGraphExpr;

		[DxfCodeValue(90)]
		internal int Value90 { get; set; }

		[DxfCodeValue(98)]
		internal int Value98 { get; set; }

		[DxfCodeValue(99)]
		internal int Value99 { get; set; }
	}

	public abstract class BlockElement : EvaluationExpression
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockElement;

		/// <summary>
		/// Block element name.
		/// </summary>
		[DxfCodeValue(300)]
		public string ElementName { get; set; }

		//Repeats 98 and 99 with the same values as it's parent
	}

	public abstract class BlockParameter : BlockElement
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockParameter;

		[DxfCodeValue(280)]
		internal bool Value280 { get; set; }

		[DxfCodeValue(281)]
		internal bool Value281 { get; set; }
	}

	public abstract class Block2PtParameter : BlockParameter
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Block2PtParameter;

		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ Start { get; set; }

		[DxfCodeValue(1011, 1021, 1031)]
		public XYZ End { get; set; }

		[DxfCodeValue(DxfReferenceType.Count, 170)]
		[DxfCollectionCodeValue(91)]
		public List<int> Value170 { get; set; } = new();

		[DxfCodeValue(171)]
		//Follows a list of:
		//171 (int) - 92 (int) - 301 (string)
		//172 - 93 - 302
		//...
		public List<object> Value171 { get; set; } = new();
	}

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
