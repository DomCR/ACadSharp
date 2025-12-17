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
	public partial class TableEntity : Insert
	{
		[Flags]
		internal enum BorderOverrideFlags
		{
			None = 0,
			TitleHorizontalTop = 0x01,
			TitleHorizontalInsert = 0x02,
			TitleHorizontalBottom = 0x04,
			TitleVerticalLeft = 0x8,
			TitleVerticalInsert = 0x10,
			TitleVerticalRight = 0x20,
			HeaderHorizontalTop = 0x40,
			HeaderHorizontalInsert = 0x80,
			HeaderHorizontalBottom = 0x100,
			HeaderVerticalLeft = 0x200,
			HeaderVerticalInsert = 0x400,
			HeaderVerticalRight = 0x800,
			DataHorizontalTop = 0x1000,
			DataHorizontalInsert = 0x2000,
			DataHorizontalBottom = 0x4000,
			DataVerticalLeft = 0x8000,
			DataVerticalInsert = 0x10000,
			DataVerticalRight = 0x20000,
		}

		[Flags]
		internal enum TableOverrideFlags
		{
			None = 0,
			TitleSuppressed = 0x0001,
			HeaderSuppressed = 0x02,
			FlowDirection = 0x0004,
			HorizontalCellMargin = 0x0008,
			VerticalCellMargin = 0x0010,

			TitleRowColor = 0x0020,
			HeaderRowColor = 0x00040,
			DataRowColor = 0x0080,

			TitleRowFillNone = 0x0100,
			HeaderRowFillNone = 0x0200,
			DataRowFillNone = 0x0400,

			TitleRowFillColor = 0x0800,
			HeaderRowFillColor = 0x1000,
			DataRowFillColor = 0x2000,

			TitleRowAlign = 0x4000,
			HeaderRowAlign = 0x8000,
			DataRowAlign = 0x10000,

			TitleTextStyle = 0x20000,
			HeaderTextStyle = 0x40000,
			DataTextStyle = 0x80000,

			TitleRowHeight = 0x100000,
			HeaderRowHeight = 0x200000,
			DataRowHeight = 0x400000,
		}

		/// <summary>
		/// Table columns
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 92)]
		public List<Column> Columns { get { return this.Content.Columns; } }

		/// <summary>
		/// Horizontal direction vector
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ HorizontalDirection { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityTable;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

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
		/// Flag for an override.
		/// </summary>
		[DxfCodeValue(93)]
		public bool OverrideFlag { get; set; }

		/// <summary>
		/// Table rows.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Count, 91)]
		public List<Row> Rows { get { return this.Content.Rows; } }

		/// <summary>
		/// Gets or sets the table style associated with this object.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 342)]
		public TableStyle Style
		{
			get { return this.Content.Style; }
			set
			{
				this.Content.Style = value;
			}
		}

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableEntity;

		/// <summary>
		/// Flag for table value.
		/// </summary>
		[DxfCodeValue(90)]
		public int ValueFlag { get; set; }

		/// <summary>
		/// Table data version
		/// </summary>
		[DxfCodeValue(280)]
		public short Version { get; set; }

		internal List<BreakRowRange> BreakRowRanges { get; set; } = new();

		internal TableContent Content { get; set; } = new();

		[DxfCodeValue(DxfReferenceType.Handle, 343)]
		internal BlockRecord TableBlock { get { return this.Block; } }

		internal BreakData TableBreakData { get; set; } = new();

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			return base.Clone();
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		public Cell GetCell(int row, int column)
		{
			return this.Rows[row].Cells[column];
		}
	}
}