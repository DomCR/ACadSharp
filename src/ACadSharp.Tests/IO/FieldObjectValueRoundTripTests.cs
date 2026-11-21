using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

public class FieldObjectValueRoundTripTests
{
	[Fact]
	public void ObjectValueIsResolvedDwg()
	{
		//The value of an AcObjProp field that points to the object ("ObjectPropertyId") was read as a handle and
		//never resolved: it came back null, the writer saved 0 and AutoCAD showed "####" for the field on open.
		CadDocument doc = this.readBackDwg(this.textWithAreaField());
		this.assertObjectValue(doc);
	}

	[Fact]
	public void ObjectValueIsResolvedDxf()
	{
		//Also covers the DXF writer: it wrote the handle of each child field (360) but never the child itself.
		CadDocument doc = this.readBackDxf(this.textWithAreaField());
		this.assertObjectValue(doc);
	}

	private void assertObjectValue(CadDocument doc)
	{
		LwPolyline room = doc.Entities.OfType<LwPolyline>().Single();
		MText text = doc.Entities.OfType<MText>().Single();
		Assert.True(text.XDictionary.TryGetEntry("ACAD_FIELD", out CadDictionary fields));
		Assert.True(fields.TryGetEntry("TEXT", out Field root));

		Field area = Assert.Single(root.Children);
		Assert.Same(room, Assert.Single(area.CadObjects));
		Assert.True(area.Values.TryGetValue("ObjectPropertyId", out CadValue value));
		Assert.Equal(CadValueType.Handle, value.ValueType);
		Assert.Same(room, value.Value);
	}

	private CadDocument textWithAreaField()
	{
		CadDocument doc = new CadDocument();
		LwPolyline room = new LwPolyline { IsClosed = true };
		room.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		room.Vertices.Add(new LwPolyline.Vertex(new XY(4, 0)));
		room.Vertices.Add(new LwPolyline.Vertex(new XY(4, 4)));
		room.Vertices.Add(new LwPolyline.Vertex(new XY(0, 4)));
		doc.Entities.Add(room);
		MText text = new MText { Value = "16.00", InsertPoint = XYZ.Zero, Height = 1 };
		doc.Entities.Add(text);

		Field root = new Field { EvaluatorId = "_text", FieldCode = "%<\\_FldIdx 0>%", FormatString = "x", EvaluationErrorMessage = "x" };
		Field area = new Field { EvaluatorId = "AcObjProp", FieldCode = "\\AcObjProp Object(%<\\_ObjIdx 0>%).Area", FormatString = "x", EvaluationErrorMessage = "x" };
		area.CadObjects.Add(room);
		CadValue value = new CadValue { Format = "x", FormattedValue = "x" };
		value.SetValue(room, CadValueType.Handle);
		area.Values.Add("ObjectPropertyId", value);
		root.Children.Add(area);

		CadDictionary fields = new CadDictionary("ACAD_FIELD");
		text.CreateExtendedDictionary().Add(fields);
		fields.Add("TEXT", root);
		return doc;
	}

	private CadDocument readBackDwg(CadDocument doc)
	{
		MemoryStream stream = new MemoryStream();
		using (DwgWriter writer = new DwgWriter(stream, doc))
		{
			writer.Write();
		}

		return DwgReader.Read(new MemoryStream(stream.ToArray()));
	}

	private CadDocument readBackDxf(CadDocument doc)
	{
		MemoryStream stream = new MemoryStream();
		using (DxfWriter writer = new DxfWriter(stream, doc, false))
		{
			writer.Write();
		}

		return DxfReader.Read(new MemoryStream(stream.ToArray()));
	}
}
