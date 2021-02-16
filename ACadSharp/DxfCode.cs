using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-3F0380A5-1C15-464D-BC66-2C5F094BCFB9

	/// <summary>
	/// DXF Group Codes
	/// </summary>
	/// <remarks>
	/// AcDb::DxfCode
	/// </remarks>
	public enum DxfCode	//TODO: DXF codes are a mess, is this a usless enum??
	{
		Invalid = -9999,
		XDictionary = -6,
		PReactors = -5,
		Operator = -4,
		XDataStart = -3,
		HeaderId = -2,
		FirstEntityId = -2,
		/// <summary>
		/// APP: entity name. The name changes each time a drawing is opened. It is never saved (fixed)
		/// </summary>
		End = -1,
		Start = 0,
		/// <summary>
		/// Primary text value for an entity
		/// </summary>
		Text = 1,
		/// <summary>
		/// Primary text value for an entity
		/// </summary>
		XRefPath = 1,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		ShapeName = 2,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		BlockName = 2,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		AttributeTag = 2,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		SymbolTableName = 2,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		MlineStyleName = 2,
		/// <summary>
		/// Name (attribute tag, block name, and so on)
		/// </summary>
		SymbolTableRecordName = 2,
		AttributePrompt = 3,
		DimStyleName = 3,
		LinetypeProse = 3,
		TextFontFile = 3,
		Description = 3,
		DimPostString = 3,
		TextBigFontFile = 4,
		DimensionAlternativePrefixSuffix = 4,
		CLShapeName = 4,
		SymbolTableRecordComments = 4,
		/// <summary>
		/// Cad object handle
		/// </summary>
		Handle = 5,
		/// <summary>
		/// Text string of up to 16 hexadecimal digits (fixed)
		/// </summary>
		DimensionBlock = 5,
		DimBlk1 = 6,
		LinetypeName = 6,
		DimBlk2 = 7,
		TextStyleName = 7,
		/// <summary>
		/// Layer name (fixed)
		/// </summary>
		LayerName = 8,
		/// <summary>
		/// DXF: variable name identifier (used only in HEADER section of the DXF file)
		/// </summary>
		CLShapeText = 9,
		XCoordinate = 10,
		XCoordinate1 = 11,
		YCoordinate = 20,
		YCoordinate1 = 21,
		ZCoordinate = 30,
		ZCoordinate1 = 31,
		Elevation = 38,
		Thickness = 39,
		Real = 40,
		ViewportHeight = 40,
		TxtSize = 40,
		StartWith = 40,
		TxtStyleXScale = 41,
		XScaleFactor = 41,
		ViewWidth = 41,
		ViewportAspect = 41,
		EndWith = 41,
		StartParameter = 41,
		TxtStylePSize = 42,
		ViewLensLength = 42,
		Bulge = 42,
		EndParameter = 42,
		YScaleFactor = 42,
		ViewFrontClip = 43,
		ZScaleFactor = 43,
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
		ObliqueAngle = 51,
		EndAngle = 51,
		Visibility = 60,
		LayerLinetype = 61,
		Color = 62,
		HasSubentities = 66,
		ViewportVisibility = 67,
		ViewportActive = 68,
		ViewportNumber = 69,
		Int16 = 70,
		ViewMode = 71,
		HatchAssociative = 71,
		RegAppFlags = 71,
		TxtStyleFlags = 71,
		TxtMirrorFlags = 71,
		CircleSides = 72,
		LinetypeAlign = 72,
		HorizontalTextAlignment = 72,
		VerticalTextAlignment = 73,
		ViewportZoom = 73,
		LinetypePdc = 73,
		ViewportIcon = 74,
		VerticalTextJustification1 = 74,
		SmoothType = 75,
		ViewportSnap = 75,
		ViewportGrid = 76,
		ViewportSnapStyle = 77,
		ViewportSnapPair = 78,
		Int32 = 90,
		Identifier = 91,
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
		MaterialHandleId = 347,
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
		HardPointHandle = 480,
		HardPointHandle1 = 481,
		Comment = 999,
		ExtendedDataAsciiString = 1000,
		ExtendedDataRegAppName = 1001,
		ExtendedDataControlString = 1002,
		ExtendedDataLayerName = 1003,
		ExtendedDataBinaryChunk = 1004,
		ExtendedDataHandle = 1005,
		ExtendedDataXCoordinate = 1010,
		ExtendedDataWorldXCoordinate = 1011,
		ExtendedDataWorldXDisp = 1012,
		ExtendedDataWorldXDir = 1013,
		ExtendedDataYCoordinate = 1020,
		ExtendedDataWorldYCoordinate = 1021,
		ExtendedDataWorldYDisp = 1022,
		ExtendedDataWorldYDir = 1023,
		ExtendedDataZCoordinate = 1030,
		ExtendedDataWorldZCoordinate = 1031,
		ExtendedDataWorldZDisp = 1032,
		ExtendedDataWorldZDir = 1033,
		ExtendedDataReal = 1040,
		ExtendedDataDist = 1041,
		ExtendedDataScale = 1042,
		ExtendedDataInteger16 = 1070,
		ExtendedDataInteger32 = 1071
	}
}
