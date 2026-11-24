using ACadSharp.Attributes;
using ACadSharp.Classes;
using CSUtilities.Collections.Generic;

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
	public ObservableList<CellStyle> CellStyles { get; private set; } = new();

	/// <summary>
	/// Gets the style settings applied to data cells within the table entity.
	/// </summary>
	public CellStyle DataCellStyle
	{
		get
		{
			return this._dataCellStyle;
		}
		set
		{
			this._dataCellStyle = value;
			this._dataCellStyle.Owner = this;
		}
	}

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
	public CellStyle HeaderCellStyle
	{
		get
		{
			return this._headerCellStyle;
		}
		set
		{
			this._headerCellStyle = value;
			this._headerCellStyle.Owner = this;
		}
	}

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

	/// <summary>
	/// Gets or sets a value indicating whether the header row is suppressed.
	/// </summary>
	[DxfCodeValue(281)]
	public bool SuppressHeaderRow { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the title should be suppressed.
	/// </summary>
	[DxfCodeValue(280)]
	public bool SuppressTitle { get; set; }

	/// <summary>
	/// Gets or sets the cell style applied to table cells.
	/// </summary>
	/// <remarks>When set, the style's Owner is assigned to the current table instance.</remarks>
	public CellStyle TableCellStyle
	{
		get
		{
			return this._tableCellStyle;
		}
		set
		{
			this._tableCellStyle = value;
			this._tableCellStyle.Owner = this;
		}
	}

	/// <summary>
	/// Gets or sets the cell style used for title cells.
	/// </summary>
	/// <remarks>When set, the style owner is assigned to the current instance.</remarks>
	public CellStyle TitleCellStyle
	{
		get
		{
			return this._titleCellStyle;
		}
		set
		{
			this._titleCellStyle = value;
			this._titleCellStyle.Owner = this;
		}
	}

	/// <summary>
	/// Gets or sets the vertical margin, in drawing units, applied to the content within a cell.
	/// </summary>
	[DxfCodeValue(41)]
	public double VerticalCellMargin { get; set; } = 0.06;

	/// <summary>
	/// Represents the default name used when no specific name is provided.
	/// </summary>
	public const string DefaultName = "Standard";

	private CellStyle _dataCellStyle = null;

	private CellStyle _headerCellStyle = null;

	private CellStyle _tableCellStyle = null;

	private CellStyle _titleCellStyle = null;

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
		this.initialize();
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

	private void initialize()
	{
		this.HeaderCellStyle = CellStyle.DefaultHeaderCellStyle;
		this.DataCellStyle = CellStyle.DefaultDataCellStyle;
		this.TitleCellStyle = CellStyle.DefaultTitleCellStyle;
		this.TableCellStyle = CellStyle.DefaultTableCellStyle;

		this.CellStyles = new ObservableList<CellStyle>();
		this.CellStyles.OnBeforeAdd += this.onBeforeAdd;
		this.CellStyles.OnRemove += this.onRemove;
	}

	private void onBeforeAdd<T>(object sender, ObservableListEventArgs<T> e)
		where T : CellStyle
	{
		if (e.Item.Owner != null)
		{
			throw new System.ArgumentException($"{e.Item.GetType().FullName} already belongs to a table.");
		}

		e.Item.Owner = this;
	}

	private void onRemove<T>(object sender, ObservableListEventArgs<T> e)
		where T : CellStyle
	{
		e.Item.Owner = null;
	}
}