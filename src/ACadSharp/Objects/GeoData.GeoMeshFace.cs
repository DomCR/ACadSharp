using ACadSharp.Attributes;

namespace ACadSharp.Objects
{

	public partial class GeoData
	{
		public class GeoMeshFace
		{
			/// <summary>
			/// Point index for face 1.
			/// </summary>
			[DxfCodeValue(97)]
			public int Index1 { get; set; }

			/// <summary>
			/// Point index for face 2.
			/// </summary>
			[DxfCodeValue(98)]
			public int Index2 { get; set; }

			/// <summary>
			/// Point index for face 3.
			/// </summary>
			[DxfCodeValue(99)]
			public int Index3 { get; set; }
		}
	}
}
