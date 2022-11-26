using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DXF;
using ACadSharp.Tests.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

			this.checkDxfDocumentInAutocad(Path.GetFullPath(path));
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

			this.checkDxfDocumentInAutocad(path);
		}

		[Theory]
		[MemberData(nameof(Versions))]
		public void WriteDocumentWithEntitiesTest(ACadVersion version)
		{
			CadDocument doc = new CadDocument();
			doc.Header.Version = version;

			List<Entity> entities = new List<Entity>
			{
				new Point
				{
					Location = new CSMath.XYZ(0, 10, 0)
				},
				EntityFactory.Create<Line>(),
				EntityFactory.Create<Polyline2D>(),
				EntityFactory.Create<Polyline3D>(),
				new Line
				{
					StartPoint = new CSMath.XYZ(0, 0, 0),
					EndPoint = new CSMath.XYZ(10, 10, 0)
				},
				new Arc
				{
					Center = new CSMath.XYZ(0, 5, 0),
					Radius = 20,
					StartAngle = 1,
					EndAngle = 2
				}
			};


			doc.Entities.AddRange(entities);

			string path = Path.Combine(_samplesOutFolder, $"out_sample_{version}_ascii.dxf");

			using (var wr = new DxfWriter(path, doc, false))
			{
				wr.OnNotification += this.onNotification;
				wr.Write();
			}

			this.checkDxfDocumentInAutocad(path);
		}
	}
}