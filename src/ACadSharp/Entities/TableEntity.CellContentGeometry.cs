using CSMath;

namespace ACadSharp.Entities
{
	public partial class TableEntity
	{
		public class CellContentGeometry
		{
			public XYZ DistanceTopLeft { get; set; }
			public XYZ DistanceCenter { get; set; }
			public double ContentWidth { get; set; }
			public double ContentHeight { get; set; }
			public double Width { get; set; }
			public double Height { get; set; }
			public int Flags { get; set; }
		}
	}
}
