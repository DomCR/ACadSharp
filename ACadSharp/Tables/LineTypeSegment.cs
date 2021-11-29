using CSMath;

namespace ACadSharp.Tables
{
	public class LineTypeSegment
	{
		public double Length { get; set; }
		public LinetypeShapeFlags Shapeflag { get; set; }
		public XY Offset { get; set; }
		public double Rotation { get; internal set; }
		public double Scale { get; internal set; }
	}
}