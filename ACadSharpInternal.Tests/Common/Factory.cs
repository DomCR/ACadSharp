namespace ACadSharp.Tests.Common
{
	public abstract class Factory
	{
		public static int Seed
		{
			get
			{
				return _seed;
			}
			set
			{
				_seed = value;
				_random = new CSMathRandom(_seed);
			}
		}

		protected static int _seed;

		protected static CSMathRandom _random;

		static Factory()
		{
			_random = new CSMathRandom();
		}
	}
}
