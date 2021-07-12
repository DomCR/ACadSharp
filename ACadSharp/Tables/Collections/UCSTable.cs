using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class UCSTable : Table<UCS>
	{
		public override ObjectType ObjectType => ObjectType.UCS_CONTROL_OBJ;

		public UCSTable() { }
		internal UCSTable(DxfTableTemplate template) : base(template) { }
	}
}