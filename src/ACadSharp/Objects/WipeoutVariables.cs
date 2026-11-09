using ACadSharp.Attributes;
using ACadSharp.Classes;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="WipeoutVariables"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectWipeoutVariables"/> <br/>
/// Dxf class name <see cref="DxfSubclassMarker.WipeoutVariables"/>
/// </remarks>
[DxfName(DxfFileToken.ObjectWipeoutVariables)]
[DxfSubClass(DxfSubclassMarker.WipeoutVariables)]
public class WipeoutVariables : NonGraphicalObject, IDxfClassDefined
{
	/// <summary>
	/// Gets or sets a value indicating whether the image frame is displayed.
	/// </summary>
	[DxfCodeValue(70)]
	public bool DisplayImageFrame { get; set; }

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectWipeoutVariables;

	/// <inheritdoc/>
	public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.WipeoutVariables;

	/// <inheritdoc/>
	public DxfClass GetDxfClass()
	{
		return new DxfClass
		{
			ApplicationName = "\"WipeOut|Product Desc:     WipeOut Dbx Application|Company:          Autodesk, Inc.|WEB Address:      www.autodesk.com\"",
			CppClassName = DxfSubclassMarker.WipeoutVariables,
			DwgVersion = ACadVersion.AC1018,
			DxfName = DxfFileToken.ObjectWipeoutVariables,
			ItemClassId = 499,
			MaintenanceVersion = 0,
			ProxyFlags = ProxyFlags.None,
			WasZombie = false,
		};
	}
}