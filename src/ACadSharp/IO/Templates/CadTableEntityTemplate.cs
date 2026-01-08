using ACadSharp.Entities;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.IO.Templates
{
	internal class CadTableEntityTemplate : CadInsertTemplate
	{
		public ulong? BlockOwnerHandle { get; set; }

		public Cell CurrentCell { get { return this.CurrentCellTemplate.Cell; } }

		public CadTableCellTemplate CurrentCellTemplate { get; private set; }

		// Horizontal cell margin; override applied at the table entity level
		public double? HorizontalMargin { get; set; }

		public ulong? NullHandle { get; internal set; }

		public ulong? StyleHandle { get; set; }

		public TableEntity TableEntity { get { return this.CadObject as TableEntity; } }

		public List<CadTableCellTemplate> CadTableCellTemplates { get; } = new();

		private int _currCellIndex = 0;

		public CadTableEntityTemplate() : base(new TableEntity())
		{
		}

		public CadTableEntityTemplate(TableEntity table) : base(table)
		{
		}

		public void CreateCell(CellType type)
		{
			var rowIndex = this._currCellIndex / this.TableEntity.Columns.Count;

			var cell = new Cell();
			cell.Type = type;

			this.TableEntity.Rows[rowIndex].Cells.Add(cell);

			this.CurrentCellTemplate = new CadTableCellTemplate(cell);

			this.CadTableCellTemplates.Add(this.CurrentCellTemplate);

			this._currCellIndex++;
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);

			foreach (var cellTemplate in this.CadTableCellTemplates)
			{
				cellTemplate.Build(builder);
			}
		}

		internal class CadCellStyleTemplate : CadTableCellContentFormatTemplate
		{
			public List<Tuple<CellBorder, ulong>> BorderLinetypePairs { get; set; } = new();

			public CellStyle CellStyle { get { return this.Format as CellStyle; } }

			public CadCellStyleTemplate() : base(new CellStyle())
			{
			}

			public CadCellStyleTemplate(CellStyle style) : base(style)
			{
			}
		}

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

		internal class CadTableCellContentFormatTemplate : ICadTemplate
		{
			public ContentFormat Format { get; }

			public ulong? TextStyleHandle { get; set; }

			public string TextStyleName { get; set; }

			public CadTableCellContentFormatTemplate(ContentFormat format)
			{
				this.Format = format;
			}

			public void Build(CadDocumentBuilder builder)
			{
				throw new System.NotImplementedException();
			}
		}

		internal class CadTableCellContentTemplate : ICadTemplate
		{
			public ulong? BlockRecordHandle { get; set; }

			public TableEntity.CellContent Content { get; }

			public ulong? FieldHandle { get; set; }

			public CadTableCellContentTemplate(TableEntity.CellContent content)
			{
				Content = content;
			}

			public void Build(CadDocumentBuilder builder)
			{
				throw new System.NotImplementedException();
			}
		}

		internal class CadTableCellTemplate : ICadTemplate
		{
			public ulong? ValueHandle { get; set; }

			public TableEntity.Cell Cell { get; }

			public List<CadTableCellContentTemplate> ContentTemplates { get; } = new();

			public double? FormatTextHeight { get; set; }

			public int StyleId { get; internal set; }

			public ulong? UnknownHandle { get; internal set; }

			//TO DELTE, temporal prop to check the content
			public string CellText { get; internal set; }

			public HashSet<(ulong, string)> AttributeHandles { get; } = new();
			public ulong? TextStyleOverrideHandle { get; set; }

			public CadTableCellTemplate(TableEntity.Cell cell)
			{
				Cell = cell;
			}

			public void Build(CadDocumentBuilder builder)
			{
				if (builder.TryGetCadObject<CadObject>(this.ValueHandle, out var cadObject))
				{
				}

				if (!this.CellText.IsNullOrEmpty())
				{

				}
			}
		}
	}
}