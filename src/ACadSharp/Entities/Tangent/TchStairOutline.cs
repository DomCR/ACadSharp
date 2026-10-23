using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities.Tangent
{
	/// <summary>
	/// The outline a Tianzheng stair is drawn as, worked out from its dimensions.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The rectangular and the parallel stair store the same six things — a width, two offsets that
	/// place the handrail, how far the first tread line sits below the stair, the distance between
	/// tread lines, and how many there are — and differ only in how many flights they draw and in
	/// how many other values they write before those. So the outline lives here and each class reads
	/// its own fields and hands them over.
	/// </para>
	/// <para>
	/// What separates the two is the width. Both draw two flights against the outer edges; a stair
	/// wide enough for a third draws one down the middle, with a well on either side of it, and one
	/// that is not draws a single well between the two. The test is whether the half width is more
	/// than twice a flight, and for the rectangular stair it is not — which is why its one well
	/// reaches from flight to flight while the parallel stair's two do not.
	/// </para>
	/// </remarks>
	internal static class TchStairOutline
	{
		/// <summary>
		/// How wide the handrail's band is, drawn inside each outer flight.
		/// </summary>
		/// <remarks>
		/// Not a field of either class: both stairs in the sample draw it a hundred wide against a
		/// scale of a hundred, and the classes store where it starts rather than how wide it is. A
		/// band of a hundred millimetres at full size is what a handrail is drawn at.
		/// </remarks>
		public const double HandrailWidth = 100.0;

		/// <summary>
		/// Builds the outline of a stair.
		/// </summary>
		/// <param name="owner">The object the lines belong to, for its layer and colour.</param>
		/// <param name="position">The corner of the stair's top step.</param>
		/// <param name="width">The total width, across every flight and well.</param>
		/// <param name="handrailOffset">How far the handrail's band sits inside each outer edge.</param>
		/// <param name="handrailBand">How far a flight's edge sits inside the handrail's band.</param>
		/// <param name="firstStepOffset">How far the first tread line sits below the position.</param>
		/// <param name="treadDepth">The distance between one tread line and the next.</param>
		/// <param name="treadLines">How many tread lines the run holds.</param>
		/// <returns>The lines, top edge first.</returns>
		public static IEnumerable<Entity> Lines(
			TchEntity owner,
			XYZ position,
			double width,
			double handrailOffset,
			double handrailBand,
			double firstStepOffset,
			double treadDepth,
			int treadLines)
		{
			double flight = handrailOffset - handrailBand;
			double half = width / 2.0;
			double run = firstStepOffset + ((treadLines - 1) * treadDepth);

			//The well runs past the flights at both ends, and a flight's edge is one band inside the
			//handrail's line, so the well's ends are a band outside the first and last tread lines.
			double wellTop = firstStepOffset - handrailBand;
			double wellBottom = run + handrailBand;

			Line line(double x1, double y1, double x2, double y2)
			{
				return TchEntity.CopyCommonProperties(owner, new Line
				{
					StartPoint = new XYZ(position.X + x1, position.Y + y1, position.Z),
					EndPoint = new XYZ(position.X + x2, position.Y + y2, position.Z),
				});
			}

			yield return line(0.0, 0.0, width, 0.0);
			yield return line(0.0, 0.0, 0.0, -run);
			yield return line(width, 0.0, width, -run);

			List<double[]> flights = new List<double[]>
			{
				new[] { 0.0, flight },
				new[] { width - flight, width },
			};

			List<double[]> wells;

			if ((half - (2.0 * flight)) > 1e-9)
			{
				//Wide enough for a third flight down the middle, with a well either side of it.
				flights.Insert(1, new[] { half - flight, half + flight });
				wells = new List<double[]>
				{
					new[] { flight, half - flight },
					new[] { half + flight, width - flight },
				};
			}
			else
			{
				wells = new List<double[]> { new[] { flight, width - flight } };
			}

			foreach (double[] bounds in flights)
			{
				for (int tread = 0; tread < treadLines; tread++)
				{
					double y = -firstStepOffset - (tread * treadDepth);

					yield return line(bounds[0], y, bounds[1], y);
				}
			}

			foreach (double[] bounds in wells)
			{
				yield return line(bounds[0], -wellTop, bounds[0], -wellBottom);
				yield return line(bounds[1], -wellTop, bounds[1], -wellBottom);
				yield return line(bounds[0], -wellTop, bounds[1], -wellTop);
				yield return line(bounds[0], -wellBottom, bounds[1], -wellBottom);
			}

			//The handrail's band sits inside each outer flight, and when the stair is only just wide
			//enough for two flights the two bands meet and are one band drawn once.
			List<double[]> rails = new List<double[]>
			{
				new[] { handrailOffset, handrailOffset + HandrailWidth },
			};

			double mirrored = width - handrailOffset - HandrailWidth;

			if (Math.Abs(mirrored - handrailOffset) > 1e-9)
			{
				rails.Add(new[] { mirrored, width - handrailOffset });
			}

			foreach (double[] band in rails)
			{
				yield return line(band[0], -firstStepOffset, band[0], -run);
				yield return line(band[1], -firstStepOffset, band[1], -run);
				yield return line(band[0], -firstStepOffset, band[1], -firstStepOffset);
				yield return line(band[0], -run, band[1], -run);
			}
		}
	}
}
