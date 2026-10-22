using ACadSharp.Objects.Evaluations;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates;

internal class CadBlockLookupActionTemplate : CadBlockActionTemplate
{
	public int? NumberOfRows { get; set; }

	public int? NumberOfColumns { get; set; }

	public List<string> RowValues { get; } = new();

	public CadBlockLookupActionTemplate()
	: base(new BlockLookupAction())
	{
	}

	public CadBlockLookupActionTemplate(BlockLookupAction lookupAction)
		: base(lookupAction)
	{
	}

	protected override void build(CadDocumentBuilder builder)
	{
		BlockLookupAction action = this.CadObject as BlockLookupAction;

		base.build(builder);

		if (this.NumberOfRows.HasValue && this.NumberOfColumns.HasValue)
		{
			for (int i = 0; i < this.NumberOfRows.Value; i++)
			{
				for(int j = 0; j < this.NumberOfColumns.Value; j++)
				{
					action.Columns[j].Rows.Add(this.RowValues[(i * this.NumberOfColumns.Value) + j]);
				}
			}
		}
	}
}
