using CSUtilities.Extensions;
using System;
using Xunit;

namespace CSUtilities.Tests.Extensions
{
	public class StringExtensionsTests
	{
		public static TheoryData<string> NullEmpy { get; } = new();

		static StringExtensionsTests()
		{
			NullEmpy.Add(null);
			NullEmpy.Add(string.Empty);
		}

		[Theory]
		[MemberData(nameof(NullEmpy))]
		public void IsNullOrEmptyTest(string value)
		{
			Assert.True(value.IsNullOrEmpty());
		}

		[Theory]
		[MemberData(nameof(NullEmpy))]
		public void TrowIfNullOrEmptyTest(string value)
		{
			Assert.Throws<ArgumentException>(value.TrowIfNullOrEmpty);
			Assert.Throws<ArgumentException>(() => value.TrowIfNullOrEmpty("Message in case of null or empty"));
		}
	}

	public class ObjectExtensionsTests
	{
		[Fact]
		public void ThrowIfTest()
		{
			int zero = 0;

			zero.ThrowIf<int, ArgumentOutOfRangeException>((value) =>
			{
				return value == 0;
			});
		}
	}
}
