using CSUtilities.Attributes;
using CSUtilities.Extensions;
using System;
using Xunit;

namespace CSUtilities.Tests.Extensions
{
	public class EnumExtensionsTests
	{
		private enum MockStringValues
		{
			[StringValue("Undefined value for enum")]
			Undefined = 0,

			[StringValue("Enum String Value 1")]
			Value1 = 1,

			[StringValue("Enum String Value 2")]
			Value2,

			NoAttribute,
		}

		[Fact]
		public void GetStringValueTest()
		{
			Assert.Equal("Undefined value for enum", MockStringValues.Undefined.GetStringValue());
			Assert.Equal("Enum String Value 1", MockStringValues.Value1.GetStringValue());
			Assert.Equal("Enum String Value 2", MockStringValues.Value2.GetStringValue());
			Assert.Null(MockStringValues.NoAttribute.GetStringValue());
		}

		[Fact]
		public void ParseTest()
		{
			Assert.Equal(MockStringValues.Value1, EnumExtensions.Parse<MockStringValues>("Value1"));
			Assert.Equal(MockStringValues.Value1, EnumExtensions.Parse<MockStringValues>("value1", true));
			Assert.Throws<ArgumentException>(() => EnumExtensions.Parse<MockStringValues>("value1", false));
		}

		[Fact]
		public void TryParseTest()
		{
			MockStringValues result = MockStringValues.Undefined;
			Assert.True(EnumExtensions.TryParse("Value1", out result));
			Assert.Equal(MockStringValues.Value1, result);

			result = MockStringValues.Undefined;
			Assert.True(EnumExtensions.TryParse<MockStringValues>("value1", out result, true));
			Assert.Equal(MockStringValues.Value1, result);

			Assert.False(EnumExtensions.TryParse<MockStringValues>("value1", out result, false));
			Assert.Equal(default(MockStringValues), result);
		}
	}
}
