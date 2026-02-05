using ACadSharp.Attributes;

namespace ACadSharp.Entities
{
	/// <summary>
	/// Represents a <see cref="Wipeout"/> entity.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.EntityWipeout"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Wipeout"/>
	/// </remarks>
	[DxfName(DxfFileToken.EntityWipeout)]
	[DxfSubClass(DxfSubclassMarker.Wipeout)]
	public class Wipeout : CadWipeoutBase
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UNLISTED;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.EntityWipeout;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Wipeout;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Wipeout()
		{
			this.Flags = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowNotAlignedImage | ImageDisplayFlags.UseClippingBoundary;
		}
	}
}
