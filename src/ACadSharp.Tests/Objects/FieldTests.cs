using ACadSharp.Entities;
using ACadSharp.Objects;
using CSMath;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.Objects;

public class FieldTests
{
	[Fact]
	public void CloneDoesNotShareChildrenOrValues()
	{
		CadDocument doc = this.textWithAreaField(out Field root);
		Field original = Assert.Single(root.Children);

		Field clone = (Field)root.Clone();
		Field child = Assert.Single(clone.Children);

		Assert.NotSame(original, child);
		Assert.Same(clone, child.Owner);
		Assert.NotSame(root.Values, clone.Values);
		Assert.NotSame(original.CadObjects, child.CadObjects);
		Assert.Same(original.CadObjects[0], child.CadObjects[0]);

		child.Values["ObjectPropertyId"].FormattedValue = "changed in the clone";
		Assert.Equal("x", original.Values["ObjectPropertyId"].FormattedValue);
	}

	[Fact]
	public void ClonedFieldCanBeAddedToAnotherDocument()
	{
		//The default clone shared the child fields, which stayed owned by the original field and its document:
		//adding the clone to another document threw.
		this.textWithAreaField(out Field root);

		CadDocument other = new CadDocument();
		MText text = new MText { Value = "16.00", InsertPoint = XYZ.Zero, Height = 1 };
		other.Entities.Add(text);
		CadDictionary fields = new CadDictionary("ACAD_FIELD");
		text.CreateExtendedDictionary().Add(fields);
		Field clone = (Field)root.Clone();
		fields.Add("TEXT", clone);

		Assert.Same(other, Assert.Single(clone.Children).Document);
	}

	private CadDocument textWithAreaField(out Field root)
	{
		CadDocument doc = new CadDocument();
		LwPolyline room = new LwPolyline { IsClosed = true };
		room.Vertices.Add(new LwPolyline.Vertex(new XY(0, 0)));
		room.Vertices.Add(new LwPolyline.Vertex(new XY(4, 0)));
		room.Vertices.Add(new LwPolyline.Vertex(new XY(4, 4)));
		doc.Entities.Add(room);
		MText text = new MText { Value = "16.00", InsertPoint = XYZ.Zero, Height = 1 };
		doc.Entities.Add(text);

		root = new Field { EvaluatorId = "_text", FieldCode = "%<\\_FldIdx 0>%", FormatString = "x", EvaluationErrorMessage = "x" };
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
}
