using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using CSMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Tables
{
	/// <summary>
	/// Represents a <see cref="View"/> entry
	/// </summary>
	/// <remarks>
	/// Object name <see cref="DxfFileToken.TableView"/> <br/>
	/// Dxf class name <see cref="DxfSubclassMarker.BlockRecord"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableView)]
	[DxfSubClass(DxfSubclassMarker.View)]
	public class View : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableView;

		//TODO: finish View documentation

		/// <summary>
		/// View height (in DCS)
		/// </summary>
		[DxfCodeValue(40)]
		public double Height { get; set; }

		/// <summary>
		/// View width (in DCS)
		/// </summary>
		[DxfCodeValue(41)]
		public double Width { get; set; }

		/// <summary>
		/// View width (in DCS)
		/// </summary>
		[DxfCodeValue(42)]
		public double LensLength { get; set; }

		/// <summary>
		/// Front clipping plane (offset from target point)
		/// </summary>
		[DxfCodeValue(43)]
		public double FrontClipping { get; set; }

		/// <summary>
		/// Back clipping plane (offset from target point)
		/// </summary>
		[DxfCodeValue(44)]
		public double BackClipping { get; set; }

		/// <summary>
		/// Twist angle
		/// </summary>
		[DxfCodeValue(50)]
		public double Angle { get; set; }

		/// <summary>
		/// View mode (see VIEWMODE system variable)
		/// </summary>
		[DxfCodeValue(71)]
		public byte ViewMode { get; set; }

		/// <summary>
		/// 1 if there is a UCS associated to this view; 0 otherwise
		/// </summary>
		[DxfCodeValue(72)]
		public bool IsUcsAssociated { get; set; }

		/// <summary>
		/// 1 if the camera is plottable
		/// </summary>
		[DxfCodeValue(73)]
		public bool IsPlottable { get; set; }

		/// <summary>
		/// View mode (see VIEWMODE system variable)
		/// </summary>
		[DxfCodeValue(281)]
		public RenderMode RenderMode { get; set; }

		/// <summary>
		/// View center point (in DCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; }

		/// <summary>
		/// View direction from target (in WCS)
		/// </summary>
		[DxfCodeValue(11, 21, 31)]
		public XYZ Direction { get; set; }

		/// <summary>
		/// Target point (in WCS)
		/// </summary>
		[DxfCodeValue(12, 22, 32)]
		public XYZ Target { get; set; }

		//332	Soft-pointer ID/handle to background object (optional)

		//334	Soft-pointer ID/handle to live section object (optional)

		//348	Hard-pointer ID/handle to visual style object (optional)

		//361	Sun hard ownership ID

		public View() : base() { }

		public View(string name) : base(name) { }
	}
}
