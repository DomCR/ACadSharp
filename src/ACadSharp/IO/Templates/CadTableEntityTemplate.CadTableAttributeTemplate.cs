using ACadSharp.Entities;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableAttributeTemplate : ICadTemplate
	{
		public ulong? AttDefHandle { get; internal set; }

		private TableEntity.TableAttribute _tableAtt;

		public CadTableAttributeTemplate(TableEntity.TableAttribute tableAtt)
		{
			this._tableAtt = tableAtt;
		}

		public void Build(CadDocumentBuilder builder)
		{
			throw new System.NotImplementedException();
		}
	}
}