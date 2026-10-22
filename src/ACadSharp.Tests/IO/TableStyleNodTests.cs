using ACadSharp.IO;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ACadSharp.Tests.IO;

/// <summary>
/// A document written without the ACAD_TABLESTYLE named object dictionary entry must be read
/// back without the reader re-creating that entry.
/// </summary>
public class TableStyleNodTests : IOTestsBase
{
	public TableStyleNodTests(ITestOutputHelper output) : base(output) { }

	public static TheoryData<ACadVersion> Versions { get; } = new TheoryData<ACadVersion>
	{
		ACadVersion.AC1027,
		ACadVersion.AC1032,
	};

	[Theory]
	[MemberData(nameof(Versions))]
	public void RemovedTableStyleShouldNotBeRestoredInTheNodOnDxfWrite(ACadVersion version)
	{
		CadDocument doc = this.createDocumentWithoutTableStyle(version);

		string path = Path.Combine(TestVariables.OutputSingleCasesFolder, $"out_nod_tablestyle_{version}.dxf");

		using (DxfWriter writer = new DxfWriter(path, doc, false))
		{
			writer.Write();
		}

		CadDocument readed = new DxfReader(path).Read();

		Assert.False(readed.RootDictionary.ContainsKey(CadDictionary.AcadTableStyle));
		Assert.Null(readed.TableStyles);
	}

	[Theory]
	[MemberData(nameof(Versions))]
	public void RemovedTableStyleShouldNotBeRestoredInTheNodOnDwgWrite(ACadVersion version)
	{
		CadDocument doc = this.createDocumentWithoutTableStyle(version);

		string path = Path.Combine(TestVariables.OutputSingleCasesFolder, $"out_nod_tablestyle_{version}.dwg");

		using (DwgWriter writer = new DwgWriter(path, doc))
		{
			writer.Write();
		}

		CadDocument readed = new DwgReader(path).Read();

		Assert.False(readed.RootDictionary.ContainsKey(CadDictionary.AcadTableStyle));
		Assert.Null(readed.TableStyles);
	}

	private CadDocument createDocumentWithoutTableStyle(ACadVersion version)
	{
		CadDocument doc = new CadDocument();
		doc.Header.Version = version;

		Assert.True(doc.RootDictionary.ContainsKey(CadDictionary.AcadTableStyle));

		doc.TableStyles.Remove(TableStyle.DefaultName);
		doc.RootDictionary.Remove(CadDictionary.AcadTableStyle);

		return doc;
	}
}
