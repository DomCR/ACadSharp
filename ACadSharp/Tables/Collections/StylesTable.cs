using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class StylesTable : Table<Style>
	{
		public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

		public StylesTable() { }
		internal StylesTable(DxfTableTemplate template) : base(template) { }
	}
}