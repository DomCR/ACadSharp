using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.Linq;
using ACadSharp.Tests.Common;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO.DXF;

public class DxfWriterTests : IOTestsBase
{
	public DxfWriterTests(ITestOutputHelper output) : base(output) { }

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteEmptyAsciiTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_empty_sample_{version}_ascii.dxf");

		using (var wr = new DxfWriter(path, doc, false))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}

		this._output.WriteLine("Writer successful");

		using (var re = new DxfReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteEmptyBinaryTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_empty_sample_{version}_binary.dxf");

		using (var wr = new DxfWriter(path, doc, true))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}

		this._output.WriteLine("Writer successful");

		using (var re = new DxfReader(path, this.onNotification))
		{
			CadDocument readed = re.Read();
		}
	}

	[Fact]
	public void WriteFieldWithoutOwnerTest()
	{
		//A FIELD read without its owner is still listed by ACAD_FIELDLIST, the writer must not fail on it
		CadDocument doc = new CadDocument();
		doc.Header.Version = ACadVersion.AC1032;

		FieldList list = new FieldList();
		list.Fields.Add(new Field { FormatString = string.Empty });
		doc.RootDictionary.Remove(CadDictionary.AcadFieldList);
		doc.RootDictionary.Add(CadDictionary.AcadFieldList, list);

		MemoryStream stream = new MemoryStream();
		DxfWriter.Write(stream, doc, false, notification: this.onNotification);

		CadDocument read = DxfReader.Read(new MemoryStream(stream.ToArray()), this.onNotification);
		Assert.NotNull(read);
	}

	[Fact]
	public void WriteDimensionAssociationTest()
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = ACadVersion.AC1032;

		Line line = new Line(XYZ.Zero, new XYZ(10, 0, 0));
		DimensionLinear dimension = new DimensionLinear();
		doc.Entities.Add(line);
		doc.Entities.Add(dimension);

		DimensionAssociation association = new DimensionAssociation
		{
			Dimension = dimension,
			AssociativityFlags = AssociativityFlags.FirstPointReference,
			FirstPointRef = new DimensionAssociation.OsnapPointRef
			{
				ObjectOsnapType = ObjectOsnapType.Endpoint,
				Geometry = line,
				OsnapPoint = new XYZ(10, 0, 0),
			},
		};
		dimension.CreateExtendedDictionary().Add("ACAD_DIMASSOC", association);

		MemoryStream stream = new MemoryStream();
		DxfWriter.Write(stream, doc, false, notification: this.onNotification);

		CadDocument read = DxfReader.Read(new MemoryStream(stream.ToArray()), this.onNotification);
		DimensionLinear readDimension = Assert.Single(read.Entities.OfType<DimensionLinear>());
		DimensionAssociation readAssociation = Assert.IsType<DimensionAssociation>(readDimension.XDictionary["ACAD_DIMASSOC"]);
		Assert.Equal(AssociativityFlags.FirstPointReference, readAssociation.AssociativityFlags);
		Assert.Equal(readDimension.Handle, readAssociation.Dimension.Handle);
		Assert.Equal(new XYZ(10, 0, 0), readAssociation.FirstPointRef.OsnapPoint);
		Assert.Equal(ObjectOsnapType.Endpoint, readAssociation.FirstPointRef.ObjectOsnapType);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void WriteDocumentWithEntitiesTest(ACadVersion version)
	{
		if (version < ACadVersion.AC1015)
			// Not implemented
			return;

		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		List<Entity> entities = new List<Entity>
		{
			EntityFactory.Create<Point>(),
			EntityFactory.Create<Line>(),
			EntityFactory.Create<Polyline2D>(),
			EntityFactory.Create<Polyline3D>(),
			EntityFactory.Create<Line>(),
			EntityFactory.Create<Arc>(),
			EntityFactory.Create<LwPolyline>(),
		};


		doc.Entities.AddRange(entities);

		string path = Path.Combine(TestVariables.OutputSamplesFolder, $"out_sample_{version}_ascii.dxf");

		using (var wr = new DxfWriter(path, doc, false))
		{
			wr.OnNotification += this.onNotification;
			wr.Write();
		}
	}
}