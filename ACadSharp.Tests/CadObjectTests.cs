using System;
using Xunit;
using ACadSharp.Tests.Common;
using ACadSharp.Tables.Collections;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		public static readonly TheoryData<Type> ACadTypes = new TheoryData<Type>();

		static CadObjectTests()
		{
			foreach (Type item in DataFactory.GetTypes<CadObject>())
			{
				ACadTypes.Add(item);
			}
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