#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using ACadSharp.Attributes;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	public class Viewport : Entity
	{
		public override ObjectType ObjectType => ObjectType.VIEWPORT;
		public override string ObjectName => DxfFileToken.EntityViewport;

		//100	Subclass marker(AcDbViewport)

		/// <summary>
		/// Center point(in WCS)
		/// </summary>
		[DxfCodeValue(10, 20, 30)]
		public XYZ Center { get; set; }

		/// <summary>
		/// Width in paper space units
		/// </summary>
		[DxfCodeValue(40)]
		public double Width { get; set; }

		/// <summary>
		/// Height in paper space units
		/// </summary>
		[DxfCodeValue(41)]
		public double Height { get; set; }

		/// <summary>
		/// Viewport ID
		/// </summary>
		[DxfCodeValue(69)]
		public short Id { get; set; }

		/// <summary>
		/// View center point(in DCS)
		/// </summary>
		[DxfCodeValue(12, 22)]
		public XY ViewCenter { get; set; }

		/// <summary>
		/// Snap base point
		/// </summary>
		[DxfCodeValue(13, 23)]
		public XY SnapBase { get; set; }

		/// <summary>
		/// Snap spacing
		/// </summary>
		[DxfCodeValue(14, 24)]
		public XY SnapSpacing { get; set; }

		/// <summary>
		/// Grid spacing
		/// </summary>
		[DxfCodeValue(15, 25)]
		public XY GridSpacing { get; set; }

		/// <summary>
		/// View direction vector(in WCS)
		/// </summary>
		[DxfCodeValue(16, 26, 36)]
		public XYZ ViewDirection { get; set; }

		/// <summary>
		/// View target point(in WCS)
		/// </summary>
		[DxfCodeValue(17, 27, 37)]
		public XYZ ViewTarget { get; set; }

		/// <summary>
		/// Perspective lens length
		/// </summary>
		[DxfCodeValue(42)]
		public double LensLength { get; set; }

		/// <summary>
		/// Front clip plane Z value
		/// </summary>
		[DxfCodeValue(43)]
		public double FrontClipPlane { get; set; }

		/// <summary>
		/// Back clip plane Z value
		/// </summary>
		[DxfCodeValue(44)]
		public double BackClipPlane { get; set; }

		/// <summary>
		/// View height(in model space units)
		/// </summary>
		[DxfCodeValue(45)]
		public double ViewHeight { get; set; }

		/// <summary>
		/// Snap angle
		/// </summary>
		[DxfCodeValue(50)]
		public double SnapAngle { get; set; }

		/// <summary>
		/// View twist angle
		/// </summary>
		[DxfCodeValue(51)]
		public double TwistAngle { get; set; }

		/// <summary>
		/// Circle zoom percent
		/// </summary>
		[DxfCodeValue(72)]
		public short CircleZoomPercent { get; set; }

		/// <summary>
		/// Frozen layer object ID/handle(multiple entries may exist)
		/// </summary>
		[DxfCodeValue(331)]
		public List<Layer> FrozenLayers { get; private set; }

		/// <summary>
		/// Viewport status.
		/// </summary>
		[DxfCodeValue(90)]
		public ViewportStatusFlags Status { get; set; }

		/// <summary>
		/// Hard-pointer ID/handle to entity that serves as the viewport's clipping boundary (only present if viewport is non-rectangular)
		/// </summary>
		[DxfCodeValue(340)]
		public Entity Boundary { get; set; }

		/// <summary>
		/// Plot style sheet name assigned to this viewport
		/// </summary>
		[DxfCodeValue(1)]
		public string StyleSheetName { get; set; }

		/// <summary>
		/// Render mode
		/// </summary>
		[DxfCodeValue(281)]
		public RenderMode RenderMode { get; set; }

		/// <summary>
		/// UCS per viewport flag
		/// </summary>
		/// <remarks>
		///0 = The UCS will not change when this viewport becomes active.
		///1 = This viewport stores its own UCS which will become the current UCS whenever the viewport is activated
		/// </remarks>
		[DxfCodeValue(71)]
		public bool UcsPerViewport { get; set; }

		/// <summary>
		/// Display UCS icon at UCS origin flag
		/// </summary>
		/// <remarks>
		/// Controls whether UCS icon represents viewport UCS or current UCS(these will be different if UCSVP is 1 and viewport is not active). However, this field is currently being ignored and the icon always represents the viewport UCS
		/// </remarks>
		[DxfCodeValue(74)]
		public bool DisplayUcsIcon { get; set; }

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

		//345

		//ID/handle of AcDbUCSTableRecord if UCS is a named UCS.If not present, then UCS is unnamed

		//346

		//ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic(79 code is non-zero). If not present and 79 code is non-zero, then base UCS is taken to be WORLD

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(79)]
		public OrthographicType UcsOrthographicType { get; set; }

		/// <summary>
		/// Viewport elevation
		/// </summary>
		[DxfCodeValue(146)]
		public double Elevation { get; set; }

		/// <summary>
		/// Orthographic type of UCS
		/// </summary>
		[DxfCodeValue(170)]
		public ShadePlotMode ShadePlotMode { get; set; }

		/// <summary>
		/// Frequency of major grid lines compared to minor grid lines
		/// </summary>
		[DxfCodeValue(61)]
		public short MajorGridLineFrequency { get; set; }

		//332	Background ID/Handle(optional)

		//333	Shade plot ID/Handle(optional)

		//348	Visual style ID/Handle(optional)

		/// <summary>
		/// Default lighting flag.On when no user lights are specified.
		/// </summary>
		[DxfCodeValue(292)]
		public bool UseDefaultLighting { get; set; }

		/// <summary>
		/// Default lighting type.
		/// </summary>
		/// <remarks>
		/// 0 = One distant light
		/// 1 = Two distant lights 
		/// </remarks>
		[DxfCodeValue(282)]
		public LightingType DefaultLightingType { get; set; }

		/// <summary>
		/// View brightness
		/// </summary>
		[DxfCodeValue(141)]
		public double Brightness { get; set; }

		/// <summary>
		/// View contrast
		/// </summary>
		[DxfCodeValue(142)]
		public double Constrast { get; set; }

		/// <summary>
		/// Ambient light color.Write only if not black color.
		/// </summary>
		[DxfCodeValue(63)]
		public Color AmbientLightColor { get; set; }

		//361	Sun ID/Handle(optional)

		//335

		//Soft pointer reference to viewport object (for layer VP property override)
		//343

		//Soft pointer reference to viewport object (for layer VP property override)
		//344

		//Soft pointer reference to viewport object (for layer VP property override)
		//91

		//Soft pointer reference to viewport object (for layer VP property override)

		public Viewport() : base() { }
	}
}
