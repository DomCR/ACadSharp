using CSMath;
using System;
using System.Collections.Generic;
using System.IO;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Reads the geometric primitives Tianzheng stores on an object but that are not an outline.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A class that has no filled section, a wall for instance, still stores the curve it is built
	/// on. That curve is a pair of 3-D points, and a point is written as three <c>BitDouble</c>
	/// values rather than as three raw doubles, so it is variable length: the 2-bit selector in
	/// front of each coordinate is 0 when eight bytes follow, 1 for 1.0 and 2 for 0.0. A point of a
	/// horizontal wall therefore occupies 134 bits, not the 192 a fixed layout would take.
	/// </para>
	/// <para>
	/// That variable length is why a search for a 24-byte point finds nothing, and it is the reason
	/// this helper exists rather than a fixed offset table.
	/// </para>
	/// </remarks>
	public static class TchGeometry
	{
		/// <summary>Bit size of a point whose X and Y are both stored in full and whose Z is zero.</summary>
		public const int PointBits = 134;

		/// <summary>
		/// Smallest magnitude accepted for a horizontal coordinate.
		/// </summary>
		/// <remarks>
		/// The filter has to reject the values a misaligned read produces, and those cluster near
		/// zero: reading a tag or a short as a double yields values of a few units. Architectural
		/// drawings are dimensioned in millimetres, so requiring three digits keeps the real
		/// coordinates and drops that noise. A drawing in metres would need this lowered.
		/// </remarks>
		public const double MinimumCoordinate = 100.0;

		/// <summary>
		/// Reads the 2-D position that a class stores at a recorded offset.
		/// </summary>
		/// <remarks>
		/// For the classes in <see cref="TchEntityLayout"/> the position is a bare pair of
		/// coordinates with nothing around it to recognise, so it is read at its offset rather than
		/// searched for. Use <see cref="TryFindPoint"/> for the classes that store a curve.
		/// </remarks>
		/// <param name="dxfName">DXF name of the class, for example <c>TCH_ELEVATION</c>.</param>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="position">Receives the position that was read.</param>
		/// <returns>True when the class has a recorded position and the payload is long enough.</returns>
		public static bool TryReadPosition(string dxfName, byte[] payload, ACadVersion version, out XY position)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			position = XY.Zero;

			if (!TchEntityLayout.TryGetPositionOffset(dxfName, out int xOffset, out int yOffset))
			{
				return false;
			}

			if ((long)payload.Length * 8L < yOffset + 64)
			{
				return false;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);

				reader.SetPositionInBits(xOffset);
				double x = reader.ReadDouble();

				reader.SetPositionInBits(yOffset);
				double y = reader.ReadDouble();

				position = new XY(x, y);
				return true;
			}
		}

		/// <summary>
		/// Searches a class specific payload for the first well formed 3-D point.
		/// </summary>
		/// <remarks>
		/// The point does not sit at a fixed offset, so every bit offset is tried and a candidate is
		/// accepted only when all three coordinates decode to plausible values. On a wall this
		/// returns the start of the curve the wall is built on.
		/// </remarks>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="point">Receives the point that was found.</param>
		/// <param name="bitOffset">Receives the bit offset the point was found at.</param>
		/// <returns>True when a point was found.</returns>
		public static bool TryFindPoint(byte[] payload, ACadVersion version, out XYZ point, out long bitOffset)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			point = XYZ.Zero;
			bitOffset = -1;

			if (payload.Length < 32)
			{
				return false;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				for (long offset = 0; offset < limit - PointBits; offset++)
				{
					if (!tryReadPoint(reader, offset, limit, out XYZ candidate))
					{
						continue;
					}

					point = candidate;
					bitOffset = offset;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Searches a class specific payload for the positions an object carries.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is the general form of the shape <see cref="TchText"/> and
		/// <see cref="TchEntityLayout"/> were special cases of, and it is present in every class in
		/// the corpus. A position is three raw doubles: Y, then X sixty-eight bits later, then Z
		/// twenty-four bits after that. The two gaps differ because the fields in front of them do,
		/// and they hold for every class measured.
		/// </para>
		/// <para>
		/// A wall carries two of these, its centre line; a text object one; a window one; a stair
		/// one. Nothing marks them, so the search takes every offset that produces a plausible
		/// position and returns them in order. Z is required to be small, which is what keeps a
		/// misaligned read from passing: an X or Y read at the wrong bit is almost always
		/// astronomical, but a Z read at the wrong bit can look ordinary unless it is bounded.
		/// </para>
		/// <para>
		/// Each result is confirmed against the flattened export elsewhere in this library's tests;
		/// for the wall the two positions are the ends of the centre line the export draws, and for
		/// a stair the X lands on the first line of the flight.
		/// </para>
		/// </remarks>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <returns>The positions in the order they appear; empty when there are none.</returns>
		public static IReadOnlyList<XYZ> FindPositions(byte[] payload, ACadVersion version)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			List<XYZ> positions = new List<XYZ>();

			if (payload.Length < 32)
			{
				return positions;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				for (long offset = 0; offset + XOffset + CoordinateBits <= limit; offset++)
				{
					if (tryReadPosition(reader, offset, limit, out XYZ position))
					{
						positions.Add(position);
					}
				}
			}

			return positions;
		}

		/// <summary>Bit gap between Y and X within a stored position.</summary>
		public const int XOffset = 68;

		/// <summary>Bit gap between X and Z within a stored position.</summary>
		public const int ZOffset = 24;

		private static bool tryReadPosition(IDwgStreamReader reader, long offset, long limit, out XYZ position)
		{
			position = XYZ.Zero;

			try
			{
				reader.SetPositionInBits(offset);
				double y = reader.ReadDouble();

				reader.SetPositionInBits(offset + XOffset);
				double x = reader.ReadDouble();

				reader.SetPositionInBits(offset + XOffset + ZOffset);
				double z = reader.ReadDouble();

				if (!isPlausible(x) || !isPlausible(y) || !isFinite(z) || Math.Abs(z) > 1.0e5)
				{
					return false;
				}

				position = new XYZ(x, y, z);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Searches a class specific payload for a run of 2-D points.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Several classes store the points they are drawn from as a bare run of coordinates with
		/// nothing in front of them: an index pointer is a line between two points, a section mark
		/// carries its three, and so on. Each point is written Y first then X, 64 bits apart, and
		/// the next point begins immediately after, so the whole run is a sequence of plausible
		/// coordinates at a 64 bit stride.
		/// </para>
		/// <para>
		/// The longest such run wins. A run of one coordinate is not accepted, because a single
		/// plausible double carries too little information to call it a point.
		/// </para>
		/// <para>
		/// Verified on an index pointer in the sample drawing, whose two points come out as
		/// (45665.2489, 29070.1762) and (46469.9009, 26579.0959), while the flattened export
		/// draws the line from (45665.25, 29070.18) to (46469.90, 26579.10).
		/// </para>
		/// </remarks>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="bitOffset">Receives the bit offset of the first Y coordinate.</param>
		/// <returns>The points in order; empty when the payload holds no such run.</returns>
		public static IReadOnlyList<XY> FindPoints(byte[] payload, ACadVersion version, out long bitOffset)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			bitOffset = -1;

			List<XY> best = new List<XY>();

			if (payload.Length < 32)
			{
				return best;
			}

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				for (long offset = 0; offset < limit - 2L * CoordinateBits; offset++)
				{
					List<XY> run = tryReadRun(reader, offset, limit);
					if (run != null && run.Count > best.Count)
					{
						best = run;
						bitOffset = offset;
					}
				}
			}

			return best;
		}

		private static List<XY> tryReadRun(IDwgStreamReader reader, long offset, long limit)
		{
			List<XY> points = new List<XY>();

			try
			{
				reader.SetPositionInBits(offset);

				while (reader.PositionInBits() + 2L * CoordinateBits <= limit)
				{
					double y = reader.ReadDouble();
					double x = reader.ReadDouble();

					if (!isPlausible(x) || !isPlausible(y))
					{
						break;
					}

					points.Add(new XY(x, y));
				}
			}
			catch (Exception)
			{
				return null;
			}

			//Two coordinates are one point and tell us nothing; a run is worth having from two
			//points, that is four coordinates, upwards.
			return points.Count >= 2 ? points : null;
		}

		/// <summary>Bit size of one coordinate of a stored point.</summary>
		public const int CoordinateBits = 64;

		/// <summary>
		/// Searches a class specific payload for a point whose X and Y match the given values.
		/// </summary>
		/// <param name="payload">The class specific data of the object.</param>
		/// <param name="version">Drawing version used by the stream reader.</param>
		/// <param name="x">Expected X.</param>
		/// <param name="y">Expected Y.</param>
		/// <param name="tolerance">Accepted difference, in drawing units.</param>
		/// <param name="bitOffset">Receives the bit offset the point was found at.</param>
		public static bool ContainsPoint(byte[] payload, ACadVersion version, double x, double y, double tolerance, out long bitOffset)
		{
			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			return findMatching(payload, version, x, y, tolerance, out bitOffset);
		}

		private static bool findMatching(byte[] payload, ACadVersion version, double x, double y, double tolerance, out long bitOffset)
		{
			bitOffset = -1;

			using (MemoryStream stream = new MemoryStream(payload, writable: false))
			{
				IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(version, stream, resetPositon: true);
				long limit = (long)payload.Length * 8L;

				for (long offset = 0; offset < limit - PointBits; offset++)
				{
					if (!tryReadPoint(reader, offset, limit, out XYZ candidate))
					{
						continue;
					}

					if (Math.Abs(candidate.X - x) <= tolerance && Math.Abs(candidate.Y - y) <= tolerance)
					{
						bitOffset = offset;
						return true;
					}
				}
			}

			return false;
		}

		private static bool tryReadPoint(IDwgStreamReader reader, long offset, long limit, out XYZ point)
		{
			point = XYZ.Zero;

			try
			{
				reader.SetPositionInBits(offset);

				if (reader.PositionInBits() + PointBits > limit)
				{
					return false;
				}

				double x = reader.ReadBitDouble();
				double y = reader.ReadBitDouble();
				double z = reader.ReadBitDouble();

				if (!isPlausible(x) || !isPlausible(y) || !isFinite(z) || Math.Abs(z) > 1.0e7)
				{
					return false;
				}

				point = new XYZ(x, y, z);
				return true;
			}
			catch (Exception)
			{
				//A misaligned offset produces values the reader rejects.
				return false;
			}
		}

		private static bool isPlausible(double value)
		{
			return isFinite(value) && Math.Abs(value) > MinimumCoordinate && Math.Abs(value) < 1.0e7;
		}

		private static bool isFinite(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value);
		}
	}
}
