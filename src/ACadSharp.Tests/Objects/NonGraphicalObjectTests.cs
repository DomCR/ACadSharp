using ACadSharp.Objects;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public abstract class NonGraphicalObjectTests<T>
		where T : NonGraphicalObject
	{
		[Fact]
		public void InitName()
		{
			string name = "custom_name";
			T obj = (T)Activator.CreateInstance(typeof(T), name);
			
			Assert.NotNull(obj.Name);
			Assert.Equal(name, obj.Name);
		}
	}
}
