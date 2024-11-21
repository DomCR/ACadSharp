using ACadSharp.Attributes;
using System.Linq;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="BookColor"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDBColor"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.DbColor"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDBColor)]
	[DxfSubClass(DxfSubclassMarker.DbColor)]
	public class BookColor : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectDBColor;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.DbColor;

		public string ColorName { get { return this.Name.Split('$').Last(); } }

		public string BookName { get { return this.Name.Split('$').First(); } }
		
		[DxfCodeValue(62, 420)]
		public Color Color { get; set; }
	}
}
