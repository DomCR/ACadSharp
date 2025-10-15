using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.IO;
using ACadSharp.Tests.TestModels;
using CSMath;
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

			List<Entity> entities = new();
			foreach (var spline in doc.Entities.OfType<Spline>())
			{
				try
				{
					if (spline.FitPoints.Count > 2)
					{
						spline.UpdateFromFitPoints();
					}

					foreach (var cp in spline.ControlPoints)
					{
						entities.Add(new Circle()
						{
							Center = cp,
							Color = Color.Red,
							Radius = 0.5
						});
					}

					foreach (var fp in spline.FitPoints)
					{
						entities.Add(new Circle()
						{
							Center = fp,
							Color = Color.Green,
							Radius = 0.5
						});
					}

					entities.Add(new Circle()
					{
						Center = spline.PointOnSpline(0),
						Color = Color.Yellow,
						Radius = 0.5
					});
					entities.Add(new Circle()
					{
						Center = spline.PointOnSpline(0.5),
						Color = Color.Yellow,
						Radius = 0.5
					});
					entities.Add(new Circle()
					{
						Center = spline.PointOnSpline(1 - MathHelper.Epsilon),
						Color = Color.Yellow,
						Radius = 0.5
					});

					System.Collections.Generic.List<CSMath.XYZ> v = spline.PolygonalVertexes(256);
					LwPolyline poly = new LwPolyline(v.Select(v => v.Convert<XY>()));
					poly.IsClosed = spline.IsClosed;
					poly.Color = Color.Blue;
					entities.Add(poly);
				}
				catch (System.Exception)
				{
				}
			}

			doc.Entities.AddRange(entities);

			DwgWriter.Write(Path.Combine(TestVariables.DesktopFolder, "output", "test.dwg"), doc);
		}

		[Theory]
		[MemberData(nameof(UserDxfFiles))]
		public void ReadUserDxf(FileModel test)
		{
			if (string.IsNullOrEmpty(test.Path))
				return;

			CadDocument doc = DxfReader.Read(test.Path, this.onNotification);
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

			if (extension == ".dxf")
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