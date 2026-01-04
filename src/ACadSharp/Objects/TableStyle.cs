using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="TableStyle"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableStyle"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableStyle"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableStyle)]
	[DxfSubClass(DxfSubclassMarker.TableStyle)]
	public class TableStyle : NonGraphicalObject
	{
		public const string DefaultName = "Standard";

		public override string ObjectName => DxfFileToken.TableStyle;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string SubclassMarker { get; } = DxfSubclassMarker.TableStyle;

		/// <summary>
		/// Gets or sets a value indicating whether the title should be suppressed.
		/// </summary>
		[DxfCodeValue(280)]
		public bool SuppressTitle { get; set; }
	}
}