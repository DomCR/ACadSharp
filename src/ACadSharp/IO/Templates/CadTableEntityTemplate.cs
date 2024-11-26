using ACadSharp.Entities;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public ulong? StyleHandle { get; set; }

		public ulong? BlockOwnerHandle { get; set; }

		public ulong? NullHandle { get; internal set; }

		public CadTableEntityTemplate() : base(new TableEntity()) { }

		public CadTableEntityTemplate(TableEntity table) : base(table) { }

		internal class CadTableAttributeTemplate : ICadObjectTemplate
		{
			public ulong? AttDefHandle { get; internal set; }
			
			public CadObject CadObject { get; }

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
}
