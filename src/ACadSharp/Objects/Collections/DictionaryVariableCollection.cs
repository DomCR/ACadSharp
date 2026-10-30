namespace ACadSharp.Objects.Collections
{
	/// <summary>
	/// Represents a collection of named <see cref="DictionaryVariable"/> stored as key-value pairs, providing methods to add, update, and
	/// retrieve variable values by name.
	/// </summary>
	/// <remarks>This collection is typically used to manage custom variables within a CAD dictionary context.
	/// Variable names are case-insensitive and must be unique within the collection. Adding a variable with an existing name
	/// updates its value.</remarks>
	public class DictionaryVariableCollection : ObjectDictionaryCollection<DictionaryVariable>
	{
		/// <inheritdoc/>
		public DictionaryVariableCollection(CadDictionary dictionary) : base(dictionary)
		{
		}

		/// <summary>
		/// Adds a variable with the specified name and value to the collection. If a variable with the same name already
		/// exists, its value is updated.
		/// </summary>
		/// <param name="name">The name of the variable to add or update. Cannot be null.</param>
		/// <param name="value">The value to assign to the variable. Can be null.</param>
		public void AddOrUpdateVariable(string name, string value)
		{
			if (this.TryGet(name, out DictionaryVariable v))
			{
				v.Value = value;
			}
			else
			{
				this.Add(new DictionaryVariable(name, value));
			}
		}

		/// <summary>
		/// Adds a new variable with the specified name and value to the collection if a variable with the same name does not
		/// already exist.
		/// </summary>
		/// <param name="name">The name of the variable to add. Cannot be null or empty.</param>
		/// <param name="value">The value to assign to the variable. Can be null or empty.</param>
		public void AddVariable(string name, string value)
		{
			if (!this.ContainsKey(name))
			{
				this.Add(new DictionaryVariable(name, value));
			}
		}

		/// <summary>
		/// Creates and adds default variables to the collection.
		/// </summary>
		/// <remarks>Call this method to ensure that the collection contains the standard default variables.
		/// Calling this method multiple times has no effect on existing entries.</remarks>
		public void CreateDefaults()
		{
			this.AddVariable(DictionaryVariable.CurrentMultiLeaderStyle, MultiLeaderStyle.DefaultName);
			this.AddVariable(DictionaryVariable.CurrentAnnotationScale, Scale.DefaultName);
			this.AddVariable(DictionaryVariable.CurrentTableStyle, TableStyle.DefaultName);
			this.AddVariable(DictionaryVariable.WipeoutFrame, ((int)WipeoutFrameType.DisplayAndPlotted).ToString());

			//View Section Style object not implemented
			this.AddVariable("CVIEWDETAILSTYLE", "Metric50");
			this.AddVariable("CVIEWSECTIONSTYLE", "Metric50");
		}

		/// <summary>
		/// Retrieves the value associated with the specified variable name.
		/// </summary>
		/// <param name="name">The name of the variable whose value to retrieve. Cannot be null.</param>
		/// <returns>The value of the variable if found; otherwise, null.</returns>
		public string GetValue(string name)
		{
			if (this.TryGet(name, out DictionaryVariable v))
			{
				return v.Value;
			}
			else
			{
				return null;
			}
		}
	}
}