using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class ScaleCollectionTests
	{
		[Fact]
		public void InitScaleCollection()
		{
			CadDocument doc = new CadDocument();

			Assert.NotNull(doc.Scales);
		}
	}
}
