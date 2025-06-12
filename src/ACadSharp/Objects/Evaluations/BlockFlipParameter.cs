using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ACadSharp.Attributes;

using CSMath;

namespace ACadSharp.Objects.Evaluations {

	[DxfSubClass(DxfSubclassMarker.BlockFlipParameter)]
	public class BlockFlipParameter : Block2PtParameter {

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockFlipParameter;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockFlipParameter;


		//305	S	Umkehrstatus1
		public string Caption { get; set; }

		//306	S	
		public string Description { get; set; }

		//307	S	Nicht umgekehrt
		public string BaseStateName { get; set; }

		//308	S	Umgekehrt
		public string FlippedStateName { get; set; }

		//1012	BD	-17.54898980138068
		//1022	BD	108.2007882010403
		//1032	BD	0.0
		public XYZ CaptionLocation { get; set; }

		//309	S	UpdatedFlip
		public string Caption309 { get; set; }

		//96	BL	7
		public int Value96 { get; set; }

		//1001	S	ACAUTHENVIRON
		public string Caption1001 { get; set; }

		//1010	BD	23.1286989982713
		//1020	BD	27.67408655057769
		//1030	BD	0.0
		public XYZ Point1010 { get; set; }
	}
}


/*
0	S	BLOCKFLIPPARAMETER
5	H	7F959
330	H	7F953
100	S	AcDbEvalExpr
90	BL	6
98	BL	33
99	BL	329
100	S	AcDbBlockElement
300	S	Umkehren
98	BL	33
99	BL	329
1071	BL	0
100	S	AcDbBlockParameter
280	BS	1
281	BS	0
100	S	AcDbBlock2PtParameter
1010	BD	-0.0000000001218723
1020	BD	50.80278554897086
1030	BD	0.0
1011	BD	-0.0000000001218723
1021	BD	109.8133196764683
1031	BD	0.0
170	BS	4
91	BL	7
91	BL	0
91	BL	0
91	BL	0
171	BS	0
172	BS	0
173	BS	0
174	BS	0
177	BS	0
100	S	AcDbBlockFlipParameter
305	S	Umkehrstatus1
306	S	
307	S	Nicht umgekehrt
308	S	Umgekehrt
1012	BD	-17.54898980138068
1022	BD	108.2007882010403
1032	BD	0.0
309	S	UpdatedFlip
96	BL	7
1001	S	ACAUTHENVIRON
1010	BD	23.1286989982713
1020	BD	27.67408655057769
1030	BD	0.0

*/