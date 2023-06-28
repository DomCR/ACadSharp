using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Scale"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectScale"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Scale"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectScale)]
	[DxfSubClass(DxfSubclassMarker.Scale)]
	public class Scale : CadObject
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType { get { return ObjectType.UNLISTED; } }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectScale;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Scale;

		public short Unknown { get; set; }
		public string Name { get; set; }
		public double PaperUnits { get; set; }
		public double DrawingUnits { get; set; }
		public bool IsUnitScale { get; set; }
	}
}
