using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	public class MLine : Entity
	{
		public override ObjectType ObjectType => ObjectType.MLINE;

		public override string ObjectName => DxfFileToken.EntityMLine;

		//100	Subclass marker(AcDbMline)

		/// <summary>
		/// String of up to 32 characters.The name of the style used for this mline.An entry for this style must exist in the MLINESTYLE dictionary.
		/// </summary>
		/// <remarks>
		/// Do not modify this field without also updating the associated entry in the MLINESTYLE dictionary
		/// </remarks>
		//340	Pointer-handle/ID of MLINESTYLE object
		[DxfCodeValue(2)]
		public MLStyle Style { get; set; }

		/// <summary>
		/// Scale factor
		/// </summary>
		[DxfCodeValue(40)]
		public double ScaleFactor { get; set; }

		/// <summary>
		/// Justification
		/// </summary>
		[DxfCodeValue(70)]
		public MLineJustification Justification { get; set; }

		/// <summary>
		/// Flags
		/// </summary>
		[DxfCodeValue(71)]
		public MLineFlags Flags { get; set; }

		/// <summary>
		/// Start point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ StartPoint { get; set; }

		/// <summary>
		/// Extrusion direction
		/// </summary>
		[DxfCodeValue(210, 220, 230)]
		public XYZ Extrusion { get; set; } = XYZ.AxisZ;

		//72	Number of vertices

		public List<Vertex> Vertices { get; set; } = new List<Vertex>();

		public MLine() : base() { }

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

			public List<Segment> Segments { get; set; } = new List<Segment>();

			public class Segment
			{
				//74	Number of parameters for this element (repeats for each element in segment)
				/// <summary>
				/// Element parameters
				/// </summary>
				[DxfCodeValue(41)]
				public List<double> Parameters { get; set; } = new List<double>();

				//75	Number of area fill parameters for this element(repeats for each element in segment)

				/// <summary>
				/// Area fill parameters
				/// </summary>
				[DxfCodeValue(42)]
				public List<double> AreaFillParameters { get; set; } = new List<double>();
			}
		}
	}
}
