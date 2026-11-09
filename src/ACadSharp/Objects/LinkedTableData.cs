using ACadSharp.Attributes;
using ACadSharp.Entities;
using CSUtilities.Collections.Generic;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="LinkedTableData"/> object.
/// </summary>
/// <remarks>
/// Dxf class name <see cref="DxfSubclassMarker.LinkedTableData"/>
/// </remarks>
[DxfSubClass(DxfSubclassMarker.LinkedTableData)]
public abstract class LinkedTableData : LinkedData
{
	/// <summary>
	/// Gets the collection of columns defined for the table entity.
	/// </summary>
	public ObservableList<TableEntity.Column> Columns { get; private set; } = new();

	/// <summary>
	/// Gets the collection of rows in the table.
	/// </summary>
	/// <remarks>The returned collection is initialized and can be observed for changes.</remarks>
	public ObservableList<TableEntity.Row> Rows { get; private set; } = new();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.LinkedTableData;

	/// <summary>
	/// Initializes a new instance of the LinkedTableData class and registers internal handlers for row and column add and
	/// remove events.
	/// </summary>
	public LinkedTableData()
	{
		this.initialize();
	}

	private void initialize()
	{
		this.Rows.OnBeforeAdd += this.onBeforeAdd;
		this.Columns.OnBeforeAdd += this.onBeforeAdd;
		this.Rows.OnRemove += this.onRemove;
		this.Columns.OnRemove += this.onRemove;
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		LinkedTableData clone = base.Clone() as LinkedTableData;
		clone.Columns = new ObservableList<TableEntity.Column>();
		clone.Rows = new ObservableList<TableEntity.Row>();
		clone.initialize();

		foreach (var item in this.Columns)
		{
			clone.Columns.Add(item.Clone() as TableEntity.Column);

		}

		foreach (var item in this.Rows)
		{
			clone.Rows.Add(item.Clone() as TableEntity.Row);
		}

		return clone;
	}

	private void onBeforeAdd<T>(object sender, ObservableListEventArgs<T> e)
		where T : TableEntity.TableComponent
	{
		if (e.Item.Owner != null)
		{
			throw new System.ArgumentException($"{e.Item.GetType().FullName} already belongs to a table.");
		}

		e.Item.Owner = this;
	}

	private void onRemove<T>(object sender, ObservableListEventArgs<T> e)
		where T : TableEntity.TableComponent
	{
		e.Item.Owner = null;
	}
}