using ACadSharp.IO.Templates;
using System;

namespace ACadSharp.Tables.Collections
{
	public class TextStylesTable : Table<TextStyle>
	{
		public override ObjectType ObjectType => ObjectType.STYLE_CONTROL_OBJ;

		public TextStylesTable(CadDocument document) : base(document) { }

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