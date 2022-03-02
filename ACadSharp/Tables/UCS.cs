using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

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

		//TODO: finish UCS documentation

		public XYZ PaperSpaceInsertionBase { get; set; }
		public XYZ PaperSpaceExtMin { get; set; }
		public XYZ PaperSpaceExtMax { get; set; }
		public XY PaperSpaceLimitsMin { get; set; }
		public XY PaperSpaceLimitsMax { get; set; }

		/// <summary>
		/// Elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// Origin (in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Origin { get; set; }

		/// <summary>
		/// X-axis direction (in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ XAxis { get; set; }

		/// <summary>
		/// Y-axis direction(in WCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ YAxis { get; set; }

		public XYZ OrthographicTopDOrigin { get; set; }
		public XYZ OrthographicBottomDOrigin { get; set; }
		public XYZ OrthographicLeftDOrigin { get; set; }
		public XYZ OrthographicRightDOrigin { get; set; }
		public XYZ OrthographicFrontDOrigin { get; set; }
		public XYZ OrthographicBackDOrigin { get; set; }
		public OrthographicType OrthographicViewType { get; internal set; }
		public OrthographicType OrthographicType { get; internal set; }

		public UCS() : base() { }
	}
}
