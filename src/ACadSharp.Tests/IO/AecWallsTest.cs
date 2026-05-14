using ACadSharp.Entities.AecObjects;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO
{
	public class AecWallsTests : IOTestsBase
	{
		private const string _aecObjectsFileName = "aec_objects/AecObjects.dwg";
		private CadDocument _testFileDocument;

		public AecWallsTests(ITestOutputHelper output) : base(output)
		{
			string path = Path.Combine(TestVariables.SamplesFolder, _aecObjectsFileName);

			if (!File.Exists(path))
			{
				_output.WriteLine($"Sample file not found: {path}");
				return;
			}

			_testFileDocument = DwgReader.Read(path, this.onNotification);
		}

		[Fact]
		public void ReadAecWall_HasAecWallClassDefinition()
		{
			bool hasAecWallClass = _testFileDocument.Classes.TryGetByName("AEC_WALL", out var wallClass);

			Assert.True(hasAecWallClass, "AEC_WALL class definition should be present in CLASSES section");
			Assert.NotNull(wallClass);
		}

		[Fact]
		public void ReadAecWall_HasCorrectProperties()
		{
			Assert.True(_testFileDocument.Classes.TryGetByName("AEC_WALL", out var wallClass));

			_output.WriteLine($"AEC_WALL class properties:");
			_output.WriteLine($"  DxfName: {wallClass.DxfName}");
			_output.WriteLine($"  CppClassName: {wallClass.CppClassName}");
			_output.WriteLine($"  ApplicationName: {wallClass.ApplicationName}");

			Assert.Equal("AEC_WALL", wallClass.DxfName);
			Assert.True(wallClass.IsAnEntity, "AEC_WALL should be marked as an entity");
			Assert.StartsWith("AecDb", wallClass.CppClassName);
		}

		[Fact]
		public void ReadAecWall_NameAndMarkerCorrect()
		{
			var walls = _testFileDocument.Entities.OfType<Wall>().ToList();

			_output.WriteLine($"Wall entities found: {walls.Count}");
			Assert.NotEmpty(walls);

			foreach (var wall in walls)
			{
				Assert.NotEqual(0ul, wall.Handle);
				Assert.Equal(DxfFileToken.EntityAecWall, wall.ObjectName);
				Assert.Equal(DxfSubclassMarker.AecWall, wall.SubclassMarker);
			}
		}
	}
}