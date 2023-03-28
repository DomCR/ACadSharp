using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
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

		public string RandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			return new string(Enumerable.Repeat(chars, length)
				.Select(s => s[this.Next(s.Length)]).ToArray());
		}
	}
}
