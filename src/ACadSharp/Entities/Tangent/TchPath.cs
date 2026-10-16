using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A Tianzheng object whose geometry is a run of points.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Where <see cref="TchOutline"/> closes a shape, these draw a path through their points: an
	/// index pointer is a line between two, a section mark a line, a multi leader two segments, an
	/// axis label a line. The points are written as consecutive coordinates Y then X, 64 bits apart,
	/// with nothing in front of the run and nothing marking where it ends, so it is found by taking
	/// the longest sequence that reads as coordinates.
	/// </para>
	/// <para>
	/// Confirmed against the flattened export on the sample drawing: of fifteen points across seven
	/// objects, thirteen land exactly on an endpoint of a line or polyline the export draws. The two
	/// that do not are the first point of an index pointer, where the export's shape starts some
	/// hundreds of units along the same line, and both points of one composing symbol.
	/// </para>
	/// <para>
	/// The points are the path and nothing else is derived, so conversion is a direct mapping. What
	/// the export does with them afterwards — splitting a line into two where it crosses something,
	/// for instance — is the export's business.
	/// </para>
	/// </remarks>
	public class TchPath : TchEntity
	{
		internal TchPath(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the points the path is drawn through, in order.
		/// </summary>
		public IReadOnlyList<XY> Points { get; internal set; } = new List<XY>();

		/// <summary>
		/// Gets the bit offset the run was found at, for diagnosis.
		/// </summary>
		public long BitOffset { get; internal set; } = -1;

		/// <summary>
		/// Reads a Tianzheng path object from the class specific data of an object.
		/// </summary>
		/// <param name="dxfClass">Class the object was read from.</param>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		/// <returns>The object, with whatever could be read filled in.</returns>
		public static TchPath Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchPath path = new TchPath(dxfClass)
			{
				RawData = payload,
			};

			path.Fill(payload, version);
			return path;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				return;
			}

			this.Points = TchGeometry.FindPoints(payload, version, out long offset);
			this.BitOffset = offset;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// An open polyline through the points, carrying the object's properties.
		/// </remarks>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (this.Points.Count < 2)
			{
				yield break;
			}

			LwPolyline polyline = new LwPolyline
			{
				IsClosed = false,
			};

			foreach (XY point in this.Points)
			{
				polyline.Vertices.Add(new LwPolyline.Vertex(point));
			}

			polyline.MatchProperties(this);

			yield return polyline;
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			List<XY> moved = new List<XY>(this.Points.Count);

			foreach (XY point in this.Points)
			{
				XYZ result = transform.ApplyTransform(new XYZ(point.X, point.Y, 0.0));
				moved.Add(new XY(result.X, result.Y));
			}

			this.Points = moved;
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			if (this.Points.Count == 0)
			{
				return BoundingBox.Null;
			}

			List<XYZ> points = new List<XYZ>(this.Points.Count);

			foreach (XY point in this.Points)
			{
				points.Add(new XYZ(point.X, point.Y, 0.0));
			}

			return BoundingBox.FromPoints(points);
		}
	}
}
