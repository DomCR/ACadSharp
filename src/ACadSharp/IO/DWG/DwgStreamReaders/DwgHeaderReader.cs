using ACadSharp.Header;
using ACadSharp.Types.Units;
using CSMath;
using CSUtilities.IO;
using System;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// Util class to read the cad header.
	/// </summary>
	internal class DwgHeaderReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Header; } }

		private IDwgStreamReader _reader;

		private CadHeader _header;

		public DwgHeaderReader(ACadVersion version, IDwgStreamReader reader, CadHeader header) : base(version)
		{
			this._reader = reader;
			this._header = header;
			this._header.Version = version;
		}

		public void Read(int acadMaintenanceVersion, out DwgHeaderHandlesCollection objectPointers)
		{
			//Save the parameter handler in a local variable
			IDwgStreamReader mainreader = _reader;

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
				_header.RequiredVersions = _reader.ReadBitLongLong();

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
			_header.AssociatedDimensions = _reader.ReadBit();
			//B: DIMSHO
			_header.UpdateDimensionsWhileDragging = _reader.ReadBit();

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B : DIMSAV Undocumented.
				_header.DIMSAV = _reader.ReadBit();
			}

			//Common:
			//B: PLINEGEN
			_header.PolylineLineTypeGeneration = _reader.ReadBit();
			//B : ORTHOMODE
			_header.OrthoMode = _reader.ReadBit();
			//B: REGENMODE
			_header.RegenerationMode = _reader.ReadBit();
			//B : FILLMODE
			_header.FillMode = _reader.ReadBit();
			//B : QTEXTMODE
			_header.QuickTextMode = _reader.ReadBit();
			//B : PSLTSCALE
			_header.PaperSpaceLineTypeScaling = _reader.ReadBit() ? SpaceLineTypeScaling.Normal : SpaceLineTypeScaling.Viewport;
			//B : LIMCHECK
			_header.LimitCheckingOn = _reader.ReadBit();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
				//B : BLIPMODE
				_header.BlipMode = _reader.ReadBit();
			//R2004+:
			if (R2004Plus)
				//B : Undocumented
				_reader.ReadBit();

			//Common:
			//B: USRTIMER(User timer on / off).
			_header.UserTimer = _reader.ReadBit();
			//B : SKPOLY
			_header.SketchPolylines = _reader.ReadBit();
			//B : ANGDIR
			_header.AngularDirection = (AngularDirection)_reader.ReadBitAsShort();
			//B : SPLFRAME
			_header.ShowSplineControlPoints = _reader.ReadBit();

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
			_header.MirrorText = _reader.ReadBit();
			//B : WORLDVIEW
			_header.WorldView = _reader.ReadBit();

			//R13 - R14 Only:
			if (R13_14Only)
				//B: WIREFRAME Undocumented.
				_reader.ReadBit();

			//Common:
			//B: TILEMODE
			_header.ShowModelSpace = _reader.ReadBit();
			//B : PLIMCHECK
			_header.PaperSpaceLimitsChecking = _reader.ReadBit();
			//B : VISRETAIN
			_header.RetainXRefDependentVisibilitySettings = _reader.ReadBit();

			//R13 - R14 Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//B : DELOBJ
				_reader.ReadBit();

			//Common:
			//B: DISPSILH
			_header.DisplaySilhouetteCurves = _reader.ReadBit();
			//B : PELLIPSE(not present in DXF)
			_header.CreateEllipseAsPolyline = _reader.ReadBit();
			//BS: PROXYGRAPHICS
			_header.ProxyGraphics = _reader.ReadBitShortAsBool();

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS : DRAGMODE
				_reader.ReadBitShort();
			}

			//Common:
			//BS: TREEDEPTH
			_header.SpatialIndexMaxTreeDepth = _reader.ReadBitShort();
			//BS : LUNITS
			_header.LinearUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
			//BS : LUPREC
			var linearUnitPrecision = _reader.ReadBitShort();
			if (linearUnitPrecision >= 0 && linearUnitPrecision <= 8)
			{
				_header.LinearUnitPrecision = linearUnitPrecision;
			}
			//BS : AUNITS
			_header.AngularUnit = (AngularUnitFormat)_reader.ReadBitShort();
			//BS : AUPREC
			var angularUnitPrecision = _reader.ReadBitShort();
			if (angularUnitPrecision >= 0 && angularUnitPrecision <= 8)
			{
				this._header.AngularUnitPrecision = angularUnitPrecision;
			}

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS: OSMODE
				_header.ObjectSnapMode = (ObjectSnapMode)_reader.ReadBitShort();
			}

			//Common:
			//BS: ATTMODE
			_header.AttributeVisibility = (AttributeVisibilityMode)_reader.ReadBitShort();

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: COORDS
				_reader.ReadBitShort();

			//Common:
			//BS: PDMODE
			_header.PointDisplayMode = _reader.ReadBitShort();

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
			_header.UserShort1 = _reader.ReadBitShort();
			//BS : USERI2
			_header.UserShort2 = _reader.ReadBitShort();
			//BS : USERI3
			_header.UserShort3 = _reader.ReadBitShort();
			//BS : USERI4
			_header.UserShort4 = _reader.ReadBitShort();
			//BS : USERI5
			_header.UserShort5 = _reader.ReadBitShort();

			//BS: SPLINESEGS
			_header.NumberOfSplineSegments = _reader.ReadBitShort();
			//BS : SURFU
			_header.SurfaceDensityU = _reader.ReadBitShort();
			//BS : SURFV
			_header.SurfaceDensityV = _reader.ReadBitShort();
			//BS : SURFTYPE
			_header.SurfaceType = _reader.ReadBitShort();
			//BS : SURFTAB1
			_header.SurfaceMeshTabulationCount1 = _reader.ReadBitShort();
			//BS : SURFTAB2
			_header.SurfaceMeshTabulationCount2 = _reader.ReadBitShort();
			//BS : SPLINETYPE
			_header.SplineType = (SplineType)_reader.ReadBitShort();
			//BS : SHADEDGE
			_header.ShadeEdge = (ShadeEdgeType)_reader.ReadBitShort();
			//BS : SHADEDIF
			_header.ShadeDiffuseToAmbientPercentage = _reader.ReadBitShort();
			//BS: UNITMODE
			_header.UnitMode = _reader.ReadBitShort();
			//BS : MAXACTVP
			_header.MaxViewportCount = _reader.ReadBitShort();
			//BS : ISOLINES
			var surfaceIsoLineCount = _reader.ReadBitShort();
			if (surfaceIsoLineCount >= 0 && surfaceIsoLineCount <= 2047)
			{
				_header.SurfaceIsolineCount = surfaceIsoLineCount;
			}
			//BS : CMLJUST
			_header.CurrentMultiLineJustification = (Entities.VerticalAlignmentType)_reader.ReadBitShort();
			//BS : TEXTQLTY
			var textQuality = _reader.ReadBitShort();
			if (textQuality >= 0 && textQuality <= 100)
			{
				_header.TextQuality = textQuality;
			}
			//BD : LTSCALE
			_header.LineTypeScale = _reader.ReadBitDouble();
			//BD : TEXTSIZE
			_header.TextHeightDefault = _reader.ReadBitDouble();
			//BD : TRACEWID
			_header.TraceWidthDefault = _reader.ReadBitDouble();
			//BD : SKETCHINC
			_header.SketchIncrement = _reader.ReadBitDouble();
			//BD : FILLETRAD
			_header.FilletRadius = _reader.ReadBitDouble();
			//BD : THICKNESS
			_header.ThicknessDefault = _reader.ReadBitDouble();
			//BD : ANGBASE
			_header.AngleBase = _reader.ReadBitDouble();
			//BD : PDSIZE
			_header.PointDisplaySize = _reader.ReadBitDouble();
			//BD : PLINEWID
			_header.PolylineWidthDefault = _reader.ReadBitDouble();
			//BD : USERR1
			_header.UserDouble1 = _reader.ReadBitDouble();
			//BD : USERR2
			_header.UserDouble2 = _reader.ReadBitDouble();
			//BD : USERR3
			_header.UserDouble3 = _reader.ReadBitDouble();
			//BD : USERR4
			_header.UserDouble4 = _reader.ReadBitDouble();
			//BD : USERR5
			_header.UserDouble5 = _reader.ReadBitDouble();
			//BD : CHAMFERA
			_header.ChamferDistance1 = _reader.ReadBitDouble();
			//BD : CHAMFERB
			_header.ChamferDistance2 = _reader.ReadBitDouble();
			//BD : CHAMFERC
			_header.ChamferLength = _reader.ReadBitDouble();
			//BD : CHAMFERD
			_header.ChamferAngle = _reader.ReadBitDouble();
			//BD : FACETRES
			var facetResolution = _reader.ReadBitDouble();
			if (facetResolution > 0 && facetResolution <= 10)
			{
				_header.FacetResolution = facetResolution;
			}
			//BD : CMLSCALE
			_header.CurrentMultilineScale = _reader.ReadBitDouble();
			//BD : CELTSCALE
			_header.CurrentEntityLinetypeScale = _reader.ReadBitDouble();

			//TV: MENUNAME
			_header.MenuFileName = _reader.ReadVariableText();

			//Common:
			//BL: TDCREATE(Julian day)
			//BL: TDCREATE(Milliseconds into the day)
			_header.CreateDateTime = _reader.ReadDateTime();
			//BL: TDUPDATE(Julian day)
			//BL: TDUPDATE(Milliseconds into the day)
			_header.UpdateDateTime = _reader.ReadDateTime();

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
			_header.TotalEditingTime = _reader.ReadTimeSpan();
			//BL: TDUSRTIMER(Days)
			//BL: TDUSRTIMER(Milliseconds into the day)
			_header.UserElapsedTimeSpan = _reader.ReadTimeSpan();

			//CMC : CECOLOR
			_header.CurrentEntityColor = _reader.ReadCmColor(); //TODO: Check the method for CECOLOR

			//H : HANDSEED The next handle, with an 8-bit length specifier preceding the handle
			//bytes (standard hex handle form) (code 0). The HANDSEED is not part of the handle
			//stream, but of the normal data stream (relevant for R21 and later).
			_header.HandleSeed = mainreader.HandleReference();

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
				_header.ViewportDefaultViewScaleFactor = _reader.ReadBitDouble();
			}

			//Common:
			//3BD: INSBASE(PSPACE)
			_header.PaperSpaceInsertionBase = _reader.Read3BitDouble();
			//3BD: EXTMIN(PSPACE)
			_header.PaperSpaceExtMin = _reader.Read3BitDouble();
			//3BD: EXTMAX(PSPACE)
			_header.PaperSpaceExtMax = _reader.Read3BitDouble();
			//2RD: LIMMIN(PSPACE)
			_header.PaperSpaceLimitsMin = _reader.Read2RawDouble();
			//2RD: LIMMAX(PSPACE)
			_header.PaperSpaceLimitsMax = _reader.Read2RawDouble();
			//BD: ELEVATION(PSPACE)
			_header.PaperSpaceElevation = _reader.ReadBitDouble();
			//3BD: UCSORG(PSPACE)
			_header.PaperSpaceUcsOrigin = _reader.Read3BitDouble();
			//3BD: UCSXDIR(PSPACE)
			_header.PaperSpaceUcsXAxis = _reader.Read3BitDouble();
			//3BD: UCSYDIR(PSPACE)
			_header.PaperSpaceUcsYAxis = _reader.Read3BitDouble();

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
				_header.PaperSpaceOrthographicTopDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGBOTTOM
				_header.PaperSpaceOrthographicBottomDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGLEFT
				_header.PaperSpaceOrthographicLeftDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGRIGHT
				_header.PaperSpaceOrthographicRightDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGFRONT
				_header.PaperSpaceOrthographicFrontDOrigin = _reader.Read3BitDouble();
				//3BD: PUCSORGBACK
				_header.PaperSpaceOrthographicBackDOrigin = _reader.Read3BitDouble();
			}

			//Common:
			//3BD: INSBASE(MSPACE)
			_header.ModelSpaceInsertionBase = _reader.Read3BitDouble();
			//3BD: EXTMIN(MSPACE)
			_header.ModelSpaceExtMin = _reader.Read3BitDouble();
			//3BD: EXTMAX(MSPACE)
			_header.ModelSpaceExtMax = _reader.Read3BitDouble();
			//2RD: LIMMIN(MSPACE)
			_header.ModelSpaceLimitsMin = _reader.Read2RawDouble();
			//2RD: LIMMAX(MSPACE)
			_header.ModelSpaceLimitsMax = _reader.Read2RawDouble();
			//BD: ELEVATION(MSPACE)
			_header.Elevation = _reader.ReadBitDouble();
			//3BD: UCSORG(MSPACE)
			_header.ModelSpaceOrigin = _reader.Read3BitDouble();
			//3BD: UCSXDIR(MSPACE)
			_header.ModelSpaceXAxis = _reader.Read3BitDouble();
			//3BD: UCSYDIR(MSPACE)
			_header.ModelSpaceYAxis = _reader.Read3BitDouble();

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
				_header.ModelSpaceOrthographicTopDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGBOTTOM
				_header.ModelSpaceOrthographicBottomDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGLEFT
				_header.ModelSpaceOrthographicLeftDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGRIGHT
				_header.ModelSpaceOrthographicRightDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGFRONT
				_header.ModelSpaceOrthographicFrontDOrigin = _reader.Read3BitDouble();
				//3BD: UCSORGBACK
				_header.ModelSpaceOrthographicBackDOrigin = _reader.Read3BitDouble();

				//TV : DIMPOST
				_header.DimensionPostFix = _reader.ReadVariableText();
				//TV : DIMAPOST
				_header.DimensionAlternateDimensioningSuffix = _reader.ReadVariableText();
			}

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B: DIMTOL
				_header.DimensionGenerateTolerances = _reader.ReadBit();
				//B : DIMLIM
				_header.DimensionLimitsGeneration = _reader.ReadBit();
				//B : DIMTIH
				_header.DimensionTextInsideHorizontal = _reader.ReadBit();
				//B : DIMTOH
				_header.DimensionTextOutsideHorizontal = _reader.ReadBit();
				//B : DIMSE1
				_header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSE2
				_header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//B : DIMALT
				_header.DimensionAlternateUnitDimensioning = _reader.ReadBit();
				//B : DIMTOFL
				_header.DimensionTextOutsideExtensions = _reader.ReadBit();
				//B : DIMSAH
				_header.DimensionSeparateArrowBlocks = _reader.ReadBit();
				//B : DIMTIX
				_header.DimensionTextInsideExtensions = _reader.ReadBit();
				//B : DIMSOXD
				_header.DimensionSuppressOutsideExtensions = _reader.ReadBit();
				//RC : DIMALTD
				_header.DimensionAlternateUnitDecimalPlaces = (short)_reader.ReadRawChar();
				//RC : DIMZIN
				_header.DimensionZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//B : DIMSD1
				_header.DimensionSuppressFirstDimensionLine = _reader.ReadBit();
				//B : DIMSD2
				_header.DimensionSuppressSecondDimensionLine = _reader.ReadBit();
				//RC : DIMTOLJ
				_header.DimensionToleranceAlignment = (Tables.ToleranceAlignment)_reader.ReadRawChar();
				//RC : DIMJUST
				_header.DimensionTextHorizontalAlignment = (Tables.DimensionTextHorizontalAlignment)_reader.ReadRawChar();
				//RC : DIMFIT
				_header.DimensionFit = (short)_reader.ReadRawChar();
				//B : DIMUPT
				_header.DimensionCursorUpdate = _reader.ReadBit();
				//RC : DIMTZIN
				_header.DimensionToleranceZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC: DIMALTZ
				_header.DimensionAlternateUnitZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC : DIMALTTZ
				_header.DimensionAlternateUnitToleranceZeroHandling = (Tables.ZeroHandling)_reader.ReadRawChar();
				//RC : DIMTAD
				_header.DimensionTextVerticalAlignment = (Tables.DimensionTextVerticalAlignment)_reader.ReadRawChar();
				//BS : DIMUNIT
				_header.DimensionUnit = _reader.ReadBitShort();
				//BS : DIMAUNIT
				_header.DimensionAngularDimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMDEC
				_header.DimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMTDEC
				_header.DimensionToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMALTU
				_header.DimensionAlternateUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMALTTD
				_header.DimensionAlternateUnitToleranceDecimalPlaces = _reader.ReadBitShort();
				//H : DIMTXSTY(hard pointer)
				objectPointers.DIMTXSTY = _reader.HandleReference();
			}

			//Common:
			//BD: DIMSCALE
			_header.DimensionScaleFactor = _reader.ReadBitDouble();
			//BD : DIMASZ
			_header.DimensionArrowSize = _reader.ReadBitDouble();
			//BD : DIMEXO
			_header.DimensionExtensionLineOffset = _reader.ReadBitDouble();
			//BD : DIMDLI
			_header.DimensionLineIncrement = _reader.ReadBitDouble();
			//BD : DIMEXE
			_header.DimensionExtensionLineExtension = _reader.ReadBitDouble();
			//BD : DIMRND
			_header.DimensionRounding = _reader.ReadBitDouble();
			//BD : DIMDLE
			_header.DimensionLineExtension = _reader.ReadBitDouble();
			//BD : DIMTP
			_header.DimensionPlusTolerance = _reader.ReadBitDouble();
			//BD : DIMTM
			_header.DimensionMinusTolerance = _reader.ReadBitDouble();

			//R2007 + Only:
			if (R2007Plus)
			{
				//BD: DIMFXL
				_header.DimensionFixedExtensionLineLength = _reader.ReadBitDouble();
				//BD : DIMJOGANG
				var dimensionJoggedRadiusDimensionTransverseSegmentAngle = _reader.ReadBitDouble();
				var rounded = Math.Round(dimensionJoggedRadiusDimensionTransverseSegmentAngle, 6);
				if (rounded > MathHelper.DegToRad(5) && rounded < MathHelper.HalfPI)
				{
					_header.DimensionJoggedRadiusDimensionTransverseSegmentAngle = dimensionJoggedRadiusDimensionTransverseSegmentAngle;
				}
				//BS : DIMTFILL
				_header.DimensionTextBackgroundFillMode = (Tables.DimensionTextBackgroundFillMode)_reader.ReadBitShort();
				//CMC : DIMTFILLCLR
				_header.DimensionTextBackgroundColor = _reader.ReadCmColor();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//B: DIMTOL
				_header.DimensionGenerateTolerances = _reader.ReadBit();
				//B : DIMLIM
				_header.DimensionLimitsGeneration = _reader.ReadBit();
				//B : DIMTIH
				_header.DimensionTextInsideHorizontal = _reader.ReadBit();
				//B : DIMTOH
				_header.DimensionTextOutsideHorizontal = _reader.ReadBit();
				//B : DIMSE1
				_header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSE2
				_header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//BS : DIMTAD
				_header.DimensionTextVerticalAlignment = (Tables.DimensionTextVerticalAlignment)(char)_reader.ReadBitShort();
				//BS : DIMZIN
				_header.DimensionZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS : DIMAZIN
				_header.DimensionAngularZeroHandling = (Tables.AngularZeroHandling)_reader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//BS: DIMARCSYM
				_header.DimensionArcLengthSymbolPosition = (Tables.ArcLengthSymbolPosition)_reader.ReadBitShort();
			}

			//Common:
			//BD: DIMTXT
			_header.DimensionTextHeight = _reader.ReadBitDouble();
			//BD : DIMCEN
			_header.DimensionCenterMarkSize = _reader.ReadBitDouble();
			//BD: DIMTSZ
			_header.DimensionTickSize = _reader.ReadBitDouble();
			//BD : DIMALTF
			_header.DimensionAlternateUnitScaleFactor = _reader.ReadBitDouble();
			//BD : DIMLFAC
			_header.DimensionLinearScaleFactor = _reader.ReadBitDouble();
			//BD : DIMTVP
			_header.DimensionTextVerticalPosition = _reader.ReadBitDouble();
			//BD : DIMTFAC
			_header.DimensionToleranceScaleFactor = _reader.ReadBitDouble();
			//BD : DIMGAP
			_header.DimensionLineGap = _reader.ReadBitDouble();

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//T: DIMPOST
				_header.DimensionPostFix = _reader.ReadVariableText();
				//T : DIMAPOST
				_header.DimensionAlternateDimensioningSuffix = _reader.ReadVariableText();
				//T : DIMBLK
				_header.DimensionBlockName = _reader.ReadVariableText();
				//T : DIMBLK1
				_header.DimensionBlockNameFirst = _reader.ReadVariableText();
				//T : DIMBLK2
				_header.DimensionBlockNameSecond = _reader.ReadVariableText();
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//BD: DIMALTRND
				_header.DimensionAlternateUnitRounding = _reader.ReadBitDouble();
				//B : DIMALT
				_header.DimensionAlternateUnitDimensioning = _reader.ReadBit();
				//BS : DIMALTD
				_header.DimensionAlternateUnitDecimalPlaces = (short)(char)_reader.ReadBitShort();
				//B : DIMTOFL
				_header.DimensionTextOutsideExtensions = _reader.ReadBit();
				//B : DIMSAH
				_header.DimensionSeparateArrowBlocks = _reader.ReadBit();
				//B : DIMTIX
				_header.DimensionTextInsideExtensions = _reader.ReadBit();
				//B : DIMSOXD
				_header.DimensionSuppressOutsideExtensions = _reader.ReadBit();
			}

			//Common:
			//CMC: DIMCLRD
			_header.DimensionLineColor = _reader.ReadCmColor();
			//CMC : DIMCLRE
			_header.DimensionExtensionLineColor = _reader.ReadCmColor();
			//CMC : DIMCLRT
			_header.DimensionTextColor = _reader.ReadCmColor();

			//R2000 + Only:
			if (R2000Plus)
			{
				//BS: DIMADEC
				_header.DimensionAngularDimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMDEC
				_header.DimensionDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMTDEC
				_header.DimensionToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMALTU
				_header.DimensionAlternateUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMALTTD
				_header.DimensionAlternateUnitToleranceDecimalPlaces = _reader.ReadBitShort();
				//BS : DIMAUNIT
				_header.DimensionAngularUnit = (AngularUnitFormat)_reader.ReadBitShort();
				//BS : DIMFRAC
				_header.DimensionFractionFormat = (Tables.FractionFormat)_reader.ReadBitShort();
				//BS : DIMLUNIT
				_header.DimensionLinearUnitFormat = (LinearUnitFormat)_reader.ReadBitShort();
				//BS : DIMDSEP
				_header.DimensionDecimalSeparator = (char)_reader.ReadBitShort();
				//BS : DIMTMOVE
				_header.DimensionTextMovement = (Tables.TextMovement)_reader.ReadBitShort();
				//BS : DIMJUST
				_header.DimensionTextHorizontalAlignment = (Tables.DimensionTextHorizontalAlignment)(char)_reader.ReadBitShort();
				//B : DIMSD1
				_header.DimensionSuppressFirstExtensionLine = _reader.ReadBit();
				//B : DIMSD2
				_header.DimensionSuppressSecondExtensionLine = _reader.ReadBit();
				//BS : DIMTOLJ
				_header.DimensionToleranceAlignment = (Tables.ToleranceAlignment)(char)_reader.ReadBitShort();
				//BS : DIMTZIN
				_header.DimensionToleranceZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS: DIMALTZ
				_header.DimensionAlternateUnitZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//BS : DIMALTTZ
				_header.DimensionAlternateUnitToleranceZeroHandling = (Tables.ZeroHandling)(char)_reader.ReadBitShort();
				//B : DIMUPT
				_header.DimensionCursorUpdate = _reader.ReadBit();
				//BS : DIMATFIT
				_header.DimensionDimensionTextArrowFit = (Tables.TextArrowFitType)_reader.ReadBitShort();
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//B: DIMFXLON
				_header.DimensionIsExtensionLineLengthFixed = _reader.ReadBit();
			}

			//R2010 + Only:
			if (R2010Plus)
			{
				//B: DIMTXTDIRECTION
				_header.DimensionTextDirection = _reader.ReadBit() ? Tables.TextDirection.RightToLeft : Tables.TextDirection.LeftToRight;
				//BD : DIMALTMZF
				_header.DimensionAltMzf = _reader.ReadBitDouble();
				//T : DIMALTMZS
				_header.DimensionAltMzs = _reader.ReadVariableText();
				//BD : DIMMZF
				_header.DimensionMzf = _reader.ReadBitDouble();
				//T : DIMMZS
				_header.DimensionMzs = _reader.ReadVariableText();
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
				_header.DimensionLineWeight = (LineWeightType)_reader.ReadBitShort();
				//BS : DIMLWE
				_header.ExtensionLineWeight = (LineWeightType)_reader.ReadBitShort();
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
				_header.StackedTextAlignment = _reader.ReadBitShort();
				//BS: TSTACKSIZE, default = 70(not present in DXF)
				_header.StackedTextSizePercentage = _reader.ReadBitShort();

				//TV: HYPERLINKBASE
				_header.HyperLinkBase = _reader.ReadVariableText();
				//TV : STYLESHEET
				_header.StyleSheetName = _reader.ReadVariableText();

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
				_header.CurrentEntityLineWeight = (LineWeightType)(flags & 0x1F);
				//ENDCAPS Flags & 0x0060
				_header.EndCaps = (short)(flags & 0x60);
				//JOINSTYLE Flags & 0x0180
				_header.JoinStyle = (short)(flags & 0x180);
				//LWDISPLAY!(Flags & 0x0200)
				_header.DisplayLineWeight = (flags & 0x200) == 1;
				//XEDIT!(Flags & 0x0400)
				_header.XEdit = (short)(flags & 0x400) == 1;
				//EXTNAMES Flags & 0x0800
				_header.ExtendedNames = (flags & 0x800) == 1;
				//PSTYLEMODE Flags & 0x2000
				_header.PlotStyleMode = (short)(flags & 0x2000);
				//OLESTARTUP Flags & 0x4000
				_header.LoadOLEObject = (flags & 0x4000) == 1;

				//BS: INSUNITS
				_header.InsUnits = (UnitsType)_reader.ReadBitShort();
				//BS : CEPSNTYPE
				_header.CurrentEntityPlotStyle = (EntityPlotStyleType)_reader.ReadBitShort();

				if (_header.CurrentEntityPlotStyle == EntityPlotStyleType.ByObjectId)
				{
					//H: CPSNID(present only if CEPSNTYPE == 3) (hard pointer)
					objectPointers.CPSNID = _reader.HandleReference();
				}

				//TV: FINGERPRINTGUID
				_header.FingerPrintGuid = _reader.ReadVariableText();
				//TV : VERSIONGUID
				_header.VersionGuid = _reader.ReadVariableText();
			}

			//R2004 +:
			if (R2004Plus)
			{
				//RC: SORTENTS
				_header.EntitySortingFlags = (ObjectSortingFlags)_reader.ReadByte();
				//RC : INDEXCTL
				_header.IndexCreationFlags = (IndexCreationFlags)_reader.ReadByte();
				//RC : HIDETEXT
				_header.HideText = _reader.ReadByte();
				//RC : XCLIPFRAME, before R2010 the value can be 0 or 1 only.
				_header.ExternalReferenceClippingBoundaryType = (XClipFrameType)_reader.ReadByte();
				//RC : DIMASSOC
				_header.DimensionAssociativity = (DimensionAssociation)_reader.ReadByte();
				//RC : HALOGAP
				_header.HaloGapPercentage = _reader.ReadByte();
				//BS : OBSCUREDCOLOR
				_header.ObscuredColor = new Color(_reader.ReadBitShort());
				//BS : INTERSECTIONCOLOR
				_header.InterfereColor = new Color(_reader.ReadBitShort());
				//RC : OBSCUREDLTYPE
				_header.ObscuredType = _reader.ReadByte();
				//RC: INTERSECTIONDISPLAY
				_header.IntersectionDisplay = _reader.ReadByte();

				//TV : PROJECTNAME
				_header.ProjectName = _reader.ReadVariableText();
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
				_header.CameraDisplayObjects = _reader.ReadBit();

				//BL : unknown
				_reader.ReadBitLong();
				//BL : unknown
				_reader.ReadBitLong();
				//BD : unknown
				_reader.ReadBitDouble();

				//BD : STEPSPERSEC
				var stepsPerSecond = _reader.ReadBitDouble();
				if (stepsPerSecond >= 1 && stepsPerSecond <= 30)
				{
					_header.StepsPerSecond = stepsPerSecond;
				}
				//BD : STEPSIZE
				_header.StepSize = _reader.ReadBitDouble();
				//BD : 3DDWFPREC
				_header.Dw3DPrecision = _reader.ReadBitDouble();
				//BD : LENSLENGTH
				_header.LensLength = _reader.ReadBitDouble();
				//BD : CAMERAHEIGHT
				_header.CameraHeight = _reader.ReadBitDouble();
				//RC : SOLIDHIST
				_header.SolidsRetainHistory = _reader.ReadRawChar();
				//RC : SHOWHIST
				_header.ShowSolidsHistory = _reader.ReadRawChar();
				//BD : PSOLWIDTH
				_header.SweptSolidWidth = _reader.ReadBitDouble();
				//BD : PSOLHEIGHT
				_header.SweptSolidHeight = _reader.ReadBitDouble();
				//BD : LOFTANG1
				_header.DraftAngleFirstCrossSection = _reader.ReadBitDouble();
				//BD : LOFTANG2
				_header.DraftAngleSecondCrossSection = _reader.ReadBitDouble();
				//BD : LOFTMAG1
				_header.DraftMagnitudeFirstCrossSection = _reader.ReadBitDouble();
				//BD : LOFTMAG2
				_header.DraftMagnitudeSecondCrossSection = _reader.ReadBitDouble();
				//BS : LOFTPARAM
				_header.SolidLoftedShape = _reader.ReadBitShort();
				//RC : LOFTNORMALS
				_header.LoftedObjectNormals = _reader.ReadRawChar();
				//BD : LATITUDE
				_header.Latitude = _reader.ReadBitDouble();
				//BD : LONGITUDE
				_header.Longitude = _reader.ReadBitDouble();
				//BD : NORTHDIRECTION
				_header.NorthDirection = _reader.ReadBitDouble();
				//BL : TIMEZONE
				_header.TimeZone = _reader.ReadBitLong();
				//RC : LIGHTGLYPHDISPLAY
				_header.DisplayLightGlyphs = _reader.ReadRawChar();
				//RC : TILEMODELIGHTSYNCH	??
				_reader.ReadRawChar();
				//RC : DWFFRAME
				_header.DwgUnderlayFramesVisibility = _reader.ReadRawChar();
				//RC : DGNFRAME
				_header.DgnUnderlayFramesVisibility = _reader.ReadRawChar();

				//B : unknown
				_reader.ReadBit();

				//CMC : INTERFERECOLOR
				_header.InterfereColor = _reader.ReadCmColor();

				//H : INTERFEREOBJVS(hard pointer)
				objectPointers.INTERFEREOBJVS = _reader.HandleReference();
				//H: INTERFEREVPVS(hard pointer)
				objectPointers.INTERFEREVPVS = _reader.HandleReference();
				//H: DRAGVS(hard pointer)
				objectPointers.DRAGVS = _reader.HandleReference();

				//RC: CSHADOW
				_header.ShadowMode = (ShadowMode)_reader.ReadByte();
				//BD : SHADOWPLANELOCATION
				_header.ShadowPlaneLocation = _reader.ReadBitDouble();
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
		}
	}
}