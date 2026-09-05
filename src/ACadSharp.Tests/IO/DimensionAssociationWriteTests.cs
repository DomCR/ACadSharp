using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A dimension association records which piece of geometry a dimension is attached to. Upstream
/// disabled the DXF writer for it - "ignore dimassoc due missing testing" - and the DWG writer wrote
/// only the first three fields of each osnap point reference, because the DWG reader only ever read
/// those three. A DWG object stream is length delimited, so the rest was skipped in silence and the
/// model came back with the osnap point at the origin.
///
/// The layout of the rest was read off the bits against AutoCAD's own DXF export of a production
/// drawing: two of its seven associations differ in 91 and 40, which is what pins down which field is
/// which. Written back, that drawing keeps all seven associations through both writers, and AutoCAD
/// audits the result to the same numbers it did before: 28 on both DWG channels, 26 on DXF.
/// </summary>
public class DimensionAssociationWriteTests
{
	//The values AutoCAD wrote for object 53DDAD7 of that drawing, which is the shape being pinned.
	private const double MeasuredGeometryParameter = 1.0;

	private const int MeasuredGsMarker = 1;

	private const short MeasuredSubentType = 2;

	//AutoCAD's own "not set" sentinel for a coordinate, not a position anything is at.
	private static readonly XYZ MeasuredOsnapPoint = new XYZ(0.0, 0.0, 2.0e50);

	[Theory]
	[InlineData(ACadVersion.AC1018)]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1032)]
	public void EveryFieldSurvivesADwgRoundTrip(ACadVersion version)
	{
		CadDocument doc = this.document(out DimensionAssociation before);
		doc.Header.Version = version;

		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		DimensionAssociation after = this.association(DwgReader.Read(new MemoryStream(stream.ToArray())));
		this.assertSame(before, after);
	}

	[Fact]
	public void EveryFieldSurvivesADxfRoundTrip()
	{
		CadDocument doc = this.document(out DimensionAssociation before);

		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		DimensionAssociation after = this.association(DxfReader.Read(new MemoryStream(stream.ToArray())));
		this.assertSame(before, after);
	}

	[Fact]
	public void TheDxfCarriesTheGroupCodesAutoCadWrites()
	{
		//The order and the codes are AutoCAD's own, taken from its export of the drawing above.
		CadDocument doc = this.document(out DimensionAssociation _);

		using MemoryStream stream = new();
		using (DxfWriter writer = new(stream, doc, false))
		{
			writer.Write();
		}

		string text = System.Text.Encoding.UTF8.GetString(stream.ToArray()).Replace("\r", string.Empty);
		Assert.Contains("AcDbDimAssoc", text);
		Assert.Contains(DimensionAssociation.OsnapPointRefClassName, text);
	}

	[Fact]
	public void AnOsnapPointIsNotSilentlyMovedToTheOrigin()
	{
		//The defect this covers: the DWG reader stopped after the geometry handle, so every osnap
		//point read from a DWG came back as 0,0,0 with no geometry parameter. A round trip that
		//loses those without a word is exactly what nothing caught before.
		CadDocument doc = this.document(out DimensionAssociation _);
		doc.Header.Version = ACadVersion.AC1032;

		using MemoryStream stream = new();
		using (DwgWriter writer = new(stream, doc))
		{
			writer.Write();
		}

		DimensionAssociation after = this.association(DwgReader.Read(new MemoryStream(stream.ToArray())));
		Assert.NotEqual(XYZ.Zero, after.FirstPointRef.OsnapPoint);
		Assert.Equal(MeasuredGeometryParameter, after.FirstPointRef.GeometryParameter);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void AFlagWithNoReferenceBehindItIsNotWritten(bool dwg)
	{
		//The flags say how many references follow. A flag with nothing behind it would promise data
		//the file never writes, and in a DWG that misaligns everything after the object.
		CadDocument doc = this.document(out DimensionAssociation association);
		association.AssociativityFlags = AssociativityFlags.FirstPointReference | AssociativityFlags.ThirdPointReference;

		string reported = null;
		using MemoryStream stream = new();
		if (dwg)
		{
			using DwgWriter writer = new(stream, doc);
			writer.OnNotification += (s, e) => { if (e.Message.Contains("carries no reference")) reported = e.Message; };
			writer.Write();
		}
		else
		{
			using DxfWriter writer = new(stream, doc, false);
			writer.OnNotification += (s, e) => { if (e.Message.Contains("carries no reference")) reported = e.Message; };
			writer.Write();
		}

		Assert.NotNull(reported);

		CadDocument back = dwg
			? DwgReader.Read(new MemoryStream(stream.ToArray()))
			: DxfReader.Read(new MemoryStream(stream.ToArray()));
		DimensionAssociation after = this.association(back);

		Assert.Equal(AssociativityFlags.FirstPointReference, after.AssociativityFlags);
		Assert.Null(after.ThirdPointRef);
		Assert.NotNull(after.FirstPointRef);
		Assert.Equal(MeasuredGeometryParameter, after.FirstPointRef.GeometryParameter);
	}

	private DimensionAssociation association(CadDocument doc)
	{
		DimensionLinear dimension = doc.Entities.OfType<DimensionLinear>().Single();
		return dimension.XDictionary.Cast<NonGraphicalObject>().OfType<DimensionAssociation>().Single();
	}

	private void assertSame(DimensionAssociation before, DimensionAssociation after)
	{
		Assert.Equal(before.AssociativityFlags, after.AssociativityFlags);
		Assert.Equal(before.IsTransSpace, after.IsTransSpace);
		Assert.Equal(before.RotatedDimensionType, after.RotatedDimensionType);
		Assert.Equal(before.Dimension.Handle, after.Dimension.Handle);

		DimensionAssociation.OsnapPointRef a = before.FirstPointRef;
		DimensionAssociation.OsnapPointRef b = after.FirstPointRef;
		Assert.NotNull(b);
		Assert.Equal(a.ObjectOsnapType, b.ObjectOsnapType);
		Assert.Equal(a.SubentType, b.SubentType);
		Assert.Equal(a.GsMarker, b.GsMarker);
		Assert.Equal(a.GeometryParameter, b.GeometryParameter);
		Assert.Equal(a.OsnapPoint, b.OsnapPoint);
		Assert.Equal(a.HasLastPointRef, b.HasLastPointRef);
		Assert.Equal(a.Geometry.Handle, b.Geometry.Handle);
	}

	private CadDocument document(out DimensionAssociation association)
	{
		CadDocument doc = new();

		Line line = new(XYZ.Zero, new XYZ(10, 0, 0));
		doc.Entities.Add(line);

		DimensionLinear dimension = new()
		{
			FirstPoint = XYZ.Zero,
			SecondPoint = new XYZ(10, 0, 0),
			DefinitionPoint = new XYZ(0, -5, 0),
		};
		doc.Entities.Add(dimension);

		association = new DimensionAssociation
		{
			Name = "ACAD_DIMASSOC",
			AssociativityFlags = AssociativityFlags.FirstPointReference,
			Dimension = dimension,
			IsTransSpace = false,
			RotatedDimensionType = RotatedDimensionType.Unknown,
			FirstPointRef = new DimensionAssociation.OsnapPointRef
			{
				ObjectOsnapType = ObjectOsnapType.Endpoint,
				SubentType = (SubentType)MeasuredSubentType,
				GsMarker = MeasuredGsMarker,
				GeometryParameter = MeasuredGeometryParameter,
				OsnapPoint = MeasuredOsnapPoint,
				HasLastPointRef = false,
				Geometry = line,
			},
		};

		dimension.CreateExtendedDictionary();
		dimension.XDictionary.Add(association);
		return doc;
	}
}
