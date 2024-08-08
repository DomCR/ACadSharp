using System.Collections.Generic;

namespace CSMath
{
	/// <summary>
	/// Bounding box representation aligned to XYZ axis.
	/// </summary>
	public struct BoundingBox
	{
		/// <summary>
		/// Instance of a null bounding box.
		/// </summary>
		public static readonly BoundingBox Null = new BoundingBox(BoundingBoxExtent.Null);

		/// <summary>
		/// Instance of an infinite bounding box.
		/// </summary>
		public static readonly BoundingBox Infinite = new BoundingBox(BoundingBoxExtent.Infinite);

		public BoundingBoxExtent Extent { get; }

		/// <summary>
		/// Get the min corner of the bounding box.
		/// </summary>
		public XYZ Min { get; set; }

		/// <summary>
		/// Get the max corner of the bounding box.
		/// </summary>
		public XYZ Max { get; set; }

		/// <summary>
		/// Center of the box.
		/// </summary>
		public XYZ Center
		{
			get
			{
				return this.Min + (this.Max - this.Min) * 0.5;
			}
		}

		private BoundingBox(BoundingBoxExtent extent)
		{
			this.Extent = extent;
		}

		public BoundingBox(XYZ point)
		{
			this.Extent = BoundingBoxExtent.Point;
			this.Min = point;
			this.Max = point;
		}

		/// <summary>
		/// Bounding box constructor with 2 points.
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public BoundingBox(XYZ min, XYZ max)
		{
			if (min == max)
			{
				this.Extent = BoundingBoxExtent.Point;
			}
			else
			{
				this.Extent = BoundingBoxExtent.Finite;
			}

			this.Min = new XYZ(System.Math.Min(min.X, max.X), System.Math.Min(min.Y, max.Y), System.Math.Min(min.Z, max.Z));
			this.Max = new XYZ(System.Math.Max(min.X, max.X), System.Math.Max(min.Y, max.Y), System.Math.Max(min.Z, max.Z));
		}

		/// <summary>
		/// Bounding box contructor.
		/// </summary>
		/// <param name="minX"></param>
		/// <param name="minY"></param>
		/// <param name="minZ"></param>
		/// <param name="maxX"></param>
		/// <param name="maxY"></param>
		/// <param name="maxZ"></param>
		public BoundingBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
		{
			this = new BoundingBox(new XYZ(minX, minY, minZ), new XYZ(maxX, maxY, maxZ));
		}

		/// <summary>
		/// Merge 2 boxes into the common one.
		/// </summary>
		/// <param name="box"></param>
		/// <returns>The merged box.</returns>
		public BoundingBox Merge(BoundingBox box)
		{
			var min = new XYZ(
				System.Math.Min(this.Min.X, box.Min.X),
				System.Math.Min(this.Min.Y, box.Min.Y),
				System.Math.Min(this.Min.Z, box.Min.Z));
			var max = new XYZ(
				System.Math.Max(this.Max.X, box.Max.X),
				System.Math.Max(this.Max.Y, box.Max.Y),
				System.Math.Max(this.Max.Z, box.Max.Z));

			return new BoundingBox(min, max);
		}

		/// <summary>
		/// Create a bounding box from a collection of points.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static BoundingBox FromPoints(IEnumerable<XYZ> points)
		{
			BoundingBox boundingBox = Null;

			foreach (var point in points)
			{
				boundingBox = boundingBox.Merge(new BoundingBox(point));
			}

			return boundingBox;
		}
	}
}
