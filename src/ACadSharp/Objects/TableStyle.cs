using ACadSharp.Attributes;
using ACadSharp.Entities;
using System.Collections.Generic;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="TableStyle"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableStyle)]
	[DxfSubClass(DxfSubclassMarker.TableStyle)]
	public class TableStyle : NonGraphicalObject
	{
		/// <summary>
		/// Gets the default MultiLeaderStyle
		/// </summary>
		public static TableStyle Default { get { return new TableStyle(DefaultName); } }

		/// <summary>
		/// Gets the collection of cell styles applied to the table entity.
		/// </summary>
		public List<TableEntity.CellStyle> CellStyles { get; } = new();

		/// <summary>
		/// Gets the style settings applied to data cells within the table entity.
		/// </summary>
		public TableEntity.CellStyle DataCellStyle { get; } = new TableEntity.CellStyle();

		/// <summary>
		/// Table style description.
		/// </summary>
		/// <remarks>
		/// 255 characters maximum.
		/// </remarks>
		[DxfCodeValue(3)]
		public string Description { get; set; }

		/// <summary>
		/// Meaning unknown.
		/// </summary>
		[DxfCodeValue(71)]
		public short Flags { get; internal set; }

		/// <summary>
		/// Gets or sets the direction in which table rows and columns are arranged.
		/// </summary>
		[DxfCodeValue(70)]
		public TableFlowDirectionType FlowDirection { get; set; }

		/// <summary>
		/// Gets the style settings applied to the header cells of the table.
		/// </summary>
		public TableEntity.CellStyle HeaderCellStyle { get; } = new TableEntity.CellStyle();

		/// <summary>
		/// Gets or sets the horizontal margin, in drawing units, applied to the content within each cell.
		/// </summary>
		[DxfCodeValue(40)]
		public double HorizontalCellMargin { get; set; } = 0.06d;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableStyle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker { get; } = DxfSubclassMarker.TableStyle;

		[DxfCodeValue(281)]
		public bool SuppressHeaderRow { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the title should be suppressed.
		/// </summary>
		[DxfCodeValue(280)]
		public bool SuppressTitle { get; set; }

		public TableEntity.CellStyle TableCellStyle { get; } = new TableEntity.CellStyle();

		public TableEntity.CellStyle TitleCellStyle { get; } = new TableEntity.CellStyle();

		/// <summary>
		/// Gets or sets the vertical margin, in drawing units, applied to the content within a cell.
		/// </summary>
		[DxfCodeValue(41)]
		public double VerticalCellMargin { get; set; } = 0.06;

		/// <summary>
		/// Represents the default name used when no specific name is provided.
		/// </summary>
		public const string DefaultName = "Standard";

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStyle"/> class.
		/// </summary>
		public TableStyle() : this(string.Empty) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="TableStyle"/> class
		/// and sets the name of this style.
		/// </summary>
		public TableStyle(string name) : base(name)
		{
		}
	}
}