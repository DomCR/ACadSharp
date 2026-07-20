using ACadSharp.Entities;
using ACadSharp.IO;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DXF;

public class DxfSurfaceTests
{
	[Fact]
	public void ReadGenericSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("SURFACE", "AcDbSurface"));

		Surface surface = Assert.IsType<Surface>(doc.Entities.Single());
		Assert.Equal(6, surface.UIsolines);
		Assert.Equal(7, surface.VIsolines);
		Assert.Equal(SatHeader + "\n" + SatBody, surface.GetAcisText());
	}

	[Fact]
	public void ReadPlaneSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("PLANESURFACE", "AcDbSurface", "AcDbPlaneSurface"));

		PlaneSurface surface = Assert.IsType<PlaneSurface>(doc.Entities.Single());
		Assert.Equal(6, surface.UIsolines);
		Assert.Equal(7, surface.VIsolines);
		Assert.Equal(SatHeader + "\n" + SatBody, surface.GetAcisText());
	}

	[Fact]
	public void ReadExtrudedSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("EXTRUDEDSURFACE", "AcDbSurface", "AcDbExtrudedSurface"));

		ExtrudedSurface surface = Assert.IsType<ExtrudedSurface>(doc.Entities.Single());
		Assert.Equal(6, surface.UIsolines);
		Assert.Equal(7, surface.VIsolines);
		Assert.Equal(SatHeader + "\n" + SatBody, surface.GetAcisText());
	}

	[Fact]
	public void ReadLoftedSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("LOFTEDSURFACE", "AcDbSurface", "AcDbLoftedSurface"));

		Assert.IsType<LoftedSurface>(doc.Entities.Single());
	}

	[Fact]
	public void ReadRevolvedSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("REVOLVEDSURFACE", "AcDbSurface", "AcDbRevolvedSurface"));

		Assert.IsType<RevolvedSurface>(doc.Entities.Single());
	}

	[Fact]
	public void ReadSweptSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("SWEPTSURFACE", "AcDbSurface", "AcDbSweptSurface"));

		Assert.IsType<SweptSurface>(doc.Entities.Single());
	}

	[Fact]
	public void ReadNurbSurface()
	{
		CadDocument doc = readDxf(surfaceDxf("NURBSURFACE", "AcDbSurface", "AcDbNurbSurface"));

		Assert.IsType<NurbSurface>(doc.Entities.Single());
	}

	private const string SatHeader = "400 0 1 0";

	private const string SatBody = "body $-1 $1 $-1 $-1 #";

	//A minimal entities section with one surface entity: the modeler geometry
	//codes (70, 1, 3) carry the SAT text, the AcDbSurface codes 71 and 72 the
	//isoline counts, followed by the empty subclass of the derived surface.
	private static string surfaceDxf(string name, params string[] subclasses)
	{
		StringBuilder dxf = new StringBuilder();
		void add(string code, string value)
		{
			dxf.Append(code).Append('\n').Append(value).Append('\n');
		}

		add("0", "SECTION");
		add("2", "ENTITIES");
		add("0", name);
		add("5", "A1");
		add("100", "AcDbEntity");
		add("8", "0");
		add("100", "AcDbModelerGeometry");
		add("70", "1");
		add("1", SatHeader);
		add("1", SatBody);

		foreach (string subclass in subclasses)
		{
			add("100", subclass);

			if (subclass == "AcDbSurface")
			{
				add("71", "6");
				add("72", "7");
			}
		}

		add("0", "ENDSEC");
		add("0", "EOF");

		return dxf.ToString();
	}

	private static CadDocument readDxf(string dxf)
	{
		using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(dxf)))
		using (DxfReader reader = new DxfReader(stream))
		{
			return reader.Read();
		}
	}
}
