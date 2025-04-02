using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.SVG
{
	public class SvgWriterTests : IOTestsBase
	{
		public static readonly TheoryData<Type> EntityTypes = new TheoryData<Type>();

		static SvgWriterTests()
		{
			foreach (var item in DataFactory.GetTypes<Entity>())
			{
				if (item == typeof(UnknownEntity))
				{
					continue;
				}

				EntityTypes.Add(item);
			}
		}

		public SvgWriterTests(ITestOutputHelper output) : base(output) { }

		[Fact]
		public void WriteModel()
		{
			string svg = Path.Combine(TestVariables.OutputSvgFolder, $"model.svg");
			string dwg = Path.Combine(TestVariables.SamplesFolder, "svg", $"export_sample.dwg");
			var doc = DwgReader.Read(dwg);

			using (SvgWriter writer = new SvgWriter(svg, doc))
			{
				writer.OnNotification += this.onNotification;
				writer.Write();
			}
		}
	}
}
