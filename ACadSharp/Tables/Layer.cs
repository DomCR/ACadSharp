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
		/// Default layer 0, it will always exist in a file
		/// </summary>
		public const string DefaultName = "0";

		/// <summary>
		/// Default layer in all cad formats, it will always exist in a file
		/// </summary>
		public static Layer Default { get { return new Layer(DefaultName); } }

		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.LAYER;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableLayer;

		/// <inheritdoc/>
		public override string SubclassMarker => DxfSubclassMarker.Layer;

		/// <summary>
		/// Layer state flags.
		/// </summary>
		public new LayerFlags Flags { get { return (LayerFlags)base.Flags; } set { base.Flags = (StandardFlags)value; } }

		/// <summary>
		/// Color
		/// </summary>
		/// <remarks>
		/// if the index is negative, layer is off
		/// </remarks>
		[DxfCodeValue(62, 420, 430)]
		public Color Color { get; set; }

		/// <summary>
		/// The linetype of an object. The default linetype is the linetype of the layer (ByLayer).
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
					this._lineType = this.updateTable(value, this.Document.LineTypes);
				}
				else
				{
					this._lineType = value;
				}
			}
		}

		/// <summary>
		/// Specifies if the layer is plottable.
		/// </summary>
		[DxfCodeValue(290)]
		public bool PlotFlag { get; set; } = true;

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineweightType LineWeight { get; set; } = LineweightType.Default;

		/// <summary>
		/// PlotStyleName object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Unprocess, 390)]
		public ulong PlotStyleName { get; internal set; } = 0;   //Note: The handle points to an ACDBPLACEHOLDER

		/// <summary>
		/// Hard-pointer ID/handle to Material object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }    //TODO: Implement ulong handles, change to internal or private, implement the material class

		/// <summary>
		/// Indicates if the Layer is visible in the model
		/// </summary>
		public bool IsOn { get; set; } = true;

		private LineType _lineType = LineType.Continuous;

		internal Layer() : base() { }

		public Layer(string name) : base(name) { }

		/// <inheritdoc/>
		public override CadObject Clone()
		{
			Layer clone = new Layer(this.Name);
			clone.LineType = (LineType)this.LineType.Clone();
			clone.Material = (Material)(this.Material?.Clone());
			return clone;
		}

		internal override void AssignDocument(CadDocument doc)
		{
			base.AssignDocument(doc);

			this._lineType = this.updateTable(this.LineType, doc.LineTypes);

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
