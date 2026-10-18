using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.IO;
using CSMath;
using Xunit;
using System.Linq;

namespace ACadSharp.Tests.IO.DWG;

public class ObjectLayoutRegressionTests
{
	[Theory]
	[InlineData(ACadVersion.AC1014)]
	[InlineData(ACadVersion.AC1018)]
	public void LegacyExtendedDataUsesEncodedByteLength(ACadVersion version)
	{
		var document = new CadDocument();
		document.Header.Version = version;
		var line = new Line();
		document.Entities.Add(line);
		var app = new ACadSharp.Tables.AppId("TEST_EED");
		document.AppIds.Add(app);
		line.ExtendedData.Add(app, new ACadSharp.XData.ExtendedData(new ACadSharp.XData.ExtendedDataRecord[]
		{
			new ACadSharp.XData.ExtendedDataString("Grüße"),
			new ACadSharp.XData.ExtendedDataInteger32(1729),
		}));
		var result = ModelerGeometryWriterTests.Roundtrip(document);
		Assert.True(result.GetCadObject<Line>(line.Handle).ExtendedData.TryGet("TEST_EED", out var data));
		Assert.Equal("Grüße", data.Records[0].RawValue);
		Assert.Equal(1729, data.Records[1].RawValue);
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1032)]
	public void DimensionPointReferencesRoundtrip(ACadVersion version)
	{
		var document = new CadDocument();
		document.Header.Version = version;
		var first = new Line();
		var second = new Circle();
		document.Entities.Add(first);
		document.Entities.Add(second);
		var point = new DimensionAssociation.OsnapPointRef
		{
			ObjectOsnapType = ObjectOsnapType.Intersection,
			Geometry = first, GeometryParameter = 0.75, GsMarker = 42,
			OsnapPoint = new XYZ(1, 2, 3), IntersectionGsMarker = 99,
			HasLastPointRef = true,
			LastPointRef = new DimensionAssociation.OsnapPointRef { OsnapPoint = new XYZ(4, 5, 6) },
		};
		point.GeometryPath.Add(first);
		point.GeometryPath.Add(second);
		point.XrefPaths.Add("primary");
		point.IntersectionGeometry.Add(second);
		point.IntersectionXrefPaths.Add("intersection");
		var association = new DimensionAssociation
		{
			AssociativityFlags = AssociativityFlags.FirstPointReference | AssociativityFlags.SecondPointReference,
			FirstPointRef = point,
		};
		document.RootDictionary.Add("TestAssociation", association);
		var result = ModelerGeometryWriterTests.Roundtrip(document);
		var actual = result.GetCadObject<DimensionAssociation>(association.Handle).FirstPointRef;
		Assert.Equal(2, actual.GeometryPath.Count);
		Assert.Equal(first.Handle, actual.Geometry.Handle);
		Assert.Equal(second.Handle, Assert.Single(actual.IntersectionGeometry).Handle);
		Assert.Equal("primary", Assert.Single(actual.XrefPaths));
		Assert.Equal("intersection", Assert.Single(actual.IntersectionXrefPaths));
		Assert.Equal(0.75, actual.GeometryParameter);
		Assert.Equal(42, actual.GsMarker);
		Assert.Equal(99, actual.IntersectionGsMarker);
		Assert.Equal(new XYZ(1, 2, 3), actual.OsnapPoint);
		Assert.Equal(new XYZ(4, 5, 6), actual.LastPointRef.OsnapPoint);
	}

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1032)]
	public void XRecordHandlesKeepFollowingValuesAligned(ACadVersion version)
	{
		var document = new CadDocument();
		document.Header.Version = version;
		var line = new Line();
		document.Entities.Add(line);
		var record = new XRecord();
		record.CreateEntry(5, line);
		record.CreateEntry(1, "after handle");
		document.RootDictionary.Add("TestRecord", record);
		var result = ModelerGeometryWriterTests.Roundtrip(document);
		var actual = result.GetCadObject<XRecord>(record.Handle);
		Assert.Equal(2, actual.Entries.Count());
		Assert.Equal(line.Handle, actual.Entries.ElementAt(0).GetReference().Handle);
		Assert.Equal("after handle", actual.Entries.ElementAt(1).Value);
	}

	[Fact]
	public void TextFrameWithoutFillKeepsR2018ColumnsAligned()
	{
		var document = new CadDocument();
		document.Header.Version = ACadVersion.AC1032;
		var text = new MText { Value = "frame", BackgroundFillFlags = BackgroundFillFlags.TextFrame };
		document.Entities.Add(text);
		var result = ModelerGeometryWriterTests.Roundtrip(document);
		var actual = result.GetCadObject<MText>(text.Handle);
		Assert.Equal(text.Value, actual.Value);
		Assert.Equal(BackgroundFillFlags.TextFrame, actual.BackgroundFillFlags);
		Assert.Equal(text.IsAnnotative, actual.IsAnnotative);
	}
}
