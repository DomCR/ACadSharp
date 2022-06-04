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
			Line line = new Line();

			var copy = CadObject.CreateCopy(line);

		}
	}
}
