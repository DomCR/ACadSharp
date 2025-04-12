using ACadSharp.Entities;
using ACadSharp.Objects;
using System;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class GroupTests : NonGraphicalObjectTests<Group>
	{
		[Fact]
		public void AddTest()
		{
			CadDocument doc = new CadDocument();
			Line l = new Line();
			var group = new Group();

			group.Add(l);

			Assert.Contains(group, l.Reactors);
			Assert.Throws<InvalidOperationException>(() => doc.Groups.Add(group));

			group.Clear();

			Assert.DoesNotContain(group, l.Reactors);

			doc.Groups.Add(group);
			Assert.Contains(group, doc.Groups);

			Assert.Throws<InvalidOperationException>(() => group.Add(l));

			doc.Entities.Add(l);
			group.Add(l);

			Assert.Contains(l, group.Entities);
		}

		[Fact]
		public void IsUnnamedTest()
		{
			var group = new Group();

			Assert.True(group.IsUnnamed);
			group.Name = null;
			Assert.True(group.IsUnnamed);
			group.Name = " ";
			Assert.True(group.IsUnnamed);

			group.Name = "my_group";
			Assert.False(group.IsUnnamed);
		}
	}
}