using ACadSharp.Objects;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class CadDictionaryTests : NonGraphicalObjectTests<CadDictionary>
	{
		[Fact]
		public void AvoidDuplicatedEntries()
		{
			CadDictionary cadDictionary = new CadDictionary();
			Scale scale = new Scale();
			scale.Name = "scale_test";

			cadDictionary.Add(scale);

			Scale scale1 = new Scale();
			scale1.Name = "scale_test";

			Assert.Throws<ArgumentException>(() => cadDictionary.Add(scale1));

			scale.Name = "changed_name";
			scale1.Name = "changed_name";

			Assert.Throws<ArgumentException>(() => cadDictionary.Add(scale1));
		}

		[Fact]
		public void TryAddTest()
		{
			CadDictionary cadDictionary = new CadDictionary();
			Scale scale = new Scale();
			scale.Name = "scale_test";

			Assert.True(cadDictionary.TryAdd(scale));
			Assert.False(cadDictionary.TryAdd(scale));
		}
	}
}
