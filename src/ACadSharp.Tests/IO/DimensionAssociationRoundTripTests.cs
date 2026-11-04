using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class DimensionAssociationRoundTripTests
{
	[Theory]
	[InlineData("dwg")]
	[InlineData("dxf")]
	public void AllPointReferenceCasesRoundTrip(string format)
	{
		CadDocument read = this.writeAndRead(this.dimensionWithEveryCase(), format);

		Dimension dimension = read.Entities.OfType<Dimension>().Single();
		Assert.True(dimension.XDictionary.TryGetEntry<DimensionAssociation>("ACAD_DIMASSOC", out DimensionAssociation association));
		Assert.Same(dimension, association.AssociatedEntity);
		Assert.Same(dimension, association.Dimension);
		Assert.Equal(AssociativityFlags.FirstPointReference | AssociativityFlags.SecondPointReference, association.AssociativityFlags);

		Line[] lines = read.Entities.OfType<Line>().ToArray();
		Insert column = read.Entities.OfType<Insert>().Single();

		//1st point: an intersection snap (the second object block)
		DimensionAssociation.OsnapPointRef first = association.FirstPointRef;
		Assert.Equal(ObjectOsnapType.Intersection, first.ObjectOsnapType);
		Assert.Equal(new CadObject[] { lines[0] }, first.GeometryPath);
		Assert.Equal(new CadObject[] { lines[1] }, first.IntersectionPath);
		Assert.Equal(SubentType.Edge, first.SubentType);
		Assert.Equal(1, first.GsMarker);
		Assert.Equal(0.5, first.GeometryParameter);
		Assert.Equal(new XYZ(5, 0, 0), first.OsnapPoint);
		Assert.Equal(SubentType.Edge, first.IntersectionSubType);
		Assert.Equal(1, first.IntersectionGsMarker);
		Assert.Null(first.LastPointRef);

		//2nd point: a perpendicular snap to a line INSIDE a block (path of 2: the insert, then the object), with a
		//chained reference (75 = 1), as AutoCAD writes it
		DimensionAssociation.OsnapPointRef second = association.SecondPointRef;
		Assert.Equal(ObjectOsnapType.Perpendicular, second.ObjectOsnapType);
		Assert.Equal(2, second.GeometryPath.Count);
		Assert.Same(column, second.GeometryPath[0]);
		Assert.Same(column.Block.Entities.OfType<Line>().Single(), second.GeometryPath[1]);
		Assert.Same(column, second.Geometry);
		Assert.Equal(0.4703191426445606, second.GeometryParameter);
		Assert.Equal(2e50, second.OsnapPoint.Z);
		Assert.True(second.HasLastPointRef);
		Assert.NotNull(second.LastPointRef);
		Assert.Equal(ObjectOsnapType.None, second.LastPointRef.ObjectOsnapType);
		Assert.Empty(second.LastPointRef.GeometryPath);
		Assert.Equal(4.712388980384689, second.LastPointRef.GeometryParameter);
	}

	[Fact]
	public void LeaderArrowAssociationKeepsItsOwner()
	{
		//AutoCAD also attaches a DIMASSOC to the arrowhead of a LEADER (and MULTILEADER): typed as Dimension, the
		//owner was read as null
		CadDocument doc = new CadDocument();
		Line wall = new Line(new XYZ(-5, 0, 0), new XYZ(5, 0, 0));
		Leader leader = new Leader();
		leader.Vertices.Add(new XYZ(0, 0, 0));
		leader.Vertices.Add(new XYZ(10, 10, 0));
		doc.Entities.Add(wall);
		doc.Entities.Add(leader);
		DimensionAssociation association = new DimensionAssociation { AssociatedEntity = leader, AssociativityFlags = AssociativityFlags.FirstPointReference };
		association.FirstPointRef = new DimensionAssociation.OsnapPointRef { ObjectOsnapType = ObjectOsnapType.Nearest, SubentType = SubentType.Edge, GsMarker = 1, GeometryParameter = 5 };
		association.FirstPointRef.GeometryPath.Add(wall);
		leader.CreateExtendedDictionary().Add("ACAD_DIMASSOC", association);

		CadDocument read = this.writeAndRead(doc, "dwg");

		Leader readLeader = read.Entities.OfType<Leader>().Single();
		Assert.True(readLeader.XDictionary.TryGetEntry<DimensionAssociation>("ACAD_DIMASSOC", out DimensionAssociation readAssociation));
		Assert.Same(readLeader, readAssociation.AssociatedEntity);
		Assert.Null(readAssociation.Dimension);
		Assert.Same(read.Entities.OfType<Line>().Single(), readAssociation.FirstPointRef.Geometry);
	}

	[Fact]
	public void CloneDoesNotSharePointReferences()
	{
		CadDocument doc = this.dimensionWithEveryCase();
		doc.Entities.OfType<Dimension>().Single().XDictionary.TryGetEntry<DimensionAssociation>("ACAD_DIMASSOC", out DimensionAssociation association);

		DimensionAssociation clone = (DimensionAssociation)association.Clone();
		clone.FirstPointRef.GeometryPath.Clear();
		clone.SecondPointRef.LastPointRef.GeometryParameter = 0;

		Assert.Single(association.FirstPointRef.GeometryPath);
		Assert.Equal(4.712388980384689, association.SecondPointRef.LastPointRef.GeometryParameter);
	}

	[Fact]
	public void AssociationWithAnOwnerFromAnotherDocumentIsNotWritten()
	{
		//Like reactors (CleanReactors), an association that points to another document's objects is skipped:
		//those handles mean nothing in this file
		(CadDocument doc, DimensionAssociation association) = this.simpleAssociation();
		Assert.True(this.cameBackAssociative(this.simpleAssociation().Doc));   //control
		association.AssociatedEntity = this.dimensionWithEveryCase().Entities.OfType<Dimension>().Single();
		Assert.False(this.cameBackAssociative(doc));
	}

	[Fact]
	public void AssociationWithGeometryFromAnotherDocumentIsNotWritten()
	{
		(CadDocument doc, DimensionAssociation association) = this.simpleAssociation();
		association.FirstPointRef.Geometry = this.dimensionWithEveryCase().Entities.OfType<Line>().First();
		Assert.False(this.cameBackAssociative(doc));
	}

	private CadDocument dimensionWithEveryCase()
	{
		CadDocument doc = new CadDocument();
		Line wall = new Line(XYZ.Zero, new XYZ(10, 0, 0));
		Line other = new Line(new XYZ(5, -5, 0), new XYZ(5, 5, 0));
		doc.Entities.Add(wall);
		doc.Entities.Add(other);
		BlockRecord block = new BlockRecord("COLUMN");
		Line inside = new Line(XYZ.Zero, new XYZ(0, 3, 0));
		block.Entities.Add(inside);
		doc.BlockRecords.Add(block);
		Insert column = new Insert(block) { InsertPoint = new XYZ(20, 0, 0) };
		doc.Entities.Add(column);
		DimensionAligned dimension = new DimensionAligned(XYZ.Zero, new XYZ(20, 0, 0));
		doc.Entities.Add(dimension);

		DimensionAssociation association = new DimensionAssociation
		{
			Dimension = dimension,
			AssociativityFlags = AssociativityFlags.FirstPointReference | AssociativityFlags.SecondPointReference,
		};
		DimensionAssociation.OsnapPointRef first = new DimensionAssociation.OsnapPointRef
		{
			ObjectOsnapType = ObjectOsnapType.Intersection,
			SubentType = SubentType.Edge,
			GsMarker = 1,
			GeometryParameter = 0.5,
			OsnapPoint = new XYZ(5, 0, 0),
			IntersectionSubType = SubentType.Edge,
			IntersectionGsMarker = 1,
		};
		first.GeometryPath.Add(wall);
		first.IntersectionPath.Add(other);
		DimensionAssociation.OsnapPointRef second = new DimensionAssociation.OsnapPointRef
		{
			ObjectOsnapType = ObjectOsnapType.Perpendicular,
			SubentType = SubentType.Edge,
			GsMarker = 1,
			GeometryParameter = 0.4703191426445606,
			OsnapPoint = new XYZ(0, 0, 2e50),
			HasLastPointRef = true,
			LastPointRef = new DimensionAssociation.OsnapPointRef { ObjectOsnapType = ObjectOsnapType.None, GeometryParameter = 4.712388980384689 },
		};
		second.GeometryPath.Add(column);
		second.GeometryPath.Add(inside);
		association.FirstPointRef = first;
		association.SecondPointRef = second;
		dimension.CreateExtendedDictionary().Add("ACAD_DIMASSOC", association);
		return doc;
	}

	private (CadDocument Doc, DimensionAssociation Association) simpleAssociation()
	{
		CadDocument doc = new CadDocument();
		Line wall = new Line(XYZ.Zero, new XYZ(20, 0, 0));
		DimensionAligned dimension = new DimensionAligned(XYZ.Zero, new XYZ(20, 0, 0));
		doc.Entities.Add(wall);
		doc.Entities.Add(dimension);
		DimensionAssociation association = new DimensionAssociation { AssociatedEntity = dimension, AssociativityFlags = AssociativityFlags.FirstPointReference };
		association.FirstPointRef = new DimensionAssociation.OsnapPointRef { ObjectOsnapType = ObjectOsnapType.Endpoint, SubentType = SubentType.Edge };
		association.FirstPointRef.GeometryPath.Add(wall);
		dimension.CreateExtendedDictionary().Add("ACAD_DIMASSOC", association);
		return (doc, association);
	}

	private bool cameBackAssociative(CadDocument doc)
	{
		return this.writeAndRead(doc, "dwg").Entities.OfType<Dimension>().Single().XDictionary?.ContainsKey("ACAD_DIMASSOC") == true;
	}

	private CadDocument writeAndRead(CadDocument doc, string format)
	{
		string path = Path.Combine(Path.GetTempPath(), $"acadsharp-{Guid.NewGuid():N}.{format}");
		try
		{
			if (format == "dwg")
			{
				using (FileStream fs = File.Create(path))
				{
					new DwgWriter(fs, doc).Write();
				}

				return DwgReader.Read(path);
			}

			new DxfWriter(path, doc, false).Write();
			return DxfReader.Read(path);
		}
		finally
		{
			File.Delete(path);
		}
	}
}
