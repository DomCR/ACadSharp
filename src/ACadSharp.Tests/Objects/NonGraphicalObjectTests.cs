using ACadSharp.Extensions;
using ACadSharp.Objects;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public abstract class NonGraphicalObjectTests<T>
		where T : NonGraphicalObject
	{
		[Fact]
		public void DetachNameEvent()
		{
			string name = "custom_name";
			T obj = (T)Activator.CreateInstance(typeof(T), name);

			CadDictionary dict = new CadDictionary();
			dict.Add(obj);

			T clone = obj.CloneTyped();
			string cloneName = "test-event";
			clone.Name = cloneName;

			Assert.False(dict.ContainsKey(cloneName));
		}

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