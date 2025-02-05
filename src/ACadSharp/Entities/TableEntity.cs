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
	[Obsolete("TableEntity is in a work in progress, is only supported in Dwg and for versions higher than AC1021")]
	public partial class TableEntity : Insert
	{
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
		public short Version { get; set; }

		/// <summary>
		/// Horizontal direction vector
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ HorizontalDirection { get; set; }

		/// <summary>
		/// Flag for table value.
		/// </summary>
		[DxfCodeValue(90)]
		public int ValueFlag { get; set; }

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
		[Obsolete("Is it needed??")]
		internal BlockRecord TableBlock { get { return this.Block; } }

		public TableContent Content { get; set; } = new();

		internal BreakData TableBreakData { get; set; } = new();

		internal List<BreakRowRange> BreakRowRanges { get; set; } = new();

		public Cell GetCell(int row, int column)
		{
			return this.Rows[row].Cells[column];
		}

		/// <inheritdoc/>
		public override BoundingBox GetBoundingBox()
		{
			return BoundingBox.Null;
		}

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			return base.Clone();
		}
	}
}
