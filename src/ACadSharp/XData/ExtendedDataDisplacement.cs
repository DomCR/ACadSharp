using CSMath;

namespace ACadSharp.XData
{
	public class ExtendedDataDisplacement : ExtendedDataRecord<XYZ>
	{
		public ExtendedDataDisplacement(XYZ displacement) : base(DxfCode.ExtendedDataWorldXDisp, displacement) { }
	}
}
