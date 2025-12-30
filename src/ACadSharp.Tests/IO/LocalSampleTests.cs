using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class LocalSampleTests : IOTestsBase
	{
		public static TheoryData<FileModel> UserDwgFiles { get; } = new();

		public static TheoryData<FileModel> UserDxfFiles { get; } = new();

		public static TheoryData<FileModel> StressFiles { get; } = new();

		static LocalSampleTests()
		{
			loadLocalSamples("user_files", "dwg", UserDwgFiles);
			loadLocalSamples("user_files", "dxf", UserDxfFiles);
			loadLocalSamples("stress", "*", StressFiles);
		}

		public LocalSampleTests(ITestOutputHelper output) : base(output)
		{
		}

		[Theory]
		[MemberData(nameof(UserDwgFiles))]
		public void ReadUserDwg(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DwgReader.Read(test.Path, this._dwgConfiguration, this.onNotification);

			Spline s = doc.GetCadObject<Spline>(0x2C4);
			s.TryPolygonalVertexes(6, out var pt);

			doc.Entities.Add(new Polyline3D(pt) { Color = Color.Red });

			if (false)
			{
				List<Entity> entities = new List<Entity>();
				foreach (var spline in doc.Entities.OfType<Spline>())
				{
					if (!spline.TryPolygonalVertexes(256, out var pts))
					{
						continue;
					}

					Circle circle = new Circle(spline.PointOnSpline(1));
					circle.Color = Color.Cyan;

					entities.Add(circle);

					Circle circle1 = new Circle(spline.PointOnSpline(0));
					circle1.Color = Color.Yellow;

					entities.Add(circle1);

					entities.Add(new Polyline3D(pts) { Color = Color.Red });
				}
				doc.Entities.AddRange(entities);
			}

			DwgWriter.Write(Path.Combine(TestVariables.DesktopFolder, "output", "test.dwg"), doc, notification: onNotification);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DxfReader.Read(test.Path, this.onNotification);

			Spline s = doc.GetCadObject<Spline>(0x2C4);
			s.TryPolygonalVertexes(6, out var pt);

			doc.Entities.Add(new Polyline3D(pt) { Color = Color.Red });

			DxfWriter.Write(Path.Combine(TestVariables.DesktopFolder, "output", "test.dxf"), doc, notification: onNotification);
		}

		[Theory]
		[MemberData(nameof(StressFiles))]
		public void ReadStressFiles(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = null;
			string extension = Path.GetExtension(test.Path);

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (test.IsDxf)
			{
				doc = DxfReader.Read(test.Path, this.onNotification);
			}
			else if (extension.Equals(".dwg", System.StringComparison.OrdinalIgnoreCase))
			{
				doc = DwgReader.Read(test.Path, this.onNotification);
			}

			stopwatch.Stop();
			this._output.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());

			//Files tested have a size of ~100MB
			//Cannot exceed 10 seconds
			Assert.True(stopwatch.Elapsed.TotalSeconds < 10);
		}
	}
}