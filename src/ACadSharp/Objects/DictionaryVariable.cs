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
		/// The name of the system variable that represents the current annotation
		/// scale for the current space.
		/// </summary>
		/// <remarks>
		/// string, initial value: 1:1
		/// </remarks>
		public const string CurrentAnnotationStyle = "CANNOSCALE";

		/// <summary>
		/// The name of the system variable that represents of the current multi-leader style.
		/// </summary>
		/// <remarks>This constant is used to identify the current multi-leader style in the application. The value is
		/// a string literal: "CMLEADERSTYLE".</remarks>
		public const string CurrentMultiLeaderStyle = "CMLEADERSTYLE";

		/// <summary>
		/// The name of system variable that represents the current table style.
		/// </summary>
		///	<remarks>
		///	string, initial value: Standard
		///	</remarks>
		public const string CurrentTableStyle = "CTABLESTYLE";

		/// <summary>
		/// The name of the system variable that represents of the current detail view style.
		/// The current detail view style controls the appearance of all
		/// new model documentation detail views, detail boundaries and
		/// leader lines you create.
		/// </summary>
		/// <remarks>
		/// string, intial value: Imperial24 (imperial) or Metric50 (metric)
		/// </remarks>
		public const string CurrentViewDetailStyle = "CVIEWDETAILSTYLE";

		/// <summary>
		/// The name of the system variable that represents the name of
		/// the current section view style.
		/// The current section view style controls the appearance of all
		/// new model documentation section views and section lines you create.
		/// </summary>
		public const string CurrentViewSectionStyle = "CVIEWSECTIONSTYLE";

		/// <summary>
		/// The name of the system variable that controls the display
		/// of frames for wipeout objects.
		/// </summary>
		/// <remarks>Integer, initial value: 1, allowed values:
		/// <list>
		/// <item>
		/// 0 Frames are not displayed or plotted<br/>
		///   Frames are temporarily displayed for object selection and selection preview.
		/// </item><item>
		/// 1 Frames are displayed and plotted
		/// </item><item>
		/// 2 Frames are displayed, but not plotted
		/// </item>
		/// </list>
		/// </remarks>
		public const string WipeoutFrame = "WIPEOUTFRAME";

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