using ACadSharp.Attributes;
using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Objects
{
	/// <summary>
	/// Represents a <see cref="Layout"/> object
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.ObjectLayout"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Layout"/>
	/// </remarks>
	[DxfName(DxfFileToken.ObjectLayout)]
	[DxfSubClass(DxfSubclassMarker.Layout)]
	public class Layout : PlotSettings
	{
		public const string LayoutModelName = "Model";

		public static Layout Default { get { return new Layout(LayoutModelName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LAYOUT;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.ObjectLayout;

		/// <summary>
		/// Layout name
		/// </summary>
		[DxfCodeValue(1)]
		public string Name { get; set; }

		/// <summary>
		/// Layout flags
		/// </summary>
		[DxfCodeValue(70)]
		public LayoutFlags LayoutFlags { get; set; }

		/// <summary>
		/// Tab order.This number is an ordinal indicating this layout's ordering in the tab control that is attached to the drawing window. Note that the “Model” tab always appears as the first tab regardless of its tab order
		/// </summary>
		[DxfCodeValue(71)]
		public int TabOrder { get; set; }

		/// <summary>
		/// Minimum limits for this layout (defined by LIMMIN while this layout is current)
		/// </summary>
		[DxfCodeValue(10, 20)]
		public XY MinLimits { get; set; } = new XY(-20.0, -7.5);

		/// <summary>
		/// Maximum limits for this layout(defined by LIMMAX while this layout is current)
		/// </summary>
		[DxfCodeValue(11, 21)]
		public XY MaxLimits { get; set; } = new XY(277.0, 202.5);

		/// <summary>
		/// Insertion base point for this layout(defined by INSBASE while this layout is current) 
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ InsertionBasePoint { get; set; }

		/// <summary>
		/// Minimum extents for this layout(defined by EXTMIN while this layout is current)
		/// </summary>
		[DxfCodeValue(14, 24, 34)]
		public XYZ MinExtents { get; set; } = new XYZ(25.7, 19.5, 0.0);

		/// <summary>
		/// Maximum extents for this layout(defined by EXTMAX while this layout is current)
		/// </summary>
		[DxfCodeValue(15, 25, 35)]
		public XYZ MaxExtents { get; set; } = new XYZ(231.3, 175.5, 0.0);

		/// <summary>
		/// Layout elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// UCS origin
		/// </summary>
		[DxfCodeValue(13, 23, 33)]
		public XYZ Origin { get; set; } = XYZ.Zero;

		/// <summary>
		/// UCS X-axis
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ XAxis { get; set; } = XYZ.AxisX;

		/// <summary>
		/// UCS Y-axis
		/// </summary>
		[DxfCodeValue(17, 27, 37)]
		public XYZ YAxis { get; set; } = XYZ.AxisY;

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(76)]
		public OrthographicType UcsOrthographicType { get; set; }

		/// <summary>
		/// The associated paper space block table record
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 330)]
		public BlockRecord AssociatedBlock
		{
			get { return this._blockRecord; }
			internal set
			{
				this._blockRecord = value;
				if (this._blockRecord == null)
					return;

				if (this._blockRecord.Name.Equals(BlockRecord.ModelSpaceName, System.StringComparison.OrdinalIgnoreCase))
				{
					this.Viewport = null;
					base.PlotFlags =
						PlotFlags.Initializing |
						PlotFlags.UpdatePaper |
						PlotFlags.ModelType |
						PlotFlags.DrawViewportsFirst |
						PlotFlags.PrintLineweights |
						PlotFlags.PlotPlotStyles |
						PlotFlags.UseStandardScale;
				}
				else
				{
					this.Viewport = new Viewport();
					this.Viewport.ViewCenter = new XY(50.0, 100.0);
					this.Viewport.Status =
						ViewportStatusFlags.AdaptiveGridDisplay |
						ViewportStatusFlags.DisplayGridBeyondDrawingLimits |
						ViewportStatusFlags.CurrentlyAlwaysEnabled |
						ViewportStatusFlags.UcsIconVisibility;
				}
			}
		}

		/// <summary>
		/// Viewport that was last active in this layout when the layout was current
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 331)]
		public Viewport Viewport { get; internal set; }	//TODO: The owner of the viewports is the blockrecord

		/// <summary>
		/// Layout's UCS
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 345)]
		public UCS UCS { get; set; }

		//346	ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic(76 code is non-zero).
		//If not present and 76 code is non-zero, then base UCS is taken to be WORLD

		//333	Shade plot ID

		public List<Viewport> Viewports { get; } = new List<Viewport>();

		private BlockRecord _blockRecord;

		public Layout() : this(null) { }

		public Layout(string name) : base()
		{
			this.Name = name;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.ObjectName}:{this.Name}";
		}
	}
}
