using ACadSharp.Entities;
using ACadSharp.Tables;
using ACadSharp.Tables.Collections;
using System;
using Xunit;

namespace ACadSharp.Tests.Tables.Collections;

public abstract class TableEntryCommonTests<T>
	where T : TableEntry
{
	[Fact]
	public void AddEntityAssignsDocumentTableEntryTest()
	{
		CadDocument doc = new CadDocument();

		Line line = new Line();
		T detached = this.getEntryFromEntity(line);

		doc.Entities.Add(line);

		var table = this.getTable(doc);
		var entry = this.getEntryFromEntity(line);

		Assert.Equal(doc, entry.Document);
		Assert.NotEqual(detached, entry);
		Assert.Equal(table.GetDefaultEntry(), entry);
		Assert.Contains(line, table.GetReferences(table.GetDefaultEntry().Name));
	}

	[Fact]
	public void AssignEntryNotInTableAddsItTest()
	{
		string name = "new_entry";
		CadDocument doc = new CadDocument();

		Line line = new Line();
		doc.Entities.Add(line);

		T entry = this.createInstance(name);
		this.setEntryToEntity(line, entry);

		Assert.True(getTable(doc).Contains(name));
		Assert.Equal(doc, getEntryFromEntity(line).Document);
		Assert.Equal(entry, getEntryFromEntity(line));
		Assert.Contains(line, getTable(doc).GetReferences(name));
	}

	[Fact]
	public void ChangeEntryUpdatesReferencesTest()
	{
		CadDocument doc = new CadDocument();

		T first = this.createInstance("first");
		T second = this.createInstance("second");
		getTable(doc).Add(first);
		getTable(doc).Add(second);

		Line line = new Line();
		doc.Entities.Add(line);

		setEntryToEntity(line, first);
		Assert.Contains(line, getTable(doc).GetReferences("first"));

		setEntryToEntity(line, second);
		Assert.Equal(second, getEntryFromEntity(line));
		Assert.Contains(line, getTable(doc).GetReferences("second"));
	}

	[Fact]
	public void LayerRenameUpdatesReferencesTest()
	{
		CadDocument doc = new CadDocument();

		T layer = this.createInstance("original");
		getTable(doc).Add(layer);

		Line line = new Line();
		doc.Entities.Add(line);
		this.setEntryToEntity(line, layer);

		layer.Name = "renamed";

		Assert.True(getTable(doc).Contains("renamed"));
		Assert.False(getTable(doc).Contains("original"));
		Assert.Contains(line, getTable(doc).GetReferences("renamed"));
		Assert.Empty(getTable(doc).GetReferences("original"));
	}

	[Fact]
	public void RemoveEntityUnassignReferenceTest()
	{
		CadDocument doc = new CadDocument();

		T entry = this.createInstance("detachable");
		this.getTable(doc).Add(entry);

		Line line = new Line();
		doc.Entities.Add(line);
		setEntryToEntity(line, entry);

		doc.Entities.Remove(line);

		Assert.Null(line.Document);
		Assert.Null(getEntryFromEntity(line).Document);
		Assert.Equal(entry.Name, getEntryFromEntity(line).Name);
		Assert.Empty(this.getTable(doc).GetReferences("detachable"));
	}

	[Fact]
	public void RemoveEntityUnassignsReferenceTest()
	{
		CadDocument doc = new CadDocument();

		T entry = this.createInstance("detachable");
		getTable(doc).Add(entry);

		Line line = new Line();
		doc.Entities.Add(line);
		setEntryToEntity(line, entry);

		doc.Entities.Remove(line);

		Assert.Null(line.Document);
		Assert.Null(getEntryFromEntity(line).Document);
		Assert.Equal(entry.Name, getEntryFromEntity(line).Name);
		Assert.Empty(getTable(doc).GetReferences("detachable"));
	}

	[Fact]
	public void TableEntryReferenceTest()
	{
		string name = "existing";
		T entry = this.createInstance(name);

		CadDocument doc = new CadDocument();
		getTable(doc).Add(entry);

		Line line = new Line();
		doc.Entities.Add(line);

		Assert.Equal(doc, getEntryFromEntity(line).Document);
		Assert.Empty(getTable(doc).GetReferences(name));
		Assert.NotEmpty(getTable(doc).GetReferences(getTable(doc).GetDefaultEntry().Name));
		Assert.NotEmpty(getTable(doc).GetReferences(getEntryFromEntity(line).Name));

		setEntryToEntity(line, entry);
		Assert.Equal(entry, getEntryFromEntity(line));

		getTable(doc).Remove(name);

		Assert.Equal(getTable(doc).GetDefaultEntry(), getEntryFromEntity(line));
	}

	protected abstract T createInstance(string name);

	protected T getEntryFromEntity(Entity entity)
	{
		if (typeof(T) == typeof(Layer))
		{
			return (T)(object)entity.Layer;
		}
		else if (typeof(T) == typeof(LineType))
		{
			return (T)(object)entity.LineType;
		}
		else
		{
			throw new NotImplementedException($"getEntryFromEntity is not implemented for type {typeof(T).Name}");
		}
	}

	protected abstract Table<T> getTable(CadDocument document);

	protected void setEntryToEntity(Entity entity, T entry)
	{
		switch (entry)
		{
			case Layer layer:
				entity.Layer = layer;
				break;
			case LineType lineType:
				entity.LineType = lineType;
				break;
			default:
				throw new NotImplementedException();
		}
	}
}
