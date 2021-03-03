using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
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

		public override string ObjectName => DxfFileToken.TableLayer;
		public override bool XrefDependant
		{
			get
			{
				return Flags.HasFlag(LayerFlags.XrefDependent);
			}
			set
			{
				if (value)
					Flags |= LayerFlags.XrefDependent;
				else
					Flags &= ~LayerFlags.XrefDependent;
			}
		}

		/// <summary>
		/// Layer state flags.
		/// </summary>
		[DxfCodeValue(DxfCode.Int16)]
		public LayerFlags Flags { get; set; }
		/// <summary>
		/// Specifies the Color of an object.
		/// </summary>
		[DxfCodeValue(DxfCode.Color)]
		public Color Color { get; set; }
		/// <summary>
		/// The linetype of an object. The default linetype is the linetype of the layer (ByLayer).
		/// </summary>
		[DxfCodeValue(DxfCode.LinetypeName)]
		public LineType LineType { get; set; }  //TODO: implement default linetype
		/// <summary>
		/// Specifies if the layer is plottable.
		/// </summary>
		[DxfCodeValue(DxfCode.Bool)]
		public bool PlotFlag { get; set; }
		/// <summary>
		/// Specifies the lineweight of an individual object or the default lineweight for the drawing.
		/// </summary>
		[DxfCodeValue(DxfCode.LineWeight)]
		public Lineweight LineWeight { get; set; }
		/// <summary>
		/// Hard-pointer ID/handle of PlotStyleName object
		/// </summary>
		[DxfCodeValue(DxfCode.PlotStyleNameType)]
		public string PlotStyleName { get; set; }
		/// <summary>
		/// Hard-pointer ID/handle to Material object
		/// </summary>
		[DxfCodeValue(DxfCode.MaterialHandleId)]
		public string Material { get; set; }    //TODO: Implement ulong handles, change to internal or private, implement the material class

		public bool IsOn { get; set; }

		public Layer(string name) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name), "Layer must have a name.");
		}

		internal Layer(DxfEntryTemplate template) : base(template)
		{
			if (string.IsNullOrEmpty(template.Name))
				throw new ArgumentNullException(nameof(template.Name), "Layer must have a name.");
		}
	}
}
