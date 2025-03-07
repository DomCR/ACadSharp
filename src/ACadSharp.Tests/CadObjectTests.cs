using System;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.Collections.Generic;
using System.Reflection.Emit;

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

			List<ExtendedDataRecord> records = new();
			records.Add(new ExtendedDataControlString(false));
			records.Add(new ExtendedDataInteger16(5));
			records.Add(new ExtendedDataInteger32(33));
			records.Add(new ExtendedDataString("my extended data string"));
			records.Add(new ExtendedDataControlString(true));

			cadObject.ExtendedData.Add(new AppId("hello"), records);

			CadObjectTestUtils.AssertClone(cadObject, clone);
		}
	}
}