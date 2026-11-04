using ACadSharp.Attributes;
using ACadSharp.Objects;
using System.Collections.Generic;
using static ACadSharp.Objects.TableStyle;

namespace ACadSharp.Entities;

public partial class TableEntity
{
	public abstract class TableComponent : ITableComponent
	{
		/// <summary>
		/// Gets or sets the collection of cells contained in the current object.
		/// </summary>
		public List<Cell> Cells { get; private set; } = new();

		/// <inheritdoc/>
		public CellStyle CellStyle { get; set; }

		/// <inheritdoc/>
		public CellStyle CellStyleOverride { get; set; } = new();

		/// <inheritdoc/>
		[DxfCodeValue(90)]
		public int CustomData { get; set; }

		/// <inheritdoc/>
		public List<CustomDataEntry> CustomDataCollection { get; } = new();

		/// <inheritdoc/>
		public LinkedTableData Owner
		{
			get { return _owner; }

			internal set
			{
				this._owner = value;

				this.CellStyle?.Owner = this._owner;
			}
		}

		private LinkedTableData _owner;
	}
}