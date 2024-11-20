using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Group"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableGroup"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Group"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableGroup)]
	[DxfSubClass(DxfSubclassMarker.Group)]
	public class BookColor : NonGraphicalObject
	{   
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDBColor;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DbColor;

		public string BookName { get; internal set; }
		public Color Color { get; internal set; }
	}
}
