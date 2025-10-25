using ACadSharp.Attributes;
using ACadSharp.Objects;
using System.Collections.Generic;
using static ACadSharp.Entities.TableEntity;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="LinkedData"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.LinkedData"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.LinkedData)]
	public abstract class LinkedData : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinkedData;

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		[DxfCodeValue(1)]
		public override string Name { get => base.Name; set => base.Name = value; }

		/// <summary>
		/// Gets or sets the description associated with the object.
		/// </summary>
		[DxfCodeValue(300)]
		public string Description { get; set; }
	}

	/// <summary>
	/// Represents a <see cref="LinkedTableData"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.LinkedTableData"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.LinkedTableData)]
	public abstract class LinkedTableData : LinkedData
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.LinkedTableData;
	}

	/// <summary>
	/// Represents a <see cref="FormattedTableData"/> object.
	/// </summary>
	/// <remarks>
	/// Dxf class name <see cref="DxfSubclassMarker.FormattedTableData"/>
	/// </remarks>
	[DxfSubClass(DxfSubclassMarker.FormattedTableData)]
	public abstract class FormattedTableData : LinkedTableData
	{
		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.FormattedTableData;

		public List<CellRange> MergedCellRanges { get; set; } = new();

		public CellStyle CellStyleOverride { get; set; } = new();
	}
}
