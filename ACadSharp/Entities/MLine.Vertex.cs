using ACadSharp.Attributes;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public partial class MLine
	{
		public class Vertex
		{
			/// <summary>
			/// Vertex coordinates
			/// </summary>
			[DxfCodeValue(11, 21, 31)]
			public XYZ Position { get; set; }

			/// <summary>
			/// Direction vector of segment starting at this vertex
			/// </summary>
			[DxfCodeValue(12, 22, 32)]
			public XYZ Direction { get; set; }

			/// <summary>
			/// Direction vector of miter at this vertex 
			/// </summary>
			[DxfCodeValue(13, 23, 33)]
			public XYZ Miter { get; set; }

			//73	Number of elements in MLINESTYLE definition
			[DxfCodeValue(DxfReferenceType.Count, 73)]
			public List<Segment> Segments { get; set; } = new List<Segment>();

			public class Segment
			{
				//74	Number of parameters for this element (repeats for each element in segment)
				/// <summary>
				/// Element parameters
				/// </summary>
				[DxfCodeValue(DxfReferenceType.Count, 74)]
				//41	Element parameters(repeats based on previous code 74)
				public List<double> Parameters { get; set; } = new List<double>();

				/// <summary>
				/// Area fill parameters
				/// </summary>
				//75	Number of area fill parameters for this element(repeats for each element in segment)
				//42	Area fill parameters(repeats based on previous code 75)
				[DxfCodeValue(DxfReferenceType.Count, 75)]
				public List<double> AreaFillParameters { get; set; } = new List<double>();

			}
		}
	}
}
