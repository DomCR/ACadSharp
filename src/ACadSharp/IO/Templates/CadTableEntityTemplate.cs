using ACadSharp.Entities;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public ulong? StyleHandle { get; set; }

		public ulong? BlockOwnerHandle { get; set; }

		public ulong? NullHandle { get; internal set; }

		public Cell CurrentCell { get; private set; }

		public TableEntity TableEntity { get { return this.CadObject as TableEntity; } }

		private int _currCellIndex = 0;

		public CadTableEntityTemplate() : base(new TableEntity()) { }

		public CadTableEntityTemplate(TableEntity table) : base(table) { }

		public void CreateCell(CellType type)
		{
			var rowIndex = this._currCellIndex / this.TableEntity.Columns.Count;

			this.CurrentCell = new Cell();
			this.CurrentCell.Type = type;

			this.TableEntity.Rows[rowIndex].Cells.Add(this.CurrentCell);

			this._currCellIndex++;
		}

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
