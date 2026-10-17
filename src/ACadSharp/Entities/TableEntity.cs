using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using CSMath.Extensions;
using System;
using System.Collections.Generic;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="TableEntity"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityTable"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.TableEntity"/>
/// </remarks>
[DxfName(DxfFileToken.EntityTable)]
[DxfSubClass(DxfSubclassMarker.TableEntity)]
public partial class TableEntity : Insert, IDxfClassDefined
{
	/// <summary>
	/// Gets the table break data that defines how the table should be divided across breaks, including spacing, flow direction,
	/// </summary>
	public TableBreakData BreakData { get; } = new();

	/// <summary>
	/// Gets the collection of columns in the table entity.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Count, 92)]
	public List<Column> Columns { get { return this.Content.Columns; } }

	/// <summary>
	/// Gets or sets a value indicating whether bread data is available.
	/// </summary>
	public bool HasBreadData { get; set; }

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

	/// <summary>
	/// Initializes a new instance of the <see cref="TableEntity"/> class.
	/// </summary>
	public TableEntity() : base()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="TableEntity"/> class
	/// bound to the given anonymous <see cref="BlockRecord"/> that will host the table cache.
	/// </summary>
	/// <param name="block">Anonymous block record used as the table's owning block (DXF 343).</param>
	/// <exception cref="ArgumentNullException"></exception>
	public TableEntity(BlockRecord block) : base()
	{
		if (block is null) throw new ArgumentNullException(nameof(block));
		this.Block = block;
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		TableEntity clone = (TableEntity)base.Clone();

		return clone;
	}

	/// <inheritdoc/>
	public override BoundingBox GetBoundingBox()
	{
		return BoundingBox.Null;
	}

	/// <summary>
	/// Retrieves the cell at the specified row and column indices.
	/// </summary>
	/// <param name="row">The zero-based index of the row containing the cell to retrieve.</param>
	/// <param name="column">The zero-based index of the column containing the cell to retrieve.</param>
	/// <returns>The cell located at the specified row and column.</returns>
	public Cell GetCell(int row, int column)
	{
		return this.Rows[row].Cells[column];
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			CppClassName = DxfSubclassMarker.TableEntity,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.EntityTable,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.DisablesProxyWarningDialog,
			WasZombie = false,
		};
	}

	/// <summary>
	/// Gets the collection of merged cell ranges associated with this table.
	/// </summary>
	public List<CellRange> MergedCellRanges { get { return this.Content.MergedCellRanges; } }

	/// <summary>
	/// Gets the cell style override applied at the table level.
	/// Use this to set table-level overrides such as flow direction (FlowDirectionBottomToTop).
	/// </summary>
	public TableStyle.CellStyle CellStyleOverride { get { return this.Content.CellStyleOverride; } }

	/// <inheritdoc/>
	/// <remarks>
	/// AutoCAD TransformBy 语义（已用 sample_VBAtransformed.dwg 基准逐值核实）：
	/// - 插入点按完整变换移动；
	/// - 旋转与缩放被“烘焙”进匿名块的内容：块内每个实体按线性部分（不含平移）
	///   绕块原点变换（块内 MText 对齐点/高度/宽度、Solid 角点、Line 端点均如此）；
	/// - 表格自身的 Rotation 保持原值（0 仍为 0），参照的 XScale/YScale/ZScale 也保持
	///   原值（基准文件均为 1）——旋转体现在块几何与 HorizontalDirection 向量中；
	/// - 水平方向向量按线性部分旋转，不受平移影响；
	/// - 列宽与行高按缩放因子缩放（60 → 59.88 即 60 × 0.998）；
	/// - 块内实体的透明度被规范化为 ByBlock（基准文件块内 Solid/MText 均为
	///   (ByBlock, 100)，源文件为 (ByLayer, -1)/(ByBlock, 0)）。
	/// </remarks>
	public override void ApplyTransform(Transform transform)
	{
		// [PATCH] 基类 Insert.ApplyTransform 会把变换旋转叠加到 Rotation、把缩放乘到
		// XScale/YScale/ZScale 上；但 AutoCAD 对表格不这样做（基准文件 Rotation=0、
		// xscale=yscale=1 均保持原值），旋转/缩放烘焙进块内容。先记下原值，基类执行后还原。
		double originalRotation = this.Rotation;
		double originalXScale = this.XScale;
		double originalYScale = this.YScale;
		double originalZScale = this.ZScale;
		base.ApplyTransform(transform);
		this.Rotation = originalRotation;
		this.XScale = originalXScale;
		this.YScale = originalYScale;
		this.ZScale = originalZScale;

		// 水平方向向量：只旋转，不受平移影响
		XYZ dir = this.HorizontalDirection;
		if (dir != XYZ.Zero)
		{
			this.HorizontalDirection = transform.ApplyRotation(dir);
		}

		// 缩放因子：取各轴的平均缩放
		XYZ sc = transform.Scale;
		double uniform = (Math.Abs(sc.X) + Math.Abs(sc.Y) + Math.Abs(sc.Z)) / 3.0;
		if (uniform.IsZero())
		{
			uniform = MathHelper.Epsilon;
		}

		// 列宽与行高按缩放因子缩放
		foreach (var column in this.Columns)
		{
			column.Width *= uniform;
		}
		foreach (var row in this.Rows)
		{
			row.Height *= uniform;
		}

		// [PATCH] AutoCAD 变换表格时会把每行单元格的有效文字高度物化为单元格级
		// 样式覆盖并按均匀缩放（已对 sample_VBAtransformed.dwg 逐值核实：标题行
		// 6.0 → 5.988，数据行 4.5 → 4.491；HasData 保持 false）。行分配规则：
		// 首行为标题样式，次行为表头样式，其余为数据样式。
		TableStyle tstyle = this.Style;
		if (tstyle != null)
		{
			bool hasTitle = !tstyle.SuppressTitle;
			bool hasHeader = !tstyle.SuppressHeaderRow;
			int rowIdx = 0;
			foreach (var row in this.Rows)
			{
				TableStyle.CellStyle cs;
				if (hasTitle && rowIdx == 0)
				{
					cs = tstyle.TitleCellStyle;
				}
				else if (hasHeader && rowIdx == (hasTitle ? 1 : 0))
				{
					cs = tstyle.HeaderCellStyle;
				}
				else
				{
					cs = tstyle.DataCellStyle;
				}

				if (cs != null && cs.TextHeight > 0)
				{
					double h = cs.TextHeight * uniform;
					foreach (var cell in row.Cells)
					{
						cell.StyleOverride.TextHeight = h;
					}
				}

				rowIdx++;
			}
		}

		// [PATCH] 把旋转与缩放烘焙进匿名块内容：块内每个实体按线性部分
		//（不含平移）绕块原点变换；并把块内实体透明度规范化为 ByBlock。
		if (this.Block != null)
		{
			Matrix4 m = transform.Matrix;
			Matrix4 linear = new Matrix4(
				m.M00, m.M10, m.M20, 0,
				m.M01, m.M11, m.M21, 0,
				m.M02, m.M12, m.M22, 0,
				0, 0, 0, 1);
			Transform local = new Transform(linear);

			foreach (Entity e in this.Block.Entities)
			{
				e.ApplyTransform(local);
				e.Transparency = Transparency.ByBlock;
			}
		}
	}
}