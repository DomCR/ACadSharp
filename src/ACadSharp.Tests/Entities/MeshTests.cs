using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.IO;
using ACadSharp.Tests.Common;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class MeshTests : CommonEntityTests<Mesh>
	{
		[Fact]
		public override void GetBoundingBoxTest()
		{
			Mesh mesh = new Mesh();
			mesh.Vertices.Add(new XYZ(-2, -3, -4));
			mesh.Vertices.Add(new XYZ(5, 6, 7));
			mesh.Vertices.Add(new XYZ(1, 0, 2));

			BoundingBox boundingBox = mesh.GetBoundingBox();

			Assert.Equal(new XYZ(-2, -3, -4), boundingBox.Min);
			Assert.Equal(new XYZ(5, 6, 7), boundingBox.Max);
		}

		[Fact]
		public void TextureCoordinatesDefaultsToEmpty()
		{
			Mesh mesh = new Mesh();

			Assert.NotNull(mesh.TextureCoordinates);
			Assert.Empty(mesh.TextureCoordinates);
		}

		[Fact]
		public void TextureCoordinatesClonedIndependently()
		{
			Mesh mesh = new Mesh();
			mesh.TextureCoordinates.Add(new XYZ(0.25, 0.5, 0));

			Mesh clone = mesh.CloneTyped();

			Assert.Equal(mesh.TextureCoordinates.Count, clone.TextureCoordinates.Count);
			Assert.Equal(mesh.TextureCoordinates[0], clone.TextureCoordinates[0]);

			clone.TextureCoordinates.Add(new XYZ(1, 1, 0));
			Assert.NotEqual(mesh.TextureCoordinates.Count, clone.TextureCoordinates.Count);
		}

		[Fact]
		public void TextureCoordinatesDwgRoundTrip()
		{
			CadDocument doc = new CadDocument(ACadVersion.AC1027);

			Mesh mesh = new Mesh();
			mesh.Vertices.Add(new XYZ(0, 0, 0));
			mesh.Vertices.Add(new XYZ(1, 0, 0));
			mesh.Vertices.Add(new XYZ(1, 1, 0));
			mesh.Vertices.Add(new XYZ(0, 0, 0));
			mesh.Vertices.Add(new XYZ(1, 1, 0));
			mesh.Vertices.Add(new XYZ(0, 1, 0));
			mesh.Faces.Add(new[] { 0, 1, 2 });
			mesh.Faces.Add(new[] { 3, 4, 5 });
			mesh.TextureCoordinates.Add(new XYZ(0.1, 0.2, 0));
			mesh.TextureCoordinates.Add(new XYZ(0.3, 0.4, 0));
			mesh.TextureCoordinates.Add(new XYZ(0.5, 0.6, 0));
			mesh.TextureCoordinates.Add(new XYZ(0.7, 0.8, 0));
			mesh.TextureCoordinates.Add(new XYZ(0.9, 0.1, 0));
			mesh.TextureCoordinates.Add(new XYZ(0.2, 0.3, 0));

			doc.Entities.Add(mesh);

			MemoryStream ms = new MemoryStream();
			DwgWriter.Write(ms, doc);
			using MemoryStream readStream = new MemoryStream(ms.ToArray());
			CadDocument rt = DwgReader.Read(readStream);

			Mesh roundTripped = rt.Entities.OfType<Mesh>().Single();

			Assert.Equal(mesh.Vertices.Count, roundTripped.Vertices.Count);
			Assert.Equal(mesh.Faces.Count, roundTripped.Faces.Count);
			Assert.Equal(mesh.TextureCoordinates.Count, roundTripped.TextureCoordinates.Count);

			for (int i = 0; i < mesh.TextureCoordinates.Count; i++)
			{
				Assert.Equal(mesh.TextureCoordinates[i].X, roundTripped.TextureCoordinates[i].X, 9);
				Assert.Equal(mesh.TextureCoordinates[i].Y, roundTripped.TextureCoordinates[i].Y, 9);
				Assert.Equal(mesh.TextureCoordinates[i].Z, roundTripped.TextureCoordinates[i].Z, 9);
			}
		}

		[Fact]
		public void TextureCoordinatesEmittedAsXRecord()
		{
			// Verify the writer materializes the AcDbSubDMesh-shaped XRecord on the mesh
			// extension dictionary, not just on the in-memory property.
			CadDocument doc = new CadDocument(ACadVersion.AC1027);

			Mesh mesh = new Mesh();
			mesh.Vertices.Add(new XYZ(0, 0, 0));
			mesh.Vertices.Add(new XYZ(1, 0, 0));
			mesh.Vertices.Add(new XYZ(0, 1, 0));
			mesh.Faces.Add(new[] { 0, 1, 2 });
			mesh.TextureCoordinates.Add(new XYZ(0, 0, 0));
			mesh.TextureCoordinates.Add(new XYZ(1, 0, 0));
			mesh.TextureCoordinates.Add(new XYZ(0, 1, 0));

			doc.Entities.Add(mesh);

			MemoryStream ms = new MemoryStream();
			DwgWriter.Write(ms, doc);
			using MemoryStream readStream = new MemoryStream(ms.ToArray());
			CadDocument rt = DwgReader.Read(readStream);

			Mesh roundTripped = rt.Entities.OfType<Mesh>().Single();
			Assert.NotNull(roundTripped.XDictionary);
			Assert.True(roundTripped.XDictionary.ContainsKey("ADSK_XREC_SUBDVERTEXTEXCOORDS"));
		}
	}
}
