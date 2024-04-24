using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public ulong? StyleHandle { get; set; }

		public ulong NullHandle { get; internal set; }

		public CadTableEntityTemplate(TableEntity table) : base(table) { }

		internal class CadTableAttributeTemplate : ICadObjectTemplate
		{
			private TableEntity.TableAttribute _tableAtt;

			public CadTableAttributeTemplate(TableEntity.TableAttribute tableAtt)
			{
				this._tableAtt = tableAtt;
			}

			public ulong? AttDefHandle { get; internal set; }

			public void Build(CadDocumentBuilder builder)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
