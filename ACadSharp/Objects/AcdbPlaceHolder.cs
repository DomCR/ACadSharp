using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="AcdbPlaceHolder"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectPlaceholder"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.AcDbPlaceHolder"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectPlaceholder)]
	[DxfSubClass(DxfSubclassMarker.AcDbPlaceHolder)]
	public class AcdbPlaceHolder : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.ACDBPLACEHOLDER; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectPlaceholder;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.AcDbPlaceHolder;
	}
}
