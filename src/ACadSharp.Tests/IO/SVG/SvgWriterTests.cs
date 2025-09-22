using ACadSharp.IO;
using ACadSharp.Objects;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.SVG
{
	public class SvgWriterTests : IOTestsBase
	{
		public static CadDocument Document { get; }

		public static readonly TheoryData<string> LayoutNames = new();

		static SvgWriterTests()
		{
			string dwgFile = Path.Combine(TestVariables.SamplesFolder, "svg", $"export_sample.dwg");
			Document = DwgReader.Read(dwgFile);

			foreach (var item in Document.Layouts)
			{
				if (!item.IsPaperSpace)
				{
					continue;
				}

				LayoutNames.Add(item.Name);
			}
		}

		public SvgWriterTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(LayoutNames))]
		public void WriteLayouts(string name)
		{
			Layout layout = Document.Layouts[name];

			using (SvgWriter writer = createWriter($"{name}.svg", Document))
			{
				writer.Write(layout);
			}
		}

		[Fact]
		public void WriteModel()
		{
			using (SvgWriter writer = createWriter($"model.svg", Document))
			{
				writer.Write();
			}
		}

		private SvgWriter createWriter(string filename, CadDocument doc)
		{
			string output = Path.Combine(TestVariables.OutputSvgFolder, filename);

			var writer = new SvgWriter(output, doc);
			writer.Configuration = this._svgConfiguration;
			writer.OnNotification += this.onNotification;
			return writer;
		}
	}
}