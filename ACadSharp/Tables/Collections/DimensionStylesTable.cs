using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStylesTable : Table<DimensionStyle>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMSTYLE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableDimstyle;

		internal DimensionStylesTable() : base() { }

		internal DimensionStylesTable(CadDocument document) : base(document) { }
	}
}