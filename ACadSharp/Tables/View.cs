using ACadSharp.Attributes;
using ACadSharp.IO.Templates;
using ACadSharp.Objects;
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
	/// Dxf class name <see cref="DxfSubclassMarker.View"/>
	/// </remarks>
	[DxfName(DxfFileToken.TableView)]
	[DxfSubClass(DxfSubclassMarker.View)]
	public class View : TableEntry
	{
		/// <inheritdoc/>
		public override ObjectType ObjectType => ObjectType.VIEW;

		/// <inheritdoc/>
		public override string ObjectName => DxfFileToken.TableView;

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
		public ViewModeType ViewMode { get; set; }

		/// <summary>
		/// 1 if there is a UCS associated to this view; 0 otherwise
		/// </summary>
		[DxfCodeValue(72)]
		public bool IsUcsAssociated { get; set; } = false;

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
		[DxfCodeValue(10, 20)]
		public XY Center { get; set; }

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

		/// <summary>
		/// Visual style object (optional)
		/// </summary>
		[DxfCodeValue(DxfReferenceType.Handle, 348)]
		public VisualStyle VisualStyle { get; set; }

		/// <summary>
		/// UCS origin
		/// </summary>
		[DxfCodeValue(110, 120, 130)]
		public XYZ UcsOrigin { get; set; }

		/// <summary>
		/// UCS X-axis
		/// </summary>
		[DxfCodeValue(111, 121, 131)]
		public XYZ UcsXAxis { get; set; }

		/// <summary>
		/// UCS Y-axis
		/// </summary>
		[DxfCodeValue(112, 122, 132)]
		public XYZ UcsYAxis { get; set; }

		/// <summary>
		/// UCS elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double UcsElevation { get; set; }

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(79)]
		public OrthographicType UcsOrthographicType { get; set; }

		//361	Sun hard ownership ID

		//332	Soft-pointer ID/handle to background object (optional)

		//334	Soft-pointer ID/handle to live section object (optional)

		//345	ID/handle of AcDbUCSTableRecord if UCS is a named UCS.If not present, then UCS is unnamed(appears only if code 72 is set to 1)

		//346	ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic(79 code is non-zero). If not present and 79 code is non-zero, then base UCS is taken to be WORLD(appears only if code 72 is set to 1)

		public View() : base() { }

		public View(string name) : base(name) { }

		public override object Clone()
		{
			throw new NotImplementedException();
		}
	}
}
