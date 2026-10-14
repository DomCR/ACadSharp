using ACadSharp.Entities;
using CSMath;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// A vertex of a <see cref="TchPolygon"/>.
	/// </summary>
	public readonly struct TchVertex
	{
		/// <summary>Gets the position of the vertex.</summary>
		public XY Position { get; }

		/// <summary>
		/// Gets the bulge of the edge that starts at this vertex, which is the tangent of a quarter
		/// of the arc's included angle. Zero means a straight segment.
		/// </summary>
		public double Bulge { get; }

		internal TchVertex(double x, double y, double bulge)
		{
			this.Position = new XY(x, y);
			this.Bulge = bulge;
		}

		/// <inheritdoc/>
		public override string ToString() => $"({this.Position.X}, {this.Position.Y}) bulge {this.Bulge}";
	}

	/// <summary>
	/// A polygon as Tianzheng stores the outline of an object.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The layout matches <c>TchExport2::ReadPoly2D</c> (<c>0x1C4C6880</c>): a flags byte, a vertex
	/// count, then that many vertices, and finally an object id. Every value is read through a plain
	/// filer call, so the outline is stored in the clear and no part of the obfuscation layer
	/// applies to it.
	/// </para>
	/// <code>
	/// flags      byte
	/// count      byte
	/// repeat count:
	///     X      64-bit little endian double
	///     Y      64-bit little endian double
	///     bulge  BitDouble: 2-bit selector, 0 reads a further double, 1 is 1.0, 2 is 0.0
	/// </code>
	/// <para>
	/// The vertex stride is therefore 130 bits for a straight edge and 194 bits for an arc, which is
	/// what makes the layout self-delimiting.
	/// </para>
	/// <para>
	/// Verified on a <c>TCH_COLUMN</c> in the sample drawing: the recovered four vertices form the
	/// 600 by 600 section of the column, and their coordinates land on the same grid as the walls in
	/// Tianzheng's own flattened export of the same drawing.
	/// </para>
	/// </remarks>
	public sealed class TchPolygon
	{
		/// <summary>Bit size of a vertex whose edge is straight.</summary>
		public const int StraightVertexBits = 130;

		/// <summary>Bit size of a vertex whose edge is an arc.</summary>
		public const int ArcVertexBits = 194;

		private readonly List<TchVertex> _vertices = new List<TchVertex>();

		private TchPolygon(byte flags)
		{
			this.Flags = flags;
		}

		/// <summary>Gets the flags byte that precedes the vertex list.</summary>
		public byte Flags { get; }

		/// <summary>Gets the vertices in the order they are stored.</summary>
		public IReadOnlyList<TchVertex> Vertices => _vertices;

		/// <summary>Gets a value indicating whether the outline was read in full.</summary>
		public bool IsComplete { get; private set; }

		/// <summary>
		/// Reads a polygon at the current position of <paramref name="reader"/>.
		/// </summary>
		internal static TchPolygon Read(IDwgStreamReader reader)
		{
			byte flags = reader.ReadByte();
			byte count = reader.ReadByte();

			TchPolygon polygon = new TchPolygon(flags);

			for (int i = 0; i < count; i++)
			{
				double x = reader.ReadDouble();
				double y = reader.ReadDouble();
				double bulge = reader.ReadBitDouble();

				polygon._vertices.Add(new TchVertex(x, y, bulge));
			}

			polygon.IsComplete = true;
			return polygon;
		}

		/// <summary>
		/// Searches a class specific payload for the outline Tianzheng stored for an object.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The outline does not sit at a fixed offset: it follows the fields the class wrote before
		/// it, and for some classes it is only present when a flag is set. The search therefore tries
		/// every bit offset and keeps a candidate only when every vertex decodes to a coordinate of a
		/// plausible magnitude, which is a strong enough filter that the correct offset is normally
		/// the only one that survives.
		/// </para>
		/// <para>
		/// Returns null when the object carries no outline, which is the normal case for a class with
		/// no filled section, such as a text object.
		/// </para>
		/// </remarks>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="bitOffset">Receives the bit offset the outline was found at.</param>
		public static TchPolygon FindInPayload(byte[] payload, ACadVersion version, out long bitOffset)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			bitOffset = -1;

			if (payload.Length < 32)
			{
				return null;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				TchPolygon best = null;
				long bestOffset = -1;

				for (long offset = 0; offset < limit - StraightVertexBits; offset++)
				{
					TchPolygon candidate = tryReadAt(reader, offset, limit);
					if (candidate == null)
					{
						continue;
					}

					//Keep the longest: a match that starts inside a real outline yields a shorter
					//run of that outline's vertices, so the longest candidate is the whole one.
					if (best == null || candidate._vertices.Count > best._vertices.Count)
					{
						best = candidate;
						bestOffset = offset;
					}
				}

				bitOffset = bestOffset;
				return best;
			}
		}

		private static TchPolygon tryReadAt(IDwgStreamReader reader, long offset, long limit)
		{
			try
			{
				return tryReadAtCore(reader, offset, limit);
			}
			catch (Exception)
			{
				//A misaligned offset produces values the reader rejects, which is the expected
				//outcome for all but one of the offsets tried.
				return null;
			}
		}

		private static TchPolygon tryReadAtCore(IDwgStreamReader reader, long offset, long limit)
		{
			reader.SetPositionInBits(offset);

			if (reader.PositionInBits() + 16 > limit)
			{
				return null;
			}

			byte flags = reader.ReadByte();
			byte count = reader.ReadByte();

			if (count < 2 || count > 64)
			{
				return null;
			}

			TchPolygon polygon = new TchPolygon(flags);

			for (int i = 0; i < count; i++)
			{
				if (reader.PositionInBits() + StraightVertexBits > limit)
				{
					return null;
				}

				double x = reader.ReadDouble();
				double y = reader.ReadDouble();
				double bulge = reader.ReadBitDouble();

				if (!isPlausible(x) || !isPlausible(y))
				{
					return null;
				}

				polygon._vertices.Add(new TchVertex(x, y, bulge));
			}

			polygon.IsComplete = true;
			return polygon;
		}

		/// <summary>
		/// Converts the outline into a lightweight polyline, which is how a consumer draws it.
		/// </summary>
		/// <remarks>
		/// The two representations line up exactly: a vertex carries a position and a bulge in both,
		/// and the bulge has the same meaning, so the shape survives the conversion without
		/// approximation. Nothing is lost except the flags byte.
		/// </remarks>
		/// <param name="closed">Whether to mark the polyline closed, which is the usual case for an
		/// object outline but not for every class.</param>
		/// <returns>A polyline with one vertex per outline vertex.</returns>
		public LwPolyline ToLwPolyline(bool closed = true)
		{
			LwPolyline polyline = new LwPolyline();
			polyline.IsClosed = closed;

			foreach (TchVertex vertex in _vertices)
			{
				polyline.Vertices.Add(new LwPolyline.Vertex(vertex.Position)
				{
					Bulge = vertex.Bulge
				});
			}

			return polyline;
		}

		private static bool isPlausible(double value)
		{
			//A drawing coordinate. The bound is loose on purpose: it only has to reject the bit
			//patterns that a misaligned read produces, which are almost always astronomically large
			//or denormal.
			return !double.IsNaN(value)
				&& !double.IsInfinity(value)
				&& Math.Abs(value) > 1.0
				&& Math.Abs(value) < 1.0e7;
		}
	}
}
