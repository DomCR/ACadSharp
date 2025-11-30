using ACadSharp.Attributes;
using ACadSharp.Objects;
using System;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="Layer"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableLayer"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.Layer"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableLayer)]
	[DxfSubClass(DxfSubclassMarker.Layer)]
	public class Layer : TableEntry
	{
		/// <summary>
		/// Default layer in all cad formats, it will always exist in a file
		/// </summary>
		public static Layer Default { get { return new Layer(DefaultName); } }

		/// <summary>
		/// Defpoints layer, this layer usually stores definition points that will not be plot.
		/// </summary>
		public static Layer Defpoints { get { return new Layer(DefpointsName) { PlotFlag = false }; } }

		/// <summary>
		/// Color
		/// </summary>
		/// <remarks>
		/// if the index is negative, layer is off
		/// </remarks>
		[DxfCodeValue(62, 420, 430)]
		public Color Color
		{
			get { return this._color; }
			set
			{
				if (value.IsByLayer || value.IsByBlock)
				{
					throw new ArgumentException("The layer color cannot be ByLayer or ByBlock", nameof(value));
				}

				this._color = value;
			}
		}

		/// <summary>
		/// Layer state flags.
		/// </summary>
		public new LayerFlags Flags { get { return (LayerFlags)base.Flags; } set { base.Flags = (StandardFlags)value; } }

		/// <summary>
		/// Indicates if the Layer is visible in the model
		/// </summary>
		public bool IsOn { get; set; } = true;

		/// <summary>
		/// The line type of an object. The default line type is the line type of the layer (ByLayer).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 6)]
		public LineType LineType
		{
			get { return this._lineType; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}

				if (this.Document != null)
				{
					this._lineType = CadObject.updateCollection(value, this.Document.LineTypes);
				}
				else
				{
					this._lineType = value;
				}
			}
		}

		/// <summary>
		/// Specifies the line weight of an individual object or the default line weight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineWeightType LineWeight { get; set; } = LineWeightType.Default;

		/// <summary>
		/// Hard-pointer ID/handle to Material object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLayer;

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LAYER;

		/// <summary>
		/// Specifies if the layer is plottable.
		/// </summary>
		[DxfCodeValue(290)]
		public bool PlotFlag
		{
			get
			{
				if (this.name.Equals(DefpointsName, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				return this._plotFlag;
			}
			set
			{
				this._plotFlag = value;
			}
		}

		/// <summary>
		/// PlotStyleName object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Unprocess, 390)]
		public ulong PlotStyleName { get; internal set; } = 0;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Layer;

		/// <summary>
		/// Default layer 0, it will always exist in a file
		/// </summary>
		public const string DefaultName = "0";

		/// <summary>
		/// DefPoints layer name.
		/// </summary>
		public const string DefpointsName = "defpoints";

		private Color _color = new Color(7);

		//TODO: Implement ulong handles, change to internal or private, implement the material class
		private LineType _lineType = LineType.Continuous;

		private bool _plotFlag = true;

		public Layer(string name) : base(name)
		{
		}

		//Note: The handle points to an ACDBPLACEHOLDER
		internal Layer() : base() { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Layer clone = (Layer)base.Clone();

			clone.LineType = (LineType)this.LineType.Clone();
			clone.Material = (Material)(this.Material?.Clone());

			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._lineType = CadObject.updateCollection(this.LineType, doc.LineTypes);

			doc.LineTypes.OnRemove += this.tableOnRemove;
		}

		internal override void UnassignDocument()
		{
			this.Document.LineTypes.OnRemove -= this.tableOnRemove;

			base.UnassignDocument();

			this.LineType = (LineType)this.LineType.Clone();
		}

		protected virtual void tableOnRemove(object sender, CollectionChangedEventArgs e)
		{
			if (e.Item.Equals(this.LineType))
			{
				this.LineType = this.Document.LineTypes[LineType.ContinuousName];
			}
		}
	}
}