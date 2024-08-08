using System;

namespace CSMath.Tests
{
	public class VectorTestCaseFactory
	{
		private Random _random;

		public VectorTestCaseFactory()
		{
			_random = new Random();
		}

		public VectorTestCaseFactory(int seed)
		{
			_random = new Random(seed);
		}

		public T CreatePoint<T>(double? def = null)
			where T : IVector, new()
		{
			T pt = new T();

			for (int i = 0; i < pt.Dimension; i++)
			{
				if (def.HasValue)
				{
					pt[i] = def.Value;
				}
				else
				{
					pt[i] = _random.Next(-100, 100);
				}
			}

			return pt;
		}

		public (T, T, T) CreateOperationCase<T>(Func<double, double, double> op)
			where T : IVector, new()
		{
			T v1 = new T();
			T v2 = new T();
			T result = new T();

			for (int i = 0; i < v1.Dimension; i++)
			{
				var a = _random.Next(-100, 100);
				var b = _random.Next(-100, 100);

				v1[i] = (a);
				v2[i] = (b);
				result[i] = (op(a, b));
			}

			return (v1, v2, result);
		}
	}
}
