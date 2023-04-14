using ACadSharp.Tables;
using System;

namespace ACadSharp.Tests.Common
{
	public class TableEntryFactory : Factory
	{
		/// <summary>
		/// Create a default entity
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="randomize">Populate the entity with random values and fields</param>
		/// <returns></returns>
		public static T Create<T>(string name = null, bool randomize = true)
			where T : TableEntry
		{
			return (T)Create(typeof(T), name, randomize);

		}

		public static TableEntry Create(Type type, string name = null, bool randomize = true)
		{

			if (string.IsNullOrEmpty(name))
			{
				name = _random.RandomString(10);
			}

			object entry = Activator.CreateInstance(type, name);

			if (!randomize)
				return (TableEntry)entry;

			map((TableEntry)entry);

			return (TableEntry)entry;
		}

		private static void map(TableEntry entry)
		{
			switch (entry)
			{
				case BlockRecord record:
					RandomizeBlockRecord(record);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		public static void RandomizeBlockRecord(BlockRecord record)
		{
			record.IsExplodable = _random.Next<bool>();
			record.CanScale = _random.Next<bool>();
		}
	}
}
