using ACadSharp.Entities;
using ACadSharp.Tables;
using System;
using System.Linq;

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

		public static CadObject CreateObject(Type type, bool randomize = true)
		{
			return createObject(type, type, randomize);
		}

		protected static T map<T>(T e)
		{
			foreach (var p in e.GetType()
				.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
				.Where(o => o.CanWrite && !o.PropertyType.IsClass && !o.PropertyType.IsEnum && !o.PropertyType.IsInterface))
			{
				p.SetValue(e, _random.Next(p.PropertyType));
			}

			return e;
		}

		private static CadObject createObject(Type type, Type original, bool randomize)
		{
			if (type == null)
			{
				return null;
			}

			if (type.BaseType == typeof(Entity))
			{
				return EntityFactory.Create(original, randomize);
			}
			else if (type.BaseType == typeof(TableEntry))
			{
				return TableEntryFactory.Create(original, randomize: randomize);
			}

			return createObject(type.BaseType, original, randomize);
		}

	}
}
