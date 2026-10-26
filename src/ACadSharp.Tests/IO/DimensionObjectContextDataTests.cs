using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

public class DimensionObjectContextDataTests : IOTestsBase
{
	private const string _fileName = "annotative/annotative_dimensions_AC1032";

	public DimensionObjectContextDataTests(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
	public void DwgAndDxfMatch()
	{
		var dwg = contextData(DwgReader.Read(path("dwg"), this.onNotification));
		var dxf = contextData(DxfReader.Read(path("dxf"), this.onNotification));

		Assert.Equal(dxf.Count, dwg.Count);

		foreach (DimensionObjectContextData d in dwg.Values)
		{
			DimensionObjectContextData x = dxf[d.Handle];

			Assert.Equal(x.GetType(), d.GetType());
			Assert.Equal(x.Scale.Name, d.Scale.Name);
			Assert.Equal(x.Block.Name, d.Block.Name);
			Assert.Equal(round(x.TextLocation), round(d.TextLocation));
			Assert.Equal(x.IsDefaultTextLocation, d.IsDefaultTextLocation);
			Assert.Equal(x.TextRotation, d.TextRotation);
			Assert.Equal(x.OverrideFlags, d.OverrideFlags);
			Assert.Equal(x.TextOutsideExtensions, d.TextOutsideExtensions);
			Assert.Equal(x.SuppressOutsideExtensions, d.SuppressOutsideExtensions);
			Assert.Equal(x.TextInsideExtensions, d.TextInsideExtensions);
			Assert.Equal(x.FlipFirstArrow, d.FlipFirstArrow);
			Assert.Equal(x.FlipSecondArrow, d.FlipSecondArrow);

			switch (d)
			{
				case AlignedDimensionObjectContextData a:
					Assert.Equal(round(((AlignedDimensionObjectContextData)x).DimensionLinePoint), round(a.DimensionLinePoint));
					break;
				case AngularDimensionObjectContextData a:
					Assert.Equal(round(((AngularDimensionObjectContextData)x).ArcPoint), round(a.ArcPoint));
					break;
				case DiametricDimensionObjectContextData a:
					Assert.Equal(round(((DiametricDimensionObjectContextData)x).FirstArcPoint), round(a.FirstArcPoint));
					Assert.Equal(round(((DiametricDimensionObjectContextData)x).DefinitionPoint), round(a.DefinitionPoint));
					break;
				case OrdinateDimensionObjectContextData a:
					Assert.Equal(round(((OrdinateDimensionObjectContextData)x).DefinitionPoint), round(a.DefinitionPoint));
					Assert.Equal(round(((OrdinateDimensionObjectContextData)x).LeaderEndpoint), round(a.LeaderEndpoint));
					break;
				case RadialDimensionObjectContextData a:
					Assert.Equal(round(((RadialDimensionObjectContextData)x).FirstArcPoint), round(a.FirstArcPoint));
					break;
			}
		}
	}

	[Fact]
	public void ReadDwg()
	{
		var all = contextData(DwgReader.Read(path("dwg"), this.onNotification)).Values.ToList();

		//7 annotative dimensions, each with a 1:1 and a 1:20 representation
		//the jogged radial dimension (LARGE_RADIAL_DIMENSION) is not read as an entity, so its 2 are not reached
		Assert.Equal(14, all.Count);
		Assert.Equal(4, all.OfType<AlignedDimensionObjectContextData>().Count());
		Assert.Equal(4, all.OfType<AngularDimensionObjectContextData>().Count());
		Assert.Equal(2, all.OfType<DiametricDimensionObjectContextData>().Count());
		Assert.Equal(2, all.OfType<OrdinateDimensionObjectContextData>().Count());
		Assert.Equal(2, all.OfType<RadialDimensionObjectContextData>().Count());
		Assert.All(all, d => Assert.NotNull(d.Block));
		Assert.Equal(14, all.Select(d => d.Block.Name).Distinct().Count());

		var diametric = all.OfType<DiametricDimensionObjectContextData>().First();
		Assert.Equal(new XY(240, 30), round(diametric.TextLocation));
		Assert.False(diametric.IsDefaultTextLocation);
		Assert.Equal(new XYZ(224, 18, 0), round(diametric.FirstArcPoint));
		Assert.Equal(new XYZ(176, -18, 0), round(diametric.DefinitionPoint));

		var ordinate = all.OfType<OrdinateDimensionObjectContextData>().First();
		Assert.True(ordinate.IsDefaultTextLocation);
		Assert.Equal(new XYZ(120, -40, 0), round(ordinate.LeaderEndpoint));
	}

	[Fact]
	public void ReadOverrides()
	{
		//The 1:20 representation of some dimensions has overridden variables
		foreach (CadDocument doc in new[] { DwgReader.Read(path("dwg"), this.onNotification), DxfReader.Read(path("dxf"), this.onNotification) })
		{
			var all = contextData(doc).Values.ToList();

			var tix = Assert.Single(all, d => d.TextInsideExtensions);
			Assert.IsType<AlignedDimensionObjectContextData>(tix);
			Assert.Equal(8, tix.OverrideFlags);

			var tofl = Assert.Single(all, d => d.TextOutsideExtensions);
			Assert.IsType<AlignedDimensionObjectContextData>(tofl);
			Assert.Equal(1, tofl.OverrideFlags);

			Assert.IsType<AngularDimensionObjectContextData>(Assert.Single(all, d => d.OverrideFlags == 4));
			Assert.IsType<DiametricDimensionObjectContextData>(Assert.Single(all, d => d.OverrideFlags == 16));
			Assert.IsType<AngularDimensionObjectContextData>(Assert.Single(all, d => d.FlipSecondArrow));
		}

		var dxf = contextData(DxfReader.Read(path("dxf"), this.onNotification)).Values;
		Assert.Equal(TextArrowFitType.BestFit, Assert.Single(dxf, d => d.OverrideFlags == 4).DimensionTextArrowFit);
		Assert.Equal(TextMovement.FreeTextPosition, Assert.Single(dxf, d => d.OverrideFlags == 16).TextMovement);
	}

	private static Dictionary<ulong, DimensionObjectContextData> contextData(CadDocument doc)
	{
		var result = new Dictionary<ulong, DimensionObjectContextData>();
		foreach (Entity e in doc.Entities)
		{
			collect(e.XDictionary, result);
		}
		return result;
	}

	private static void collect(CadDictionary dictionary, Dictionary<ulong, DimensionObjectContextData> result)
	{
		if (dictionary == null)
		{
			return;
		}

		foreach (NonGraphicalObject item in dictionary)
		{
			if (item is DimensionObjectContextData data)
			{
				result[data.Handle] = data;
			}
			else if (item is CadDictionary sub)
			{
				collect(sub, result);
			}
		}
	}

	private static string path(string ext)
	{
		return Path.Combine(TestVariables.SamplesFolder, $"{_fileName}.{ext}");
	}

	private static XY round(XY p) => new XY(System.Math.Round(p.X, 6), System.Math.Round(p.Y, 6));

	private static XYZ round(XYZ p) => new XYZ(System.Math.Round(p.X, 6), System.Math.Round(p.Y, 6), System.Math.Round(p.Z, 6));
}
