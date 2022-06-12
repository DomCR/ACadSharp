using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class LineTypesTable : Table<LineType>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LTYPE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLinetype;

		protected override string[] _defaultEntries
		{
			get
			{
				return new string[]
				{
					LineType.ByLayerName,
					LineType.ByBlockName,
					LineType.ContinuousName
				};
			}
		}

		internal LineTypesTable() : base() { }

		internal LineTypesTable(CadDocument document) : base(document) { }
	}
}