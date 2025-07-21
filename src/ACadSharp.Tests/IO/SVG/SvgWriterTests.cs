using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tests.Common;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.SVG
{
	public class SvgWriterTests : IOTestsBase
	{
		public static CadDocument Document { get; }

		public static TheoryData<Layout> Layouts { get; } = new();

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

				Layouts.Add(item);
			}
		}

		public SvgWriterTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(Layouts))]
		public void WriteLayouts(Layout layout)
		{
			using (SvgWriter writer = createWriter($"{layout.Name}.svg", Document))
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
			writer.OnNotification += this.onNotification;
			return writer;
		}
	}
}