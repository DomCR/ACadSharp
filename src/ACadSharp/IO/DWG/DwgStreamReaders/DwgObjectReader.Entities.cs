using ACadSharp.Entities;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
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

			if (!this.R2010Plus)
			{
				return null;
			}

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
				this.readTableContent(table.Content, template);

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

			//Common:
			//Flag for table value BS 90
			//	Bit flags, 0x06(0x02 + 0x04): has block,
			//	0x10: table direction, 0 = up, 1 = down,
			//	0x20: title suppressed.
			//	Normally 0x06 is always set.
			table.ValueFlag = (int)this._mergedReaders.ReadBitShort();

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

			//Cells have more handles
			//H 342 Table Style ID (hard pointer)
			template.StyleHandle = this.handleReference();

			for (int n = 0; n < table.Rows.Count; n++)
			{
				for (int m = 0; m < table.Columns.Count; m++)
				{
					//Cell data, repeats for all cells in n x m table:
					TableEntity.Cell cell = new TableEntity.Cell();
					CadTableCellTemplate cellTemplate = new CadTableCellTemplate(cell);

					table.Rows[n].Cells.Add(cell);

					this.readTableCellData(cellTemplate);
				}
			}

			return template;
		}

		private void readTableCellData(CadTableCellTemplate template)
		{
			TableEntity.Cell cellPlaceholder = template.Cell;

			//Cell type BS 171 1 = text, 2 = block.
			//In AutoCAD 2007 a cell can contain either 1 text
			//or 1 block.In AutoCAD 2008 this changed(TODO).
			cellPlaceholder.Type = (CellType)this._mergedReaders.ReadBitShort();

			throw new NotImplementedException();
		}

		private void readTableContent(TableContent content, CadTableEntityTemplate template)
		{
			//See paragraph 20.4.97.
			TableEntity tableEntity = template.TableEntity;

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
				tableEntity.Columns.Add(column);

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
				CadCellStyleTemplate colStyleTemplate = new(column.StyleOverride);
				this.readCellStyle(colStyleTemplate);

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
				tableEntity.Rows.Add(row);

				//BL 90 Number of cells in row.
				int ncells = this._mergedReaders.ReadBitLong();
				//Begin repeat cells
				for (int j = 0; j < ncells; j++)
				{
					Cell cell = new();
					CadTableCellTemplate cellTemplate = new CadTableCellTemplate(cell);

					this.readTableCell(cellTemplate);

					row.Cells.Add(cell);
				}

				//BL 91 32 bit integer containing custom data
				row.CustomData = this._mergedReaders.ReadBitLong();

				//BL 90 Number of custom data items
				int dataItems = this._mergedReaders.ReadBitLong();
				//Begin repeat custom data items
				for (int j = 0; j < dataItems; j++)
				{
					CustomDataEntry entry = new();

					//Custom data collection, see paragraph 20.4.100
					this.readCustomTableData(entry);

					row.CustomDataCollection.Add(entry);
				}

				//Cell style data, see paragraph 20.4.101.4, this contains cell style overrides for the column.
				CadCellStyleTemplate colStyleTemplate = new(row.CellStyleOverride);
				this.readCellStyle(colStyleTemplate);

				//BL 90 Cell style ID, points to the cell style in the table’s table style that is used as the
				//base cell style for the column. 0 if not present.
				this._mergedReaders.ReadBitLong();

				//40 Row height
				row.Height = this._mergedReaders.ReadBitDouble();
			}

			//BL Number of cell contents that contain a field reference.
			int nfields = this._mergedReaders.ReadBitLong();
			//Begin repeat field references
			for (int n = 0; n < nfields; n++)
			{
				//H Handle to field (AcDbField), hard owner.
				this._mergedReaders.HandleReference();
			}

			//The table’s cell style override fields (see paragraph 20.4.101.4). The table’s
			//base cell style is the table style’s overall cell style (present from R2010 onwards).
			this.readCellStyle(new CadCellStyleTemplate(content.CellStyleOverride));

			//Bl 90 Number of merged cell ranges
			int nranges = this._mergedReaders.ReadBitLong();
			for (int i = 0; i < nranges; i++)
			{
				CellRange dxfTableCellRange = new();
				//BL 91 Top row index
				dxfTableCellRange.TopRowIndex = this._mergedReaders.ReadBitLong();
				//BL 92 Left column index
				dxfTableCellRange.LeftColumnIndex = this._mergedReaders.ReadBitLong();
				//BL 93 Bottom row index
				dxfTableCellRange.BottomRowIndex = this._mergedReaders.ReadBitLong();
				//BL 94 Right column index
				dxfTableCellRange.RightColumnIndex = this._mergedReaders.ReadBitLong();

				content.MergedCellRanges.Add(dxfTableCellRange);
			}

			//H 340 Handle to table style(hard pointer).
			template.StyleHandle = this.handleReference();
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
			value.ValueType = (CellValueType)this._mergedReaders.ReadBitLong();
			if (!this.R2007Plus || !value.IsEmpty)
			{
				//Varies by type: Not present in case bit 1 in Flags is set
				switch (value.ValueType)
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
					default:
						throw new NotImplementedException();
				}
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Unit type BL 94 0 = no units, 1 = distance, 2 = angle, 4 = area, 8 = volume
				value.Units = (ValueUnitType)this._mergedReaders.ReadBitLong();
				//Format String TV 300
				value.Format = this._mergedReaders.ReadVariableText();
				//Value String TV 302
				value.FormatedValue = this._mergedReaders.ReadVariableText();
			}
		}

		private string readStringCellValue()
		{
			//General, BL containing the byte count followed by a
			//byte array. (introduced in R2007, use Unknown before
			//R2007).

			//NOTE: It also seems to be valid for the string values

			int length = this._mergedReaders.ReadBitLong();
			byte[] arr = this._mergedReaders.ReadBytes(length);

			if (this.R2007Plus)
			{
				return System.Text.Encoding.Unicode.GetString(arr, 0, length - 2);
			}
			else
			{
				return this._mergedReaders.Encoding.GetString(arr, 0, length);
			}
		}

		private System.DateTime? readDateCellValue()
		{
			//data size N, 
			int data = this._mergedReaders.ReadBitLong();

			if (data > 0)
			{
				byte[] array = this._mergedReaders.ReadBytes(data);
			}

			//TODO: Finish implementation
			return null;
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

		private void readCellStyle(CadCellStyleTemplate template)
		{
			//20.4.101.4
			CellStyle cellStyle = (CellStyle)template.Format;

			//BL 90 Cell style type
			cellStyle.Type = (CellStyleTypeType)this._mergedReaders.ReadBitLong();

			//BS 170 Data flags, 0 = no data, 1 = data is present
			//If data is present
			cellStyle.HasData = this._mergedReaders.ReadBitShortAsBool();
			if (!cellStyle.HasData)
			{
				return;
			}

			//BL 91 Property override flags. The definition is the same as the content format
			//propery override flags, see paragraph 20.4.101.3.
			cellStyle.PropertyOverrideFlags = (TableCellStylePropertyFlags)this._mergedReaders.ReadBitLong();
			//BL  92 Merge flags, but may only for bits 0x8000 and 0x10000.
			cellStyle.TableCellStylePropertyFlags = (TableCellStylePropertyFlags)this._mergedReaders.ReadBitLong();
			//TC 62 Background color
			cellStyle.BackgroundColor = this._mergedReaders.ReadCmColor();

			//BL 93 Content layout flags
			cellStyle.ContentLayoutFlags = (TableCellContentLayoutFlags)this._mergedReaders.ReadBitLong();

			//Content format fields (see paragraph 20.4.101.3)
			this.readCellContentFormat(template, cellStyle);

			//BS 171 Margin override flags, bit 1 is set if margin overrides are present
			//If margin overrides are present
			cellStyle.MarginOverrideFlags = (MarginFlags)this._mergedReaders.ReadBitShort();
			if (cellStyle.MarginOverrideFlags.HasFlag(MarginFlags.Override))
			{
				//BD 40 Vertical margin
				cellStyle.VerticalMargin = this._mergedReaders.ReadBitDouble();
				//BD 40 Horizontal margin
				cellStyle.HorizontalMargin = this._mergedReaders.ReadBitDouble();
				//BD 40 Bottom margin
				cellStyle.BottomMargin = this._mergedReaders.ReadBitDouble();
				//BD 40 Right margin
				cellStyle.RightMargin = this._mergedReaders.ReadBitDouble();
				//BD 40 Margin horizontal spacing
				cellStyle.MarginHorizontalSpacing = this._mergedReaders.ReadBitDouble();
				//BD 40 Margin vertical spacing
				cellStyle.MarginVerticalSpacing = this._mergedReaders.ReadBitDouble();
			}

			//BL 94 Number of borders present (0-6)
			int nborders = this._mergedReaders.ReadBitLong();
			//Begin repeat borders
			for (int i = 0; i < nborders; i++)
			{
				//BL 95 Edge flags
				CellEdgeFlags edgeFlags = (CellEdgeFlags)this._mergedReaders.ReadBitLong();
				// If edge flags is non - zero
				if (edgeFlags != 0)
				{
					CellBorder border = new CellBorder(edgeFlags);
					cellStyle.Borders.Add(border);

					this.readBorder(template, border);
				}
			}
		}

		private void readBorder(CadCellStyleTemplate template, CellBorder border)
		{
			//BL 90 Border property override flags
			border.PropertyOverrideFlags = (TableBorderPropertyFlags)this._mergedReaders.ReadBitLong();
			//BL 91 Border type
			border.Type = ((BorderType)this._mergedReaders.ReadBitLong());
			//TC 62 Color
			border.Color = (this._mergedReaders.ReadCmColor());
			//BL 92 Line weight
			border.LineWeight = ((short)this._mergedReaders.ReadBitLong());
			//H 40 Line type (hard pointer)
			template.BorderLinetypePairs.Add(new Tuple<CellBorder, ulong>(border, this.handleReference()));
			//BL 93 Invisibility: 1 = invisible, 0 = visible.
			border.IsInvisible = (this._mergedReaders.ReadBitLong() == 1);
			//BD 40 Double line spacing
			border.DoubleLineSpacing = (this._mergedReaders.ReadBitDouble());
		}

		private void readTableCell(CadTableCellTemplate template)
		{
			Cell cell = template.Cell;

			//BL 90 Cell state flags:
			cell.StateFlags = (TableCellStateFlags)this._mergedReaders.ReadBitLong();
			//TV 300 Tooltip
			cell.ToolTip = this._mergedReaders.ReadVariableText();
			//BL 91 32 bit integer containing custom data
			cell.CustomData = this._mergedReaders.ReadBitLong();

			//... Custom data collection, see paragraph 20.4.100.
			int ndata = this._mergedReaders.ReadBitLong();
			for (int i = 0; i < ndata; i++)
			{
				CustomDataEntry customData = new CustomDataEntry();
				this.readCustomTableData(customData);
				cell.CustomDataCollection.Add(customData);
			}

			//BL 92 Has linked data flags, 0 = false, 1 = true If has linked data
			var data = this._mergedReaders.ReadBitLong();
			cell.HasLinkedData = data == 1;
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
				TableEntity.CellContent cellContent = new();
				cell.Contents.Add(cellContent);

				CadTableCellContentTemplate cellContentTemplate = new CadTableCellContentTemplate(cellContent);
				template.ContentTemplates.Add(cellContentTemplate);

				this.readTableCellContent(cellContentTemplate);
			}

			CadCellStyleTemplate formatTemplate = new(cell.StyleOverride);
			this.readCellStyle(formatTemplate);

			//BL 90 Cell style ID, points to the cell style in the table’s table style that is used as the
			//base cell style for the cell. 0 if not present.
			template.StyleId = this._mergedReaders.ReadBitLong();

			//BL 91 Unknown flag
			var unknownFlag = this._mergedReaders.ReadBitLong();
			if (unknownFlag != 0)
			{
				//If unknown flag is non-zero
				//BL 91 Unknown
				this._mergedReaders.ReadBitLong();
				//BD 40 Unknown
				this._mergedReaders.ReadBitDouble();
				//BD 40 Unknown
				this._mergedReaders.ReadBitDouble();
				//BL Geometry data flags
				var geomFlags = this._mergedReaders.ReadBitLong();

				//H Unknown ()
				template.UnknownHandle = this.handleReference();
				if (geomFlags != 0)
				{
					cell.Geometry = new CellContentGeometry();
					this.readCellContentGeometry(cell.Geometry);
				}
			}
		}

		private void readCellContentGeometry(CellContentGeometry geometry)
		{
			// Cell content geometry, see paragraph 20.4.98.

			//3BD	Distance to top left
			geometry.DistanceTopLeft = this._mergedReaders.Read3BitDouble();
			//3BD	Distance to center
			geometry.DistanceCenter = this._mergedReaders.Read3BitDouble();
			//BD	Content width
			geometry.ContentWidth = this._mergedReaders.ReadBitDouble();
			//BD	Content height
			geometry.ContentHeight = this._mergedReaders.ReadBitDouble();
			//BD	Width
			geometry.Width = this._mergedReaders.ReadBitDouble();
			//BD	Height
			geometry.Height = this._mergedReaders.ReadBitDouble();
			//BL	Unknown flags
			geometry.Flags = this._mergedReaders.ReadBitLong();
		}

		private void readTableCellContent(CadTableCellContentTemplate template)
		{
			//BL 90 Cell content type:
			template.Content.ContentType = (TableCellContentType)this._mergedReaders.ReadBitLong();

			switch (template.Content.ContentType)
			{
				case TableCellContentType.Unknown:
					break;
				case TableCellContentType.Value:
					this.readCustomTableDataValue(template.Content.Value);
					break;
				case TableCellContentType.Field:
					//H 340 Handle to AcDbField object (hard pointer).
					template.FieldHandle = this.handleReference();
					break;
				case TableCellContentType.Block:
					//H 340 Handle to block record (hard pointer).
					template.BlockRecordHandle = this.handleReference();
					break;
			}

			//BL 91 Number of attributes
			int natts = this._mergedReaders.ReadBitLong();
			for (int i = 0; i < natts; i++)
			{
				TableAttribute tableAttribute = new TableAttribute();
				CadTableAttributeTemplate attTemplate = new CadTableAttributeTemplate(tableAttribute);

				//H 330 Handle to attribute definition (ATTDEF), soft pointer.
				attTemplate.AttDefHandle = this.handleReference();
				//TV 301 Attribute value.
				tableAttribute.Value = this._mergedReaders.ReadVariableText();
				//BL 92 Index (starts at 1).
				this._mergedReaders.ReadBitLong();
				//End repeat attributes
			}

			//BS 170 Has content format overrides flag
			template.Content.Format.HasData = this._mergedReaders.ReadBitShortAsBool();
			if (template.Content.Format.HasData)
			{
				CadTableCellContentFormatTemplate formatTemplate = new CadTableCellContentFormatTemplate(template.Content.Format);
				this.readCellContentFormat(formatTemplate, template.Content.Format);
			}
		}

		private void readCellContentFormat(CadTableCellContentFormatTemplate template, ContentFormat format)
		{
			//Cell.ContentFormat format = template.Content.Format;

			//20.4.101.3 Content format

			//BL 90 Property override flags
			format.PropertyOverrideFlags = (TableCellStylePropertyFlags)this._mergedReaders.ReadBitLong();
			//BL 91 Property flags. Contains property bit values for property Auto Scale only
			format.PropertyFlags = this._mergedReaders.ReadBitLong();
			//BL 92 Value data type, see also paragraph 20.4.98.
			format.ValueDataType = this._mergedReaders.ReadBitLong();
			//BL 93 Value unit type, see also paragraph 20.4.98.
			format.ValueUnitType = this._mergedReaders.ReadBitLong();
			//TV 300 Value format string
			format.ValueFormatString = this._mergedReaders.ReadVariableText();
			//BD 40 Rotation
			format.Rotation = this._mergedReaders.ReadBitDouble();
			//BD 140 Block scale
			format.Scale = this._mergedReaders.ReadBitDouble();
			//BL  94 Cell alignment
			format.Alignment = this._mergedReaders.ReadBitLong();
			//TC 62 Content color
			format.Color = this._mergedReaders.ReadCmColor();
			//H 340 Text style handle (hard pointer)
			template.TextStyleHandle = this.handleReference();
			//BD 144 Text height
			format.TextHeight = this._mergedReaders.ReadBitDouble();
		}
	}
}
