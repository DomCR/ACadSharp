using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A wall, as Tianzheng stores one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The class stores a baseline of two points and how far each face sits from it, and the outline
	/// follows from those three things. Nothing about the shape has to be worked out or looked up:
	/// the wall is a rectangle that runs the length of its baseline, and the drawing shows its two
	/// faces and the two ends.
	/// </para>
	/// <para>
	/// The baseline is the last thing the class writes before its string stream, and where it starts
	/// is not recorded. It is found from its end instead, which is known — see
	/// <see cref="TchFieldMap.TryReadInlinePath"/>. Counting is not an option here even though the
	/// class's fields are fixed, because the property block before them is not: it carries the wall's
	/// construction, so a wall of different materials is a different length and every field after it
	/// moves.
	/// </para>
	/// <para>
	/// The flattened export draws this outline in pieces rather than whole, because it also cuts the
	/// wall open where an opening sits in it. That cutting is not part of the wall: it needs the
	/// openings, which are separate objects and are converted separately. The outline here is the
	/// wall's own, and the pieces the export draws lie on it edge for edge.
	/// </para>
	/// </remarks>
	public sealed class TchWall : TchEntity
	{
		/// <summary>
		/// How many points the baseline holds.
		/// </summary>
		/// <remarks>
		/// A fixed pair rather than a list: the class reads two points and no count, so a wall is
		/// straight as far as the format is concerned.
		/// </remarks>
		public const int BaselinePointCount = 2;

		/// <summary>
		/// How many strings sit between the property block and the class's own fields.
		/// </summary>
		/// <remarks>
		/// A class identifier and three empty values, the same in both walls available. Fixed fields
		/// of the classes above, which is why the reading is anchored to the end of the property block
		/// rather than to the start of the stream.
		/// </remarks>
		public const int StringsAfterPropertyBlock = 4;

		/// <summary>
		/// Where the class's own fields begin when the object carries no property block.
		/// </summary>
		/// <remarks>
		/// Measured, and used only when there is no block to anchor to. Every object in the corpus
		/// carries one, so this is a fallback rather than a rule.
		/// </remarks>
		public const int PropertiesAbsentSkippedStrings = 28;

		/// <summary>
		/// Initialises a wall from the class it was read as.
		/// </summary>
		internal TchWall(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets where the wall's baseline begins.
		/// </summary>
		public XYZ Start { get; private set; }

		/// <summary>
		/// Gets where the wall's baseline ends.
		/// </summary>
		public XYZ End { get; private set; }

		/// <summary>
		/// Gets how far the face on the left of the baseline sits from it.
		/// </summary>
		/// <remarks>
		/// Which of the two stored distances belongs to which side is not established: both are 100
		/// in the only two walls available, and they are the same wall in two orientations. The
		/// assignment follows the order the class writes them in, and the geometry is the same
		/// either way for a wall whose faces are equal, which is every wall here.
		/// </remarks>
		public double LeftOffset { get; private set; }

		/// <inheritdoc cref="LeftOffset"/>
		public double RightOffset { get; private set; }

		/// <summary>
		/// Gets how tall the wall is.
		/// </summary>
		/// <remarks>
		/// Not drawn by a plan view and not part of the outline, which is the two dimensions of the
		/// wall seen from above. Kept because it is the wall's, and a caller raising it into three
		/// dimensions needs it.
		/// </remarks>
		public double Height { get; private set; }

		/// <summary>
		/// Gets how thick the wall is.
		/// </summary>
		public double Thickness => this.LeftOffset + this.RightOffset;

		/// <summary>
		/// Gets a value indicating whether the object described a wall this can convert.
		/// </summary>
		public bool IsConvertible { get; private set; }

		/// <summary>
		/// Gets why <see cref="IsConvertible"/> is false, or null when it is true.
		/// </summary>
		public string Reason { get; private set; }

		/// <summary>
		/// Reads a wall from the bytes a class's own fields were written to.
		/// </summary>
		public static TchWall Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchWall wall = new TchWall(dxfClass);

			wall.StringStreamOffset = FindStringStream(payload, payload.Length * 8L);
			wall.Fill(payload, version);

			return wall;
		}

		/// <inheritdoc/>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (!this.IsConvertible)
			{
				yield break;
			}

			//Each face runs the length of the baseline, offset across it, and the two ends close the
			//outline. The direction across is the baseline turned a quarter turn, so a wall's faces
			//follow it whichever way it runs.
			double dx = this.End.X - this.Start.X;
			double dy = this.End.Y - this.Start.Y;
			double length = Math.Sqrt((dx * dx) + (dy * dy));

			if (length <= 0.0)
			{
				yield break;
			}

			double nx = -dy / length;
			double ny = dx / length;

			LwPolyline outline = new LwPolyline();

			outline.Vertices.Add(new LwPolyline.Vertex(new XY(
				this.Start.X + (this.LeftOffset * nx),
				this.Start.Y + (this.LeftOffset * ny))));
			outline.Vertices.Add(new LwPolyline.Vertex(new XY(
				this.End.X + (this.LeftOffset * nx),
				this.End.Y + (this.LeftOffset * ny))));
			outline.Vertices.Add(new LwPolyline.Vertex(new XY(
				this.End.X - (this.RightOffset * nx),
				this.End.Y - (this.RightOffset * ny))));
			outline.Vertices.Add(new LwPolyline.Vertex(new XY(
				this.Start.X - (this.RightOffset * nx),
				this.Start.Y - (this.RightOffset * ny))));

			outline.IsClosed = true;

			yield return CopyCommonProperties(this, outline);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			//The outline is worked out from the baseline rather than kept, so moving it would mean
			//recomputing the stored fields, which is not what a transform asks for.
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			BoundingBox box = BoundingBox.Null;

			foreach (Entity entity in this.ToStandardEntities())
			{
				box = box.Merge(entity.GetBoundingBox());
			}

			return box;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null || this.StringStreamOffset < 0)
			{
				this.Reason = "the object has no string stream, so its baseline cannot be placed";
				return;
			}

			if (!TchFieldMap.TryReadInlinePath(payload, BaselinePointCount, this.StringStreamOffset, out XYZ[] baseline))
			{
				this.Reason = "no pair of points fills the space before the string stream, so the baseline is not identified";
				return;
			}

			this.Start = baseline[0];
			this.End = baseline[1];

			TchStreamReader reader = TchFieldMap.SkipPropertyBlock(payload, this.StringStreamOffset, PropertiesAbsentSkippedStrings);

			for (int i = 0; i < StringsAfterPropertyBlock; i++)
			{
				reader = TchFieldMap.SkipString(reader);
			}

			this.LeftOffset = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.RightOffset = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);
			this.Height = TchFieldMap.ReadDouble(reader, TgDataEncrypt.ObjectXor);

			if (!isUsable(this.LeftOffset) || !isUsable(this.RightOffset) || !isUsable(this.Height))
			{
				this.Reason = "the distances to the wall's faces do not read as lengths";
				return;
			}

			this.IsConvertible = true;
		}

		/// <summary>
		/// Whether a value read as a length is one.
		/// </summary>
		/// <remarks>
		/// The reading is anchored to a marker and then counted from, and a count that lands in the
		/// wrong place gives numbers of no particular size rather than an error. Requiring positive
		/// and bounded is what turns that into a refusal.
		/// </remarks>
		private static bool isUsable(double value)
		{
			return !double.IsNaN(value) && value > 0.0 && value < 1e6;
		}
	}
}
