using ACadSharp.Tables;

namespace ACadSharp.IO.Templates
{
	internal class CadTextStyleTemplate : CadTableEntryTemplate<TextStyle>
	{
		public CadTextStyleTemplate(TextStyle entry) : base(entry) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.CadObject.IsShapeFile)
			{

			}
		}
	}
}
