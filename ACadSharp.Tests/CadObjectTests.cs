using ACadSharp.Entities;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		[Fact]
		public void CloneTest()
		{
			Circle c = new Circle();

			Circle copy = c.Clone() as Circle;

			CadObjectTestUtils.AssertEntityClone(c, copy);
		}
	}
}
