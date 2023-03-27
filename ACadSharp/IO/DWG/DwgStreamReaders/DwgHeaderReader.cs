using ACadSharp.Types.Units;
using ACadSharp.Header;
using CSUtilities.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Util class to read the cad header.
	/// </summary>
	internal class DwgHeaderReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Header; } }

		private IDwgStreamReader _reader;

		public DwgHeaderReader(ACadVersion version, IDwgStreamReader reader) : base(version)
		{
			this._reader = reader;
		}

		public CadHeader Read(int acadMaintenanceVersion, out DwgHeaderHandlesCollection objectPointers)
		{
			//Save the parameter handler in a local variable
			IDwgStreamReader mainreader = _reader;

			CadHeader header = new CadHeader(_version);
			objectPointers = new DwgHeaderHandlesCollection();

			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			this.checkSentinel(this._reader, DwgSectionDefinition.StartSentinels[SectionName]);

			//RL : Size of the section.
			long size = _reader.ReadRawLong();

			//R2010/R2013 (only present if the maintenance version is greater than 3!) or R2018+:
			if (R2010Plus && acadMaintenanceVersion > 3 || R2018Plus)
			{
				//Unknown (4 byte long), might be part of a 64-bit size.
				long unknown = _reader.ReadRawLong();
			}

			long initialPos = _reader.PositionInBits();

			//+R2007 Only:
			if (R2007Plus)
			{
				//RL : Size in bits
				long sizeInBits = _reader.ReadRawLong();

				long lastPositionInBits = initialPos + sizeInBits - 1L;

				//Setup the text handler for versions 2007 and above
				IDwgStreamReader textReader = DwgStreamReaderBase.GetStreamHandler(_version,
					//Create a copy of the stream
					new StreamIO(_reader.Stream, true).Stream);
				//Set the position and use the flag
				textReader.SetPositionByFlag(lastPositionInBits);

				//Setup the handler for the references for versions 2007 and above
				IDwgStreamReader referenceReader = DwgStreamReaderBase.GetStreamHandler(_version,
					//Create a copy of the stream
					new StreamIO(_reader.Stream, true).Stream);
				//Set the position and jump the flag
				referenceReader.SetPositionInBits(lastPositionInBits + 0b1);

				_reader = new DwgMergedReader(_reader, textReader, referenceReader);
			}

			//R2013+:
			if (R2013Plus)
				//BLL : Variabele REQUIREDVERSIONS, default value 0, read only.
				header.RequiredVersions = _reader.ReadBitLongLong();

			//Common:
			//BD : Unknown, default value 412148564080.0
			double unknownbd1 = _reader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			double unknownbd2 = _reader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			double unknownbd3 = _reader.ReadBitDouble();
			//BD: Unknown, default value 1.0
			double unknownbd4 = _reader.ReadBitDouble();
			//TV: Unknown text string, default "m"
			string unknowntv = _reader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = _reader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = _reader.ReadVariableText();
			//TV: Unknown text string, default ""
			unknowntv = _reader.ReadVariableText();
			//BL : Unknown long, default value 24L
			var unknownbl = _reader.ReadBitLong();
			//BL: Unknown long, default value 0L;
			unknownbl = _reader.ReadBitLong();

			//R13-R14 Only:
			if (R13_14Only)
			{
				//BS : Unknown short, default value 0
				short unknowns = _reader.ReadBitShort();
			}

			//Pre-2004 Only:
			if (R2004Pre)
			{
				//H : Handle of the current viewport entity header (hard pointer)
				ulong pointerViewPort = _reader.HandleReference();
			}

			//Common:
			//B: DIMASO
			header.AssociatedDimensions = _reader.ReadBit();
			//B: DIMSHO
			header.UpdateDimensionsWhileDragging = _reader.ReadBit();

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B : DIMSAV Undocumented.
				header.DIMSAV = _reader.ReadBit();
			}

			//Common:
			//B: PLINEGEN
			header.PolylineLineTypeGeneration = _reader.ReadBit();
			//B : ORTHOMODE
			header.OrthoMode = _reader.ReadBit();
			//B: REGENMODE
			header.RegenerationMode = _reader.ReadBit();
			//B : FILLMODE
			header.FillMode = _reader.ReadBit();
			//B : QTEXTMODE
			header.QuickTextMode = _reader.ReadBit();
			//B : PSLTSCALE
			header.PaperSpaceLineTypeScaling = _reader.ReadBit() ? SpaceLineTypeScaling.Normal : SpaceLineTypeScaling.Viewport;
			//B : LIMCHECK
			header.LimitCheckingOn = _reader.ReadBit();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
				//B : BLIPMODE
				header.BlipMode = _reader.ReadBit();
			//R2004+:
			if (R2004Plus)
				//B : Undocumented
				_reader.ReadBit();

			//Common:
			//B: USRTIMER(User timer on / off).
			header.UserTimer = _reader.ReadBit();
			//B : SKPOLY
			header.SketchPolylines = _reader.ReadBit();
			//B : ANGDIR
			header.AngularDirection = (AngularDirection)_reader.ReadBitAsShort();
			//B : SPLFRAME
			header.ShowSplineControlPoints = _reader.ReadBit();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//B : ATTREQ
				_reader.ReadBit();
				//B : ATTDIA
				_reader.ReadBit();
			}

			//Common:
			//B: MIRRTEXT
			header.MirrorText = _reader.ReadBit();
			//B : WORLDVIEW
			header.WorldView = _reader.ReadBit();

			//R13 - R14 Only:
			if (R13_14Only)
				//B: WIREFRAME Undocumented.
				_reader.ReadBit();

			//Common:
			//B: TILEMODE
			header.ShowModelSpace = _reader.ReadBit();
			//B : PLIMCHECK
			header.PaperSpaceLimitsChecking = _reader.ReadBit();
			//B : VISRETAIN
			header.RetainXRefDependentVisibilitySettings = _reader.ReadBit();

			//R13 - R14 Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//B : DELOBJ
				_reader.ReadBit();

			//Common:
			//B: DISPSILH
			header.DisplaySilhouetteCurves = _reader.ReadBit();
			//B : PELLIPSE(not present in DXF)
			header.CreateEllipseAsPolyline = _reader.ReadBit();
			//BS: PROXYGRAPHICS
			header.ProxyGraphics = _reader.ReadBitShortAsBool();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS : DRAGMODE
				_reader.ReadBitShort();
			}

			//Common:
			//BS: TREEDEPTH
			header.SpatialIndexMaxTreeDepth = _reader.ReadBitShort();
			//BS : LUNITS
			header.LinearUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
			//BS : LUPREC
			header.LinearUnitPrecision = _reader.ReadBitShort();
			//BS : AUNITS
			header.AngularUnit = (AngularUnitFormat)_reader.ReadBitShort();
			//BS : AUPREC
			header.AngularUnitPrecision = _reader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: OSMODE
				header.ObjectSnapMode = (ObjectSnapMode)_reader.ReadBitShort();

			//Common:
			//BS: ATTMODE
			header.AttributeVisibility = (AttributeVisibilityMode)_reader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: COORDS
				_reader.ReadBitShort();

			//Common:
			//BS: PDMODE
			header.PointDisplayMode = _reader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: PICKSTYLE
				_reader.ReadBitShort();

			//R2004 +:
			if (R2004Plus)
			{
				//BL: Unknown
				_reader.ReadBitLong();
				//BL: Unknown
				_reader.ReadBitLong();
				//BL: Unknown
				_reader.ReadBitLong();
			}

			//Common:
			//BS : USERI1
			header.UserShort1 = _reader.ReadBitShort();
			//BS : USERI2
			header.UserShort2 = _reader.ReadBitShort();
			//BS : USERI3
			header.UserShort3 = _reader.ReadBitShort();
			//BS : USERI4
			header.UserShort4 = _reader.ReadBitShort();
			//BS : USERI5
			header.UserShort5 = _reader.ReadBitShort();

			//BS: SPLINESEGS
			header.NumberOfSplineSegments = _reader.ReadBitShort();
			//BS : SURFU
			header.SurfaceDensityU = _reader.ReadBitShort();
			//BS : SURFV
			header.SurfaceDensityV = _reader.ReadBitShort();
			//BS : SURFTYPE
			header.SurfaceType = _reader.ReadBitShort();
			//BS : SURFTAB1
			header.SurfaceMeshTabulationCount1 = _reader.ReadBitShort();
			//BS : SURFTAB2
			header.SurfaceMeshTabulationCount2 = _reader.ReadBitShort();
			//BS : SPLINETYPE
			header.SplineType = (SplineType)_reader.ReadBitShort();
			//BS : SHADEDGE
			header.ShadeEdge = (ShadeEdgeType)_reader.ReadBitShort();
			//BS : SHADEDIF
			header.ShadeDiffuseToAmbientPercentage = _reader.ReadBitShort();
			//BS: UNITMODE
			header.UnitMode = _reader.ReadBitShort();
			//BS : MAXACTVP
			header.MaxViewportCount = _reader.ReadBitShort();
			//BS : ISOLINES
			header.SurfaceIsolineCount = _reader.ReadBitShort();
			//BS : CMLJUST
			header.CurrentMultilineJustification = (Entities.VerticalAlignmentType)_reader.ReadBitShort();
			//BS : TEXTQLTY
			header.TextQuality = _reader.ReadBitShort();
			//BD : LTSCALE
			header.LineTypeScale = _reader.ReadBitDouble();
			//BD : TEXTSIZE
			header.TextHeightDefault = _reader.ReadBitDouble();
			//BD : TRACEWID
			header.TraceWidthDefault = _reader.ReadBitDouble();
			//BD : SKETCHINC
			header.SketchIncrement = _reader.ReadBitDouble();
			//BD : FILLETRAD
			header.FilletRadius = _reader.ReadBitDouble();
			//BD : THICKNESS
			header.ThicknessDefault = _reader.ReadBitDouble();
			//BD : ANGBASE
			header.AngleBase = _reader.ReadBitDouble();
			//BD : PDSIZE
			header.PointDisplaySize = _reader.ReadBitDouble();
			//BD : PLINEWID
			header.PolylineWidthDefault = _reader.ReadBitDouble();
			//BD : USERR1
			header.UserDouble1 = _reader.ReadBitDouble();
			//BD : USERR2
			header.UserDouble2 = _reader.ReadBitDouble();
			//BD : USERR3
			header.UserDouble3 = _reader.ReadBitDouble();
			//BD : USERR4
			header.UserDouble4 = _reader.ReadBitDouble();
			//BD : USERR5
			header.UserDouble5 = _reader.ReadBitDouble();
			//BD : CHAMFERA
			header.ChamferDistance1 = _reader.ReadBitDouble();
			//BD : CHAMFERB
			header.ChamferDistance2 = _reader.ReadBitDouble();
			//BD : CHAMFERC
			header.ChamferLength = _reader.ReadBitDouble();
			//BD : CHAMFERD
			header.ChamferAngle = _reader.ReadBitDouble();
			//BD : FACETRES
			header.FacetResolution = _reader.ReadBitDouble();
			//BD : CMLSCALE
			header.CurrentMultilineScale = _reader.ReadBitDouble();
			//BD : CELTSCALE
			header.CurrentEntityLinetypeScale = _reader.ReadBitDouble();

			//TV: MENUNAME
			header.MenuFileName = _reader.ReadVariableText();

			//Common:
			//BL: TDCREATE(Julian day)
			//BL: TDCREATE(Milliseconds into the day)
			header.CreateDateTime = _reader.ReadDateTime();
			//BL: TDUPDATE(Julian day)
			//BL: TDUPDATE(Milliseconds into the day)
			header.UpdateDateTime = _reader.ReadDateTime();

			//R2004 +:
			if (R2004Plus)
			{
				//BL : Unknown
				_reader.ReadBitLong();
				//BL : Unknown
				_reader.ReadBitLong();
				//BL : Unknown
				_reader.ReadBitLong();
			}

			//Common:
			//BL: TDINDWG(Days)
			//BL: TDINDWG(Milliseconds into the day)
			header.TotalEditingTime = _reader.ReadTimeSpan();
			//BL: TDUSRTIMER(Days)
			//BL: TDUSRTIMER(Milliseconds into the day)
			header.UserElapsedTimeSpan = _reader.ReadTimeSpan();

			//CMC : CECOLOR
			header.CurrentEntityColor = _reader.ReadCmColor();

			//H : HANDSEED The next handle, with an 8-bit length specifier preceding the handle
			//bytes (standard hex handle form) (code 0). The HANDSEED is not part of the handle
			//stream, but of the normal data stream (relevant for R21 and later).
			header.HandleSeed = mainreader.HandleReference();

			//H : CLAYER (hard pointer)
			objectPointers.CLAYER = _reader.HandleReference();
			//H: TEXTSTYLE(hard pointer)
			objectPointers.TEXTSTYLE = _reader.HandleReference();
			//H: CELTYPE(hard pointer)
			objectPointers.CELTYPE = _reader.HandleReference();

			//R2007 + Only:
			if (R2007Plus)
			{
				//H: CMATERIAL(hard pointer)
				objectPointers.CMATERIAL = _reader.HandleReference();
			}

			//Common:
			//H: DIMSTYLE (hard pointer)
			objectPointers.DIMSTYLE = _reader.HandleReference();
			//H: CMLSTYLE (hard pointer)
			objectPointers.CMLSTYLE = _reader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//BD: PSVPSCALE
				header.ViewportDefaultViewScaleFactor = _reader.ReadBitDouble();
			}

			//Common:
			//3BD: INSBASE(PSPACE)
			header.PaperSpaceInsertionBase = _reader.Read3BitDouble();
			//3BD: EXTMIN(PSPACE)
			header.PaperSpaceExtMin = _reader.Read3BitDouble();
			//3BD: EXTMAX(PSPACE)
			header.PaperSpaceExtMax = _reader.Read3BitDouble();
			//2RD: LIMMIN(PSPACE)
			header.PaperSpaceLimitsMin = _reader.Read2RawDouble();
			//2RD: LIMMAX(PSPACE)
			header.PaperSpaceLimitsMax = _reader.Read2RawDouble();
			//BD: ELEVATION(PSPACE)
			header.PaperSpaceElevation = _reader.ReadBitDouble();
			//3BD: UCSORG(PSPACE)
			header.PaperSpaceUcsOrigin = _reader.Read3BitDouble();
			//3BD: UCSXDIR(PSPACE)
			header.PaperSpaceUcsXAxis = _reader.Read3BitDouble();
			//3BD: UCSYDIR(PSPACE)
			header.PaperSpaceUcsYAxis = _reader.Read3BitDouble();

			//H: UCSNAME (PSPACE) (hard pointer)
			objectPointers.UCSNAME_PSPACE = _reader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//H : PUCSORTHOREF (hard pointer)
				objectPointers.PUCSORTHOREF = _reader.HandleReference();

				//BS : PUCSORTHOVIEW	??
				int PUCSORTHOVIEW = _reader.ReadBitShort();

				//H: PUCSBASE(hard pointer)
				objectPointers.PUCSBASE = _reader.HandleReference();

				//3BD: PUCSORGTOP
				header.PaperSpaceOrthographicTopDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGBOTTOM
				header.PaperSpaceOrthographicBottomDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGLEFT
				header.PaperSpaceOrthographicLeftDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGRIGHT
				header.PaperSpaceOrthographicRightDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGFRONT
				header.PaperSpaceOrthographicFrontDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGBACK
				header.PaperSpaceOrthographicBackDOrigin = _reader.Read3BitDouble();
			}

			//Common:
			//3BD: INSBASE(MSPACE)
			header.ModelSpaceInsertionBase = _reader.Read3BitDouble();
			//3BD: EXTMIN(MSPACE)
			header.ModelSpaceExtMin = _reader.Read3BitDouble();
			//3BD: EXTMAX(MSPACE)
			header.ModelSpaceExtMax = _reader.Read3BitDouble();
			//2RD: LIMMIN(MSPACE)
			header.ModelSpaceLimitsMin = _reader.Read2RawDouble();
			//2RD: LIMMAX(MSPACE)
			header.ModelSpaceLimitsMax = _reader.Read2RawDouble();
			//BD: ELEVATION(MSPACE)
			header.Elevation = _reader.ReadBitDouble();
			//3BD: UCSORG(MSPACE)
			header.ModelSpaceOrigin = _reader.Read3BitDouble();
			//3BD: UCSXDIR(MSPACE)
			header.ModelSpaceXAxis = _reader.Read3BitDouble();
			//3BD: UCSYDIR(MSPACE)
			header.ModelSpaceYAxis = _reader.Read3BitDouble();

			//H: UCSNAME(MSPACE)(hard pointer)
			objectPointers.UCSNAME_MSPACE = _reader.HandleReference();

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: UCSORTHOREF(hard pointer)
				objectPointers.UCSORTHOREF = _reader.HandleReference();

				//BS: UCSORTHOVIEW	??
				short UCSORTHOVIEW = _reader.ReadBitShort();

				//H : UCSBASE(hard pointer)
				objectPointers.UCSBASE = _reader.HandleReference();

				//3BD: UCSORGTOP
				header.ModelSpaceOrthographicTopDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGBOTTOM
				header.ModelSpaceOrthographicBottomDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGLEFT
				header.ModelSpaceOrthographicLeftDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGRIGHT
				header.ModelSpaceOrthographicRightDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGFRONT
				header.ModelSpaceOrthographicFrontDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGBACK
				header.ModelSpaceOrthographicBackDOrigin = _reader.Read3BitDouble();

				//TV : DIMPOST
				header.DimensionPostFix = _reader.ReadVariableText();
				//TV : DIMAPOST
				header.DimensionAlternateDimensioningSuffix = _reader.ReadVariableText();
			}

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B: DIMTOL
				header.DimensionGenerateTolerances = _reader.ReadBit();
				//B : DIMLIM
				header.DimensionLimitsGeneration = _reader.ReadBit();
				//B : DIMTIH
				header.DimensionTextInsideHorizontal = _reader.ReadBit();
				//B : DIMTOH
				header.DimensionTextOutsideHorizontal = _reader.ReadBit();
				//B : DIMSE1
				header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSE2
				header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//B : DIMALT
				header.DimensionAlternateUnitDimensioning = _reader.ReadBit();
				//B : DIMTOFL
				header.DimensionTextOutsideExtensions = _reader.ReadBit();
				//B : DIMSAH
				header.DimensionSeparateArrowBlocks = _reader.ReadBit();
				//B : DIMTIX
				header.DimensionTextInsideExtensions = _reader.ReadBit();
				//B : DIMSOXD
				header.DimensionSuppressOutsideExtensions = _reader.ReadBit();
				//RC : DIMALTD
				header.DimensionAlternateUnitDecimalPlaces = (short)_reader.ReadRawChar();
				//RC : DIMZIN
				header.DimensionZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//B : DIMSD1
				header.DimensionSuppressFirstDimensionLine = _reader.ReadBit();
				//B : DIMSD2
				header.DimensionSuppressSecondDimensionLine = _reader.ReadBit();
				//RC : DIMTOLJ
				header.DimensionToleranceAlignment = (Tables.ToleranceAlignment)_reader.ReadRawChar();
				//RC : DIMJUST
				header.DimensionTextHorizontalAlignment = (Tables.DimensionTextHorizontalAlignment)_reader.ReadRawChar();
				//RC : DIMFIT
				header.DimensionFit = _reader.ReadRawChar();
				//B : DIMUPT
				header.DimensionCursorUpdate = _reader.ReadBit();
				//RC : DIMTZIN
				header.DimensionToleranceZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC: DIMALTZ
				header.DimensionAlternateUnitZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC : DIMALTTZ
				header.DimensionAlternateUnitToleranceZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC : DIMTAD
				header.DimensionTextVerticalAlignment = (Tables.DimensionTextVerticalAlignment)_reader.ReadRawChar();
				//BS : DIMUNIT
				header.DimensionUnit = _reader.ReadBitShort();
				//BS : DIMAUNIT
				header.DimensionAngularDimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMDEC
				header.DimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMTDEC
				header.DimensionToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMALTU
				header.DimensionAlternateUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMALTTD
				header.DimensionAlternateUnitToleranceDecimalPlaces = _reader.ReadBitShort();
				//H : DIMTXSTY(hard pointer)
				objectPointers.DIMTXSTY = _reader.HandleReference();
			}

			//Common:
			//BD: DIMSCALE
			header.DimensionScaleFactor = _reader.ReadBitDouble();
			//BD : DIMASZ
			header.DimensionArrowSize = _reader.ReadBitDouble();
			//BD : DIMEXO
			header.DimensionExtensionLineOffset = _reader.ReadBitDouble();
			//BD : DIMDLI
			header.DimensionLineIncrement = _reader.ReadBitDouble();
			//BD : DIMEXE
			header.DimensionExtensionLineExtension = _reader.ReadBitDouble();
			//BD : DIMRND
			header.DimensionRounding = _reader.ReadBitDouble();
			//BD : DIMDLE
			header.DimensionLineExtension = _reader.ReadBitDouble();
			//BD : DIMTP
			header.DimensionPlusTolerance = _reader.ReadBitDouble();
			//BD : DIMTM
			header.DimensionMinusTolerance = _reader.ReadBitDouble();

			//R2007 + Only:
			if (R2007Plus)
			{
				//BD: DIMFXL
				header.DimensionFixedExtensionLineLength = _reader.ReadBitDouble();
				//BD : DIMJOGANG
				header.DimensionJoggedRadiusDimensionTransverseSegmentAngle = _reader.ReadBitDouble();
				//BS : DIMTFILL
				header.DimensionTextBackgroundFillMode = (Tables.DimensionTextBackgroundFillMode)_reader.ReadBitShort();
				//CMC : DIMTFILLCLR
				header.DimensionTextBackgroundColor = _reader.ReadCmColor();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//B: DIMTOL
				header.DimensionGenerateTolerances = _reader.ReadBit();
				//B : DIMLIM
				header.DimensionLimitsGeneration = _reader.ReadBit();
				//B : DIMTIH
				header.DimensionTextInsideHorizontal = _reader.ReadBit();
				//B : DIMTOH
				header.DimensionTextOutsideHorizontal = _reader.ReadBit();
				//B : DIMSE1
				header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSE2
				header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//BS : DIMTAD
				header.DimensionTextVerticalAlignment = (Tables.DimensionTextVerticalAlignment)(char)_reader.ReadBitShort();
				//BS : DIMZIN
				header.DimensionZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS : DIMAZIN
				header.DimensionAngularZeroHandling = (Tables.ZeroHandling)_reader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//BS: DIMARCSYM
				header.DimensionArcLengthSymbolPosition = (Tables.ArcLengthSymbolPosition)_reader.ReadBitShort();
			}

			//Common:
			//BD: DIMTXT
			header.DimensionTextHeight = _reader.ReadBitDouble();
			//BD : DIMCEN
			header.DimensionCenterMarkSize = _reader.ReadBitDouble();
			//BD: DIMTSZ
			header.DimensionTickSize = _reader.ReadBitDouble();
			//BD : DIMALTF
			header.DimensionAlternateUnitScaleFactor = _reader.ReadBitDouble();
			//BD : DIMLFAC
			header.DimensionLinearScaleFactor = _reader.ReadBitDouble();
			//BD : DIMTVP
			header.DimensionTextVerticalPosition = _reader.ReadBitDouble();
			//BD : DIMTFAC
			header.DimensionToleranceScaleFactor = _reader.ReadBitDouble();
			//BD : DIMGAP
			header.DimensionLineGap = _reader.ReadBitDouble();

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//T: DIMPOST
				header.DimensionPostFix = _reader.ReadVariableText();
				//T : DIMAPOST
				header.DimensionAlternateDimensioningSuffix = _reader.ReadVariableText();
				//T : DIMBLK
				header.DimensionBlockName = _reader.ReadVariableText();
				//T : DIMBLK1
				header.DimensionBlockNameFirst = _reader.ReadVariableText();
				//T : DIMBLK2
				header.DimensionBlockNameSecond = _reader.ReadVariableText();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//BD: DIMALTRND
				header.DimensionAlternateUnitRounding = _reader.ReadBitDouble();
				//B : DIMALT
				header.DimensionAlternateUnitDimensioning = _reader.ReadBit();
				//BS : DIMALTD
				header.DimensionAlternateUnitDecimalPlaces = (short)(char)_reader.ReadBitShort();
				//B : DIMTOFL
				header.DimensionTextOutsideExtensions = _reader.ReadBit();
				//B : DIMSAH
				header.DimensionSeparateArrowBlocks = _reader.ReadBit();
				//B : DIMTIX
				header.DimensionTextInsideExtensions = _reader.ReadBit();
				//B : DIMSOXD
				header.DimensionSuppressOutsideExtensions = _reader.ReadBit();
			}

			//Common:
			//CMC: DIMCLRD
			header.DimensionLineColor = _reader.ReadCmColor();
			//CMC : DIMCLRE
			header.DimensionExtensionLineColor = _reader.ReadCmColor();
			//CMC : DIMCLRT
			header.DimensionTextColor = _reader.ReadCmColor();

			//R2000 + Only:
			if (R2000Plus)
			{
				//BS: DIMADEC
				header.DimensionAngularDimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMDEC
				header.DimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMTDEC
				header.DimensionToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMALTU
				header.DimensionAlternateUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMALTTD
				header.DimensionAlternateUnitToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMAUNIT
				header.DimensionAngularUnit = (AngularUnitFormat)_reader.ReadBitShort();
				//BS : DIMFRAC
				header.DimensionFractionFormat = (Tables.FractionFormat)_reader.ReadBitShort();
				//BS : DIMLUNIT
				header.DimensionLinearUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMDSEP
				header.DimensionDecimalSeparator = (char)_reader.ReadBitShort();
				//BS : DIMTMOVE
				header.DimensionTextMovement = (Tables.TextMovement)_reader.ReadBitShort();
				//BS : DIMJUST
				header.DimensionTextHorizontalAlignment = (Tables.DimensionTextHorizontalAlignment)(char)_reader.ReadBitShort();
				//B : DIMSD1
				header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSD2
				header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//BS : DIMTOLJ
				header.DimensionToleranceAlignment = (Tables.ToleranceAlignment)(char)_reader.ReadBitShort();
				//BS : DIMTZIN
				header.DimensionToleranceZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS: DIMALTZ
				header.DimensionAlternateUnitZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS : DIMALTTZ
				header.DimensionAlternateUnitToleranceZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//B : DIMUPT
				header.DimensionCursorUpdate = _reader.ReadBit();
				//BS : DIMATFIT
				header.DimensionDimensionTextArrowFit = _reader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//B: DIMFXLON
				header.DimensionIsExtensionLineLengthFixed = _reader.ReadBit();
			}

			//R2010 + Only:
			if (R2010Plus)
			{
				//B: DIMTXTDIRECTION
				header.DimensionTextDirection = _reader.ReadBit() ? Tables.TextDirection.RightToLeft : Tables.TextDirection.LeftToRight;
				//BD : DIMALTMZF
				header.DimensionAltMzf = _reader.ReadBitDouble();
				//T : DIMALTMZS
				header.DimensionAltMzs = _reader.ReadVariableText();
				//BD : DIMMZF
				header.DimensionMzf = _reader.ReadBitDouble();
				//T : DIMMZS
				header.DimensionMzs = _reader.ReadVariableText();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: DIMTXSTY(hard pointer)
				objectPointers.DIMTXSTY = _reader.HandleReference();
				//H: DIMLDRBLK(hard pointer)
				objectPointers.DIMLDRBLK = _reader.HandleReference();
				//H: DIMBLK(hard pointer)
				objectPointers.DIMBLK = _reader.HandleReference();
				//H: DIMBLK1(hard pointer)
				objectPointers.DIMBLK1 = _reader.HandleReference();
				//H: DIMBLK2(hard pointer)
				objectPointers.DIMBLK2 = _reader.HandleReference();
			}

			//R2007+ Only:
			if (R2007Plus)
			{
				//H : DIMLTYPE (hard pointer)
				objectPointers.DIMLTYPE = _reader.HandleReference();
				//H: DIMLTEX1(hard pointer)
				objectPointers.DIMLTEX1 = _reader.HandleReference();
				//H: DIMLTEX2(hard pointer)
				objectPointers.DIMLTEX2 = _reader.HandleReference();
			}

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: DIMLWD
				header.DimensionLineWeight = (LineweightType)_reader.ReadBitShort();
				//BS : DIMLWE
				header.ExtensionLineWeight = (LineweightType)_reader.ReadBitShort();
			}

			//H: BLOCK CONTROL OBJECT(hard owner)
			objectPointers.BLOCK_CONTROL_OBJECT = _reader.HandleReference();
			//H: LAYER CONTROL OBJECT(hard owner)
			objectPointers.LAYER_CONTROL_OBJECT = _reader.HandleReference();
			//H: STYLE CONTROL OBJECT(hard owner)
			objectPointers.STYLE_CONTROL_OBJECT = _reader.HandleReference();
			//H: LINETYPE CONTROL OBJECT(hard owner)
			objectPointers.LINETYPE_CONTROL_OBJECT = _reader.HandleReference();
			//H: VIEW CONTROL OBJECT(hard owner)
			objectPointers.VIEW_CONTROL_OBJECT = _reader.HandleReference();
			//H: UCS CONTROL OBJECT(hard owner)
			objectPointers.UCS_CONTROL_OBJECT = _reader.HandleReference();
			//H: VPORT CONTROL OBJECT(hard owner)
			objectPointers.VPORT_CONTROL_OBJECT = _reader.HandleReference();
			//H: APPID CONTROL OBJECT(hard owner)
			objectPointers.APPID_CONTROL_OBJECT = _reader.HandleReference();
			//H: DIMSTYLE CONTROL OBJECT(hard owner)
			objectPointers.DIMSTYLE_CONTROL_OBJECT = _reader.HandleReference();

			//R13 - R15 Only:
			if (R13_15Only)
			{
				//H: VIEWPORT ENTITY HEADER CONTROL OBJECT(hard owner)
				objectPointers.VIEWPORT_ENTITY_HEADER_CONTROL_OBJECT = _reader.HandleReference();
			}

			//Common:
			//H: DICTIONARY(ACAD_GROUP)(hard pointer)
			objectPointers.DICTIONARY_ACAD_GROUP = _reader.HandleReference();
			//H: DICTIONARY(ACAD_MLINESTYLE)(hard pointer)
			objectPointers.DICTIONARY_ACAD_MLINESTYLE = _reader.HandleReference();

			//H : DICTIONARY (NAMED OBJECTS) (hard owner)
			objectPointers.DICTIONARY_NAMED_OBJECTS = _reader.HandleReference();

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: TSTACKALIGN, default = 1(not present in DXF)
				header.StackedTextAlignment = _reader.ReadBitShort();
				//BS: TSTACKSIZE, default = 70(not present in DXF)
				header.StackedTextSizePercentage = _reader.ReadBitShort();

				//TV: HYPERLINKBASE
				header.HyperLinkBase = _reader.ReadVariableText();
				//TV : STYLESHEET
				header.StyleSheetName = _reader.ReadVariableText();

				//H : DICTIONARY(LAYOUTS)(hard pointer)
				objectPointers.DICTIONARY_LAYOUTS = _reader.HandleReference();
				//H: DICTIONARY(PLOTSETTINGS)(hard pointer)
				objectPointers.DICTIONARY_PLOTSETTINGS = _reader.HandleReference();
				//H: DICTIONARY(PLOTSTYLES)(hard pointer)
				objectPointers.DICTIONARY_PLOTSTYLES = _reader.HandleReference();
			}

			//R2004 +:
			if (R2004Plus)
			{
				//H: DICTIONARY (MATERIALS) (hard pointer)
				objectPointers.DICTIONARY_MATERIALS = _reader.HandleReference();
				//H: DICTIONARY (COLORS) (hard pointer)
				objectPointers.DICTIONARY_COLORS = _reader.HandleReference();
			}

			//R2007 +:
			if (R2007Plus)
			{
				//H: DICTIONARY(VISUALSTYLE)(hard pointer)
				objectPointers.DICTIONARY_VISUALSTYLE = _reader.HandleReference();

				//R2013+:
				if (this.R2013Plus)
					//H : UNKNOWN (hard pointer)
					objectPointers.DICTIONARY_VISUALSTYLE = _reader.HandleReference();
			}

			//R2000 +:
			if (R2000Plus)
			{
				//BL: Flags:
				int flags = _reader.ReadBitLong();
				//CELWEIGHT Flags & 0x001F
				header.CurrentEntityLineWeight = (LineweightType)(flags & 0x1F);
				//ENDCAPS Flags & 0x0060
				header.EndCaps = (short)(flags & 0x60);
				//JOINSTYLE Flags & 0x0180
				header.JoinStyle = (short)(flags & 0x180);
				//LWDISPLAY!(Flags & 0x0200)
				header.DisplayLineWeight = (flags & 0x200) == 1;
				//XEDIT!(Flags & 0x0400)
				header.XEdit = (short)(flags & 0x400) == 1;
				//EXTNAMES Flags & 0x0800
				header.ExtendedNames = (flags & 0x800) == 1;
				//PSTYLEMODE Flags & 0x2000
				header.PlotStyleMode = (short)(flags & 0x2000);
				//OLESTARTUP Flags & 0x4000
				header.LoadOLEObject = (flags & 0x4000) == 1;

				//BS: INSUNITS
				header.InsUnits = (UnitsType)_reader.ReadBitShort();
				//BS : CEPSNTYPE
				header.CurrentEntityPlotStyleType = _reader.ReadBitShort();

				if (header.CurrentEntityPlotStyleType == 3)
				{
					//H: CPSNID(present only if CEPSNTYPE == 3) (hard pointer)
					objectPointers.CPSNID = _reader.HandleReference();
				}

				//TV: FINGERPRINTGUID
				header.FingerPrintGuid = _reader.ReadVariableText();
				//TV : VERSIONGUID
				header.VersionGuid = _reader.ReadVariableText();
			}

			//R2004 +:
			if (R2004Plus)
			{
				//RC: SORTENTS
				header.EntitySortingFlags = (ObjectSortingFlags)_reader.ReadByte();
				//RC : INDEXCTL
				header.IndexCreationFlags = _reader.ReadByte();
				//RC : HIDETEXT
				header.HideText = _reader.ReadByte();
				//RC : XCLIPFRAME, before R2010 the value can be 0 or 1 only.
				header.ExternalReferenceClippingBoundaryType = _reader.ReadByte();
				//RC : DIMASSOC
				header.DimensionAssociativity = (DimensionAssociation)_reader.ReadByte();
				//RC : HALOGAP
				header.HaloGapPercentage = _reader.ReadByte();
				//BS : OBSCUREDCOLOR
				header.ObscuredColor = new Color(_reader.ReadBitShort());
				//BS : INTERSECTIONCOLOR
				header.InterfereColor = new Color(_reader.ReadBitShort());
				//RC : OBSCUREDLTYPE
				header.ObscuredType = _reader.ReadByte();
				//RC: INTERSECTIONDISPLAY
				header.IntersectionDisplay = _reader.ReadByte();

				//TV : PROJECTNAME
				header.ProjectName = _reader.ReadVariableText();
			}

			//Common:
			//H: BLOCK_RECORD(*PAPER_SPACE)(hard pointer)
			objectPointers.PAPER_SPACE = _reader.HandleReference();
			//H: BLOCK_RECORD(*MODEL_SPACE)(hard pointer)
			objectPointers.MODEL_SPACE = _reader.HandleReference();
			//H: LTYPE(BYLAYER)(hard pointer)
			objectPointers.BYLAYER = _reader.HandleReference();
			//H: LTYPE(BYBLOCK)(hard pointer)
			objectPointers.BYBLOCK = _reader.HandleReference();
			//H: LTYPE(CONTINUOUS)(hard pointer)
			objectPointers.CONTINUOUS = _reader.HandleReference();

			//R2007 +:
			if (R2007Plus)
			{
				//B: CAMERADISPLAY
				header.CameraDisplayObjects = _reader.ReadBit();

				//BL : unknown
				_reader.ReadBitLong();
				//BL : unknown
				_reader.ReadBitLong();
				//BD : unknown
				_reader.ReadBitDouble();

				//BD : STEPSPERSEC
				header.StepsPerSecond = _reader.ReadBitDouble();
				//BD : STEPSIZE
				header.StepSize = _reader.ReadBitDouble();
				//BD : 3DDWFPREC
				header.Dw3DPrecision = _reader.ReadBitDouble();
				//BD : LENSLENGTH
				header.LensLength = _reader.ReadBitDouble();
				//BD : CAMERAHEIGHT
				header.CameraHeight = _reader.ReadBitDouble();
				//RC : SOLIDHIST
				header.SolidsRetainHistory = _reader.ReadRawChar();
				//RC : SHOWHIST
				header.ShowSolidsHistory = _reader.ReadRawChar();
				//BD : PSOLWIDTH
				header.SweptSolidWidth = _reader.ReadBitDouble();
				//BD : PSOLHEIGHT
				header.SweptSolidHeight = _reader.ReadBitDouble();
				//BD : LOFTANG1
				header.DraftAngleFirstCrossSection = _reader.ReadBitDouble();
				//BD : LOFTANG2
				header.DraftAngleSecondCrossSection = _reader.ReadBitDouble();
				//BD : LOFTMAG1
				header.DraftMagnitudeFirstCrossSection = _reader.ReadBitDouble();
				//BD : LOFTMAG2
				header.DraftMagnitudeSecondCrossSection = _reader.ReadBitDouble();
				//BS : LOFTPARAM
				header.SolidLoftedShape = _reader.ReadBitShort();
				//RC : LOFTNORMALS
				header.LoftedObjectNormals = _reader.ReadRawChar();
				//BD : LATITUDE
				header.Latitude = _reader.ReadBitDouble();
				//BD : LONGITUDE
				header.Longitude = _reader.ReadBitDouble();
				//BD : NORTHDIRECTION
				header.NorthDirection = _reader.ReadBitDouble();
				//BL : TIMEZONE
				header.TimeZone = _reader.ReadBitLong();
				//RC : LIGHTGLYPHDISPLAY
				header.DisplayLightGlyphs = _reader.ReadRawChar();
				//RC : TILEMODELIGHTSYNCH	??
				_reader.ReadRawChar();
				//RC : DWFFRAME
				header.DwgUnderlayFramesVisibility = _reader.ReadRawChar();
				//RC : DGNFRAME
				header.DgnUnderlayFramesVisibility = _reader.ReadRawChar();

				//B : unknown
				_reader.ReadBit();

				//CMC : INTERFERECOLOR
				header.InterfereColor = _reader.ReadCmColor();

				//H : INTERFEREOBJVS(hard pointer)
				objectPointers.INTERFEREOBJVS = _reader.HandleReference();
				//H: INTERFEREVPVS(hard pointer)
				objectPointers.INTERFEREVPVS = _reader.HandleReference();
				//H: DRAGVS(hard pointer)
				objectPointers.DRAGVS = _reader.HandleReference();

				//RC: CSHADOW
				header.ShadowMode = _reader.ReadByte();
				//BD : unknown
				header.ShadowPlaneLocation = _reader.ReadBitDouble();
			}

			try
			{
				//Not necessary for the integrity of the data

				//Set the position at the end of the section
				mainreader.SetPositionInBits(initialPos + size * 8);
				mainreader.ResetShift();

				//Ending sentinel: 0x30,0x84,0xE0,0xDC,0x02,0x21,0xC7,0x56,0xA0,0x83,0x97,0x47,0xB1,0x92,0xCC,0xA0
				this.checkSentinel(this._reader, DwgSectionDefinition.EndSentinels[SectionName]);
			}
			catch (System.Exception ex)
			{
				this.notify($"An error ocurred at the end of the Header reading", NotificationType.Error, ex);
			}

			return header;
		}
	}
}