using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.Objects;
using CSMath;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class TableContentSerializationTests
{
	[Theory]
	[InlineData(ACadVersion.AC1024)]
	[InlineData(ACadVersion.AC1027)]
	[InlineData(ACadVersion.AC1032)]
	public void ModernTableRetainsContentReferencesAndGeometry(ACadVersion version)
	{
		var document = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, $"sample_{version}.dwg"));
		var table = document.GetCadObject<TableEntity>(0xA35);
		var field = new Field { FormatString = "" };
		document.RootDictionary.Add("TableField", field);
		var dataLink = new XRecord();
		document.RootDictionary.Add("TableDataLink", dataLink);
		var definition = new AttributeDefinition { Tag = "TABLE_ATTR" };
		table.Block.Entities.Add(definition);
		var cell = table.GetCell(1, 1);
		cell.Contents.Clear();
		var block = new TableEntity.CellContent { ContentType = TableEntity.TableCellContentType.Block, BlockRecord = table.Block };
		block.Attributes.Add(new TableEntity.TableAttribute { AttributeDefinition = definition, Index = 3, Value = "attribute value" });
		cell.Contents.Add(block);
		cell.Contents.Add(new TableEntity.CellContent { ContentType = TableEntity.TableCellContentType.Field, Field = field });
		cell.HasLinkedData = true;
		cell.DataLink = dataLink;
		cell.LinkedRowCount = 2;
		cell.LinkedColumnCount = 3;
		cell.LinkedDataFlags = 17;
		cell.GeometryCollection.Clear();
		cell.GeometryCollection.Add(new TableEntity.CellContentGeometry { DistanceCenter = new XYZ(1, 2, 3), Width = 4, Height = 5, Flags = 6 });
		cell.GeometryCollection.Add(new TableEntity.CellContentGeometry { ContentWidth = 7, ContentHeight = 8 });
		table.Content.FieldReferences.Add(field);
		table.GetCell(1, 2).Content.CadValue.SetValue("");
		var result = ModelerGeometryWriterTests.Roundtrip(document);
		var actual = result.GetCadObject<TableEntity>(table.Handle);
		Assert.Equal(table.Rows.Count, actual.Rows.Count);
		Assert.Equal(table.Columns.Count, actual.Columns.Count);
		Assert.Equal(table.GetCell(0, 0).Content.CadValue.Value, actual.GetCell(0, 0).Content.CadValue.Value);
		Assert.Equal("", actual.GetCell(1, 2).Content.CadValue.Value);
		var actualCell = actual.GetCell(1, 1);
		Assert.Equal(table.Block.Handle, actualCell.Contents[0].BlockRecord.Handle);
		Assert.Equal(field.Handle, actualCell.Contents[1].Field.Handle);
		var attribute = Assert.Single(actualCell.Contents[0].Attributes);
		Assert.Equal(definition.Handle, attribute.AttributeDefinition.Handle);
		Assert.Equal(3, attribute.Index);
		Assert.Equal("attribute value", attribute.Value);
		Assert.Equal(dataLink.Handle, actualCell.DataLink.Handle);
		Assert.Equal(2, actualCell.LinkedRowCount);
		Assert.Equal(3, actualCell.LinkedColumnCount);
		Assert.Equal(17, actualCell.LinkedDataFlags);
		Assert.Equal(2, actualCell.GeometryCollection.Count);
		Assert.Equal(new XYZ(1, 2, 3), actualCell.GeometryCollection[0].DistanceCenter);
		Assert.Equal(8, actualCell.GeometryCollection[1].ContentHeight);
		Assert.Equal(field.Handle, Assert.Single(actual.Content.FieldReferences).Handle);
	}
}
