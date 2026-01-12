using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;

using CSMath;

namespace ACadSharp.Objects.Evaluations
{

	//BLOCKFLIPGRIP
	//100	S	AcDbEvalExpr
	//100	S	AcDbBlockElement

	//100	S	AcDbBlockGrip
	//91	BL	9
	//92	BL	10
	//1010	BD	-0.0000000001218723
	//1020	BD	50.80278554897086
	//1030	BD	0.0
	//280	BS	0
	//93	BL	-1

	[DxfSubClass(DxfSubclassMarker.BlockGrip)]
	public abstract class BlockGrip : BlockElement
    {
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockGrip;


		[DxfCodeValue(9)]
		public int Value9 { get; set; }


		[DxfCodeValue(10)]
		public int Value10 { get; set; }


		[DxfCodeValue(1010, 1020, 1030)]
		public XYZ Location { get; set; }


		[DxfCodeValue(10)]
		public short Value280 { get; set; }


		[DxfCodeValue(93)]
		public int Value93 { get; set; }
	}
}
