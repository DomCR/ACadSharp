using ACadSharp.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ACadSharp.Tests
{
	public class CadObjectTests
	{
		[Fact]
		public void CreateCopyTest()
		{
			Circle c = new Circle();

			var copy = c.Clone();

		}
	}
}
