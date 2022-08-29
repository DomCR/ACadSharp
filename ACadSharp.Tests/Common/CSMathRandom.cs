using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tests.Common
{
	public class CSMathRandom : Random
	{
		public CSMathRandom() : base() { }

		public CSMathRandom(int seed) : base(seed) { }

		public short NextShort()
		{
			return (short)Next(short.MinValue, short.MaxValue);
		}

		public XY NextXY()
		{
			return new XY(this.NextDouble(), this.NextDouble());
		}

		public XYZ NextXYZ()
		{
			return new XYZ(this.NextDouble(), this.NextDouble(), this.NextDouble());
		}

		public Color NextColor()
		{
			return new Color(this.NextShort());
		}

	}
}
