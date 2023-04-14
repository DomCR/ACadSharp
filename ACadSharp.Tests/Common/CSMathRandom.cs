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

		public T Next<T>()
			where T : struct
		{
			T value = default(T);

			switch (value)
			{
				case bool:
					return (T)Convert.ChangeType(this.Next(0, 1) == 1, typeof(T));
				case byte:
					return (T)Convert.ChangeType(this.Next(byte.MinValue, byte.MaxValue), typeof(T));
				case short:
					return (T)Convert.ChangeType(this.Next(short.MinValue, short.MaxValue), typeof(T));
			}

			return value;
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
