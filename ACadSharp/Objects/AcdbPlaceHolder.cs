using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="AcdbPlaceHolder"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectDictionary"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Dictionary"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectDictionary)]
	[DxfSubClass(DxfSubclassMarker.Dictionary)]
	public class AcdbPlaceHolder : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.ACDBPLACEHOLDER; } }
	}
}
