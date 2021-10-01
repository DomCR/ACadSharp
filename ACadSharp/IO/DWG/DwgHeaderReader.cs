using ACadSharp.Geometry.Units;
using ACadSharp.Header;

using CSUtilities.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Util class to read the cad header.
	/// </summary>
	internal class DwgHeaderReader : DwgSectionReader
	{
		private IDwgStreamReader m_mainReader;

		public DwgHeaderReader(ACadVersion version) : base(version)
		{
		}

		public CadHeader Read(IDwgStreamReader sreader,
			int acadMaintenanceVersion, out DwgHeaderHandlesCollection objectPointers)
		{
			//Save the parameter handler in a local variable
			m_mainReader = sreader;

			CadHeader header = new CadHeader(m_version);
			objectPointers = new DwgHeaderHandlesCollection();

			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			byte[] sn = sreader.ReadSentinel();

			//RL : Size of the section.
			long size = sreader.ReadRawLong();

			//R2010/R2013 (only present if the maintenance version is greater than 3!) or R2018+:
			if (R2010Plus && acadMaintenanceVersion > 3 || R2018Plus)
			{
				//Unknown (4 byte long), might be part of a 64-bit size.
				long unknown = sreader.ReadRawLong();
			}

			long initialPos = sreader.PositionInBits();

			//+R2007 Only:
			if (R2007Plus)
			{
				//RL : Size in bits
				long sizeInBits = sreader.ReadRawLong();

				long lastPositionInBits = initialPos + sizeInBits - 1L;

				//Setup the text handler for versions 2007 and above
				IDwgStreamReader textReader = DwgStreamReader.GetStreamHandler(m_version,
					//Create a copy of the stream
					new StreamIO(sreader.Stream, true).Stream);
				//Set the position and use the flag
				textReader.SetPositionByFlag(lastPositionInBits);

				//Setup the handler for the references for versions 2007 and above
				IDwgStreamReader referenceReader = DwgStreamReader.GetStreamHandler(m_version,
					//Create a copy of the stream
					new StreamIO(sreader.Stream, true).Stream);
				//Set the position and jump the flag
				referenceReader.SetPositionInBits(lastPositionInBits + 0b1);

				sreader = new DwgMergedReader(sreader, textReader, referenceReader);
			}

			//R2013+:
			if (R2013Plus)
				//BLL : Variabele REQUIREDVERSIONS, default value 0, read only.
				header.RequiredVersions = sreader.ReadBitLongLong();

			//Common:
			//BD : Unknown, default value 412148564080.0
			double unknownbd = sreader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			unknownbd = sreader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			unknownbd = sreader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			unknownbd = sreader.ReadBitDouble();
			//TV: Unknown text string, default "m"
			var unknowntv = sreader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = sreader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = sreader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = sreader.ReadVariableText();
			//BL : Unknown long, default value 24L
			var unknownbl = sreader.ReadBitLong();
			//BL: Unknown long, default value 0L;
			unknownbl = sreader.ReadBitLong();

			//R13-R14 Only:
			if (R13_14Only)
			{
				//BS : Unknown short, default value 0
				short unknowns = sreader.ReadBitShort();
			}

			//Pre-2004 Only:
			if (R2004Pre)
			{
				//H : Handle of the current viewport entity header (hard pointer)
				ulong pointerViewPort = sreader.HandleReference();
			}

			//Common:
			//B: DIMASO
			header.AssociatedDimensions = sreader.ReadBit();
			//B: DIMSHO
			header.UpdateDimensionsWhileDragging = sreader.ReadBit();

			//R13-R14 Only:
			if (R13_14Only)
				//B : DIMSAV Undocumented.
				header.DIMSAV = sreader.ReadBit();

			//Common:
			//B: PLINEGEN
			header.PolylineLineTypeGeneration = sreader.ReadBit();
			//B : ORTHOMODE
			header.OrthoMode = sreader.ReadBit();
			//B: REGENMODE
			header.RegenerationMode = sreader.ReadBit();
			//B : FILLMODE
			header.FillMode = sreader.ReadBit();
			//B : QTEXTMODE
			header.QuickTextMode = sreader.ReadBit();
			//B : PSLTSCALE
			header.PaperSpaceLineTypeScaling = sreader.ReadBit() ? SpaceLineTypeScaling.Normal : SpaceLineTypeScaling.Viewport;
			//B : LIMCHECK
			header.LimitCheckingOn = sreader.ReadBit();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
				//B : BLIPMODE
				header.BlipMode = sreader.ReadBit();
			//R2004+:
			if (R2004Plus)
				//B : Undocumented
				sreader.ReadBit();

			//Common:
			//B: USRTIMER(User timer on / off).
			header.UserTimer = sreader.ReadBit();
			//B : SKPOLY
			header.SketchPolylines = sreader.ReadBit();
			//B : ANGDIR
			header.AngularDirection = (AngularDirection)sreader.ReadBitAsShort();
			//B : SPLFRAME
			header.ShowSplineControlPoints = sreader.ReadBit();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//B : ATTREQ
				sreader.ReadBit();
				//B : ATTDIA
				sreader.ReadBit();
			}

			//Common:
			//B: MIRRTEXT
			header.MirrorText = sreader.ReadBit();
			//B : WORLDVIEW
			header.WorldView = sreader.ReadBit();

			//R13 - R14 Only:
			if (R13_14Only)
				//B: WIREFRAME Undocumented.
				sreader.ReadBit();

			//Common:
			//B: TILEMODE
			header.ShowModelSpace = sreader.ReadBit();
			//B : PLIMCHECK
			header.PaperSpaceLimitsChecking = sreader.ReadBit();
			//B : VISRETAIN
			header.RetainXRefDependentVisibilitySettings = sreader.ReadBit();

			//R13 - R14 Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//B : DELOBJ
				sreader.ReadBit();

			//Common:
			//B: DISPSILH
			header.DisplaySilhouetteCurves = sreader.ReadBit();
			//B : PELLIPSE(not present in DXF)
			header.CreateEllipseAsPolyline = sreader.ReadBit();
			//BS: PROXYGRAPHICS
			header.ProxyGraphics = sreader.ReadBitShortAsBool();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS : DRAGMODE
				sreader.ReadBitShort();
			}

			//Common:
			//BS: TREEDEPTH
			header.SpatialIndexMaxTreeDepth = sreader.ReadBitShort();
			//BS : LUNITS
			header.LinearUnitFormat = (LinearUnitFormat)sreader.ReadBitShort();
			//BS : LUPREC
			header.LinearUnitPrecision = sreader.ReadBitShort();
			//BS : AUNITS
			header.AngularUnit = (AngularUnitFormat)sreader.ReadBitShort();
			//BS : AUPREC
			header.AngularUnitPrecision = sreader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: OSMODE
				header.ObjectSnapMode = (ObjectSnapMode)sreader.ReadBitShort();

			//Common:
			//BS: ATTMODE
			header.AttributeVisibility = sreader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: COORDS
				sreader.ReadBitShort();

			//Common:
			//BS: PDMODE
			header.PointDisplayMode = sreader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: PICKSTYLE
				sreader.ReadBitShort();

			//R2004 +:
			if (R2004Plus)
			{
				//BL: Unknown
				sreader.ReadBitLong();
				//BL: Unknown
				sreader.ReadBitLong();
				//BL: Unknown
				sreader.ReadBitLong();
			}

			//Common:
			//BS : USERI1
			header.UserShort1 = sreader.ReadBitShort();
			//BS : USERI2
			header.UserShort2 = sreader.ReadBitShort();
			//BS : USERI3
			header.UserShort3 = sreader.ReadBitShort();
			//BS : USERI4
			header.UserShort4 = sreader.ReadBitShort();
			//BS : USERI5
			header.UserShort5 = sreader.ReadBitShort();

			//BS: SPLINESEGS
			header.NumberOfSplineSegments = sreader.ReadBitShort();
			//BS : SURFU
			header.SurfaceDensityU = sreader.ReadBitShort();
			//BS : SURFV
			header.SurfaceDensityV = sreader.ReadBitShort();
			//BS : SURFTYPE
			header.SurfaceType = sreader.ReadBitShort();
			//BS : SURFTAB1
			header.SurfaceMeshTabulationCount1 = sreader.ReadBitShort();
			//BS : SURFTAB2
			header.SurfaceMeshTabulationCount2 = sreader.ReadBitShort();
			//BS : SPLINETYPE
			header.SplineType = (SplineType)sreader.ReadBitShort();
			//BS : SHADEDGE
			header.ShadeEdge = (ShadeEdgeType)sreader.ReadBitShort();
			//BS : SHADEDIF
			header.ShadeDiffuseToAmbientPercentage = sreader.ReadBitShort();
			//BS: UNITMODE
			header.UnitMode = sreader.ReadBitShort();
			//BS : MAXACTVP
			header.MaxViewportCount = sreader.ReadBitShort();
			//BS : ISOLINES
			header.SurfaceIsolineCount = sreader.ReadBitShort();
			//BS : CMLJUST
			header.CurrentMultilineJustification = (VerticalAlignmentType)sreader.ReadBitShort();
			//BS : TEXTQLTY
			header.TextQuality = sreader.ReadBitShort();
			//BD : LTSCALE
			header.LineTypeScale = sreader.ReadBitDouble();
			//BD : TEXTSIZE
			header.TextHeightDefault = sreader.ReadBitDouble();
			//BD : TRACEWID
			header.TraceWidthDefault = sreader.ReadBitDouble();
			//BD : SKETCHINC
			header.SketchIncrement = sreader.ReadBitDouble();
			//BD : FILLETRAD
			header.FilletRadius = sreader.ReadBitDouble();
			//BD : THICKNESS
			header.ThicknessDefault = sreader.ReadBitDouble();
			//BD : ANGBASE
			header.AngleBase = sreader.ReadBitDouble();
			//BD : PDSIZE
			header.PointDisplaySize = sreader.ReadBitDouble();
			//BD : PLINEWID
			header.PolylineWidthDefault = sreader.ReadBitDouble();
			//BD : USERR1
			header.UserDouble1 = sreader.ReadBitDouble();
			//BD : USERR2
			header.UserDouble2 = sreader.ReadBitDouble();
			//BD : USERR3
			header.UserDouble3 = sreader.ReadBitDouble();
			//BD : USERR4
			header.UserDouble4 = sreader.ReadBitDouble();
			//BD : USERR5
			header.UserDouble5 = sreader.ReadBitDouble();
			//BD : CHAMFERA
			header.ChamferDistance1 = sreader.ReadBitDouble();
			//BD : CHAMFERB
			header.ChamferDistance2 = sreader.ReadBitDouble();
			//BD : CHAMFERC
			header.ChamferLength = sreader.ReadBitDouble();
			//BD : CHAMFERD
			header.ChamferAngle = sreader.ReadBitDouble();
			//BD : FACETRES
			header.FacetResolution = sreader.ReadBitDouble();
			//BD : CMLSCALE
			header.CurrentMultilineScale = sreader.ReadBitDouble();
			//BD : CELTSCALE
			header.CurrentEntityLinetypeScale = sreader.ReadBitDouble();

			//TV: MENUNAME
			header.MenuFileName = sreader.ReadVariableText();

			//Common:
			//BL: TDCREATE(Julian day)
			//BL: TDCREATE(Milliseconds into the day)
			header.CreateDateTime = sreader.ReadDateTime();
			//BL: TDUPDATE(Julian day)
			//BL: TDUPDATE(Milliseconds into the day)
			header.UpdateDateTime = sreader.ReadDateTime();

			//R2004 +:
			if (R2004Plus)
			{
				//BL : Unknown
				sreader.ReadBitLong();
				//BL : Unknown
				sreader.ReadBitLong();
				//BL : Unknown
				sreader.ReadBitLong();
			}

			//Common:
			//BL: TDINDWG(Days)
			//BL: TDINDWG(Milliseconds into the day)
			header.TotalEditingTime = sreader.ReadTimeSpan();
			//BL: TDUSRTIMER(Days)
			//BL: TDUSRTIMER(Milliseconds into the day)
			header.UserElapsedTimeSpan = sreader.ReadTimeSpan();

			//CMC : CECOLOR
			header.CurrentEntityColor = sreader.ReadCmColor();

			//H : HANDSEED The next handle, with an 8-bit length specifier preceding the handle
			//bytes (standard hex handle form) (code 0). The HANDSEED is not part of the handle
			//stream, but of the normal data stream (relevant for R21 and later).
			header.HandleSeed = m_mainReader.HandleReference();

			//H : CLAYER (hard pointer)
			objectPointers.CLAYER = sreader.HandleReference();
			//H: TEXTSTYLE(hard pointer)
			objectPointers.TEXTSTYLE = sreader.HandleReference();
			//H: CELTYPE(hard pointer)
			objectPointers.CELTYPE = sreader.HandleReference();

			//R2007 + Only:
			if (R2007Plus)
			{
				//H: CMATERIAL(hard pointer)
				objectPointers.CMATERIAL = sreader.HandleReference();
			}

			//Common:
			//H: DIMSTYLE (hard pointer)
			objectPointers.DIMSTYLE = sreader.HandleReference();
			//H: CMLSTYLE (hard pointer)
			objectPointers.CMLSTYLE = sreader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//BD: PSVPSCALE
				header.ViewportDefaultViewScaleFactor = sreader.ReadBitDouble();
			}

			//Common:
			//3BD: INSBASE(PSPACE)
			header.PaperSpaceUcs.PaperSpaceInsertionBase = sreader.Read3BitDouble();
			//3BD: EXTMIN(PSPACE)
			header.PaperSpaceUcs.PaperSpaceExtMin = sreader.Read3BitDouble();
			//3BD: EXTMAX(PSPACE)
			header.PaperSpaceUcs.PaperSpaceExtMax = sreader.Read3BitDouble();
			//2RD: LIMMIN(PSPACE)
			header.PaperSpaceUcs.PaperSpaceLimitsMin = sreader.Read2RawDouble();
			//2RD: LIMMAX(PSPACE)
			header.PaperSpaceUcs.PaperSpaceLimitsMax = sreader.Read2RawDouble();
			//BD: ELEVATION(PSPACE)
			header.PaperSpaceUcs.PaperSpaceElevation = sreader.ReadBitDouble();
			//3BD: UCSORG(PSPACE)
			header.PaperSpaceUcs.Origin = sreader.Read3BitDouble();
			//3BD: UCSXDIR(PSPACE)
			header.PaperSpaceUcs.XAxis = sreader.Read3BitDouble();
			//3BD: UCSYDIR(PSPACE)
			header.PaperSpaceUcs.YAxis = sreader.Read3BitDouble();

			//H: UCSNAME (PSPACE) (hard pointer)
			objectPointers.UCSNAME_PSPACE = sreader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//H : PUCSORTHOREF (hard pointer)
				objectPointers.PUCSORTHOREF = sreader.HandleReference();

				//BS : PUCSORTHOVIEW	??
				int PUCSORTHOVIEW = sreader.ReadBitShort();

				//H: PUCSBASE(hard pointer)
				objectPointers.PUCSBASE = sreader.HandleReference();

				//3BD: PUCSORGTOP
				header.PaperSpaceUcs.OrthographicTopDOrigin = sreader.Read3BitDouble();
				//3BD: PUCSORGBOTTOM
				header.PaperSpaceUcs.OrthographicBottomDOrigin = sreader.Read3BitDouble();
				//3BD: PUCSORGLEFT
				header.PaperSpaceUcs.OrthographicLeftDOrigin = sreader.Read3BitDouble();
				//3BD: PUCSORGRIGHT
				header.PaperSpaceUcs.OrthographicRightDOrigin = sreader.Read3BitDouble();
				//3BD: PUCSORGFRONT
				header.PaperSpaceUcs.OrthographicFrontDOrigin = sreader.Read3BitDouble();
				//3BD: PUCSORGBACK
				header.PaperSpaceUcs.OrthographicBackDOrigin = sreader.Read3BitDouble();
			}

			//Common:
			//3BD: INSBASE(MSPACE)
			header.InsertionBase = sreader.Read3BitDouble();
			//3BD: EXTMIN(MSPACE)
			header.ExtMin = sreader.Read3BitDouble();
			//3BD: EXTMAX(MSPACE)
			header.ExtMax = sreader.Read3BitDouble();
			//2RD: LIMMIN(MSPACE)
			header.LimitsMin = sreader.Read2RawDouble();
			//2RD: LIMMAX(MSPACE)
			header.LimitsMax = sreader.Read2RawDouble();
			//BD: ELEVATION(MSPACE)
			header.Elevation = sreader.ReadBitDouble();
			//3BD: UCSORG(MSPACE)
			header.Ucs.Origin = sreader.Read3BitDouble();
			//3BD: UCSXDIR(MSPACE)
			header.Ucs.XAxis = sreader.Read3BitDouble();
			//3BD: UCSYDIR(MSPACE)
			header.Ucs.YAxis = sreader.Read3BitDouble();

			//H: UCSNAME(MSPACE)(hard pointer)
			objectPointers.UCSNAME_MSPACE = sreader.HandleReference();

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: UCSORTHOREF(hard pointer)
				objectPointers.UCSORTHOREF = sreader.HandleReference();

				//BS: UCSORTHOVIEW	??
				short UCSORTHOVIEW = sreader.ReadBitShort();

				//H : UCSBASE(hard pointer)
				objectPointers.UCSBASE = sreader.HandleReference();

				//3BD: UCSORGTOP
				header.Ucs.OrthographicTopDOrigin = sreader.Read3BitDouble();
				//3BD: UCSORGBOTTOM
				header.Ucs.OrthographicBottomDOrigin = sreader.Read3BitDouble();
				//3BD: UCSORGLEFT
				header.Ucs.OrthographicLeftDOrigin = sreader.Read3BitDouble();
				//3BD: UCSORGRIGHT
				header.Ucs.OrthographicRightDOrigin = sreader.Read3BitDouble();
				//3BD: UCSORGFRONT
				header.Ucs.OrthographicFrontDOrigin = sreader.Read3BitDouble();
				//3BD: UCSORGBACK
				header.Ucs.OrthographicBackDOrigin = sreader.Read3BitDouble();

				//TV : DIMPOST
				header.DimensionStyleOverrides.PostFix = sreader.ReadVariableText();
				//TV : DIMAPOST
				header.DimensionStyleOverrides.AlternateDimensioningSuffix = sreader.ReadVariableText();
			}

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B: DIMTOL
				header.DimensionStyleOverrides.GenerateTolerances = sreader.ReadBit();
				//B : DIMLIM
				header.DimensionStyleOverrides.LimitsGeneration = sreader.ReadBit();
				//B : DIMTIH
				header.DimensionStyleOverrides.TextInsideHorizontal = sreader.ReadBit();
				//B : DIMTOH
				header.DimensionStyleOverrides.TextOutsideHorizontal = sreader.ReadBit();
				//B : DIMSE1
				header.DimensionStyleOverrides.SuppressFirstExtensionLine = sreader.ReadBit();
				//B : DIMSE2
				header.DimensionStyleOverrides.SuppressSecondExtensionnLine = sreader.ReadBit();
				//B : DIMALT
				header.DimensionStyleOverrides.AlternateUnitDimensioning = sreader.ReadBit();
				//B : DIMTOFL
				header.DimensionStyleOverrides.TextOutsideExtensions = sreader.ReadBit();
				//B : DIMSAH
				header.DimensionStyleOverrides.SeparateArrowBlocks = sreader.ReadBit();
				//B : DIMTIX
				header.DimensionStyleOverrides.TextInsideExtensions = sreader.ReadBit();
				//B : DIMSOXD
				header.DimensionStyleOverrides.SuppressOutsideExtensions = sreader.ReadBit();
				//RC : DIMALTD
				header.DimensionStyleOverrides.AlternateUnitDecimalPlaces = sreader.ReadRawChar();
				//RC : DIMZIN
				header.DimensionStyleOverrides.ZeroHandling = sreader.ReadRawChar();
				//B : DIMSD1
				header.DimensionStyleOverrides.SuppressFirstDimensionLine = sreader.ReadBit();
				//B : DIMSD2
				header.DimensionStyleOverrides.SuppressSecondDimensionLine = sreader.ReadBit();
				//RC : DIMTOLJ
				header.DimensionStyleOverrides.ToleranceAlignment = sreader.ReadRawChar();
				//RC : DIMJUST
				header.DimensionStyleOverrides.TextHorizontalAlignment = sreader.ReadRawChar();
				//RC : DIMFIT
				header.DimensionStyleOverrides.DimensionFit = sreader.ReadRawChar();
				//B : DIMUPT
				header.DimensionStyleOverrides.CursorUpdate = sreader.ReadBit();
				//RC : DIMTZIN
				header.DimensionStyleOverrides.ToleranceZeroHandling = sreader.ReadRawChar();
				//RC: DIMALTZ
				header.DimensionStyleOverrides.AlternateUnitZeroHandling = sreader.ReadRawChar();
				//RC : DIMALTTZ
				header.DimensionStyleOverrides.AlternateUnitToleranceZeroHandling = sreader.ReadRawChar();
				//RC : DIMTAD
				header.DimensionStyleOverrides.TextVerticalAlignment = sreader.ReadRawChar();
				//BS : DIMUNIT
				header.DimensionStyleOverrides.DimensionUnit = sreader.ReadBitShort();
				//BS : DIMAUNIT
				header.DimensionStyleOverrides.AngularDimensionDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMDEC
				header.DimensionStyleOverrides.DecimalPlaces = sreader.ReadBitShort();
				//BS : DIMTDEC
				header.DimensionStyleOverrides.ToleranceDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMALTU
				header.DimensionStyleOverrides.AlternateUnitFormat = sreader.ReadBitShort();
				//BS : DIMALTTD
				header.DimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces = sreader.ReadBitShort();
				//H : DIMTXSTY(hard pointer)
				objectPointers.DIMTXSTY = sreader.HandleReference();
			}

			//Common:
			//BD: DIMSCALE
			header.DimensionStyleOverrides.ScaleFactor = sreader.ReadBitDouble();
			//BD : DIMASZ
			header.DimensionStyleOverrides.ArrowSize = sreader.ReadBitDouble();
			//BD : DIMEXO
			header.DimensionStyleOverrides.ExtensionLineOffset = sreader.ReadBitDouble();
			//BD : DIMDLI
			header.DimensionStyleOverrides.DimensionLineIncrement = sreader.ReadBitDouble();
			//BD : DIMEXE
			header.DimensionStyleOverrides.ExtensionLineExtension = sreader.ReadBitDouble();
			//BD : DIMRND
			header.DimensionStyleOverrides.Rounding = sreader.ReadBitDouble();
			//BD : DIMDLE
			header.DimensionStyleOverrides.DimensionLineExtension = sreader.ReadBitDouble();
			//BD : DIMTP
			header.DimensionStyleOverrides.PlusTolerance = sreader.ReadBitDouble();
			//BD : DIMTM
			header.DimensionStyleOverrides.MinusTolerance = sreader.ReadBitDouble();

			//R2007 + Only:
			if (R2007Plus)
			{
				//BD: DIMFXL
				header.DimensionStyleOverrides.FixedExtensionLineLength = sreader.ReadBitDouble();
				//BD : DIMJOGANG
				header.DimensionStyleOverrides.JoggedRadiusDimensionTransverseSegmentAngle = sreader.ReadBitDouble();
				//BS : DIMTFILL
				header.DimensionStyleOverrides.TextBackgroundFillMode = sreader.ReadBitShort();
				//CMC : DIMTFILLCLR
				header.DimensionStyleOverrides.TextBackgroundColor = sreader.ReadCmColor();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//B: DIMTOL
				header.DimensionStyleOverrides.GenerateTolerances = sreader.ReadBit();
				//B : DIMLIM
				header.DimensionStyleOverrides.LimitsGeneration = sreader.ReadBit();
				//B : DIMTIH
				header.DimensionStyleOverrides.TextInsideHorizontal = sreader.ReadBit();
				//B : DIMTOH
				header.DimensionStyleOverrides.TextOutsideHorizontal = sreader.ReadBit();
				//B : DIMSE1
				header.DimensionStyleOverrides.SuppressFirstExtensionLine = sreader.ReadBit();
				//B : DIMSE2
				header.DimensionStyleOverrides.SuppressSecondExtensionLine = sreader.ReadBit();
				//BS : DIMTAD
				header.DimensionStyleOverrides.TextVerticalAlignment = (char)sreader.ReadBitShort();
				//BS : DIMZIN
				header.DimensionStyleOverrides.ZeroHandling = (char)sreader.ReadBitShort();
				//BS : DIMAZIN
				header.DimensionStyleOverrides.AngularZeroHandling = sreader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//BS: DIMARCSYM
				header.DimensionStyleOverrides.ArcLengthSymbolPosition = sreader.ReadBitShort();
			}

			//Common:
			//BD: DIMTXT
			header.DimensionStyleOverrides.TextHeight = sreader.ReadBitDouble();
			//BD : DIMCEN
			header.DimensionStyleOverrides.CenterMarkSize = sreader.ReadBitDouble();
			//BD: DIMTSZ
			header.DimensionStyleOverrides.TickSize = sreader.ReadBitDouble();
			//BD : DIMALTF
			header.DimensionStyleOverrides.AlternateUnitScaleFactor = sreader.ReadBitDouble();
			//BD : DIMLFAC
			header.DimensionStyleOverrides.LinearScaleFactor = sreader.ReadBitDouble();
			//BD : DIMTVP
			header.DimensionStyleOverrides.TextVerticalPosition = sreader.ReadBitDouble();
			//BD : DIMTFAC
			header.DimensionStyleOverrides.ToleranceScaleFactor = sreader.ReadBitDouble();
			//BD : DIMGAP
			header.DimensionStyleOverrides.DimensionLineGap = sreader.ReadBitDouble();

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//T: DIMPOST
				header.DimensionStyleOverrides.PostFix = sreader.ReadVariableText();
				//T : DIMAPOST
				header.DimensionStyleOverrides.AlternateDimensioningSuffix = sreader.ReadVariableText();
				//T : DIMBLK
				header.DimensionBlockName = sreader.ReadVariableText();
				//T : DIMBLK1
				header.DimensionBlockNameFirst = sreader.ReadVariableText();
				//T : DIMBLK2
				header.DimensionBlockNameSecond = sreader.ReadVariableText();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//BD: DIMALTRND
				header.DimensionStyleOverrides.AlternateUnitRounding = sreader.ReadBitDouble();
				//B : DIMALT
				header.DimensionStyleOverrides.AlternateUnitDimensioning = sreader.ReadBit();
				//BS : DIMALTD
				header.DimensionStyleOverrides.AlternateUnitDecimalPlaces = (char)sreader.ReadBitShort();
				//B : DIMTOFL
				header.DimensionStyleOverrides.TextOutsideExtensions = sreader.ReadBit();
				//B : DIMSAH
				header.DimensionStyleOverrides.SeparateArrowBlocks = sreader.ReadBit();
				//B : DIMTIX
				header.DimensionStyleOverrides.TextInsideExtensions = sreader.ReadBit();
				//B : DIMSOXD
				header.DimensionStyleOverrides.SuppressOutsideExtensions = sreader.ReadBit();
			}

			//Common:
			//CMC: DIMCLRD
			header.DimensionStyleOverrides.DimensionLineColor = sreader.ReadCmColor();
			//CMC : DIMCLRE
			header.DimensionStyleOverrides.ExtensionLineColor = sreader.ReadCmColor();
			//CMC : DIMCLRT
			header.DimensionStyleOverrides.TextColor = sreader.ReadCmColor();

			//R2000 + Only:
			if (R2000Plus)
			{
				//BS: DIMADEC
				header.DimensionStyleOverrides.AngularDimensionDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMDEC
				header.DimensionStyleOverrides.DecimalPlaces = sreader.ReadBitShort();
				//BS : DIMTDEC
				header.DimensionStyleOverrides.ToleranceDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMALTU
				header.DimensionStyleOverrides.AlternateUnitFormat = sreader.ReadBitShort();
				//BS : DIMALTTD
				header.DimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMAUNIT
				header.DimensionStyleOverrides.AngularDimensionDecimalPlaces = sreader.ReadBitShort();
				//BS : DIMFRAC
				header.DimensionStyleOverrides.FractionFormat = sreader.ReadBitShort();
				//BS : DIMLUNIT
				header.DimensionStyleOverrides.LinearUnitFormat = sreader.ReadBitShort();
				//BS : DIMDSEP
				header.DimensionStyleOverrides.DecimalSeparator = (char)sreader.ReadBitShort();
				//BS : DIMTMOVE
				header.DimensionStyleOverrides.TextMovement = sreader.ReadBitShort();
				//BS : DIMJUST
				header.DimensionStyleOverrides.TextHorizontalAlignment = (char)sreader.ReadBitShort();
				//B : DIMSD1
				header.DimensionStyleOverrides.SuppressFirstExtensionLine = sreader.ReadBit();
				//B : DIMSD2
				header.DimensionStyleOverrides.SuppressSecondExtensionnLine = sreader.ReadBit();
				//BS : DIMTOLJ
				header.DimensionStyleOverrides.ToleranceAlignment = (char)sreader.ReadBitShort();
				//BS : DIMTZIN
				header.DimensionStyleOverrides.ToleranceZeroHandling = (char)sreader.ReadBitShort();
				//BS: DIMALTZ
				header.DimensionStyleOverrides.AlternateUnitZeroHandling = (char)sreader.ReadBitShort();
				//BS : DIMALTTZ
				header.DimensionStyleOverrides.AlternateUnitToleranceZeroHandling = (char)sreader.ReadBitShort();
				//B : DIMUPT
				header.DimensionStyleOverrides.CursorUpdate = sreader.ReadBit();
				//BS : DIMATFIT
				header.DimensionStyleOverrides.DimensionTextArrowFit = sreader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//B: DIMFXLON
				header.DimensionStyleOverrides.IsExtensionLineLengthFixed = sreader.ReadBit();
			}

			//R2010 + Only:
			if (R2010Plus)
			{
				//B: DIMTXTDIRECTION
				header.DimensionStyleOverrides.TextDirection = sreader.ReadBit();
				//BD : DIMALTMZF
				header.DimensionStyleOverrides.AltMzf = sreader.ReadBitDouble();
				//T : DIMALTMZS
				header.DimensionStyleOverrides.AltMzs = sreader.ReadVariableText();
				//BD : DIMMZF
				header.DimensionStyleOverrides.Mzf = sreader.ReadBitDouble();
				//T : DIMMZS
				header.DimensionStyleOverrides.Mzs = sreader.ReadVariableText();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: DIMTXSTY(hard pointer)
				objectPointers.DIMTXSTY = sreader.HandleReference();
				//H: DIMLDRBLK(hard pointer)
				objectPointers.DIMLDRBLK = sreader.HandleReference();
				//H: DIMBLK(hard pointer)
				objectPointers.DIMBLK = sreader.HandleReference();
				//H: DIMBLK1(hard pointer)
				objectPointers.DIMBLK1 = sreader.HandleReference();
				//H: DIMBLK2(hard pointer)
				objectPointers.DIMBLK2 = sreader.HandleReference();
			}

			//R2007+ Only:
			if (R2007Plus)
			{
				//H : DIMLTYPE (hard pointer)
				objectPointers.DIMLTYPE = sreader.HandleReference();
				//H: DIMLTEX1(hard pointer)
				objectPointers.DIMLTEX1 = sreader.HandleReference();
				//H: DIMLTEX2(hard pointer)
				objectPointers.DIMLTEX2 = sreader.HandleReference();
			}

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: DIMLWD
				header.DimensionStyleOverrides.DimensionLineWeight = sreader.ReadBitShort();
				//BS : DIMLWE
				header.DimensionStyleOverrides.ExtensionLineWeight = sreader.ReadBitShort();
			}

			//H: BLOCK CONTROL OBJECT(hard owner)
			objectPointers.BLOCK_CONTROL_OBJECT = sreader.HandleReference();
			//H: LAYER CONTROL OBJECT(hard owner)
			objectPointers.LAYER_CONTROL_OBJECT = sreader.HandleReference();
			//H: STYLE CONTROL OBJECT(hard owner)
			objectPointers.STYLE_CONTROL_OBJECT = sreader.HandleReference();
			//H: LINETYPE CONTROL OBJECT(hard owner)
			objectPointers.LINETYPE_CONTROL_OBJECT = sreader.HandleReference();
			//H: VIEW CONTROL OBJECT(hard owner)
			objectPointers.VIEW_CONTROL_OBJECT = sreader.HandleReference();
			//H: UCS CONTROL OBJECT(hard owner)
			objectPointers.UCS_CONTROL_OBJECT = sreader.HandleReference();
			//H: VPORT CONTROL OBJECT(hard owner)
			objectPointers.VPORT_CONTROL_OBJECT = sreader.HandleReference();
			//H: APPID CONTROL OBJECT(hard owner)
			objectPointers.APPID_CONTROL_OBJECT = sreader.HandleReference();
			//H: DIMSTYLE CONTROL OBJECT(hard owner)
			objectPointers.DIMSTYLE_CONTROL_OBJECT = sreader.HandleReference();

			//R13 - R15 Only:
			if (R13_15Only)
			{
				//H: VIEWPORT ENTITY HEADER CONTROL OBJECT(hard owner)
				objectPointers.VIEWPORT_ENTITY_HEADER_CONTROL_OBJECT = sreader.HandleReference();
			}

			//Common:
			//H: DICTIONARY(ACAD_GROUP)(hard pointer)
			objectPointers.DICTIONARY_ACAD_GROUP = sreader.HandleReference();
			//H: DICTIONARY(ACAD_MLINESTYLE)(hard pointer)
			objectPointers.DICTIONARY_ACAD_MLINESTYLE = sreader.HandleReference();

			//H : DICTIONARY (NAMED OBJECTS) (hard owner)
			objectPointers.DICTIONARY_NAMED_OBJECTS = sreader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: TSTACKALIGN, default = 1(not present in DXF)
				header.StackedTextAlignment = sreader.ReadBitShort();
				//BS: TSTACKSIZE, default = 70(not present in DXF)
				header.StackedTextSizePercentage = sreader.ReadBitShort();

				//TV: HYPERLINKBASE
				header.HyperLinkBase = sreader.ReadVariableText();
				//TV : STYLESHEET
				header.StyleSheetName = sreader.ReadVariableText();

				//H : DICTIONARY(LAYOUTS)(hard pointer)
				objectPointers.DICTIONARY_LAYOUTS = sreader.HandleReference();
				//H: DICTIONARY(PLOTSETTINGS)(hard pointer)
				objectPointers.DICTIONARY_PLOTSETTINGS = sreader.HandleReference();
				//H: DICTIONARY(PLOTSTYLES)(hard pointer)
				objectPointers.DICTIONARY_PLOTSTYLES = sreader.HandleReference();
			}

			//R2004 +:
			if (R2004Plus)
			{
				//H: DICTIONARY (MATERIALS) (hard pointer)
				objectPointers.DICTIONARY_MATERIALS = sreader.HandleReference();
				//H: DICTIONARY (COLORS) (hard pointer)
				objectPointers.DICTIONARY_COLORS = sreader.HandleReference();
			}

			//R2007 +:
			if (R2007Plus)
			{
				//H: DICTIONARY(VISUALSTYLE)(hard pointer)
				objectPointers.DICTIONARY_VISUALSTYLE = sreader.HandleReference();
			}

			//R2000 +:
			if (R2000Plus)
			{
				//BL: Flags:
				int flags = sreader.ReadBitLong();
				//CELWEIGHT Flags & 0x001F
				header.CurrentEntityLineWeight = (short)(flags & 0x1F);
				//ENDCAPS Flags & 0x0060
				header.EndCaps = (short)(flags & 0x60);
				//JOINSTYLE Flags & 0x0180
				header.JoinStyle = (short)(flags & 0x180);
				//LWDISPLAY!(Flags & 0x0200)
				header.DisplayLineWeight = (short)(flags & 0x200);
				//XEDIT!(Flags & 0x0400)
				header.XEdit = (short)(flags & 0x400);
				//EXTNAMES Flags & 0x0800
				header.ExtendedNames = (short)(flags & 0x800);
				//PSTYLEMODE Flags & 0x2000
				header.PlotStyleMode = (short)(flags & 0x2000);
				//OLESTARTUP Flags & 0x4000
				header.LoadOLEObject = (short)(flags & 0x4000);

				//BS: INSUNITS
				header.InsUnits = sreader.ReadBitShort();
				//BS : CEPSNTYPE
				header.CurrentEntityPlotStyleType = sreader.ReadBitShort();

				if (header.CurrentEntityPlotStyleType == 3)
				{
					//H: CPSNID(present only if CEPSNTYPE == 3) (hard pointer)
					objectPointers.CPSNID = sreader.HandleReference();
				}

				//TV: FINGERPRINTGUID
				header.FingerPrintGuid = sreader.ReadVariableText();
				//TV : VERSIONGUID
				header.VersionGuid = sreader.ReadVariableText();
			}

			//R2004 +:
			if (R2004Plus)
			{
				//RC: SORTENTS
				header.EntitySortingFlags = (ObjectSortingFlags)sreader.ReadByte();
				//RC : INDEXCTL
				header.IndexCreationFlags = sreader.ReadByte();
				//RC : HIDETEXT
				header.HideText = sreader.ReadByte();
				//RC : XCLIPFRAME, before R2010 the value can be 0 or 1 only.
				header.ExternalReferenceClippingBoundaryType = sreader.ReadByte();
				//RC : DIMASSOC
				header.DimensionAssociativity = (DimensionAssociation)sreader.ReadByte();
				//RC : HALOGAP
				header.HaloGapPercentage = sreader.ReadByte();
				//BS : OBSCUREDCOLOR
				header.ObscuredColor = CadUtils.CreateColorFromIndex(sreader.ReadBitShort());
				//BS : INTERSECTIONCOLOR
				header.InterfereColor = CadUtils.CreateColorFromIndex(sreader.ReadBitShort());
				//RC : OBSCUREDLTYPE
				header.ObscuredType = sreader.ReadByte();
				//RC: INTERSECTIONDISPLAY
				header.IntersectionDisplay = sreader.ReadByte();

				//TV : PROJECTNAME
				header.ProjectName = sreader.ReadVariableText();
			}

			//Common:
			//H: BLOCK_RECORD(*PAPER_SPACE)(hard pointer)
			objectPointers.PAPER_SPACE = sreader.HandleReference();
			//H: BLOCK_RECORD(*MODEL_SPACE)(hard pointer)
			objectPointers.MODEL_SPACE = sreader.HandleReference();
			//H: LTYPE(BYLAYER)(hard pointer)
			objectPointers.BYLAYER = sreader.HandleReference();
			//H: LTYPE(BYBLOCK)(hard pointer)
			objectPointers.BYBLOCK = sreader.HandleReference();
			//H: LTYPE(CONTINUOUS)(hard pointer)
			objectPointers.CONTINUOUS = sreader.HandleReference();

			//R2007 +:
			if (R2007Plus)
			{
				//B: CAMERADISPLAY
				header.CameraDisplayObjects = sreader.ReadBit();

				//BL : unknown
				sreader.ReadBitLong();
				//BL : unknown
				sreader.ReadBitLong();
				//BD : unknown
				sreader.ReadBitDouble();

				//BD : STEPSPERSEC
				header.StepsPerSecond = sreader.ReadBitDouble();
				//BD : STEPSIZE
				header.StepSize = sreader.ReadBitDouble();
				//BD : 3DDWFPREC
				header.Dw3DPrecision = sreader.ReadBitDouble();
				//BD : LENSLENGTH
				header.LensLength = sreader.ReadBitDouble();
				//BD : CAMERAHEIGHT
				header.CameraHeight = sreader.ReadBitDouble();
				//RC : SOLIDHIST
				header.SolidsRetainHistory = sreader.ReadRawChar();
				//RC : SHOWHIST
				header.ShowSolidsHistory = sreader.ReadRawChar();
				//BD : PSOLWIDTH
				header.SweptSolidWidth = sreader.ReadBitDouble();
				//BD : PSOLHEIGHT
				header.SweptSolidHeight = sreader.ReadBitDouble();
				//BD : LOFTANG1
				header.DraftAngleFirstCrossSection = sreader.ReadBitDouble();
				//BD : LOFTANG2
				header.DraftAngleSecondCrossSection = sreader.ReadBitDouble();
				//BD : LOFTMAG1
				header.DraftMagnitudeFirstCrossSection = sreader.ReadBitDouble();
				//BD : LOFTMAG2
				header.DraftMagnitudeSecondCrossSection = sreader.ReadBitDouble();
				//BS : LOFTPARAM
				header.SolidLoftedShape = sreader.ReadBitShort();
				//RC : LOFTNORMALS
				header.LoftedObjectNormals = sreader.ReadRawChar();
				//BD : LATITUDE
				header.Latitude = sreader.ReadBitDouble();
				//BD : LONGITUDE
				header.Longitude = sreader.ReadBitDouble();
				//BD : NORTHDIRECTION
				header.NorthDirection = sreader.ReadBitDouble();
				//BL : TIMEZONE
				header.TimeZone = sreader.ReadBitLong();
				//RC : LIGHTGLYPHDISPLAY
				header.DisplayLightGlyphs = sreader.ReadRawChar();
				//RC : TILEMODELIGHTSYNCH	??
				sreader.ReadRawChar();
				//RC : DWFFRAME
				header.DwgUnderlayFramesVisibility = sreader.ReadRawChar();
				//RC : DGNFRAME
				header.DgnUnderlayFramesVisibility = sreader.ReadRawChar();

				//B : unknown
				sreader.ReadBit();

				//CMC : INTERFERECOLOR
				header.InterfereColor = sreader.ReadCmColor();

				//H : INTERFEREOBJVS(hard pointer)
				objectPointers.INTERFEREOBJVS = sreader.HandleReference();
				//H: INTERFEREVPVS(hard pointer)
				objectPointers.INTERFEREVPVS = sreader.HandleReference();
				//H: DRAGVS(hard pointer)
				objectPointers.DRAGVS = sreader.HandleReference();

				//RC: CSHADOW
				header.ShadowMode = sreader.ReadByte();
				//BD : unknown
				header.ShadowPlaneLocation = sreader.ReadBitDouble();
			}

			//Set the position at the end of the section
			m_mainReader.SetPositionInBits(initialPos + size * 8);
			m_mainReader.ResetShift();

			//Ending sentinel: 0x30,0x84,0xE0,0xDC,0x02,0x21,0xC7,0x56,0xA0,0x83,0x97,0x47,0xB1,0x92,0xCC,0xA0
			var endsn = m_mainReader.ReadSentinel();

			return header;
		}
	}
}