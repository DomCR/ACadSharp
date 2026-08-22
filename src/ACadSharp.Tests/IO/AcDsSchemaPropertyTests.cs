using ACadSharp.IO;
using ACadSharp.Prototype1b;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO;

/// <summary>
/// Every schema in a data store carries an AcDbDs::ID property, and it always has two values.
/// They were read into the same row of the array the value count had just sized, so the first
/// came back overwritten by the second and the second row came back empty.
/// </summary>
public class AcDsSchemaPropertyTests
{
	[Fact]
	public void APropertyWithTwoValuesKeepsBothOfThem()
	{
		CadDocument doc = DwgReader.Read(Path.Combine(TestVariables.SamplesFolder, "sample_AC1032.dwg"));

		SchemaProperty[] properties = doc.DataStorage.SchemaFields
			.SelectMany(f => f.Values)
			.SelectMany(s => s.Properties)
			.Where(p => p.PropertyValueCount > 1)
			.ToArray();

		Assert.NotEmpty(properties);
		foreach (SchemaProperty property in properties)
		{
			//Every row the count promised holds something. Reading them all into row zero left the
			//rows after it untouched, so it is the last row that tells the two readings apart: in
			//this store both values of every such property are non-zero.
			Assert.Equal((int)property.PropertyValueCount, property.Values.GetLength(0));
			for (int i = 0; i < property.Values.GetLength(0); i++)
			{
				int row = i;
				Assert.Contains(Enumerable.Range(0, property.Values.GetLength(1)), j => property.Values[row, j] != 0);
			}
		}
	}
}
