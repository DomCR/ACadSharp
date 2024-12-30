using CSMath;

namespace ACadSharp.XData
{
	public class ExtendedDataDirection : ExtendedDataRecord<XYZ>
	{
		public ExtendedDataDirection(XYZ direction) : base(DxfCode.ExtendedDataWorldXDir, direction) { }
	}
}
