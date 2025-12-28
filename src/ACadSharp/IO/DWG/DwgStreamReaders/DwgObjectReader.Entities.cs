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
				//R2013
				if (this.R2013Plus)
				{
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
					table.Rows[n].Cells.Add(cell);
					CadTableCellTemplate cellTemplate = new CadTableCellTemplate(cell);
					template.CadTableCellTemplates.Add(cellTemplate);

					this.readTableCellData(cellTemplate);
				}
			}

			//Common:
			//End Cell Data(remaining data applies to entire table)
			//Has table overrides B
			if (this._mergedReaders.ReadBit())
			{
				var styleOverride = new TableStyle();
				table.Content.StyleOverride = styleOverride;

				//TODO: implement table style override

				//If has override flag == 1
				//Cell flag override BL 177 (deprecated)
				TableEntity.TableOverrideFlags flags = (TableOverrideFlags)this._mergedReaders.ReadBitLong();
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleSuppressed))
				{
					//Title suppressed B 280 Present only if bit 0x0001 is set in table overrides 
					styleOverride.SuppressTitle = this._mergedReaders.ReadBit();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleSuppressed))
				{
					//Header suppresed -- 281 Always true (do not read any data for this)

				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.FlowDirection))
				{
					//Flow direction BS 70 Present only if bit 0x0004 is set in table overrides
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HorizontalCellMargin))
				{
					//Horz. Cell margin BD 40 Present only if bit 0x0008 is set in table overrides 
					this._mergedReaders.ReadBitDouble();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.VerticalCellMargin))
				{
					//Vert. cell margin BD 41 Present only if bit 0x0010 is set in table overrides 
					this._mergedReaders.ReadBitDouble();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleRowColor))
				{
					//Title row color CMC 64 Present only if bit 0x0020 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderRowColor))
				{
					//Header row color CMC 64 Present only if bit 0x0040 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataRowColor))
				{
					//Data row color CMC 64 Present only if bit 0x0080 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleRowFillNone))
				{
					//Title row fill none B 283 Present only if bit 0x0100 is set in table overrides 
					this._mergedReaders.ReadBit();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderRowFillNone))
				{
					//Header row fill none B 283 Present only if bit 0x0200 is set in table overrides 
					this._mergedReaders.ReadBit();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataRowFillNone))
				{
					//Data row fill none B 283 Present only if bit 0x0400 is set in table overrides 
					this._mergedReaders.ReadBit();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleRowFillColor))
				{
					//Title row fill color CMC 63 Present only if bit 0x0800 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderRowFillColor))
				{
					//Header row fill clr. CMC 63 Present only if bit 0x1000 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataRowFillColor))
				{
					//Data row fill color CMC 63 Present only if bit 0x2000 is set in table overrides 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleRowAlign))
				{
					//Title row align. BS 170 Present only if bit 0x4000 is set in table overrides 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderRowAlign))
				{
					//Header row align. BS 170 Present only if bit 0x8000 is set in table overrides 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataRowAlign))
				{
					//Data row align. BS 170 Present only if bit 0x10000 is set in table 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleTextStyle))
				{
					//Title text style hnd H 7 Present only if bit 0x20000 is set in table 
					this.handleReference();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderTextStyle))
				{
					//Title text style hnd H 7 Present only if bit 0x40000 is set in table 
					this.handleReference();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataTextStyle))
				{
					//Title text style hnd H 7 Present only if bit 0x80000 is set in table 
					this.handleReference();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.TitleRowHeight))
				{
					//Title row height BD 140 Present only if bit 0x100000 is set in table 
					this._mergedReaders.ReadBitDouble();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.HeaderRowHeight))
				{
					//Header row height BD 140 Present only if bit 0x200000 is set in table 
					this._mergedReaders.ReadBitDouble();
				}
				if (flags.HasFlag(TableEntity.TableOverrideFlags.DataRowHeight))
				{
					//Data row height BD 140 Present only if bit 0x400000 is set in table 
					this._mergedReaders.ReadBitDouble();
				}
			}

			//End If has table overrides == 1
			//Has border color overrides B
			if (this._mergedReaders.ReadBit())
			{
				//Overrides flag BL 94 Border COLOR overrides
				var flags = (TableEntity.BorderOverrideFlags)this._mergedReaders.ReadBitLong();
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalTop))
				{
					//Title hor. Top. col. CMC 64 Present only if bit 0x01 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalInsert))
				{
					//Title hor. ins. col. CMC 65 Present only if bit 0x02 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalBottom))
				{
					//Title hor. bot. col. CMC 66 Present only if bit 0x04 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalLeft))
				{
					//Title ver. left.col.CMC 63 Present only if bit 0x08 is set in border color
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalInsert))
				{
					//Title ver. ins. col. CMC 68 Present only if bit 0x10 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalRight))
				{
					//Title ver. rt. col. CMC 69 Present only if bit 0x20 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalTop))
				{
					//Header hor. Top. col. CMC 64 Present only if bit 0x40 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalInsert))
				{
					//Header hor. ins. col. CMC 65 Present only if bit 0x80 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalBottom))
				{
					//Header hor. bot. col. CMC 66 Present only if bit 0x100 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalLeft))
				{
					//Header ver. left. col.CMC 63 Present only if bit 0x200 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalInsert))
				{
					//Header ver. ins. col. CMC 68 Present only if bit 0x400 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalRight))
				{
					//Header ver. rt. col. CMC 69 Present only if bit 0x800 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalTop))
				{
					//Data hor. Top. col. CMC 64 Present only if bit 0x1000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalInsert))
				{
					//Data hor. ins. col. CMC 65 Present only if bit 0x2000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalBottom))
				{
					//Data hor. bot. col. CMC 66 Present only if bit 0x4000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalLeft))
				{
					//Data ver. left. col. CMC 63 Present only if bit 0x8000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalInsert))
				{
					//Data ver. ins. col. CMC 68 Present only if bit 0x10000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalRight))
				{
					//Data ver. rt. col. CMC 69 Present only if bit 0x20000 is set in border color 
					this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
			}

			//Has border lineweight overrides B
			if (this._mergedReaders.ReadBit())
			{
				//Overrides flag BL 95 Border LINEWEIGHT overrides
				var flags = (TableEntity.BorderOverrideFlags)this._mergedReaders.ReadBitLong();
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalTop))
				{
					//Title hor. Top. lw. BS Present only if bit 0x01 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalInsert))
				{
					//Title hor. ins. lw. BS Present only if bit 0x02 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalBottom))
				{
					//Title hor. bot. lw. BS Present only if bit 0x04 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalLeft))
				{
					//Title ver. left. lw. BS Present only if bit 0x08 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalInsert))
				{
					//Title ver. ins. lw. BS Present only if bit 0x10 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalRight))
				{
					//Title ver. rt. lw. BS Present only if bit 0x20 is set in border color 
					this._mergedReaders.ReadBitShort();
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalTop))
				{
					//Header hor. Top. lw. BS Present only if bit 0x40 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalInsert))
				{
					//Header hor. ins. lw. BS Present only if bit 0x80 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalBottom))
				{
					//Header hor. bot. lw. BS Present only if bit 0x100 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalLeft))
				{
					//Header ver. left. lw. BS Present only if bit 0x200 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalInsert))
				{
					//Header ver. ins. lw. BS Present only if bit 0x400 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalRight))
				{
					//Header ver. rt. lw. BS Present only if bit 0x800 is set in border color 
					this._mergedReaders.ReadBitShort();
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalTop))
				{
					//Data hor. Top. lw. BS Present only if bit 0x1000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalInsert))
				{
					//Data hor. ins. lw. BS Present only if bit 0x2000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalBottom))
				{
					//Data hor. bot. lw. BS Present only if bit 0x4000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalLeft))
				{
					//Data ver. left. lw. BS Present only if bit 0x8000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalInsert))
				{
					//Data ver. ins. lw. BS Present only if bit 0x10000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalRight))
				{
					//Data ver. rt. lw. BS Present only if bit 0x20000 is set in border color 
					this._mergedReaders.ReadBitShort();
				}
			}

			//Has border visibility overrides B
			if (this._mergedReaders.ReadBit())
			{
				//Overrides flag BL 96 Border visibility overrides
				var flags = (TableEntity.BorderOverrideFlags)this._mergedReaders.ReadBitLong();
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalTop))
				{
					//Title hor. Top. vsb. BS Present only if bit 0x01 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalInsert))
				{
					//Title hor. ins. vsb. BS Present only if bit 0x02 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleHorizontalBottom))
				{
					//Title hor. bot. vsb. BS Present only if bit 0x04 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalLeft))
				{
					//Title ver. left. vsb. BS Present only if bit 0x08 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalInsert))
				{
					//Title ver. ins. vsb. BS Present only if bit 0x10 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.TitleVerticalRight))
				{
					//Title ver. rt. vsb. BS Present only if bit 0x20 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalTop))
				{
					//Header hor. Top. vsb. BS Present only if bit 0x40 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalInsert))
				{
					//Header hor. ins. vsb. BS Present only if bit 0x80 is set in border visibility 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderHorizontalBottom))
				{
					//Header hor. bot. vsb. BS Present only if bit 0x100 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalLeft))
				{
					//Header ver. left. vsb. BS Present only if bit 0x200 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalInsert))
				{
					//Header ver. ins. vsb. BS Present only if bit 0x400 is set in border
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.HeaderVerticalRight))
				{
					//Header ver. rt. vsb. BS Present only if bit 0x800 is set in border
					this._mergedReaders.ReadBitShortAsBool();
				}

				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalTop))
				{
					//Data hor. Top. vsb. BS Present only if bit 0x1000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalInsert))
				{
					//Data hor. ins. vsb. BS Present only if bit 0x2000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataHorizontalBottom))
				{
					//Data hor. bot. vsb. BS Present only if bit 0x4000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalLeft))
				{
					//Data ver. left. vsb. BS Present only if bit 0x8000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalInsert))
				{
					//Data ver. ins. vsb. BS Present only if bit 0x10000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
				if (flags.HasFlag(TableEntity.BorderOverrideFlags.DataVerticalRight))
				{
					//Data ver. rt. vsb. BS Present only if bit 0x20000 is set in border 
					this._mergedReaders.ReadBitShortAsBool();
				}
			}

			return template;
		}

		private void readTableCellData(CadTableCellTemplate template)
		{
			TableEntity.Cell cell = template.Cell;

			//Cell type BS 171 1 = text, 2 = block.
			//In AutoCAD 2007 a cell can contain either 1 text
			//or 1 block.In AutoCAD 2008 this changed(TODO).
			cell.Type = (CellType)this._mergedReaders.ReadBitShort();

			//Cell edge flags RC 172
			cell.EdgeFlags = this._mergedReaders.ReadByte();

			//Cell merged value B 173 Determines whether this cell is merged with another cell.
			cell.MergedValue = this._mergedReaders.ReadBit() ? (short)1 : (short)0;
			//Autofit flag B 174
			cell.AutoFit = this._mergedReaders.ReadBit();
			//Merged width flag BL 175 Represents the horizontal number of merged cells.
			cell.BorderWidth = this._mergedReaders.ReadBitLong();
			//Merged height flag BL 176 Represents the vertical number of merged cells.
			cell.BorderHeight = this._mergedReaders.ReadBitLong();
			//Rotation value BD 145
			cell.Rotation = this._mergedReaders.ReadBitDouble();

			//H 344 for text cell, 340 for block cell (hard pointer)
			template.ValueHandle = this.handleReference();

			switch (cell.Type)
			{
				//In AutoCAD 2007 a cell can contain either 1 text block.
				case CellType.Text when template.ValueHandle == 0 && this._version < ACadVersion.AC1021:
					//Text string TV 1 Present only if 344 value below is 0
					var content = new CellContent();
					content.Value.ValueType = CellValueType.String;
					content.Value.Value = this._mergedReaders.ReadVariableText();
					cell.Contents.Add(content);
					break;
				case CellType.Block:
					//Block scale BD 144
					cell.BlockScale = this._mergedReaders.ReadBitDouble();
					//Has attributes flag B
					if (this._mergedReaders.ReadBit())
					{
						// Attr. Def. count BS 179
						int natts = this._mergedReaders.ReadBitShort();
						for (int i = 0; i < natts; i++)
						{
							//H 331 Attr. Def. ID (soft pointer, present only for block
							//cells, when additional data flag == 1, and 1 entry
							//per attr.def.)
							var atthandle = this.handleReference();
							//Attr.Def.index BS  Not present in dxf
							short index = this._mergedReaders.ReadBitShort();
							//Attr. Def. text TV 300
							string text = this._mergedReaders.ReadVariableText();

							template.AttributeHandles.Add((atthandle, text));
						}
					}
					break;
			}

			//Common to both text and block cells:
			//has override flag B
			if (this._mergedReaders.ReadBit())
			{
				//Cell flag override BL 177 (deprecated)
				TableEntity.Cell.OverrideFlags flags = (Cell.OverrideFlags)this._mergedReaders.ReadBitLong();

				//Virtual edge flag RC 178 Determines which edges are virtual, see also the
				//explanation on the cell edge flags above.When an
				//edge is virtual, that edge has no border overrides.
				//1 = top, 2 = right, 4 = bottom, 8 = left.
				cell.VirtualEdgeFlag = this._mergedReaders.ReadByte();

				if (flags.HasFlag(Cell.OverrideFlags.CellAlignment))
				{
					//Cell alignment RS 170 Present only if bit 0x01 is set in cell flag 
					cell.StyleOverride.CellAlignment = (Cell.CellAlignment)this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(Cell.OverrideFlags.BackgroundFillNone))
				{
					//Background fill none B 283 Present only if bit 0x02 is set in cell flag
					cell.StyleOverride.IsFillColorOn = this._mergedReaders.ReadBit();
				}
				if (flags.HasFlag(Cell.OverrideFlags.BackgroundColor))
				{
					//Background color CMC 63 Present only if bit 0x04 is set in cell flag
					cell.StyleOverride.BackgroundColor = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.ContentColor))
				{
					//Content color CMC 64 Present only if bit 0x08 is set in cell flag 
					cell.StyleOverride.ContentColor = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.TextStyle))
				{
					//Text style H 7 Present only if bit 0x10 is set in cell flag 
					template.TextStyleOverrideHandle = this.handleReference();
				}
				if (flags.HasFlag(Cell.OverrideFlags.TextHeight))
				{
					//Text height BD 140 Present only if bit 0x20 is set in cell flag 
					cell.StyleOverride.TextHeight = this._mergedReaders.ReadBitDouble();
				}

				if (flags.HasFlag(Cell.OverrideFlags.TopGridColor))
				{
					//Top grid color CMC 69 Present only if bit 0x00040 is set in cell flag 
					cell.StyleOverride.TopBorder.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.TopGridLineWeight))
				{
					//Top grid lineweight BS 279 Present only if bit 0x00400 is set in cell flag 
					cell.StyleOverride.TopBorder.LineWeight = (LineWeightType)this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(Cell.OverrideFlags.TopGridLineWeight))
				{
					//Top visibility BS 289 Present only if bit 0x04000 is set in cell flag 
					cell.StyleOverride.TopBorder.IsInvisible = !this._mergedReaders.ReadBitShortAsBool();
				}

				if (flags.HasFlag(Cell.OverrideFlags.RightGridColor))
				{
					//Right grid color CMC 65 Present only if bit 0x00080 is set in cell flag 
					cell.StyleOverride.RightBorder.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.RightGridLineWeight))
				{
					//Right grid lineweight BS 275 Present only if bit 0x00800 is set in cell flag 
					cell.StyleOverride.RightBorder.LineWeight = (LineWeightType)this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(Cell.OverrideFlags.RightGridLineWeight))
				{
					//Right visibility BS 285 Present only if bit 0x08000 is set in cell flag 
					cell.StyleOverride.RightBorder.IsInvisible = !this._mergedReaders.ReadBitShortAsBool();
				}

				if (flags.HasFlag(Cell.OverrideFlags.BottomGridColor))
				{
					//Bottom grid color CMC 66 Present only if bit 0x00100 is set in cell flag 
					cell.StyleOverride.BottomBorder.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.BottomGridLineWeight))
				{
					//Bottom grid lineweight BS 276 Present only if bit 0x01000 is set in cell flag 
					cell.StyleOverride.BottomBorder.LineWeight = (LineWeightType)this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(Cell.OverrideFlags.BottomGridLineWeight))
				{
					//Bottom visibility BS 286 Present only if bit 0x10000 is set in cell flag 
					cell.StyleOverride.BottomBorder.IsInvisible = !this._mergedReaders.ReadBitShortAsBool();
				}

				if (flags.HasFlag(Cell.OverrideFlags.LeftGridColor))
				{
					//Left grid color CMC 68 Present only if bit 0x00200 is set in cell flag 
					cell.StyleOverride.LeftBorder.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
				}
				if (flags.HasFlag(Cell.OverrideFlags.LeftGridLineWeight))
				{
					//Left grid lineweight BS 278 Present only if bit 0x02000 is set in cell flag 
					cell.StyleOverride.LeftBorder.LineWeight = (LineWeightType)this._mergedReaders.ReadBitShort();
				}
				if (flags.HasFlag(Cell.OverrideFlags.LeftGridLineWeight))
				{
					//Left visibility BS 288 Present only if bit 0x20000 is set in cell flag 
					cell.StyleOverride.LeftBorder.IsInvisible = !this._mergedReaders.ReadBitShortAsBool();
				}
			}

			//R2007+:
			if (this.R2007Plus)
			{
				//Unknown BL
				var unknown = this._mergedReaders.ReadBitLong();
				//Value fields … See paragraph 20.4.98.
				cell.Contents.Add(new CellContent());
				this.readCustomTableDataValue(cell.Content.Value);
			}
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
				CadCellStyleTemplate colStyleTemplate = new(column.CellStyleOverride);
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
				value.FormattedValue = this._mergedReaders.ReadVariableText();
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
			cellStyle.BackgroundColor = this._mergedReaders.ReadCmColor(this.R2004Pre);

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
			border.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
			//BL 92 Line weight
			border.LineWeight = ((LineWeightType)this._mergedReaders.ReadBitLong());
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
			cell.HasLinkedData = this._mergedReaders.ReadBitLong() == 1;
			if (cell.HasLinkedData)
			{
				//H 340 Handle to data link object (hard pointer).
				template.ValueHandle = this._mergedReaders.HandleReference();
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
			format.Color = this._mergedReaders.ReadCmColor(this.R2004Pre);
			//H 340 Text style handle (hard pointer)
			template.TextStyleHandle = this.handleReference();
			//BD 144 Text height
			format.TextHeight = this._mergedReaders.ReadBitDouble();
		}
	}
}
