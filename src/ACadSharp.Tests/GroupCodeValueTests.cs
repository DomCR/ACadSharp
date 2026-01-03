using Xunit;

namespace ACadSharp.Tests
{
	public class GroupCodeValueTests
	{
		[Fact]
		public void IsValidTest()
		{
			Assert.False(GroupCodeValueType.None.IsValid(5));
			Assert.False(GroupCodeValueType.Double.IsValid(5));

			Assert.True(GroupCodeValueType.Double.IsValid(5.0d));
		}
	}
}