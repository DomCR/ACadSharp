﻿using ACadSharp.Entities;
using ACadSharp.Objects;
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

		private readonly List<CadTableCellTemplate> _cadTableCellTemplates = new();

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

			this._cadTableCellTemplates.Add(this.CurrentCellTemplate);

			this._currCellIndex++;
		}

		protected override void build(CadDocumentBuilder builder)
		{
			base.build(builder);
		}

		internal class CadCellStyleTemplate : CadTableCellContentFormatTemplate
		{
			public List<Tuple<CellBorder, ulong>> BorderLinetypePairs { get; set; } = new();

			public CadCellStyleTemplate() : base(new())
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

			public ulong? TextStyleHandle { get; internal set; }

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
			public ulong? BlockRecordHandle { get; set; }

			public TableEntity.Cell Cell { get; }

			public List<CadTableCellContentTemplate> ContentTemplates { get; } = new();

			public double? FormatTextHeight { get; set; }

			public int StyleId { get; internal set; }

			public ulong? UnknownHandle { get; internal set; }

			public CadTableCellTemplate(TableEntity.Cell cell)
			{
				Cell = cell;
			}

			public void Build(CadDocumentBuilder builder)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}