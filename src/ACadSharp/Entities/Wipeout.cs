using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Entities;

/// <summary>
/// Represents a <see cref="Wipeout"/> entity.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.EntityWipeout"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Wipeout"/>
/// </remarks>
[DxfName(DxfFileToken.EntityWipeout)]
[DxfSubClass(DxfSubclassMarker.Wipeout)]
public class Wipeout : CadWipeoutBase, IDxfClassDefined
{
	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.EntityWipeout;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.UNLISTED;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Wipeout;

	/// <summary>
	/// Default constructor.
	/// </summary>
	public Wipeout()
	{
		this.Flags = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowNotAlignedImage | ImageDisplayFlags.UseClippingBoundary;
	}

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			ApplicationName = "WipeOut",
			CppClassName = DxfSubclassMarker.Wipeout,
			DwgVersion = ACadVersion.AC1015,
			DxfName = DxfFileToken.EntityWipeout,
			ItemClassId = 498,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.EraseAllowed | ProxyFlags.TransformAllowed | ProxyFlags.ColorChangeAllowed | ProxyFlags.LayerChangeAllowed | ProxyFlags.LinetypeChangeAllowed | ProxyFlags.LinetypeScaleChangeAllowed | ProxyFlags.VisibilityChangeAllowed | ProxyFlags.R13FormatProxy,
			WasZombie = false,
		};
	}
}