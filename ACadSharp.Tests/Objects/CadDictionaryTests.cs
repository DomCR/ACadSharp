using ACadSharp.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ACadSharp.Tests.Objects
{
	public class CadDictionaryTests
	{
		[Fact]
		public void PerformanceTest()
		{
			CadDictionary cadDictionary = new CadDictionary();

			for (int i = 0; i < 100000000; i++)
			{
				Scale scale = new Scale();
				scale.Name = i.ToString();

				cadDictionary.Add("i", scale);
			}
		}
	}
}
