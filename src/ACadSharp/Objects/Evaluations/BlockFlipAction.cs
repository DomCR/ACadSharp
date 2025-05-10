using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;


namespace ACadSharp.Objects.Evaluations {

	//BLOCKFLIPACTION
	//100	S	AcDbEvalExpr
	//100	S	AcDbBlockElement
	//100	S	AcDbBlockAction

	//100	S	AcDbBlockFlipAction
	//92	BL	6
	//93	BL	6
	//94	BL	6
	//95	BL	6
	//301	S	Flip
	//302	S	UpdatedFlip
	//303	S	UpdatedBase
	//304	S	UpdatedEnd

	[DxfName(DxfFileToken.ObjectBlockFlipAction)]
	[DxfSubClass(DxfSubclassMarker.BlockFlipAction)]
	public class BlockFlipAction : BlockAction {

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockFlipAction;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockFlipAction;


		[DxfCodeValue(92)]
		public int Value92 { get; set; }


		[DxfCodeValue(93)]
		public int Value93 { get; set; }


		[DxfCodeValue(94)]
		public int Value94 { get; set; }


		[DxfCodeValue(95)]
		public int Value95 { get; set; }


		[DxfCodeValue(301)]
		public string Caption301 { get; set; }


		[DxfCodeValue(302)]
		public string Caption302 { get; set; }


		[DxfCodeValue(303)]
		public string Caption303 { get; set; }


		[DxfCodeValue(304)]
		public string Caption304 { get; set; }
	}
}
