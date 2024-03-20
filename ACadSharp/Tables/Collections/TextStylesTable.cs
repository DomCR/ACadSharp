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

		/// <inheritdoc/>
		public override void Add(TextStyle item)
		{
			if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Filename))
			{
				//TextStyles seem to accept empty names
				this.add(item.Name, item);
				return;
			}

			base.Add(item);
		}
	}
}