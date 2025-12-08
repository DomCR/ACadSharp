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