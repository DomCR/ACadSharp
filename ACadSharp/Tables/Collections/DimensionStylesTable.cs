using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class DimensionStylesTable : Table<DimensionStyle>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.DIMSTYLE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableDimstyle;

		protected override string[] _defaultEntries { get { return new string[] { DimensionStyle.DefaultName }; } }

		internal DimensionStylesTable() : base() { }

		internal DimensionStylesTable(CadDocument document) : base(document) { }
	}
}