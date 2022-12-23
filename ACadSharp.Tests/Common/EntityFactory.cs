using ACadSharp.Entities;
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
				case Point point:
					RandomizePoint(point);
					break;
				case Polyline2D pl2d:
					RandomizePolyline(pl2d);
					break;
				case Polyline3D pl3d:
					RandomizePolyline(pl3d);
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

		public static void RandomizePoint(Point point)
		{
			RandomizeEntity(point);

			point.Thickness = _random.NextDouble();
			// line.Normal = _random.NextXYZ();	//Entity becomes invisible if has a different value
			point.Location = _random.NextXYZ();
		}

		public static void RandomizePolyline(Polyline pline)
		{
			RandomizeEntity(pline);

			int nv = _random.Next(2, 100);
			for (int i = 0; i < nv; i++)
			{
				Vertex v = null;

				switch (pline)
				{
					case Polyline2D:
						v = new Vertex2D();
						break;
					case Polyline3D:
						v = new Vertex3D();
						break;
				}

				v.Id = i;
				v.Location = _random.NextXYZ();

				pline.Vertices.Add(v);
			}
		}
	}
}
