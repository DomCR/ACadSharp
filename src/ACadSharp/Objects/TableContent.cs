using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="TableContent"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectTableContent"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.TableContent"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectTableContent)]
	[DxfSubClass(DxfSubclassMarker.TableContent)]
	public class TableContent : FormattedTableData
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectTableContent;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.TableContent;

		[DxfCodeValue(DxfReferenceType.Handle, 340)]
		public TableStyle Style { get; set; } = new();

		public TableStyle StyleOverride { get; set; }
	}
}
