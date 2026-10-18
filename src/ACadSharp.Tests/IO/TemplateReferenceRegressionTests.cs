using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using Xunit;

namespace ACadSharp.Tests.IO;

public class TemplateReferenceRegressionTests
{
	[Fact]
	public void ExplicitSpaceEntitiesSupplementLegacyChainsWithoutDuplicates()
	{
		var document = new CadDocument();
		var builder = new DwgDocumentBuilder(ACadVersion.AC1018, document, new DwgReaderConfiguration());
		var block = new BlockRecord("TestSpace") { Handle = 0x100 };
		var first = new Line { Handle = 0x101 };
		var extra = new Circle { Handle = 0x102 };
		var template = new CadBlockRecordTemplate(block) { FirstEntityHandle = first.Handle, LastEntityHandle = first.Handle };
		template.OwnedObjectsHandlers.Add(first.Handle);
		template.OwnedObjectsHandlers.Add(extra.Handle);
		builder.AddTemplate(template);
		builder.AddTemplate(new CadEntityTemplate(first));
		builder.AddTemplate(new CadEntityTemplate(extra));
		template.Build(builder);
		Assert.Equal(2, block.Entities.Count);
		Assert.Same(block, first.Owner);
		Assert.Same(block, extra.Owner);
	}

	[Fact]
	public void NamedUcsOverridesBaseUcsAndVisualStyleIsBound()
	{
		var document = new CadDocument();
		var builder = new DwgDocumentBuilder(ACadVersion.AC1032, document, new DwgReaderConfiguration());
		var baseUcs = new UCS("Base") { Handle = 0x100, Origin = new XYZ(1, 2, 3) };
		var namedUcs = new UCS("Named") { Handle = 0x101, Origin = new XYZ(4, 5, 6), Elevation = 7 };
		var style = new VisualStyle("Custom") { Handle = 0x102 };
		builder.AddTemplate(new CadTableEntryTemplate<UCS>(baseUcs));
		builder.AddTemplate(new CadTableEntryTemplate<UCS>(namedUcs));
		builder.AddTemplate(new CadTemplate<VisualStyle>(style));
		var viewport = new Viewport { Handle = 0x103 };
		var template = new CadViewportTemplate(viewport)
		{
			BaseUcsHandle = baseUcs.Handle, NamedUcsHandle = namedUcs.Handle, VisualStyleHandle = style.Handle,
		};
		template.Build(builder);
		Assert.Equal(namedUcs.Origin, viewport.UcsOrigin);
		Assert.Equal(namedUcs.XAxis, viewport.UcsXAxis);
		Assert.Equal(7, viewport.Elevation);
		Assert.True(viewport.UcsPerViewport);
		Assert.Same(style, viewport.VisualStyle);
	}
}
