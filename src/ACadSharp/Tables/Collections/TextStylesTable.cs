namespace ACadSharp.Tables.Collections
{
	public class TextStylesTable : Table<TextStyle>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableStyle;

		protected override string[] defaultEntries { get { return new string[] { TextStyle.DefaultName }; } }

		internal TextStylesTable() : base() { }

		internal TextStylesTable(CadDocument document) : base(document) { }
	}
}