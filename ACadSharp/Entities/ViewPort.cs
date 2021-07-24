#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp.Entities
{
	public class Viewport : Entity
	{
		public override ObjectType ObjectType => ObjectType.VIEWPORT;
		public override string ObjectName => DxfFileToken.EntityViewport;

		//100	Subclass marker(AcDbViewport)

		//10	Center point(in WCS)
		//DXF: X value; APP: 3D point
		//20, 30	DXF: Y and Z values of center point(in WCS)

		//40	Width in paper space units

		//41

		//Height in paper space units

		//68

		//Viewport status field:

		//-1 = On, but is fully off screen, or is one of the viewports that is not active because the $MAXACTVP count is currently being exceeded.

		//0 = Off

		//<positive value> = On and active.The value indicates the order of stacking for the viewports, where 1 is the active viewport, 2 is the next, and so forth

		//69

		//Viewport ID

		//12

		//View center point(in DCS)

		//DXF: X value; APP: 2D point

		//22

		//DXF: View center point Y value(in DCS)

		//13

		//Snap base point

		//DXF: X value; APP: 2D point

		//23

		//DXF: Snap base point Y value

		//14

		//Snap spacing

		//DXF: X value; APP: 2D point

		//24

		//DXF: Snap spacing Y value

		//15

		//Grid spacing

		//DXF: X value; APP: 2D point

		//25

		//DXF: Grid spacing Y value

		//16

		//View direction vector(in WCS)

		//DXF: X value; APP: 3D vector

		//26, 36

		//DXF: Y and Z values of view direction vector(in WCS)

		//17

		//View target point(in WCS)

		//DXF: X value; APP: 3D vector

		//27, 37

		//DXF: Y and Z values of view target point(in WCS)

		//42

		//Perspective lens length

		//43

		//Front clip plane Z value

		//44

		//Back clip plane Z value

		//45

		//View height(in model space units)

		//50

		//Snap angle

		//51

		//View twist angle

		//72

		//Circle zoom percent

		//331

		//Frozen layer object ID/handle(multiple entries may exist) (optional)

		//90

		//Viewport status bit-coded flags:

		//1 (0x1) = Enables perspective mode

		//2 (0x2) = Enables front clipping

		//4 (0x4) = Enables back clipping

		//8 (0x8) = Enables UCS follow

		//16 (0x10) = Enables front clip not at eye

		//32 (0x20) = Enables UCS icon visibility

		//64 (0x40) = Enables UCS icon at origin

		//128 (0x80) = Enables fast zoom

		//256 (0x100) = Enables snap mode

		//512 (0x200) = Enables grid mode

		//1024 (0x400) = Enables isometric snap style

		//2048 (0x800) = Enables hide plot mode

		//4096 (0x1000) = kIsoPairTop.If set and kIsoPairRight is not set, then isopair top is enabled.If both kIsoPairTop and kIsoPairRight are set, then isopair left is enabled

		//8192 (0x2000) = kIsoPairRight.If set and kIsoPairTop is not set, then isopair right is enabled

		//16384 (0x4000) = Enables viewport zoom locking

		//32768 (0x8000) = Currently always enabled

		//65536 (0x10000) = Enables non-rectangular clipping

		//131072 (0x20000) = Turns the viewport off

		//262144 (0x40000) = Enables the display of the grid beyond the drawing limits

		//524288 (0x80000) = Enable adaptive grid display

		//1048576 (0x100000) = Enables subdivision of the grid below the set grid spacing when the grid display is adaptive

		//2097152 (0x200000) = Enables grid follows workplane switching

		//340

		//Hard-pointer ID/handle to entity that serves as the viewport's clipping boundary (only present if viewport is non-rectangular)

		//1

		//Plot style sheet name assigned to this viewport

		//281

		//Render mode:

		//0 = 2D Optimized(classic 2D)

		//1 = Wireframe

		//2 = Hidden line

		//3 = Flat shaded

		//4 = Gouraud shaded

		//5 = Flat shaded with wireframe

		//6 = Gouraud shaded with wireframe

		//All rendering modes other than 2D Optimized engage the new 3D graphics pipeline.These values directly correspond to the SHADEMODE command and the AcDbAbstractViewTableRecord::RenderMode enum

		//71

		//UCS per viewport flag:

		//0 = The UCS will not change when this viewport becomes active.

		//1 = This viewport stores its own UCS which will become the current UCS whenever the viewport is activated

		//74

		//Display UCS icon at UCS origin flag:

		//Controls whether UCS icon represents viewport UCS or current UCS(these will be different if UCSVP is 1 and viewport is not active). However, this field is currently being ignored and the icon always represents the viewport UCS

		//110

		//UCS origin

		//DXF: X value; APP: 3D point

		//120, 130

		//DXF: Y and Z values of UCS origin

		//111

		//UCS X-axis

		//DXF: X value; APP: 3D vector

		//121, 131

		//DXF: Y and Z values of UCS X-axis

		//112

		//UCS Y-axis

		//DXF: X value; APP: 3D vector

		//122, 132

		//DXF: Y and Z values of UCS Y-axis

		//345

		//ID/handle of AcDbUCSTableRecord if UCS is a named UCS.If not present, then UCS is unnamed

		//346

		//ID/handle of AcDbUCSTableRecord of base UCS if UCS is orthographic(79 code is non-zero). If not present and 79 code is non-zero, then base UCS is taken to be WORLD

		//79

		//Orthographic type of UCS:

		//0 = UCS is not orthographic

		//1 = Top; 2 = Bottom

		//3 = Front; 4 = Back

		//5 = Left; 6 = Right

		//146

		//Elevation

		//170

		//ShadePlot mode:

		//0 = As Displayed

		//1 = Wireframe

		//2 = Hidden

		//3 = Rendered

		//61

		//Frequency of major grid lines compared to minor grid lines

		//332

		//Background ID/Handle(optional)

		//333

		//Shade plot ID/Handle(optional)

		//348

		//Visual style ID/Handle(optional)

		//292

		//Default lighting flag.On when no user lights are specified.

		//282

		//Default lighting type:

		//0 = One distant light

		//1 = Two distant lights

		//141

		//View brightness

		//142

		//View contrast

		//63,421,431

		//Ambient light color.Write only if not black color.

		//361

		//Sun ID/Handle(optional)

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
