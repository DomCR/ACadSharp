using ACadSharp.Attributes;
using ACadSharp.Classes;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="TableStyle"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectTableStyle"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.TableStyle"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectTableStyle)]
[DxfSubClass(DxfSubclassMarker.TableStyle)]
public partial class TableStyle : NonGraphicalObject, IDxfClassDefined
{
	/// <summary>
	/// Gets the default TableStyle.
	/// </summary>
	public static TableStyle Default { get { return new TableStyle(DefaultName); } }

	/// <summary>
	/// Gets the collection of cell styles applied to the table entity.
	/// </summary>
	public List<CellStyle> CellStyles { get; } = new();

	/// <summary>
	/// Gets the style settings applied to data cells within the table entity.
	/// </summary>
	public CellStyle DataCellStyle { get; set; } = CellStyle.DefaultDataCellStyle;

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
	public short Flags { get; set; }

	/// <summary>
	/// Gets or sets the direction in which table rows and columns are arranged.
	/// </summary>
	[DxfCodeValue(70)]
	public TableFlowDirectionType FlowDirection { get; set; } = TableFlowDirectionType.Down;

	/// <summary>
	/// Gets the style settings applied to the header cells of the table.
	/// </summary>
	public CellStyle HeaderCellStyle { get; set; } = CellStyle.DefaultHeaderCellStyle;

	/// <summary>
	/// Gets or sets the horizontal margin, in drawing units, applied to the content within each cell.
	/// </summary>
	[DxfCodeValue(40)]
	public double HorizontalCellMargin { get; set; } = 0.06d;

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectTableStyle;

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

	public CellStyle TableCellStyle { get; set; } = CellStyle.DefaultTableCellStyle;

	public CellStyle TitleCellStyle { get; set; } = CellStyle.DefaultTitleCellStyle;

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
		// A valid AutoCAD table style carries a non-empty description (e.g. "Standard").
		// AutoCAD flags a default style with a blank description as invalid during AUDIT.
		this.Description = name;
	}

	/// <inheritdoc/>
	/// <remarks>
	/// [FIX] The base <see cref="NonGraphicalObject.Clone"/> uses MemberwiseClone, which shared
	/// the <see cref="CellStyles"/> list and the four <see cref="CellStyle"/> instances with the
	/// source. A TableStyle cloned into another document must carry independent cell styles;
	/// their <see cref="ContentFormat.TextStyle"/> references are re-registered by
	/// <see cref="AssignDocument"/>.
	/// </remarks>
	public override CadObject Clone()
	{
		TableStyle clone = (TableStyle)base.Clone();

		// [FIX] MemberwiseClone shares the CellStyles list instance with the source.
		// Snapshot first, then clear the shared list and refill with independent clones
		// (same snapshot-Clear-refill pattern as CadObject.Clone for ExtendedData).
		var srcStyles = this.CellStyles.ToList();
		clone.CellStyles.Clear();
		foreach (var item in srcStyles)
		{
			clone.CellStyles.Add(item.Clone());
		}

		clone.DataCellStyle = this.DataCellStyle.Clone();
		clone.HeaderCellStyle = this.HeaderCellStyle.Clone();
		clone.TableCellStyle = this.TableCellStyle.Clone();
		clone.TitleCellStyle = this.TitleCellStyle.Clone();

		return clone;
	}

	/// <inheritdoc/>
	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		// Re-register each cell style's text style into the target document (mirrors
		// LineType/Segment.AssignDocument); otherwise the writer emits the source document's
		// (dangling) text-style handle and the table text falls back to the default font.
		var styles = new List<CellStyle>
		{
			this.DataCellStyle,
			this.HeaderCellStyle,
			this.TableCellStyle,
			this.TitleCellStyle,
		};
		styles.AddRange(this.CellStyles);

		foreach (var cellStyle in styles)
		{
			if (cellStyle?.TextStyle != null)
			{
				cellStyle.TextStyle = this.updateTableEntry(cellStyle.TextStyle, s => cellStyle.TextStyle = s, doc.TextStyles);
			}
		}
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableStyle,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectTableStyle,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = (ProxyFlags)4095,
			WasZombie = false,
		};
	}
}