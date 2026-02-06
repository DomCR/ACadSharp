using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Objects.Evaluations
{
	/// <summary>
	/// Represents a BLOCKROTATIONPARAMETER object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectBlockRotationParameter"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockRotationParameter"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectBlockRotationParameter)]
	[DxfSubClass(DxfSubclassMarker.BlockRotationParameter)]
	public class BlockRotationParameter : Block2PtParameter
	{
		[DxfCodeValue(306)]
		public string Description { get; set; }

		[DxfCodeValue(305)]
		public string Name { get; set; }

		[DxfCodeValue(140)]
		public double NameOffset { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectBlockRotationParameter;

		[DxfCodeValue(1011, 1021, 1031)]
		public XYZ Point { get; set; }

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.BlockRotationParameter;

		[DxfCodeValue(141)]
		public double Value141 { get; set; }

		[DxfCodeValue(142)]
		public double Value142 { get; set; }

		[DxfCodeValue(143)]
		public double Value143 { get; set; }

		[DxfCodeValue(175)]
		public int Value175 { get; set; }

		[DxfCodeValue(307)]
		public string Value307 { get; set; }

		[DxfCodeValue(96)]
		public int Value96 { get; set; }
	}
}