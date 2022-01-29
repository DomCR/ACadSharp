#region copyright
//Copyright 2021, Albert Domenech.
//All rights reserved. 
//This source code is licensed under the MIT license. 
//See LICENSE file in the project root for full license information.
#endregion
using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	//TODO: Finish documentation DxfCode
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-3F0380A5-1C15-464D-BC66-2C5F094BCFB9

	public enum DxfCode
	{
		Invalid = -9999,
		XDictionary = -6,

		/// <summary>
		/// APP: persistent reactor chain
		/// </summary>
		PReactors = -5,

		/// <summary>
		/// APP: conditional operator (used only with ssget)
		/// </summary>
		Operator = -4,

		/// <summary>
		/// APP: extended data (XDATA) sentinel (fixed)
		/// </summary>
		XDataStart = -3,
		HeaderId = -2,

		/// <summary>
		/// APP: entity name reference (fixed)
		/// </summary>
		FirstEntityId = -2,

		/// <summary>
		/// APP: entity name. The name changes each time a drawing is opened. It is never saved (fixed)
		/// </summary>
		End = -1,

		/// <summary>
		/// Text string indicating the entity type (fixed)
		/// </summary>
		Start = 0,
		Text = 1,
		XRefPath = 1,

		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		ShapeName = 2,
		BlockName = 2,
		AttributeTag = 2,
		SymbolTableName = 2,
		MlineStyleName = 2,
		SymbolTableRecordName = 2,

		/// <summary>
		/// Other text or name values
		/// </summary>
		AttributePrompt = 3,
		DimStyleName = 3,
		LinetypeProse = 3,
		TextFontFile = 3,
		Description = 3,
		DimPostString = 3,

		/// <summary>
		/// Other text or name values
		/// </summary>
		TextBigFontFile = 4,
		DimensionAlternativePrefixSuffix = 4,
		CLShapeName = 4,
		SymbolTableRecordComments = 4,

		/// <summary>
		/// Entity handle; text string of up to 16 hexadecimal digits (fixed)
		/// </summary>
		Handle = 5,
		DimensionBlock = 5,
		DimBlk1 = 6,
		LinetypeName = 6,
		DimBlk2 = 7,
		TextStyleName = 7,
		LayerName = 8,
		CLShapeText = 9,
		XCoordinate = 10,
		YCoordinate = 20,
		ZCoordinate = 30,
		Elevation = 38,
		Thickness = 39,
		Real = 40,
		ViewportHeight = 40,
		TxtSize = 40,
		TxtStyleXScale = 41,
		ViewWidth = 41,
		ViewportAspect = 41,
		TxtStylePSize = 42,
		ViewLensLength = 42,
		ViewFrontClip = 43,
		ViewBackClip = 44,
		ShapeXOffset = 44,
		ShapeYOffset = 45,
		ViewHeight = 45,
		ShapeScale = 46,
		PixelScale = 47,
		LinetypeScale = 48,
		DashLength = 49,
		MlineOffset = 49,
		LinetypeElement = 49,
		Angle = 50,
		ViewportSnapAngle = 50,
		ViewportTwist = 51,
		Visibility = 60,
		LayerLinetype = 61,
		Color = 62,
		HasSubentities = 66,
		ViewportVisibility = 67,
		ViewportActive = 68,
		ViewportNumber = 69,
		Int16 = 70,
		ViewMode = 71,
		CircleSides = 72,
		ViewportZoom = 73,
		ViewportIcon = 74,
		ViewportSnap = 75,
		ViewportGrid = 76,
		ViewportSnapStyle = 77,
		ViewportSnapPair = 78,
		RegAppFlags = 71,
		TxtStyleFlags = 71,
		LinetypeAlign = 72,
		LinetypePdc = 73,
		Int32 = 90,
		Subclass = 100,
		EmbeddedObjectStart = 101,
		ControlString = 102,
		DimVarHandle = 105,
		UcsOrg = 110,
		UcsOrientationX = 111,
		UcsOrientationY = 112,
		XReal = 140,
		ViewBrightness = 141,
		ViewContrast = 142,
		Int64 = 160,
		XInt16 = 170,
		NormalX = 210,
		NormalY = 220,
		NormalZ = 230,
		XXInt16 = 270,
		Int8 = 280,
		RenderMode = 281,
		Bool = 290,
		XTextString = 300,
		BinaryChunk = 310,
		ArbitraryHandle = 320,
		SoftPointerId = 330,
		HardPointerId = 340,
		SoftOwnershipId = 350,
		HardOwnershipId = 360,
		LineWeight = 370,
		PlotStyleNameType = 380,
		PlotStyleNameId = 390,
		ExtendedInt16 = 400,
		LayoutName = 410,
		ColorRgb = 420,
		ColorName = 430,
		Alpha = 440,
		GradientObjType = 450,
		GradientPatType = 451,
		GradientTintType = 452,
		GradientColCount = 453,
		GradientAngle = 460,
		GradientShift = 461,
		GradientTintVal = 462,
		GradientColVal = 463,
		GradientName = 470,
		Comment = 999,
		ExtendedDataAsciiString = 1000,
		ExtendedDataRegAppName = 1001,
		ExtendedDataControlString = 1002,
		ExtendedDataLayerName = 1003,
		ExtendedDataBinaryChunk = 1004,
		ExtendedDataHandle = 1005,
		ExtendedDataXCoordinate = 1010,
		ExtendedDataYCoordinate = 1020,
		ExtendedDataZCoordinate = 1030,
		ExtendedDataWorldXCoordinate = 1011,
		ExtendedDataWorldYCoordinate = 1021,
		ExtendedDataWorldZCoordinate = 1031,
		ExtendedDataWorldXDisp = 1012,
		ExtendedDataWorldYDisp = 1022,
		ExtendedDataWorldZDisp = 1032,
		ExtendedDataWorldXDir = 1013,
		ExtendedDataWorldYDir = 1023,
		ExtendedDataWorldZDir = 1033,
		ExtendedDataReal = 1040,
		ExtendedDataDist = 1041,
		ExtendedDataScale = 1042,
		ExtendedDataInteger16 = 1070,
		ExtendedDataInteger32 = 1071
	}

	//TODO: Finish documentation for GroupCodeValueType
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-2553CF98-44F6-4828-82DD-FE3BC7448113
	public enum GroupCodeValueType
	{
		None,

		/// <summary>
		/// String (with the introduction of extended symbol names in AutoCAD 2000, the 255-character limit has been increased to 2049 single-byte characters not including the newline at the end of the line); 
		/// see the "Storage of String Values" section for more information
		/// </summary>
		/// <remarks>
		/// Code range : 0-9
		/// </remarks>
		String,

		/// <summary>
		/// Double precision 3D point value
		/// </summary>
		/// <remarks>
		/// Code range : 10-39
		/// </remarks>
		Point3D,

		/// <summary>
		/// Double-precision floating-point value
		/// </summary>
		/// <remarks>
		/// Code range : 40-59 | 110-119 | 120-129 | 130-139 | 210-239
		/// </remarks>
		Double,

		Int16,

		Int32,

		Int64,

		/// <summary>
		/// String representing hexadecimal (hex) handle value
		/// </summary>
		/// <remarks>
		/// Code range : 105
		/// </remarks>
		StringHex,

		Bool,

		Chunk,
	}

	public static class GroupCodeValue
	{
		public static GroupCodeValueType TransformValue(int code)
		{
			if (code >= 0 && code <= 9)
				return GroupCodeValueType.String;
			 if (code >= 10 && code <= 39)
				return GroupCodeValueType.Point3D;
			 if (code >= 40 && code <= 59)
				return GroupCodeValueType.Double;
			if (code >= 60 && code <= 79)
				return GroupCodeValueType.Int16;
			if (code >= 90 && code <= 99)
				return GroupCodeValueType.Int32;
			if (code == 100)
				return GroupCodeValueType.String;
			if (code == 101)
				return GroupCodeValueType.String;
			if (code == 102)
				return GroupCodeValueType.String;
			if (code == 105)
				return GroupCodeValueType.StringHex;

			if (code >= 110 && code <= 119)
				return GroupCodeValueType.Double;
			if (code >= 120 && code <= 129)
				return GroupCodeValueType.Double;
			if (code >= 130 && code <= 139)
				return GroupCodeValueType.Double;
			if (code >= 140 && code <= 149)
				return GroupCodeValueType.Double;

			if (code >= 160 && code <= 169)
				return GroupCodeValueType.Int64;

			if (code >= 170 && code <= 179)
				return GroupCodeValueType.Int16;

			if (code >= 210 && code <= 239)
				return GroupCodeValueType.Double;

			if (code >= 270 && code <= 279)
				return GroupCodeValueType.Int16;
			if (code >= 280 && code <= 289)
				return GroupCodeValueType.Int16;

			if (code >= 290 && code <= 299)
				return GroupCodeValueType.Bool;

			if (code >= 300 && code <= 309)
				return GroupCodeValueType.String;

			if (code >= 310 && code <= 319)
				return GroupCodeValueType.Chunk;

			if (code >= 320 && code <= 329)
				return GroupCodeValueType.StringHex;
			if (code >= 330 && code <= 369)
				return GroupCodeValueType.StringHex;

			if (code >= 370 && code <= 379)
				return GroupCodeValueType.Int16;
			if (code >= 380 && code <= 389)
				return GroupCodeValueType.Int16;

			if (code >= 390 && code <= 399)
				return GroupCodeValueType.StringHex;

			if (code >= 400 && code <= 409)
				return GroupCodeValueType.Int16;
			if (code >= 410 && code <= 419)
				return GroupCodeValueType.String;
			if (code >= 420 && code <= 429)
				return GroupCodeValueType.Int32;
			if (code >= 430 && code <= 439)
				return GroupCodeValueType.String;
			if (code >= 440 && code <= 449)
				return GroupCodeValueType.Int32;
			if (code >= 450 && code <= 459)
				return GroupCodeValueType.Int32;
			if (code >= 460 && code <= 469)
				return GroupCodeValueType.Double;
			if (code >= 470 && code <= 479)
				return GroupCodeValueType.String;
			if (code >= 480 && code <= 481)
				return GroupCodeValueType.StringHex;
			if (code == 999)
				return GroupCodeValueType.String;
			if (code >= 1010 && code <= 1059)
				return GroupCodeValueType.Double;
			if (code >= 1000 && code <= 1003)
				return GroupCodeValueType.String;
			if (code == 1004)
				return GroupCodeValueType.Chunk;
			if (code >= 1005 && code <= 1009)
				return GroupCodeValueType.String;
			if (code >= 1060 && code <= 1070)
				return GroupCodeValueType.Int16;
			if (code == 1071)
				return GroupCodeValueType.Int32;

			return GroupCodeValueType.None;
		}
	}
}
