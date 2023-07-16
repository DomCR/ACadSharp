using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfTablesSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.TablesSection; } }

		public DxfTablesSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder) : base(writer, document, holder)
		{
		}

		protected override void writeSection()
		{
			this.writeTable(this._document.VPorts);
			this.writeTable(this._document.LineTypes);
			this.writeTable(this._document.Layers);
			this.writeTable(this._document.TextStyles);
			this.writeTable(this._document.Views);
			this.writeTable(this._document.UCSs);
			this.writeTable(this._document.AppIds);
			this.writeTable(this._document.DimensionStyles, DxfSubclassMarker.DimensionStyleTable);
			this.writeTable(this._document.BlockRecords);
		}

		private void writeTable<T>(Table<T> table, string subclass = null)
			where T : TableEntry
		{
			this._writer.Write(DxfCode.Start, DxfFileToken.EntityTable);
			this._writer.Write(DxfCode.SymbolTableName, table.ObjectName);

			this.writeCommonObjectData(table);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Table);

			this._writer.Write(70, table.Count);

			if (!string.IsNullOrEmpty(subclass))
			{
				this._writer.Write(DxfCode.Subclass, subclass);
			}

			foreach (T entry in table)
			{
				writeEntry(entry);
			}

			this._writer.Write(DxfCode.Start, DxfFileToken.EndTable);
		}

		private void writeEntry<T>(T entry)
			where T : TableEntry
		{
			DxfMap map = DxfMap.Create<T>();

			this._writer.Write(DxfCode.Start, entry.ObjectName);

			this.writeCommonObjectData(entry);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.TableRecord);

			switch (entry)
			{
				case LineType ltype:
					this.writeLineType(ltype, map.SubClasses[ltype.SubclassMarker]);
					break;
				default:
					this.writeClassMap(map.SubClasses[entry.SubclassMarker], entry);
					break;
			}

			this.writeExtendedData(entry);
		}

		private void writeCommonEntryCodes<T>(T entry)
			where T : TableEntry
		{
			this._writer.Write(DxfCode.Subclass, entry.SubclassMarker);
			this._writer.Write(DxfCode.SymbolTableName, entry.Name);
			this._writer.Write(70, entry.Flags);
		}

		private void writeLineType(LineType linetype, DxfClassMap map)
		{
			this.writeCommonEntryCodes(linetype);

			this._writer.Write(3, linetype.Description, map);

			this._writer.Write(72, (short)linetype.Alignment, map);
			this._writer.Write(73, (short)linetype.Segments.Count(), map);
			this._writer.Write(40, linetype.PatternLen);

			foreach (LineType.Segment s in linetype.Segments)
			{
				this._writer.Write(49, s.Length);
				this._writer.Write(74, (short)s.Shapeflag);

				if (s.Shapeflag != LinetypeShapeFlags.None)
				{
					if (s.Shapeflag.HasFlag(LinetypeShapeFlags.Shape))
					{
						this._writer.Write(75, s.ShapeNumber);
					}
					if (s.Shapeflag.HasFlag(LinetypeShapeFlags.Text))
					{
						this._writer.Write(75, (short)0);
					}

					if (s.Style == null)
					{
						this._writer.Write(340, 0uL);
					}
					else
					{
						this._writer.Write(340, s.Style.Handle);
					}

					this._writer.Write(46, s.Scale);
					this._writer.Write(50, s.Rotation * MathUtils.DegToRad);
					this._writer.Write(44, s.Offset.X);
					this._writer.Write(45, s.Offset.Y);

					if (!string.IsNullOrEmpty(s.Text))
					{
						this._writer.Write(9, s.Text);
					}
				}
			}
		}
	}
}
