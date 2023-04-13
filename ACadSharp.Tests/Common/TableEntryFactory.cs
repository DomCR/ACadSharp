using ACadSharp.Tables;
using System;

namespace ACadSharp.Tests.Common
{
	public static class TableEntryFactory
	{
		public static TableEntry Create(Type type, bool randomize = true)
		{
			object e = Activator.CreateInstance(type);

			if (!randomize)
				return (TableEntry)e;

			//return (TableEntry)map(e);
			throw new NotImplementedException();
		}
	}
}
