using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Geometry
{
	public struct XY
	{
		public static XY Zero = new XY(0, 0);
		public static XY AxisX = new XY(1, 0);
		public static XY AxisY = new XY(0, 1);

		public double X { get; set; }
		public double Y { get; set; }

		public XY(double x, double y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return $"{X},{Y}";
		}
	}

	public struct XYZ
	{
		public static XYZ Zero = new XYZ(0, 0, 0);
		public static XYZ AxisX = new XYZ(1, 0, 0);
		public static XYZ AxisY = new XYZ(0, 1, 0);
		public static XYZ AxisZ = new XYZ(0, 0, 1);

		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public XYZ(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return $"{X},{Y},{Z}";
		}
	}

	public struct XYZM
	{
		public static XYZM Zero = new XYZM(0, 0, 0, 0);
		public static XYZM AxisX = new XYZM(1, 0, 0, 0);
		public static XYZM AxisY = new XYZM(0, 1, 0, 0);
		public static XYZM AxisZ = new XYZM(0, 0, 1, 0);
		public static XYZM AxisM = new XYZM(0, 0, 0, 1);

		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public double M { get; set; }

		public XYZM(double x, double y, double z, double m)
		{
			X = x;
			Y = y;
			Z = z;
			M = m;
		}
		public override string ToString()
		{
			return $"{X},{Y},{Z},{M}";
		}
	}
}
