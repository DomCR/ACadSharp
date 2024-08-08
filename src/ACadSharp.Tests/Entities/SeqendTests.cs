using ACadSharp.Entities;
using Xunit;

namespace ACadSharp.Tests.Entities
{
	public class SeqendTests
	{
		[Fact]
		public void NullTest()
		{
			Polyline2D polyline = new Polyline2D();

			Assert.Null(polyline.Vertices.Seqend);
		}

		[Fact]
		public void CreatedTest()
		{
			Polyline2D polyline = new Polyline2D();
			polyline.Vertices.Add(new Vertex2D());

			Assert.NotNull(polyline.Vertices.Seqend);
			Assert.True(polyline.Vertices.Seqend.Handle == 0);
			Assert.NotEmpty(polyline.Vertices);
		}

		[Fact]
		public void RemovedTest()
		{
			Polyline2D polyline = new Polyline2D();
			polyline.Vertices.Add(new Vertex2D());

			Assert.NotNull(polyline.Vertices.Seqend);
			Assert.True(polyline.Vertices.Seqend.Handle == 0);
			Assert.NotEmpty(polyline.Vertices);

			polyline.Vertices.Clear();  // Empty collection

			Assert.Null(polyline.Vertices.Seqend);
		}

		[Fact]
		public void AddToDocumentTest()
		{
			CadDocument doc = new CadDocument();

			Polyline2D polyline = new Polyline2D();
			polyline.Vertices.Add(new Vertex2D());

			doc.Entities.Add(polyline);

			Assert.False(polyline.Vertices.Seqend.Handle == 0);
			Seqend inner = doc.GetCadObject<Seqend>(polyline.Vertices.Seqend.Handle);
			Assert.NotNull(inner);
		}

		[Fact]
		public void OnSeqendAddedTest()
		{
			CadDocument doc = new CadDocument();

			Polyline2D polyline = new Polyline2D();

			doc.Entities.Add(polyline);

			polyline.Vertices.Add(new Vertex2D());

			Assert.False(polyline.Vertices.Seqend.Handle == 0);
			Assert.True(doc.TryGetCadObject(polyline.Vertices.Seqend.Handle, out Seqend inner));
			Assert.NotNull(inner);
		}

		[Fact]
		public void OnSeqendRemovedTest()
		{
			CadDocument doc = new CadDocument();

			Polyline2D polyline = new Polyline2D();
			polyline.Vertices.Add(new Vertex2D());
			Seqend holder = polyline.Vertices.Seqend;

			doc.Entities.Add(polyline);

			polyline.Vertices.Clear();

			Assert.True(holder.Handle == 0);
			Assert.False(doc.TryGetCadObject(holder.Handle, out Seqend inner));
			Assert.Null(inner);
		}
	}
}
