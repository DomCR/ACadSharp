using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class UCSTable : Table<UCS>
	{
		public override ObjectType ObjectType => ObjectType.UCS_CONTROL_OBJ;

		internal UCSTable(DxfTableTemplate template) : base(template) { }

		public UCSTable(CadDocument document) : base(document)
		{
		}
	}
}