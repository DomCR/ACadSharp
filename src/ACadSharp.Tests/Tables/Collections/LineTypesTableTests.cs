using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;

namespace ACadSharp.Tests.Tables.Collections;

public class LineTypesTableTests : TableEntryCommonTests<LineType>
{
	protected override LineType createInstance(string name)
	{
		return new LineType(name);
	}

	protected override Table<LineType> getTable(CadDocument document)
	{
		return document.LineTypes;
	}
}
