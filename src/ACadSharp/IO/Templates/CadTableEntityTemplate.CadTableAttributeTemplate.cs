using ACadSharp.Entities;

namespace ACadSharp.IO.Templates;

internal partial class CadTableEntityTemplate
{
	internal class CadTableAttributeTemplate : ICadTemplate
	{
		public ulong? AttDefHandle { get; internal set; }
		internal TableEntity.TableAttribute Attribute => this._tableAtt;

		private TableEntity.TableAttribute _tableAtt;

		public CadTableAttributeTemplate(TableEntity.TableAttribute tableAtt)
		{
			this._tableAtt = tableAtt;
		}

		public void Build(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject<AttributeDefinition>(this.AttDefHandle, out var definition))
				this._tableAtt.AttributeDefinition = definition;
		}
	}
}