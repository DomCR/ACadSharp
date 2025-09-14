using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTextStyleTemplate : CadTableEntryTemplate<TextStyle>
	{
		public CadTextStyleTemplate(TextStyle entry) : base(entry) { }

		protected override void build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.CadObject.IsShapeFile)
			{

			}
		}
	}
}
