using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations {

	//BLOCKVISIBILITYGRIP
	//  AcDbEvalExpr
	//  AcDbBlockElement
	//  AcDbBlockGrip
	//  AcDbBlockVisibilityGrip

	//	No more properties

	[DxfSubClass(DxfSubclassMarker.BlockVisibilityGrip)]
	public class BlockVisibilityGrip : BlockGrip {

		// /// <inheritdoc/>
		// public override string ObjectName => DxfFileToken.??;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockVisibilityGrip;
	}
}
