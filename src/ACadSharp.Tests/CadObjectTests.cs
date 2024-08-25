using System;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> ACadTypes = new TheoryData<Type>();

		static CadObjectTests()
		{
			foreach (Type item in DataFactory.GetTypes<Entity>())
			{
				if (item == typeof(UnknownEntity))
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

		[Theory]
		[MemberData(nameof(ACadTypes))]
		public void DefaultConstructor(Type t)
		{
			CadObject cadObject = Factory.CreateObject(t, false);

			Assert.NotNull(cadObject);
			Assert.True(0 == cadObject.Handle);

			Assert.NotEqual(ObjectType.UNDEFINED, cadObject.ObjectType);

			Assert.False(string.IsNullOrEmpty(cadObject.ObjectName));
			Assert.False(string.IsNullOrEmpty(cadObject.SubclassMarker));

			Assert.Null(cadObject.XDictionary);
		}

		[Fact]
		public void CreateExtendedDictionaryTest()
		{
			CadObject obj = new Line();
			obj.CreateExtendedDictionary();

			Assert.NotNull(obj.XDictionary);
			Assert.Empty(obj.XDictionary);
		}

		[Theory(Skip = "Factory refactor needed")]
		[MemberData(nameof(ACadTypes))]
		public void Clone(Type t)
		{
			CadObject cadObject = Factory.CreateObject(t);
			CadObject clone = (CadObject)cadObject.Clone();

			CadObjectTestUtils.AssertClone(cadObject, clone);
		}
	}
}