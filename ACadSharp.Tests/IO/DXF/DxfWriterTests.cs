using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DXF;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF
{
	public class DxfWriterTests : IOTestsBase
	{
		public DxfWriterTests(ITestOutputHelper output) : base(output) { }

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteEmptyAsciiTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			string path = Path.Combine(_samplesOutFolder, $"out_empty_sample_{version}_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.OnNotification += this.onNotification;
				wr.Write();
			}

			this._output.WriteLine(string.Empty);
			this._output.WriteLine("Writer successful");
			this._output.WriteLine(string.Empty);

			using (var re = new DxfReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(Path.GetFullPath(path));
		}

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteEmptyBinaryTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			string path = Path.Combine(_samplesOutFolder, $"out_empty_sample_{version}_binary.dxf");

			using (var wr = new DxfWriter(path, doc, true))
			{
				wr.OnNotification += this.onNotification;
				wr.Write();
			}

			this._output.WriteLine(string.Empty);
			this._output.WriteLine("Writer successful");
			this._output.WriteLine(string.Empty);

			using (var re = new DxfReader(path, this.onNotification))
			{
				CadDocument readed = re.Read();
			}

			this.checkDocumentInAutocad(path);
		}

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteDocumentWithEntitiesTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			Point pt = new Point();
			Line ln = new Line
			{
				StartPoint = new CSMath.XYZ(0, 0, 0),
				EndPoint = new CSMath.XYZ(10, 10, 0)
			};

			doc.Entities.Add(pt);
			doc.Entities.Add(ln);

			string path = Path.Combine(_samplesOutFolder, $"out_sample_{version}_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.OnNotification += this.onNotification;
				wr.Write();
			}
		}
	}
}