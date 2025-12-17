using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;

using CSMath;

namespace ACadSharp.Objects.Evaluations {

	//BLOCKFLIPGRIP
	//100	S	AcDbEvalExpr
	//100	S	AcDbBlockElement
	//100	S	AcDbBlockGrip

	//100	S	AcDbBlockFlipGrip
	//140	BD	0.0
	//141	BD	59.01053412749748
	//142	BD	0.0
	//93	BL	8

	[DxfSubClass(DxfSubclassMarker.BlockFlipGrip)]
	public class BlockFlipGrip : BlockGrip {

		// /// <inheritdoc/>
		//public override string ObjectName => DxfFileToken.ObjectBlockFlipAction;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockFlipGrip;


		[DxfCodeValue(140)]
		public double Value140 { get; set; }


		[DxfCodeValue(141)]
		public double Value141 { get; set; }


		[DxfCodeValue(142)]
		public double Value142 { get; set; }


		[DxfCodeValue(93)]
		public int Value93N { get; set; }
	}
}
