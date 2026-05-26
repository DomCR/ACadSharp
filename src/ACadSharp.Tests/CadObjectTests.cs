using System;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.XData;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> ACadTypes = new TheoryData<Type>();

		static CadObjectTests()
		{
			foreach (Type item in DataFactory.GetTypes<CadObject>())
			{
				if (item == typeof(UnknownEntity) 
					|| item == typeof(PdfUnderlay) 
					|| item == typeof(UnknownNonGraphicalObject))
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
		public void CleanReactorsTest()
		{
			Line main = new Line();
			Line line = new Line();
			CadDocument doc = new CadDocument();

			Assert.NotNull(main.Reactors);
			Assert.Empty(main.Reactors);

			doc.Entities.Add(main);
			doc.Entities.Add(line);
			Point point = new Point();
			main.AddReactor(point);
			main.AddReactor(line);

			Assert.Contains(point, main.Reactors);
			Assert.True(main.Reactors.Count() == 2);

			main.CleanReactors();
			Assert.NotEmpty(main.Reactors);
			Assert.True(main.Reactors.Count() == 1);

			doc.Entities.Remove(main);
			Assert.Empty(main.Reactors);
		}

		[Theory]
		[MemberData(nameof(ACadTypes))]
		public void ExtendedDataCloneTest(Type t)
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

		[Fact]
		public void CreateExtendedDictionaryTest()
		{
			CadObject obj = new Line();
			obj.CreateExtendedDictionary();

			Assert.NotNull(obj.XDictionary);
			Assert.Empty(obj.XDictionary);
		}
	}
}