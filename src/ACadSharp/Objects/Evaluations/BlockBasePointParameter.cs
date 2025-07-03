using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;

using CSMath;


namespace ACadSharp.Objects.Evaluations {

	//BLOCKBASEPOINTPARAMETER
	//	AcDbEvalExpr
	//	AcDbBlockElement
	//  AcDbBlockParameter
	//	AcDbBlock1PtParameter
	//  AcDbBlockBasepointParameter
	//1011  BD  0.0
	//1021	BD	0.0
	//1031	BD	0.0
	//1012	BD	0.0
	//1022	BD	0.0
	//1032	BD	0.0

	[DxfSubClass(DxfSubclassMarker.BlockBasePointParameter)]
	public class BlockBasePointParameter : Block1PtParameter {

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockBasePointParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockBasePointParameter;


		[DxfCodeValue(1011, 1021, 1031)]
		public XYZ Point1010 { get; set; }


		[DxfCodeValue(1012, 1022, 1032)]
		public XYZ Point1012 { get; set; }
	}
}
