using Xunit;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSMath.Tests
{
	public class VectorExtensionsTests
	{
		[Fact()]
		public void GetLengthTest()
		{
			XYZ xyz = new XYZ(1, 1, 1);
			double result = Math.Sqrt(3);

			Assert.Equal(result, xyz.GetLength());
		}

		[Fact()]
		public void NormalizeTest()
		{
			XYZ xyz = new XYZ(1, 1, 1);

			Assert.Equal(1, xyz.Normalize().GetLength());
		}
	}
}