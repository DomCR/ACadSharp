using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using CSMath;
using CSUtilities.Converters;
using System;
using static ACadSharp.Entities.TableEntity;
using static ACadSharp.Entities.TableEntity.BreakData;
using static ACadSharp.IO.Templates.CadTableEntityTemplate;

namespace ACadSharp.IO.DWG
{
	internal partial class DwgObjectReader : DwgSectionIO
	{
		private CadTemplate readTableEntity()
		{
			TableEntity table = new TableEntity();
			CadTableEntityTemplate template = new CadTableEntityTemplate(table);

			this.readInsertCommonData(template);
			this.readInsertCommonHandles(template);

			if (this.R2010Plus)
			{
				//RC Unknown (default 0)
				this._mergedReaders.ReadByte();
				//H Unknown (soft pointer, default NULL)
				template.NullHandle = this.handleReference();

				//BL Unknown (default 0)
				long longZero = this._mergedReaders.ReadBitLong();
				if (this.R2013Plus)
				{
					//R2013
					//Unknown (default 0)
					this._mergedReaders.ReadBitLong();
				}
				else
				{
					//R2010
					//B Unknown (default true)
					this._mergedReaders.ReadBit();
				}

				//Here the table content is present (see TABLECONTENT object),
				//without the common OBJECT data.
				//See paragraph 20.4.97.
				this.readTableContent(table.TableContent, template);

				//BS Unknown (default 38)
				this._mergedReaders.ReadBitShort();
				//3BD 11 Horizontal direction
				table.HorizontalDirection = this._mergedReaders.Read3BitDouble();

				//BL Has break data flag (0 = no break data, 1 = has break data)
				//Begin break data(optional)
				bool hasBreakData = this._mergedReaders.ReadBitLong() == 1;
				if (hasBreakData)
				{
					TableEntity.BreakData breakData = table.TableBreakData;
					//BL Option flags:
					breakData.Flags = (BreakOptionFlags)this._mergedReaders.ReadBitLong();
					//BL Flow direction:
					breakData.FlowDirection = (BreakFlowDirection)this._mergedReaders.ReadBitLong();
					//BD Break spacing
					breakData.BreakSpacing = this._mergedReaders.ReadBitDouble();
					//BL Unknown flags
					this._mergedReaders.ReadBitLong();
					//BL Unknown flags
					this._mergedReaders.ReadBitLong();

					//BL Number of manual positions (break heights)
					int num = this._mergedReaders.ReadBitLong();
					//Begin repeat manual positions (break heights)
					for (int i = 0; i < num; i++)
					{
						BreakHeight breakHeight = new();
						//3BD Position
						breakHeight.Position = this._mergedReaders.Read3BitDouble();
						//BD Height
						breakHeight.Height = this._mergedReaders.ReadBitDouble();
						//BL Flags(meaning unknown)
						this._mergedReaders.ReadBitLong();
						breakData.Heights.Add(breakHeight);
					}
				}

				//End break data

				//BL Number of break row ranges (there is always at least 1)
				int rowRanges = this._mergedReaders.ReadBitLong();
				for (int i = 0; i < rowRanges; i++)
				{
					TableEntity.BreakRowRange breakRowRange = new();
					//3BD Position
					breakRowRange.Position = this._mergedReaders.Read3BitDouble();
					//BL Start row index
					breakRowRange.StartRowIndex = this._mergedReaders.ReadBitLong();
					//BL End row index
					breakRowRange.EndRowIndex = this._mergedReaders.ReadBitLong();
					table.BreakRowRanges.Add(breakRowRange);
				}

				return template;
			}

			//Until R2007
			//H 342 Table Style ID (hard pointer)
			template.StyleHandle = this.handleReference();

			//Common:
			//Flag for table value BS 90
			//	Bit flags, 0x06(0x02 + 0x04): has block,
			//	0x10: table direction, 0 = up, 1 = down,
			//	0x20: title suppressed.
			//	Normally 0x06 is always set.
			table.ValueFlag = this._mergedReaders.ReadBitShort();

			//Hor.Dir.Vector 3BD 11
			table.HorizontalDirection = this._mergedReaders.Read3BitDouble();

			//Number of columns BL 92
			var ncols = this._mergedReaders.ReadBitLong();
			//Number of rows BL 91
			var nrows = this._mergedReaders.ReadBitLong();

			//Column widths BD 142 Repeats “# of columns” times
			for (int i = 0; i < ncols; i++)
			{
				TableEntity.Column c = new TableEntity.Column();
				//Column widths BD 142 Repeats “# of columns” times
				c.Width = this._mergedReaders.ReadBitDouble();

				table.Columns.Add(c);
			}

			//Row heights BD 141 Repeats “# of rows” times
			for (int i = 0; i < nrows; i++)
			{
				TableEntity.Row r = new TableEntity.Row();
				//Row heights BD 141 Repeats “# of rows” times
				r.Height = this._mergedReaders.ReadBitDouble();

				table.Rows.Add(r);
			}

			for (int i = 0; i < table.Rows.Count; i++)
			{
				for (int j = 0; j < table.Columns.Count; j++)
				{
					//Cell data, repeats for all cells in n x m table:
					this.readTableCellData(table);
				}
			}

			return template;
		}

		private void readTableCellData(TableEntity table)
		{
			throw new NotImplementedException();
		}

		private void readTableContent(TableEntity.Content content, CadTableEntityTemplate template)
		{
			//See paragraph 20.4.97.

			//TV 1 Name
			content.Name = this._mergedReaders.ReadVariableText();
			//TV 300 Description AcDbLinkedTableData fields
			content.Description = this._mergedReaders.ReadVariableText();

			//BL 90 Number of columns
			int cols = this._mergedReaders.ReadBitLong();
			//Begin repeat columns
			for (int i = 0; i < cols; i++)
			{
				TableEntity.Column column = new TableEntity.Column();

				//TV 300 Column name
				column.Name = this._mergedReaders.ReadVariableText();
				//BL 91 32 bit integer containing custom data
				column.CustomData = this._mergedReaders.ReadBitLong();

				//BL 90 Number of custom data items
				int dataItems = this._mergedReaders.ReadBitLong();
				//Begin repeat custom data items
				for (int j = 0; j < dataItems; j++)
				{
					CustomDataEntry entry = new();

					//Custom data collection, see paragraph 20.4.100
					this.readCustomTableData(entry);

					column.CustomDataCollection.Add(entry);
				}

				//Cell style data, see paragraph 20.4.101.4, this contains cell style overrides for the column.
				CellStyle cellStyle = new();
				this.readCellStyle(cellStyle);

				//BL 90 Cell style ID, points to the cell style in the table’s table style that is used as the
				//base cell style for the column. 0 if not present.
				this._mergedReaders.ReadBitLong();
				//BD 40 Column width.

				column.Width = this._mergedReaders.ReadBitDouble();
				//End repeat columns
			}

			//BL 91 Number of rows.
			int nrows = this._mergedReaders.ReadBitLong();
			for (int i = 0; i < nrows; i++)
			{
				//Begin repeat rows.
				Row row = new Row();

				//BL 90 Number of cells in row.
				int ncells = this._mergedReaders.ReadBitLong();
				//Begin repeat cells
				for (int j = 0; j < ncells; j++)
				{
					Cell cell = new();

					this.readTableCell(cell);

					row.Cells.Add(cell);
				}
			}
		}

		private void readCustomTableData(CustomDataEntry entry)
		{
			//TV 300 Item name
			entry.Name = this._mergedReaders.ReadVariableText();
			this.readCustomTableDataValue(entry.Value);
		}

		private void readCustomTableDataValue(CellValue value)
		{
			//R2007+:
			if (this.R2007Plus)
			{
				//Flags BL 93 Flags & 0x01 => type is kGeneral
				value.Flags = this._mergedReaders.ReadBitLong();
			}

			//Common:
			//Data type BL 90
			CellValueType type = (CellValueType)this._mergedReaders.ReadBitLong();
			if (!this.R2007Plus || !value.IsEmpty)
			{
				//Varies by type: Not present in case bit 1 in Flags is set
				switch (type)
				{
					case CellValueType.Unknown:
					case CellValueType.Long:
						value.Value = this._mergedReaders.ReadBitLong();
						break;
					case CellValueType.Double:
						value.Value = this._mergedReaders.ReadBitDouble();
						break;
					case CellValueType.General:
					case CellValueType.String:
						value.Value = this.readStringCellValue();
						break;
					case CellValueType.Date:
						System.DateTime? dateTime = this.readDateCellValue();
						if (dateTime.HasValue)
						{
							value.Value = dateTime.Value;
						}
						break;
					case CellValueType.Point2D:
						value.Value = this.readCellValueXY();
						break;
					case CellValueType.Point3D:
						value.Value = this.readCellValueXYZ();
						break;
					case CellValueType.Handle:
						value.Value = this.handleReference();
						break;
					case CellValueType.Buffer:
					case CellValueType.ResultBuffer:
						throw new NotImplementedException();
				}
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Unit type BL 94 0 = no units, 1 = distance, 2 = angle, 4 = area, 8 = volume
				this._mergedReaders.ReadBitLong();
				//Format String TV 300
				this._mergedReaders.ReadVariableText();
				//Value String TV 302
				this._mergedReaders.ReadVariableText();
			}
		}

		private string readStringCellValue()
		{
			throw new NotImplementedException();
		}

		private System.DateTime? readDateCellValue()
		{
			throw new NotImplementedException();
		}

		private XY? readCellValueXY()
		{
			XY? result = null;
			int length = this._mergedReaders.ReadBitLong();
			if (length > 0)
			{
				result = this._mergedReaders.Read2RawDouble();
			}
			return result;
		}

		private XYZ? readCellValueXYZ()
		{
			XYZ? result = null;
			int length = this._mergedReaders.ReadBitLong();
			if (length > 0)
			{
				result = this._mergedReaders.Read3RawDouble();
			}
			return result;
		}

		private void readCellStyle(CellStyle cellStyle)
		{
			//BL 90 Cell style type
			cellStyle.Type = (CellStypeType)this._mergedReaders.ReadBitLong();
			//BS 170 Data flags, 0 = no data, 1 = data is present
			//If data is present
			bool hasData = this._mergedReaders.ReadBitShort() == 1;
			if (!hasData)
			{
				return;
			}

			throw new NotImplementedException($"{nameof(readCellStyle)}");
		}

		private void readTableCell(Cell cell)
		{
			//BL 90 Cell state flags:
			cell.StateFlags = (TableCellStateFlags)this._mergedReaders.ReadBitLong();
			//TV 300 Tooltip
			cell.ToolTip = this._mergedReaders.ReadVariableText();
			//BL 91 32 bit integer containing custom data
			cell.CustomData = this._mergedReaders.ReadBitLong();

			//... Custom data collection, see paragraph 20.4.100.
			int num = this._mergedReaders.ReadBitLong();
			for (int i = 0; i < num; i++)
			{
				CustomDataEntry customData = new CustomDataEntry();
				this.readCustomTableData(customData);
				cell.CustomDataCollection.Add(customData);
			}

			//BL 92 Has linked data flags, 0 = false, 1 = true If has linked data
			cell.HasLinkedData = this._mergedReaders.ReadBitLong() == 1;
			if (cell.HasLinkedData)
			{
				//H 340 Handle to data link object (hard pointer).
				this._mergedReaders.HandleReference();
				//BL 93 Row count.
				this._mergedReaders.ReadBitLong();
				//BL 94 Column count.
				this._mergedReaders.ReadBitLong();
				//BL 96 Unknown.
				this._mergedReaders.ReadBitLong();
				//End if has linked data
			}

			//BL 95 Number of cell contents
			int cellContents = this._mergedReaders.ReadBitLong();
			for (int j = 0; j < cellContents; j++)
			{
				//Begin repeat cell contents
				TableEntity.Cell.Content cellContent = new();

				throw new NotImplementedException();

				cell.Contents.Add(cellContent);
			}
		}
	}
}
