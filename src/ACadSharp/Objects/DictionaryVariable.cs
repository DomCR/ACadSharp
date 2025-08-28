using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="DictionaryVariable"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionaryVar"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DictionaryVariables"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionaryVar)]
	[DxfSubClass(DxfSubclassMarker.DictionaryVariables)]
	public class DictionaryVariable : NonGraphicalObject
	{
		/// <summary>
		/// Represents the name of the current multi-leader style.
		/// </summary>
		/// <remarks>This constant is used to identify the current multi-leader style in the application. The value is
		/// a string literal: "CMLEADERSTYLE".</remarks>
		public const string CurrentMultiLeaderStyle = "CMLEADERSTYLE";

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDictionaryVar;

		/// <summary>
		/// Object schema number (currently set to 0)
		/// </summary>
		[DxfCodeValue(280)]
		public int ObjectSchemaNumber { get; internal set; }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DictionaryVariables;

		/// <summary>
		/// Value of variable
		/// </summary>
		[DxfCodeValue(1)]
		public string Value { get; set; }

		/// <inheritdoc/>
		public DictionaryVariable() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DictionaryVariable"/> class with the specified name and value.
		/// </summary>
		/// <remarks>The <paramref name="name"/> parameter is used to uniquely identify the dictionary variable, while
		/// the <paramref name="value"/> parameter represents the associated data.</remarks>
		/// <param name="name">The name of the dictionary variable.</param>
		/// <param name="value">The value associated with the dictionary variable.</param>
		public DictionaryVariable(string name, string value) : base(name)
		{
			this.Value = value;
		}
	}
}