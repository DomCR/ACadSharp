using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations {

	//BLOCKGRIPLOCATIONCOMPONENT
	//  AcDbEvalExpr
	//  AcDbBlockGripExpr
	//		91	BL	2
	//		300	S	UpdatedX

	[DxfSubClass(DxfSubclassMarker.BlockGripExpression)]
	public abstract class BlockGripExpression : EvaluationExpression {

		/// <inheritdoc/>
		///	public override string ObjectName => DxfFileToken.ObjectBlockFlipAction;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockGripExpression;


		//		91	BL	2
		//		300	S	UpdatedX
	}
}
