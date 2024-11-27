using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="TableEntity"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityTable"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableEntity"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityTable)]
	[DxfSubClass(DxfSubclassMarker.TableEntity)]
	public class TableEntity : Insert
	{
		public class TableAttribute
		{
			public string Value { get; internal set; }
		}

		public class CellValue
		{
			public int Flags { get; set; }

			public bool IsEmpty
			{
				get
				{
					return (this.Flags & 1) != 0;
				}
				set
				{
					if (value)
					{
						this.Flags |= 1;
					}
					else
					{
						this.Flags &= -2;
					}
				}
			}

			/// <summary>
			/// Text string in a cell.
			/// </summary>
			[DxfCodeValue(1)]
			public string Text { get; set; }

			public object Value { get; set; }
		}

		public enum CellValueType
		{
			/// <summary>
			/// Unknown
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// 32 bit Long value
			/// </summary>
			Long = 1,
			/// <summary>
			/// Double value
			/// </summary>
			Double = 2,
			/// <summary>
			/// String value
			/// </summary>
			String = 4,
			/// <summary>
			/// Date value
			/// </summary>
			Date = 8,
			/// <summary>
			/// 2D point value
			/// </summary>
			Point2D = 0x10,
			/// <summary>
			/// 3D point value
			/// </summary>
			Point3D = 0x20,
			/// <summary>
			/// Object handle value
			/// </summary>
			Handle = 0x40,
			/// <summary>
			/// Buffer value
			/// </summary>
			Buffer = 0x80,
			/// <summary>
			/// Result buffer value
			/// </summary>
			ResultBuffer = 0x100,
			/// <summary>
			/// General
			/// </summary>
			General = 0x200
		}

		public enum CellStypeType
		{
			Cell = 1,
			Row = 2,
			Column = 3,
			FormattedTableData = 4,
			Table = 5
		}

		[System.Flags]
		public enum TableCellStateFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0x0,

			/// <summary>
			/// Content  locked
			/// </summary>
			ContentLocked = 0x1,

			/// <summary>
			/// Content read only
			/// </summary>
			ContentReadOnly = 0x2,

			/// <summary>
			/// Linked.
			/// </summary>
			Linked = 0x4,

			/// <summary>
			/// Content modifed after update
			/// </summary>
			ContentModifiedAfterUpdate = 0x8,

			/// <summary>
			/// Format locked
			/// </summary>
			FormatLocked = 0x10,

			/// <summary>
			/// Format readonly
			/// </summary>
			FormatReadOnly = 0x20,

			/// <summary>
			/// Format was modified after update
			/// </summary>
			FormatModifiedAfterUpdate = 0x40,
		}

		public class CellStyle
		{
			public CellStypeType Type { get; set; }
		}

		public class CustomDataEntry
		{
			public string Name { get; internal set; }

			public CellValue Value { get; internal set; } = new CellValue();
		}

		public enum CellType
		{
			Text = 1,
			Block = 2
		}

		public class Cell
		{
			/// <summary>
			/// Cell type.
			/// </summary>
			[DxfCodeValue(171)]
			public CellType Type { get; set; }

			/// <summary>
			/// Cell flag value.
			/// </summary>
			[DxfCodeValue(172)]
			public int FlagValue { get; set; }

			/// <summary>
			/// Cell merged value.
			/// </summary>
			[DxfCodeValue(173)]
			public int MergedValue { get; set; }

			/// <summary>
			/// Boolean flag indicating if the autofit option is set for the cell.
			/// </summary>
			[DxfCodeValue(174)]
			public bool Autofit { get; set; }

			/// <summary>
			/// Cell border width.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(175)]
			public int BorderWidth { get; set; }

			/// <summary>
			/// Cell border height.
			/// </summary>
			/// <remarks>
			/// Applicable only for merged cells.
			/// </remarks>
			[DxfCodeValue(176)]
			public int BorderHeight { get; set; }

			/// <summary>
			/// Flag value for a virtual edge.
			/// </summary>
			[DxfCodeValue(178)]
			public short VirtualEdgeFlag { get; set; }

			/// <summary>
			/// Rotation value.
			/// </summary>
			/// <remarks>
			/// Applicable for a block-type cell and a text-type cell.
			/// </remarks>
			[DxfCodeValue(145)]
			public double Rotation { get; set; }

			public CellValue Value { get; } = new CellValue();

			public string ToolTip { get; internal set; }

			public TableCellStateFlags StateFlags { get; internal set; }

			public bool HasLinkedData { get; internal set; }

			public int CustomData { get; internal set; }

			public List<CustomDataEntry> CustomDataCollection { get; internal set; } = new();

			public List<Content> Contents { get; internal set; } = new();

			public class Content
			{
			}
		}

		public enum TableCellContentType
		{
			Value,
			Field,
			Block
		}

		public class Row
		{
			/// <summary>
			/// Row height.
			/// </summary>
			[DxfCodeValue(141)]
			public double Height { get; set; }

			public List<Cell> Cells { get; set; } = new();
		}

		public class Column
		{
			/// <summary>
			/// Column width.
			/// </summary>
			[DxfCodeValue(142)]
			public double Width { get; set; }

			public string Name { get; set; }

			public int CustomData { get; set; }

			public List<CustomDataEntry> CustomDataCollection { get; internal set; }
		}

		public class Content
		{
			public string Name { get; set; }

			public string Description { get; set; }
		}

		internal class BreakData
		{
			internal struct BreakHeight
			{
				public XYZ Position { get; internal set; }
				public double Height { get; internal set; }
			}

			public BreakOptionFlags Flags { get; internal set; }
			public BreakFlowDirection FlowDirection { get; internal set; }
			public double BreakSpacing { get; internal set; }
			public List<BreakHeight> Heights { get; internal set; } = new List<BreakHeight>();
		}


		internal class BreakRowRange
		{
			public XYZ Position { get; internal set; }
			public int StartRowIndex { get; internal set; }
			public int EndRowIndex { get; internal set; }
		}

		[System.Flags]
		public enum BreakOptionFlags
		{
			/// <summary>
			/// None
			/// </summary>
			None = 0,
			/// <summary>
			/// Enable breaks
			/// </summary>
			EnableBreaks = 1,
			/// <summary>
			/// Repeat top labels
			/// </summary>
			RepeatTopLabels = 2,
			/// <summary>
			/// Repeat bottom labels
			/// </summary>
			RepeatBottomLabels = 4,
			/// <summary>
			/// Allow manual positions
			/// </summary>
			AllowManualPositions = 8,
			/// <summary>
			/// Allow manual heights
			/// </summary>
			AllowManualHeights = 16
		}

		public enum BreakFlowDirection
		{
			/// <summary>
			/// Right
			/// </summary>
			Right = 1,
			/// <summary>
			/// Vertical
			/// </summary>
			Vertical = 2,
			/// <summary>
			/// Left
			/// </summary>
			Left = 4
		}

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityTable;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableEntity;

		/// <summary>
		/// Table data version
		/// </summary>
		[DxfCodeValue(280)]
		public short Version { get; internal set; }

		/// <summary>
		/// Horizontal direction vector
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ HorizontalDirection { get; set; }

		/// <summary>
		/// Flag for table value.
		/// </summary>
		[DxfCodeValue(90)]
		public int ValueFlag { get; internal set; }

		/// <summary>
		/// Table rows.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<Row> Rows { get; set; } = new List<Row>();

		/// <summary>
		/// Table columns
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 92)]
		public List<Column> Columns { get; set; } = new List<Column>();

		/// <summary>
		/// Flag for an override.
		/// </summary>
		[DxfCodeValue(93)]
		public bool OverrideFlag { get; set; }

		/// <summary>
		/// Flag for an override of border color.
		/// </summary>
		[DxfCodeValue(94)]
		public bool OverrideBorderColor { get; set; }

		/// <summary>
		/// Flag for an override of border line weight.
		/// </summary>
		[DxfCodeValue(95)]
		public bool OverrideBorderLineWeight { get; set; }

		/// <summary>
		/// Flag for an override of border visibility.
		/// </summary>
		[DxfCodeValue(96)]
		public bool OverrideBorderVisibility { get; set; }

		/// <summary>
		/// Table Style
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public TableStyle Style { get; set; }

		//343	Hard pointer ID of the owning BLOCK record
		[DxfCodeValue(DxfReferenceType.Handle, 343)]
		public BlockRecord BlockRecord { get; set; }

		public Content TableContent { get; set; } = new();

		internal BreakData TableBreakData { get; set; } = new();

		internal List<BreakRowRange> BreakRowRanges { get; set; } = new();
	}
}
