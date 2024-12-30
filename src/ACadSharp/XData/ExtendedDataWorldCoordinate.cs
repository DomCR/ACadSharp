using CSMath;

namespace ACadSharp.XData
{
	public class ExtendedDataWorldCoordinate : ExtendedDataRecord<XYZ>
	{
		public ExtendedDataWorldCoordinate(XYZ coordinate) : base(DxfCode.ExtendedDataWorldXCoordinate, coordinate) { }
	}
}
