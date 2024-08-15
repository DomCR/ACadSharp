﻿using ACadSharp.Attributes;
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

			/// <summary>
			/// Segments in MLineStyle definition
			/// </summary>
			[DxfCodeValue(DxfReferenceType.Count, 73)]
			public List<Segment> Segments { get; set; } = new List<Segment>();

			public Vertex Clone()
			{
				Vertex clone = (Vertex)this.MemberwiseClone();

				clone.Segments.Clear();
				foreach (var item in this.Segments)
				{
					var seg = new Segment();
					seg.Parameters.AddRange(item.Parameters);
					seg.AreaFillParameters.AddRange(item.AreaFillParameters);
					clone.Segments.Add(seg);
				}

				return clone;
			}

			public class Segment
			{
				/// <summary>
				/// Element parameters
				/// </summary>
				[DxfCodeValue(DxfReferenceType.Count, 74)]
				[DxfCollectionCodeValue(41)]
				public List<double> Parameters { get; set; } = new List<double>();

				/// <summary>
				/// Area fill parameters
				/// </summary>
				[DxfCodeValue(DxfReferenceType.Count, 75)]
				[DxfCollectionCodeValue(42)]
				public List<double> AreaFillParameters { get; set; } = new List<double>();

			}
		}
	}
}
