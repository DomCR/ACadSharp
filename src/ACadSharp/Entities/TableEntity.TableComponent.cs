using ACadSharp.Attributes;
using ACadSharp.Objects;
using CSUtilities.Collections.Generic;
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
		public ObservableList<Cell> Cells { get; private set; }

		/// <inheritdoc/>
		public CellStyle CellStyle { get; set; }

		/// <inheritdoc/>
		public CellStyle CellStyleOverride { get; set; } = new();

		/// <inheritdoc/>
		[DxfCodeValue(90)]
		public int CustomData { get; set; }

		/// <inheritdoc/>
		public List<CustomDataEntry> CustomDataCollection { get; private set; } = new();

		/// <inheritdoc/>
		public LinkedTableData Owner
		{
			get { return _owner; }

			internal set
			{
				this._owner = value;

				this.CellStyle?.Owner = this._owner;
				this.CellStyleOverride?.Owner = this._owner;
				foreach (var cell in this.Cells)
				{
					cell.Owner = this._owner;
				}
			}
		}

		private LinkedTableData _owner;

		protected TableComponent()
		{
			this.initialize();
		}

		private void initialize()
		{
			this.Cells = new ObservableList<Cell>();
			this.Cells.OnBeforeAdd += this.onBeforeAdd;
			this.Cells.OnRemove += this.onRemove;
		}

		public TableComponent Clone()
		{
			TableComponent clone = this.MemberwiseClone() as TableComponent;
			clone.initialize();
			clone.Owner = null;

			foreach (var item in this.Cells)
			{
				clone.Cells.Add(item.Clone());
			}

			return clone;
		}

		private void onBeforeAdd<T>(object sender, ObservableListEventArgs<T> e)
			where T : Cell
		{
			if (e.Item.Owner != null)
			{
				throw new System.ArgumentException($"{e.Item.GetType().FullName} already belongs to a table.");
			}

			e.Item.Owner = this.Owner;
		}

		private void onRemove<T>(object sender, ObservableListEventArgs<T> e)
			where T : Cell
		{
			e.Item.Owner = null;
		}
	}
}