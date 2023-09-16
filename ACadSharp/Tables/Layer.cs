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
		/// Default layer 0, it will always exist in a file.
		/// </summary>
		public const string DefaultName = "0";

		/// <summary>
		/// Default layer in all cad formats, it will always exist in a file.
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
		public LineType LineType { get; set; } = LineType.Continuous;

		/// <summary>
		/// Specifies if the layer is plottable.
		/// </summary>
		[DxfCodeValue(290)]
		public bool PlotFlag { get; set; }

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineweightType LineWeight { get; set; } = LineweightType.Default;

		/// <summary>
		/// PlotStyleName object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Unprocess, 390)]
		public ulong PlotStyleName { get; set; } = 0;   //Note: The handle points to an ACDBPLACEHOLDER

		/// <summary>
		/// Hard-pointer ID/handle to Material object
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 347)]
		public Material Material { get; set; }    //TODO: Implement ulong handles, change to internal or private, implement the material class

		public bool IsOn { get; set; }  //TODO: Is the same as PlotFlag???

		internal Layer() : base() { }

		public Layer(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Layer must have a name.");
		}

		public override CadObject Clone()
		{
			Layer clone = new Layer(this.Name);
			clone.LineType = (LineType)this.LineType.Clone();
			clone.Material = (Material)(this.Material?.Clone());
			return clone;
		}
	}
}
