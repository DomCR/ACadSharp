namespace ACadSharp.Tables.Collections
{
	public class TextStylesTable : Table<TextStyle>
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableStyle;

		protected override string[] _defaultEntries { get { return new string[] { TextStyle.DefaultName }; } }

		internal TextStylesTable() : base() { }

		internal TextStylesTable(CadDocument document) : base(document) { }

		public override void Add(TextStyle item)
		{
			if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Filename))
			{
				this.add(item.Filename, item);
				return;
			}

			base.Add(item);
		}
	}
}