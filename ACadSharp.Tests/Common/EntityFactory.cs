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
				case Polyline2D pl2d:
					RandomizePolyline2D(pl2d);
					break;
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
			// line.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			line.StartPoint = _random.NextXYZ();
			line.EndPoint = _random.NextXYZ();
		}

		public static void RandomizePolyline2D(Polyline2D pline)
		{
			RandomizeEntity(pline);

			int nv = _random.Next(2, 100);
			for (int i = 0; i < nv; i++)
			{
				Vertex2D v = new Vertex2D();

				v.Location = _random.NextXYZ();

				pline.Vertices.Add(v);
			}
		}
	}
}
