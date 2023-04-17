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

		public object Next(Type t)
		{
			object value = Activator.CreateInstance(t);

			return setValue(value);
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

		private T setValue<T>(T value)
		{
			switch (value)
			{
				case bool:
					return (T)Convert.ChangeType(this.Next(0, 1) == 1, typeof(bool));
				case byte:
					return (T)Convert.ChangeType(this.Next(byte.MinValue, byte.MaxValue), typeof(byte));
				case char:
					return (T)Convert.ChangeType(this.Next(byte.MinValue, byte.MaxValue), typeof(char));
				case short:
					return (T)Convert.ChangeType(this.Next(short.MinValue, short.MaxValue), typeof(short));
				case ushort:
					return (T)Convert.ChangeType(this.Next(ushort.MinValue, ushort.MaxValue), typeof(ushort));
				case int:
					return (T)Convert.ChangeType(this.Next(int.MinValue, int.MaxValue), typeof(int));
				case double:
					return (T)Convert.ChangeType(this.NextDouble(), typeof(double));
				case long:
					return (T)Convert.ChangeType(this.Next(int.MinValue, int.MaxValue), typeof(long));
				case ulong:
					return (T)Convert.ChangeType(this.Next(0, int.MaxValue), typeof(ulong));
				case string:
					return (T)Convert.ChangeType(RandomString(10), typeof(string));
				case XY:
					return (T)Convert.ChangeType(NextXY(), typeof(XY));
				case XYZ:
					return (T)Convert.ChangeType(NextXYZ(), typeof(XYZ));
				case Color:
					return (T)Convert.ChangeType(NextColor(), typeof(Color));
				case Transparency:
					return (T)Convert.ChangeType(new Transparency(), typeof(Transparency));
				default:
					throw new NotImplementedException();
			}
		}
	}
}
