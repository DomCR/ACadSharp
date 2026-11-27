using ACadSharp.Objects.Mechanical;
using Xunit;

namespace ACadSharp.Tests.Objects.Mechanical;

public class AcmBomTests
{
	[Fact]
	public void MechanicalBomObjectsHaveExpectedClassNames()
	{
		AssertClassNames(new AcmBom(), DxfFileToken.AcmBom, DxfSubclassMarker.Bom);
		AssertClassNames(new AcmBomRow(), DxfFileToken.AcmBomRow, DxfSubclassMarker.BomRow);
		AssertClassNames(new AcmDataEntryBlock(), DxfFileToken.AcmDataEntryBlock, DxfSubclassMarker.DataEntryBlock);
		AssertClassNames(new AcmDataEntryPart(), DxfFileToken.AcmDataEntryPart, DxfSubclassMarker.DataEntryPart);
	}

	[Fact]
	public void MechanicalBomObjectsHaveEmptyCollectionsByDefault()
	{
		AcmBom bom = new AcmBom();
		AcmBomRow row = new AcmBomRow();
		AcmDataEntryBlock blockData = new AcmDataEntryBlock();
		AcmDataEntryPart partData = new AcmDataEntryPart();

		Assert.Empty(bom.Rows);
		Assert.Empty(bom.RowNames);
		Assert.Empty(bom.PartLists);
		Assert.Empty(row.RawValues);
		Assert.Empty(row.PartReferences);
		Assert.Empty(row.Balloons);
		Assert.Empty(blockData.Attributes);
		Assert.Empty(blockData.RawValues);
		Assert.Empty(blockData.References);
		Assert.Empty(partData.Attributes);
		Assert.Empty(partData.RawAttributeValues);
		Assert.Empty(partData.PartReferences);
		Assert.Empty(partData.BomRows);
	}

	[Fact]
	public void MechanicalBomObjectGraphStoresRowData()
	{
		AcmDataEntryPart partData = new AcmDataEntryPart
		{
			EntryId = 42,
		};
		partData.Attributes.Add(new AcmDataEntryAttribute
		{
			Name = "PARTNO",
			Value = "P-100",
		});

		AcmBomRow row = new AcmBomRow
		{
			ItemName = "1",
			DataEntry = partData,
		};
		partData.BomRows.Add(row);

		AcmBom bom = new AcmBom
		{
			Name = "MAIN",
			ItemNumberStart = "1",
			ItemNumberStep = 1,
		};
		bom.RowNames.Add("1");
		bom.Rows.Add(row);

		Assert.Same(row, Assert.Single(bom.Rows));
		Assert.Same(partData, row.DataEntry);
		Assert.Same(row, Assert.Single(partData.BomRows));
		AcmDataEntryAttribute attribute = Assert.Single(partData.Attributes);
		Assert.Equal("PARTNO", attribute.Name);
		Assert.Equal("P-100", attribute.Value);
	}

	private static void AssertClassNames(CadObject cadObject, string objectName, string subclassMarker)
	{
		Assert.Equal(objectName, cadObject.ObjectName);
		Assert.Equal(subclassMarker, cadObject.SubclassMarker);
	}
}
