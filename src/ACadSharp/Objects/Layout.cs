using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACadSharp.Objects
{
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
		public double Elevation { get; set; }

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
		/// Layout flags.
		/// </summary>
		[DxfCodeValue(70)]
		public LayoutFlags LayoutFlags { get; set; }

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
		public OrthographicType UcsOrthographicType { get; set; }

		/// <summary>
		/// Viewport that was last active in this layout when the layout was current.
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 331)]
		public Viewport Viewport
		{
			get
			{
				if (this._lastViewport == null)
					return this.Viewports?.FirstOrDefault();
				else
					return this._lastViewport;
			}
			internal set
			{
				this._lastViewport = value;
			}
		}

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

		public const string ModelLayoutName = "Model";

		public const string PaperLayoutName = "Layout1";

		private BlockRecord _blockRecord;

		//333	Shade plot ID
		private Viewport _lastViewport;

		public Layout(string name) : this(name, name)
		{
		}

		public Layout(string name, string blockName) : base()
		{
			this.Name = name;
			this._blockRecord = new BlockRecord(blockName);
		}

		internal Layout() : base()
		{
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
}