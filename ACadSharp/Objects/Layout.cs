using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Objects
{
	public class Layout : TableEntry
	{
		public override ObjectType ObjectType => ObjectType.LAYOUT;
		public override string ObjectName => DxfFileToken.TableLayout;

		//100	//Subclass marker(AcDbPlotSettings)	//plotsettings object group codes
		//For group codes and descriptions following the AcDbPlotSettings marker, see PLOTSETTINGS

		//100	Subclass marker(AcDbLayout)

		[DxfCodeValue(1)]
		public override string Name { get; set; }

		/// <summary>
		/// Layout flags.
		/// </summary>
		[DxfCodeValue(70)]
		public LayoutFlags Flags { get; set; }

		/// <summary>
		/// Tab order.This number is an ordinal indicating this layout's ordering in the tab control that is attached to the drawing window. Note that the “Model” tab always appears as the first tab regardless of its tab order
		/// </summary>
		[DxfCodeValue(71)]
		public int TabOrder { get; set; }

		/// <summary>
		/// Minimum limits for this layout (defined by LIMMIN while this layout is current)
		/// </summary>
		[DxfCodeValue(10, 20)]
		public XY MinLimits { get; set; }

		/// <summary>
		/// Maximum limits for this layout(defined by LIMMAX while this layout is current)
		/// </summary>
		[DxfCodeValue(11, 21)]
		public XY MaxLimits { get; set; }

		/// <summary>
		/// Insertion base point for this layout(defined by INSBASE while this layout is current) 
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ InsertionBasePoint { get; set; }

		/// <summary>
		/// Minimum extents for this layout(defined by EXTMIN while this layout is current)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ MinExtents { get; set; }

		/// <summary>
		/// Maximum extents for this layout(defined by EXTMAX while this layout is current)
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ MaxExtents { get; set; }

		/// <summary>
		/// Layout elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// UCS origin
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ Origin { get; set; }

		/// <summary>
		/// UCS X-axis
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ XAxis { get; set; }

		/// <summary>
		/// UCS Y-axis
		/// </summary>
		[DxfCodeValue(17, 27, 37)]
		public XYZ YAxis { get; set; }

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(76)]
		public OrthographicType UcsOrthographicType { get; set; }

		/// <summary>
		/// The associated paper space block table record
		/// </summary>
		[DxfCodeValue(330)]
		public Block AssociatedBlock { get; set; }

		/// <summary>
		/// Viewport that was last active in this layout when the layout was current
		/// </summary>
		[DxfCodeValue(331)]
		public Viewport Viewport { get; set; }

		/// <summary>
		/// Layout's UCS
		/// </summary>
		[DxfCodeValue(345)]
		public UCS UCS { get; set; }

		//346	ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic(76 code is non-zero).
		//If not present and 76 code is non-zero, then base UCS is taken to be WORLD

		//333	Shade plot ID

		/// <summary>
		/// Plot settings for this layout.
		/// </summary>
		public PlotSettings PlotSettings { get; set; }

		public List<Viewport> Viewports { get; } = new List<Viewport>();

	}
}
