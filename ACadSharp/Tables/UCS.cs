using ACadSharp.Attributes;
using CSMath;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="UCS"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableUcs"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Ucs"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableUcs)]
	[DxfSubClass(DxfSubclassMarker.Ucs)]
	public class UCS : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.UCS;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableUcs;

		/// <summary>
		/// Elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// Origin (in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Origin { get; set; } = XYZ.Zero;

		/// <summary>
		/// X-axis direction (in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ XAxis { get; set; } = XYZ.AxisX;

		/// <summary>
		/// Y-axis direction(in WCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ YAxis { get; set; } = XYZ.AxisY;

		/// <summary>
		/// Always 0
		/// </summary>
		[DxfCodeValue(79)]
		public OrthographicType OrthographicViewType { get; set; }

		/// <summary>
		/// Orthographic type
		/// </summary>
		[DxfCodeValue(71)]
		public OrthographicType OrthographicType { get; set; }

		public UCS() : base() { }

		public override object Clone()
		{
			throw new System.NotImplementedException();
		}
	}
}