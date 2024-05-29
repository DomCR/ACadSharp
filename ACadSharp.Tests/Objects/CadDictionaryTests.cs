using ACadSharp.Objects;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class CadDictionaryTests
	{
		[Fact]
		public void AvoidDuplicatedEntries()
		{
			CadDictionary cadDictionary = new CadDictionary();
			Scale scale = new Scale();
			scale.Name = "scale_test";

			cadDictionary.Add(scale);

			Scale scale1 = new Scale();
			scale.Name = "scale_test";

			Assert.Throws<ArgumentException>(() => cadDictionary.Add(scale));
		}
	}
}
