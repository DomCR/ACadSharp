using ACadSharp.Attributes;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="RasterVariables"/> object.
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectRasterVariables"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.RasterVariables"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectRasterVariables)]
	[DxfSubClass(DxfSubclassMarker.RasterVariables)]
	public class RasterVariables : NonGraphicalObject
	{
		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectRasterVariables;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.RasterVariables;

		public short Units { get; internal set; }
		public short DisplayQuality { get; internal set; }
		public short DisplayFrame { get; internal set; }
		public int ClassVersion { get; internal set; }

		/// <inheritdoc/>
		public RasterVariables() : base()
		{
		}
	}
}
