using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects;

/// <summary>
/// Represents a <see cref="Layout"/> object.
/// </summary>
/// <remarks>
/// Object name <see cref="DxfFileToken.ObjectLayout"/>. <br/>
/// Dxf class name <see cref="DxfSubclassMarker.Layout"/>.
/// </remarks>
[DxfName(DxfFileToken.ObjectLayout)]
[DxfSubClass(DxfSubclassMarker.Layout)]
public class Layout : PlotSettings
{
	/// <summary>
	/// The associated paper space block table record.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 330)]
	public BlockRecord AssociatedBlock
	{
		get { return this._blockRecord; }
		internal set
		{
			if (value == null)
			{
				throw new System.ArgumentNullException(nameof(value));
			}

			this._blockRecord = value;
			this._blockRecord.Layout = this;
		}
	}

	/// <summary>
	/// UCSTableRecord of base UCS if UCS is orthographic (<see cref="UcsOrthographicType"/> is non-zero).
	/// </summary>
	/// <remarks>
	/// If not present and <see cref="UcsOrthographicType"/> is non-zero, then base UCS is taken to be WORLD.
	/// </remarks>
	[DxfCodeValue(DxfReferenceType.Handle, 346)]
	public UCS BaseUCS { get; set; }

	/// <summary>
	/// Layout elevation.
	/// </summary>
	[DxfCodeValue(146)]
	public double Elevation { get; set; } = 0.0d;

	/// <summary>
	/// Insertion base point for this layout(defined by INSBASE while this layout is current).
	/// </summary>
	[DxfCodeValue(12, 22, 32)]
	public XYZ InsertionBasePoint { get; set; }

	/// <summary>
	/// If true, the layout is a paper space.
	/// </summary>
	public bool IsPaperSpace
	{
		get
		{
			return !this.Name.Equals(ModelLayoutName, StringComparison.InvariantCultureIgnoreCase);
		}
	}

	/// <summary>
	/// Viewport that was last active in this layout when the layout was current.
	/// </summary>
	[DxfCodeValue(DxfReferenceType.Handle, 331)]
	public Viewport LastActiveViewport
	{
		get
		{
			return this._lastViewport;
		}
		internal set
		{
			this._lastViewport = value;
		}
	}

	/// <summary>
	/// Layout flags.
	/// </summary>
	[DxfCodeValue(70)]
	public LayoutFlags LayoutFlags { get; set; } = LayoutFlags.PaperSpaceLinetypeScaling;

	/// <summary>
	/// Maximum extents for this layout(defined by EXTMAX while this layout is current).
	/// </summary>
	[DxfCodeValue(15, 25, 35)]
	public XYZ MaxExtents { get; set; } = new XYZ(231.3, 175.5, 0.0);

	/// <summary>
	/// Maximum limits for this layout(defined by LIMMAX while this layout is current).
	/// </summary>
	[DxfCodeValue(11, 21)]
	public XY MaxLimits { get; set; } = new XY(277.0, 202.5);

	/// <summary>
	/// Minimum extents for this layout(defined by EXTMIN while this layout is current).
	/// </summary>
	[DxfCodeValue(14, 24, 34)]
	public XYZ MinExtents { get; set; } = new XYZ(25.7, 19.5, 0.0);

	/// <summary>
	/// Minimum limits for this layout (defined by LIMMIN while this layout is current).
	/// </summary>
	[DxfCodeValue(10, 20)]
	public XY MinLimits { get; set; } = new XY(-20.0, -7.5);

	/// <summary>
	/// Layout name.
	/// </summary>
	[DxfCodeValue(1)]
	public override string Name
	{
		get
		{
			return base.Name;
		}
		set
		{
			base.Name = value;
		}
	}

	/// <inheritdoc/>
	public override string ObjectName => DxfFileToken.ObjectLayout;

	/// <inheritdoc/>
	public override ObjectType ObjectType => ObjectType.LAYOUT;

	/// <summary>
	/// UCS origin.
	/// </summary>
	[DxfCodeValue(13, 23, 33)]
	public XYZ Origin { get; set; } = XYZ.Zero;

	/// <inheritdoc/>
	public override string SubclassMarker => DxfSubclassMarker.Layout;

	/// <summary>
	/// Tab order. This number is an ordinal indicating this layout's ordering in the tab control that is attached to the drawing window. Note that the “Model” tab always appears as the first tab regardless of its tab order.
	/// </summary>
	[DxfCodeValue(71)]
	public int TabOrder { get; set; }

	/// <summary>
	/// UCS Table Record if UCS is a named UCS.
	/// </summary>
	/// <remarks>
	/// If not present, then UCS is unnamed.
	/// </remarks>
	[DxfCodeValue(DxfReferenceType.Handle, 345)]
	public UCS UCS { get; set; }

	/// <summary>
	/// Orthographic type of UCS.
	/// </summary>
	[DxfCodeValue(76)]
	public OrthographicType UcsOrthographicType { get; set; } = OrthographicType.None;

	/// <summary>
	/// Gets the collection of viewports associated with this instance.
	/// </summary>
	public IEnumerable<Viewport> Viewports
	{
		get
		{
			return this.AssociatedBlock?.Viewports;
		}
	}

	/// <summary>
	/// UCS X-axis.
	/// </summary>
	[DxfCodeValue(16, 26, 36)]
	public XYZ XAxis { get; set; } = XYZ.AxisX;

	/// <summary>
	/// UCS Y-axis.
	/// </summary>
	[DxfCodeValue(17, 27, 37)]
	public XYZ YAxis { get; set; } = XYZ.AxisY;

	/// <summary>
	/// Represents the name of the model layout used in view rendering.
	/// </summary>
	public const string ModelLayoutName = "Model";

	/// <summary>
	/// Specifies the default name for the paper layout used in document generation.
	/// </summary>
	public const string PaperLayoutName = "Layout1";

	private BlockRecord _blockRecord;

	//333	Shade plot ID
	private Viewport _lastViewport;

	/// <summary>
	/// Initializes a new instance of the Layout class using the specified name for both the layout's name and display
	/// name.
	/// </summary>
	/// <param name="name">The name to assign to the layout. This value is used for both the internal name and the display name.</param>
	public Layout(string name) : this(name, name)
	{
	}

	/// <summary>
	/// Initializes a new instance of the Layout class with the specified layout name and associated block record name.
	/// </summary>
	/// <param name="name">The name to assign to the layout. Cannot be null or empty.</param>
	/// <param name="blockName">The name of the block record associated with the layout. Cannot be null or empty.</param>
	public Layout(string name, string blockName) : base()
	{
		this.Name = name;
		this._blockRecord = new BlockRecord(blockName);

		if (this.IsPaperSpace)
		{
			this.UpdatePaperViewport();
		}
	}

	internal Layout() : base()
	{
	}

	/// <summary>
	/// Adds the specified viewport to the collection of entities associated with this block.
	/// </summary>
	/// <param name="viewport">The viewport to add to the associated block's entities. Cannot be null.</param>
	public void AddViewport(Viewport viewport)
	{
		this.AssociatedBlock?.Entities.Add(viewport);
	}

	/// <inheritdoc/>
	public override CadObject Clone()
	{
		Layout clone = (Layout)base.Clone();

		clone._blockRecord = (BlockRecord)this._blockRecord?.Clone();

		return clone;
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return $"{this.ObjectName}:{this.Name}";
	}

	/// <summary>
	/// Ensures that the paper viewport exists and updates its dimensions to match the current paper size.
	/// </summary>
	/// <remarks>If no viewport is associated with the paper space, a new viewport is created and added. This method
	/// has no effect if the object is not in paper space or if there is no associated block.</remarks>
	public void UpdatePaperViewport()
	{
		if (!this.IsPaperSpace || this.AssociatedBlock == null)
		{
			return;
		}

		Viewport vp = this.Viewports.FirstOrDefault();
		if (vp == null)
		{
			vp = new Viewport();
			this.AddViewport(vp);
		}

		vp.Height = this.PaperHeight;
		vp.Width = this.PaperWidth;
	}

	internal override void AssignDocument(CadDocument doc)
	{
		base.AssignDocument(doc);

		if (this.AssociatedBlock != null)
		{
			doc.BlockRecords.Add(this.AssociatedBlock);
			doc.BlockRecords.OnRemove += this.onRemoveBlockRecord;
		}
	}

	internal override void UnassignDocument()
	{
		this.Document.BlockRecords.OnRemove -= this.onRemoveBlockRecord;

		if (this.AssociatedBlock != null)
		{
			this.AssociatedBlock.Layout = null;
			this.Document.BlockRecords.OnRemove -= this.onRemoveBlockRecord;
			this._blockRecord = (BlockRecord)this._blockRecord?.Clone();
		}

		base.UnassignDocument();
	}

	private void onRemoveBlockRecord(object sender, CollectionChangedEventArgs e)
	{
		if (this.AssociatedBlock.Equals(e.Item))
		{
			this.Document.Layouts.Remove(this.Name);
		}
	}
}