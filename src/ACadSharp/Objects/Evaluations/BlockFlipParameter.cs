using ACadSharp.Attributes;

using CSMath;

namespace ACadSharp.Objects.Evaluations;

[DxfSubClass(DxfSubclassMarker.BlockFlipParameter)]
public class BlockFlipParameter : Block2PtParameter
{
	//307	S	Nicht umgekehrt
	public string BaseStateName { get; set; }

	//305	S	Umkehrstatus1
	public string Caption { get; set; }

	//1001	S	ACAUTHENVIRON
	public string Caption1001 { get; set; }

	//309	S	UpdatedFlip
	public string Caption309 { get; set; }

	//1012	BD	-17.54898980138068
	//1022	BD	108.2007882010403
	//1032	BD	0.0
	public XYZ CaptionLocation { get; set; }

	//306	S
	public string Description { get; set; }

	//308	S	Umgekehrt
	public string FlippedStateName { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectBlockFlipParameter;

	//1010	BD	23.1286989982713
	//1020	BD	27.67408655057769
	//1030	BD	0.0
	public XYZ Point1010 { get; set; }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.BlockFlipParameter;

	//96	BL	7
	public int Value96 { get; set; }
}