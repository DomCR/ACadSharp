using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> Types;

		static CadObjectTests()
		{
			Types = new TheoryData<Type>();

			var d = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.ManifestModule.Name == "ACadSharp.dll");

			foreach (var item in d.GetTypes())
			{
				if (item.IsSubclassOf(typeof(CadObject)))
				{
					Types.Add(item);
				}
			}
		}

		[Theory]
		[MemberData(nameof(Types))]
		public void GetCadObjectMapTest(Type t)
		{
			CadObject.GetCadObjectMap(t);
		}
	}
}
