using ACadSharp.Entities;
using CSMath;
using System;

namespace ACadSharp.Tests.Common
{
	public static class EntityFactory
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

		private static int _seed;

		private static CSMathRandom _random;

		static EntityFactory()
		{
			_random = new CSMathRandom();
		}

		public static T CreateDefault<T>()
			where T : Entity, new()
		{
			return new T();
		}

		/// <summary>
		/// Create a default entity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="randomize">Populate the entity with random values and fields</param>
		/// <returns></returns>
		public static T Create<T>(bool randomize = true)
			where T : Entity, new()
		{
			T e = new();

			if (!randomize)
				return e;

			switch (e)
			{
				case Line line:
					RandomizeLine(line);
					break;
				case Polyline2D:
					return e;
				default:
					throw new NotImplementedException();
			}

			return e;
		}

		public static void RandomizeEntity(Entity entity)
		{
			entity.Color = _random.NextColor();
		}

		public static void RandomizeLine(Line line)
		{
			RandomizeEntity(line);

			line.Thickness = _random.NextDouble();
			line.Normal = _random.NextXYZ();
			line.StartPoint = _random.NextXYZ();
			line.EndPoint = _random.NextXYZ();
		}
	}
}
