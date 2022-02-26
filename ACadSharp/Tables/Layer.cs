using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

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

		/// <summary>
		/// Layer state flags.
		/// </summary>
		public new LayerFlags Flags { get; set; }

		/// <summary>
		/// Specifies the Color of an object.
		/// </summary>
		[DxfCodeValue(62)]
		public Color Color { get; set; }

		/// <summary>
		/// The linetype of an object. The default linetype is the linetype of the layer (ByLayer).
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Name, 6)]
		public LineType LineType { get; set; }  //TODO: implement default linetype

		/// <summary>
		/// Specifies if the layer is plottable.
		/// </summary>
		[DxfCodeValue(290)]
		public bool PlotFlag { get; set; }

		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(370)]
		public LineweightType LineWeight { get; set; }

		/// <summary>
		/// Hard-pointer ID/handle of PlotStyleName object
		/// </summary>
		[DxfCodeValue(380)]
		public string PlotStyleName { get; set; }   //TODO: set the plot style for the layer

		/// <summary>
		/// Hard-pointer ID/handle to Material object
		/// </summary>
		[DxfCodeValue(347)]
		public string Material { get; set; }    //TODO: Implement ulong handles, change to internal or private, implement the material class

		public bool IsOn { get; set; }  //TODO: Is the same as PlotFlag???

		internal Layer() : base() { }

		public Layer(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Layer must have a name.");
		}
	}
}
