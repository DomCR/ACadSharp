using CSMath;

namespace ACadSharp.XData
{
	public class ExtendedDataCoordinate : ExtendedDataRecord<XYZ>
	{
		public ExtendedDataCoordinate(XYZ coordinate) : base(DxfCode.ExtendedDataXCoordinate, coordinate) { }
	}
}
