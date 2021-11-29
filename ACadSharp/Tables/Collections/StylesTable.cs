using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class TextStylesTable : Table<TextStyle>
	{
		public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

		internal TextStylesTable(DxfTableTemplate template) : base(template) { }

		public TextStylesTable(CadDocument document) : base(document)
		{
		}
	}
}