using System;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using ACadSharp.Objects;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> ACadTypes = new TheoryData<Type>();

		static CadObjectTests()
		{
			foreach (Type item in DataFactory.GetTypes<CadObject>())
			{
				if (item == typeof(UnknownEntity) || item == typeof(UnknownNonGraphicalObject))
				{
					continue;
				}

				if (item.IsAbstract)
				{
					continue;
				}

				ACadTypes.Add(item);
			}
		}

		[Fact]
		public void CreateExtendedDictionaryTest()
		{
			CadObject obj = new Line();
			obj.CreateExtendedDictionary();

			Assert.NotNull(obj.XDictionary);
			Assert.Empty(obj.XDictionary);
		}

		[Theory]
		[MemberData(nameof(ACadTypes))]
		public void Clone(Type t)
		{
			CadObject cadObject = Factory.CreateObject(t);

			if (cadObject == null)
			{
				return;
			}

			CadObject clone = (CadObject)cadObject.Clone();

			CadObjectTestUtils.AssertClone(cadObject, clone);
		}
	}
}