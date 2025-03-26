using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
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

		[Theory(Skip = "Not implemented")]
		[MemberData(nameof(EntityTypes))]
		public void WriteEntitiesNoDocument(Type t)
		{
			Entity e = (Entity)Factory.CreateObject(t);
			string filename = Path.Combine(TestVariables.OutputSvgFolder, $"{e.SubclassMarker}.svg");

			using (SvgWriter writer = new SvgWriter(filename))
			{
				writer.WriteEntity(e);
			}
		}

		private void writeSvg(string filename, string svg)
		{
			using (StreamWriter sw = new StreamWriter(filename))
			{
				sw.Write(svg);
			}
		}
	}
}
