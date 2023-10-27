using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class LineTypesTable : Table<LineType>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LTYPE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLinetype;

		/// <summary>
		/// Get the ByLayer entry in the table
		/// </summary>
		public LineType ByLayer { get { return this[LineType.ByLayerName]; } }

		/// <summary>
		/// Get the ByBlock entry in the table
		/// </summary>
		public LineType ByBlock { get { return this[LineType.ByBlockName]; } }

		/// <summary>
		/// Get the Continuous entry in the table
		/// </summary>
		public LineType Continuous { get { return this[LineType.ContinuousName]; } }

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