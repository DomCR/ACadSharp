using ACadSharp.Attributes;
using CSMath;
using CSMath.Extensions;
using System;
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

			/// <inheritdoc/>
			public void ApplyTransform(Transform transform)
			{
				this.Position = transform.ApplyTransform(Position);

				// [PATCH] Direction and Miter are unit direction vectors of the segments/miters, not
				// locations: AutoCAD TransformBy rotates them (linear part, never the translation)
				// and keeps them normalized: (0, 1) rotated by -0.92 becomes (0.7956, 0.6058),
				// not (0.7940, 0.6046) (no 0.998 scale on the unit vector).
				Matrix3 linear = new Matrix3(transform.Matrix);
				XYZ newDirection = linear * this.Direction;
				this.Direction = newDirection.GetLength() > MathHelper.Epsilon ? newDirection.Normalize() : newDirection;
				XYZ newMiter = linear * this.Miter;
				this.Miter = newMiter.GetLength() > MathHelper.Epsilon ? newMiter.Normalize() : newMiter;

				// Segments keep the transform's scale so the rendered widths follow the mline.
				// [PATCH] Uniform scale = average of the decomposed components;
				// transform.Scale.GetLength() would give s·√3 and blow the offsets up
				// (-20 → -34.5717 instead of -19.96 for a 0.998 scale).
				XYZ vsc = transform.Scale;
				double scaleFactor = (Math.Abs(vsc.X) + Math.Abs(vsc.Y) + Math.Abs(vsc.Z)) / 3.0;
				foreach (var segment in Segments)
				{
					segment.ApplyScale(scaleFactor);
				}
			}

			/// <inheritdoc/>
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

				/// <inheritdoc/>
				public void ApplyScale(double scaleFactor)
				{
					for (int i = this.Parameters.Count - 1; i >= 0; i--)
					{
						this.Parameters[i] = this.Parameters[i] * scaleFactor;
					}

					for (int i = this.AreaFillParameters.Count - 1; i >= 0; i--)
					{
						this.AreaFillParameters[i] = this.AreaFillParameters[i] * scaleFactor;
					}
				}
			}
		}
	}
}
