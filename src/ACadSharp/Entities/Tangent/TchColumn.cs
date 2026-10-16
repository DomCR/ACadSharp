using ACadSharp.Classes;
using ACadSharp.IO.DWG.Tangent;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// A Tianzheng column (<c>TCH_COLUMN</c>).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The first class worked through the whole path, so it is written out as the shape the rest
	/// should take. Its <c>dwgInFields</c> reads two things and nothing else:
	/// </para>
	/// <code>
	/// readByte(&amp;tag);
	/// TDReadItem(tag, ctx, filer, (AcGePoint3d *)(this + 0x1d8));   // position
	/// TchExport2::ReadPoly2D(filer, (TGGePoly2D *)(this + 0x200));  // outline, unconditionally
	/// </code>
	/// <para>
	/// Both are in the payload in that order, which is why the position is read before the outline
	/// rather than searched for independently. The outline is the shape the column draws, so
	/// conversion is a direct mapping and needs no drawing logic.
	/// </para>
	/// <para>
	/// Verified on the sample drawing: the outline comes out as a 600 by 600 section and the 2600
	/// height appears among the scalars, and those two agree with each other and with the export.
	/// </para>
	/// </remarks>
	public class TchColumn : TchEntity
	{
		internal TchColumn(DxfClass dxfClass)
			: base(dxfClass)
		{
		}

		/// <summary>
		/// Gets the position of the column, read from the point that precedes its outline.
		/// </summary>
		public XYZ Position { get; internal set; } = XYZ.Zero;

		/// <summary>
		/// Gets a value indicating whether a position was read.
		/// </summary>
		public bool HasPosition { get; internal set; }

		/// <summary>
		/// Gets the outline of the column, or null when the payload carried none.
		/// </summary>
		/// <remarks>
		/// This is the shape that gets drawn, so it can be used as it stands.
		/// </remarks>
		public TchPolygon Outline { get; internal set; }

		/// <summary>
		/// Reads a Tianzheng column from the class specific data of an object.
		/// </summary>
		/// <param name="dxfClass">Class the object was read from.</param>
		/// <param name="payload">The class specific data.</param>
		/// <param name="version">Drawing version, for the string encoding.</param>
		/// <returns>The column, with whatever could be read filled in.</returns>
		public static TchColumn Parse(DxfClass dxfClass, byte[] payload, ACadVersion version)
		{
			TchColumn column = new TchColumn(dxfClass)
			{
				RawData = payload,
			};

			column.Fill(payload, version);
			return column;
		}

		/// <inheritdoc/>
		internal override void Fill(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				return;
			}

			readPosition(this, payload, version);

			this.Outline = TchPolygon.FindInPayload(payload, version, out _);
		}

		/// <inheritdoc/>
		/// <remarks>
		/// A column draws its outline, so this is a direct mapping and no geometry is computed.
		/// </remarks>
		public override IEnumerable<Entity> ToStandardEntities()
		{
			if (this.Outline == null)
			{
				yield break;
			}

			//A column section is a closed shape, and the source carries the properties the result
			//should have.
			yield return this.Outline.ToLwPolyline(this, closed: true);
		}

		/// <inheritdoc/>
		public override void ApplyTransform(Transform transform)
		{
			this.Position = transform.ApplyTransform(this.Position);
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			if (this.Outline == null || this.Outline.Vertices.Count == 0)
			{
				return BoundingBox.Null;
			}

			List<XYZ> points = new List<XYZ>(this.Outline.Vertices.Count);

			foreach (TchVertex vertex in this.Outline.Vertices)
			{
				points.Add(new XYZ(vertex.Position.X, vertex.Position.Y, 0.0));
			}

			return BoundingBox.FromPoints(points);
		}

		private static void readPosition(TchColumn column, byte[] payload, ACadVersion version)
		{
			//The point comes first in the payload, before the outline, and is written as a BitDouble
			//like every other position in the format.
			IReadOnlyList<XYZ> positions = TchGeometry.FindPositions(payload, version);

			if (positions.Count > 0)
			{
				column.Position = positions[0];
				column.HasPosition = true;
			}
		}
	}
}
