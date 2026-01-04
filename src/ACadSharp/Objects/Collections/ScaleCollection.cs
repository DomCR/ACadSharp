namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of named <see cref="Scale"/> objects within a CAD dictionary.
	/// </summary>
	/// <remarks>This collection provides access to individual scale definitions by name and supports enumeration of
	/// all scales in the associated CAD dictionary. Modifications to the collection are reflected in the underlying
	/// dictionary.</remarks>
	public class ScaleCollection : ObjectDictionaryCollection<Scale>
	{
		/// <inheritdoc/>
		public ScaleCollection(CadDictionary dictionary) : base(dictionary)
		{
		}

		/// <summary>
		/// Populates the collection with a standard set of default <see cref="Scale"/> definitions commonly used in technical drawings if they do not already exist.
		/// </summary>
		/// <remarks>
		/// This method adds a predefined set of scales to the collection if they are not already present.
		/// Calling this method multiple times has no effect on existing entries.
		/// </remarks>
		public void CreateDefaults()
		{
			this._dictionary.TryAdd(new Scale { Name = "1:1", PaperUnits = 1.0, DrawingUnits = 1.0, IsUnitScale = true });
			this._dictionary.TryAdd(new Scale { Name = "1:2", PaperUnits = 1.0, DrawingUnits = 2.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:4", PaperUnits = 1.0, DrawingUnits = 4.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:5", PaperUnits = 1.0, DrawingUnits = 5.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:8", PaperUnits = 1.0, DrawingUnits = 8.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:10", PaperUnits = 1.0, DrawingUnits = 10.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:16", PaperUnits = 1.0, DrawingUnits = 16.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:20", PaperUnits = 1.0, DrawingUnits = 20.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:30", PaperUnits = 1.0, DrawingUnits = 30.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:40", PaperUnits = 1.0, DrawingUnits = 40.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:50", PaperUnits = 1.0, DrawingUnits = 50.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "1:100", PaperUnits = 1.0, DrawingUnits = 100.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "2:1", PaperUnits = 2.0, DrawingUnits = 1.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "4:1", PaperUnits = 4.0, DrawingUnits = 1.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "8:1", PaperUnits = 8.0, DrawingUnits = 1.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "10:1", PaperUnits = 10.0, DrawingUnits = 1.0, IsUnitScale = false });
			this._dictionary.TryAdd(new Scale { Name = "100:1", PaperUnits = 100.0, DrawingUnits = 1.0, IsUnitScale = false });
		}
	}
}
