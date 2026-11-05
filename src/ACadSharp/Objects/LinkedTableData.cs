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
	public ObservableList<TableEntity.Column> Columns { get; } = new();

	/// <summary>
	/// Gets the collection of rows in the table.
	/// </summary>
	/// <remarks>The returned collection is initialized and can be observed for changes.</remarks>
	public ObservableList<TableEntity.Row> Rows { get; } = new();

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.LinkedTableData;

	public LinkedTableData()
	{
		this.Rows.OnBeforeAdd += this.onBeforeAdd;
		this.Columns.OnBeforeAdd += this.onBeforeAdd;
		this.Rows.OnRemove += this.onRemove;
		this.Columns.OnRemove += this.onRemove;
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