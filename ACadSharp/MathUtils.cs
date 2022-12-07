using CSMath;
using System;

namespace ACadSharp
{
	public static class MathUtils
	{
		public static XY GetCenter(XY start, XY end, double bulge)
		{
			return GetCenter(start, end, bulge, out _);
		}

		public static XY GetCenter(XY start, XY end, double bulge, out double radius)
		{
			double theta = 4 * Math.Atan(Math.Abs(bulge));
			double c = start.DistanceFrom(end) / 2.0;
			radius = c / Math.Sin(theta / 2.0);

			double gamma = (Math.PI - theta) / 2;
			double phi = XY.GetAngle(start, end) + Math.Sign(bulge) * gamma;
			return new XY(start.X + radius * Math.Cos(phi), start.Y + radius * Math.Sin(phi));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="bulge"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static T GetCenter<T>(T start, T end, double bulge)
			where T : IVector<T>, new()
		{
			//Needs a plane of reference in case is in 3D
			throw new NotImplementedException();
		}
	}
}
