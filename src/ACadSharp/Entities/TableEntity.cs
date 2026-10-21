using ACadSharp.Attributes;
using ACadSharp.Classes;
using ACadSharp.Extensions;
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

		// [PATCH] The base CadObject.Clone uses MemberwiseClone, which copies the Content reference
		// (so clone.Content === original.Content). Re-pointing clone.Content.Style in AssignDocument would
		// then corrupt the SOURCE document's table. Build an independent TableContent: the row/column cell
		// data is shared by reference (the writer only reads it during serialization, and it holds no handles),
		// while the two TableStyle references are cloned detached (Document == null) so they can be re-registered
		// into the target document without a cross-document dangling pointer (mirrors Dimension.Clone for its style).
		TableContent src = this.Content;
		TableContent content = new TableContent
		{
			Style = src.Style?.CloneTyped(),
			StyleOverride = src.StyleOverride?.CloneTyped(),
			CellStyleOverride = src.CellStyleOverride,
			Name = src.Name,
			Description = src.Description,
		};

		foreach (Row r in src.Rows)
		{
			content.Rows.Add(r);
		}

		foreach (Column c in src.Columns)
		{
			content.Columns.Add(c);
		}

		foreach (CellRange m in src.MergedCellRanges)
		{
			content.MergedCellRanges.Add(m);
		}

		clone.Content = content;

		return clone;
	}

	/// <inheritdoc/>
	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		// Re-register the table style into the target document so the writer resolves a valid handle
		// instead of the source document's (dangling) handle. By-name reuse matches the library's
		// established behavior (see Dimension.AssignDocument); a style already present in the target
		// document is reused, otherwise the detached clone is added and assigned a new handle.
		this.Content.Style = this.updateCollectionEntry(this.Content.Style, s => this.Content.Style = s, doc.TableStyles);
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
	/// AutoCAD TransformBy semantics (verified value-by-value against the sample_VBAtransformed.dwg baseline):
	/// - the insertion point is moved by the full transform;
	/// - rotation and scale are "baked" into the anonymous block's content: each entity in the block is
	///   transformed about the block origin by the linear part (without translation)
	///   (MText alignment point/height/width in the block, Solid corner points, and Line endpoints all behave this way);
	/// - the table's own Rotation keeps its original value (0 stays 0), and the referenced XScale/YScale/ZScale also keep
	///   their original values (all are 1 in the baseline file) — rotation is reflected in the block geometry and the HorizontalDirection vector;
	/// - the horizontal direction vector is rotated by the linear part only, unaffected by translation;
	/// - column widths and row heights are scaled by the scale factor (60 -> 59.88, i.e. 60 x 0.998);
	/// - the transparency of entities in the block is normalized to ByBlock (in the baseline file the block's Solid/MText are all
	///   (ByBlock, 100); the source file has (ByLayer, -1)/(ByBlock, 0)).
	/// </remarks>
	public override void ApplyTransform(Transform transform)
	{
		// [PATCH] The base Insert.ApplyTransform adds the transform rotation to Rotation and multiplies the
		// scale into XScale/YScale/ZScale; but AutoCAD does not do this for tables (in the baseline file Rotation=0,
		// xscale=yscale=1 all keep their original values), rotation/scale are baked into the block content. Record the original values first, restore them after the base runs.
		double originalRotation = this.Rotation;
		double originalXScale = this.XScale;
		double originalYScale = this.YScale;
		double originalZScale = this.ZScale;
		base.ApplyTransform(transform);
		this.Rotation = originalRotation;
		this.XScale = originalXScale;
		this.YScale = originalYScale;
		this.ZScale = originalZScale;

		// Horizontal direction vector: rotation only, unaffected by translation
		XYZ dir = this.HorizontalDirection;
		if (dir != XYZ.Zero)
		{
			this.HorizontalDirection = transform.ApplyRotation(dir);
		}

		// Scale factor: take the average scale over the axes
		XYZ sc = transform.Scale;
		double uniform = (Math.Abs(sc.X) + Math.Abs(sc.Y) + Math.Abs(sc.Z)) / 3.0;
		if (uniform.IsZero())
		{
			uniform = MathHelper.Epsilon;
		}

		// Column widths and row heights are scaled by the scale factor
		foreach (var column in this.Columns)
		{
			column.Width *= uniform;
		}
		foreach (var row in this.Rows)
		{
			row.Height *= uniform;
		}

		// [PATCH] When AutoCAD transforms a table it materializes the effective text height of each row's cells as cell-level
		// style overrides and scales them uniformly (verified value-by-value against sample_VBAtransformed.dwg: title row
		// 6.0 -> 5.988, data rows 4.5 -> 4.491; HasData stays false). Row assignment rule:
		// first row is the title style, second row is the header style, the rest are the data style.
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

		// [PATCH] Bake rotation and scale into the anonymous block content: each entity in the block is
		// transformed about the block origin by the linear part (without translation); and the transparency of entities in the block is normalized to ByBlock.
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