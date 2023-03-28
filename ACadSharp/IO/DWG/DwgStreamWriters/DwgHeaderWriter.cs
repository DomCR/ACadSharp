using ACadSharp.Header;
using ACadSharp.Objects;
using CSUtilities.IO;
using CSUtilities.Text;
using System.IO;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.Header;

		private MemoryStream _msmain;

		private IDwgStreamWriter _startWriter;
		private IDwgStreamWriter _writer;

		private CadDocument _document;
		private CadHeader _header;

		public DwgHeaderWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._document = document;
			this._header = document.Header;

			this._startWriter = DwgStreamWriterBase.GetStreamHandler(_version, stream, TextEncoding.Windows1252());

			this._msmain = new MemoryStream();
			this._writer = DwgStreamWriterBase.GetStreamHandler(_version, this._msmain, TextEncoding.Windows1252());
		}

		public void Write()
		{
			//+R2007 Only:
			if (this.R2007Plus)
			{
				//Setup the writers
				this._writer = DwgStreamWriterBase.GetMergedWriter(_version, this._msmain, TextEncoding.Windows1252());
				this._writer.SavePositonForSize();
			}

			//R2013+:
			if (this.R2013Plus)
			{
				//BLL : Variabele REQUIREDVERSIONS, default value 0, read only.
				this._writer.WriteBitLongLong(0);
			}

			//Common:
			//BD : Unknown, default value 412148564080.0
			this._writer.WriteBitDouble(412148564080.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);

			//TV: Unknown text string, default "m"
			this._writer.WriteVariableText("m");
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);

			//BL : Unknown long, default value 24L
			this._writer.WriteBitLong(24);
			//BL: Unknown long, default value 0L;
			this._writer.WriteBitLong(0);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//BS : Unknown short, default value 0
				this._writer.WriteBitShort(0);
			}

			//Pre-2004 Only:
			if (this.R2004Pre)
			{
				//H : Handle of the current viewport entity header (hard pointer)
				this._writer.HandleReference(0);
			}

			//Common:
			//B: DIMASO
			this._writer.WriteBit(this._header.AssociatedDimensions);
			//B: DIMSHO
			this._writer.WriteBit(this._header.UpdateDimensionsWhileDragging);

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//B : DIMSAV Undocumented.
				this._writer.WriteBit(this._header.DIMSAV);
			}

			//Common:
			//B: PLINEGEN
			this._writer.WriteBit(this._header.PolylineLineTypeGeneration);
			//B : ORTHOMODE
			this._writer.WriteBit(this._header.OrthoMode);
			//B: REGENMODE
			this._writer.WriteBit(this._header.RegenerationMode);
			//B : FILLMODE
			this._writer.WriteBit(this._header.FillMode);
			//B : QTEXTMODE
			this._writer.WriteBit(this._header.QuickTextMode);
			//B : PSLTSCALE
			this._writer.WriteBit(this._header.PaperSpaceLineTypeScaling == SpaceLineTypeScaling.Normal);
			//B : LIMCHECK
			this._writer.WriteBit(this._header.LimitCheckingOn);

			//R13-R14 Only (stored in registry from R15 onwards):
			if (this.R13_14Only)
				//B : BLIPMODE
				this._writer.WriteBit(this._header.BlipMode);

			//R2004+:
			if (this.R2004Plus)
				//B : Undocumented
				this._writer.WriteBit(false);

			//Common:
			//B: USRTIMER(User timer on / off).
			this._writer.WriteBit(this._header.UserTimer);
			//B : SKPOLY
			this._writer.WriteBit(this._header.SketchPolylines);
			//B : ANGDIR
			this._writer.WriteBit(this._header.AngularDirection != Types.Units.AngularDirection.CounterClockWise);
			//B : SPLFRAME
			this._writer.WriteBit(this._header.ShowSplineControlPoints);

			//R13-R14 Only (stored in registry from R15 onwards):
			if (this.R13_14Only)
			{
				//B : ATTREQ
				this._writer.WriteBit(false);
				//B : ATTDIA
				this._writer.WriteBit(false);
			}

			//Common:
			//B: MIRRTEXT
			this._writer.WriteBit(this._header.MirrorText);
			//B : WORLDVIEW
			this._writer.WriteBit(this._header.WorldView);

			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				//B: WIREFRAME Undocumented.
				this._writer.WriteBit(false);
			}

			//Common:
			//B: TILEMODE
			this._writer.WriteBit(this._header.ShowModelSpace);
			//B : PLIMCHECK
			this._writer.WriteBit(this._header.PaperSpaceLimitsChecking);
			//B : VISRETAIN
			this._writer.WriteBit(this._header.RetainXRefDependentVisibilitySettings);

			//R13 - R14 Only(stored in registry from R15 onwards):
			if (this.R13_14Only)
			{
				//B : DELOBJ
				this._writer.WriteBit(false);
			}

			//Common:
			//B: DISPSILH
			this._writer.WriteBit(this._header.DisplaySilhouetteCurves);
			//B : PELLIPSE(not present in DXF)
			this._writer.WriteBit(this._header.CreateEllipseAsPolyline);
			//BS: PROXYGRAPHICS
			this._writer.WriteBitShort((short)(this._header.ProxyGraphics ? 1 : 0));

			//R13-R14 Only (stored in registry from R15 onwards):
			if (this.R13_14Only)
			{
				//BS : DRAGMODE
				this._writer.WriteBitShort(0);
			}

			//Common:
			//BS: TREEDEPTH
			this._writer.WriteBitShort(this._header.SpatialIndexMaxTreeDepth);
			//BS : LUNITS
			this._writer.WriteBitShort((short)this._header.LinearUnitFormat);
			//BS : LUPREC
			this._writer.WriteBitShort(this._header.LinearUnitPrecision);
			//BS : AUNITS
			this._writer.WriteBitShort((short)this._header.AngularUnit);
			//BS : AUPREC
			this._writer.WriteBitShort(this._header.AngularUnitPrecision);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (this.R13_14Only)
				//BS: OSMODE
				this._writer.WriteBitShort((short)this._header.ObjectSnapMode);

			//Common:
			//BS: ATTMODE
			this._writer.WriteBitShort((short)this._header.AttributeVisibility);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (this.R13_14Only)
			{
				//BS: COORDS
				this._writer.WriteBitShort(0);
			}

			//Common:
			//BS: PDMODE
			this._writer.WriteBitShort(this._header.PointDisplayMode);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (this.R13_14Only)
			{
				//BS: PICKSTYLE
				this._writer.WriteBitShort(0);
			}

			//R2004 +:
			if (this.R2004Plus)
			{
				//BL: Unknown
				this._writer.WriteBitLong(0);
				//BL: Unknown
				this._writer.WriteBitLong(0);
				//BL: Unknown
				this._writer.WriteBitLong(0);
			}

			//Common:
			//BS : USERI1
			this._writer.WriteBitShort(this._header.UserShort1);
			//BS : USERI2
			this._writer.WriteBitShort(this._header.UserShort2);
			//BS : USERI3
			this._writer.WriteBitShort(this._header.UserShort3);
			//BS : USERI4
			this._writer.WriteBitShort(this._header.UserShort4);
			//BS : USERI5
			this._writer.WriteBitShort(this._header.UserShort5);

			//BS: SPLINESEGS
			this._writer.WriteBitShort(this._header.NumberOfSplineSegments);
			//BS : SURFU
			this._writer.WriteBitShort(this._header.SurfaceDensityU);
			//BS : SURFV
			this._writer.WriteBitShort(this._header.SurfaceDensityV);
			//BS : SURFTYPE
			this._writer.WriteBitShort(this._header.SurfaceType);
			//BS : SURFTAB1
			this._writer.WriteBitShort(this._header.SurfaceMeshTabulationCount1);
			//BS : SURFTAB2
			this._writer.WriteBitShort(this._header.SurfaceMeshTabulationCount2);
			//BS : SPLINETYPE
			this._writer.WriteBitShort((short)this._header.SplineType);
			//BS : SHADEDGE
			this._writer.WriteBitShort((short)this._header.ShadeEdge);
			//BS : SHADEDIF
			this._writer.WriteBitShort(this._header.ShadeDiffuseToAmbientPercentage);
			//BS: UNITMODE
			this._writer.WriteBitShort(this._header.UnitMode);
			//BS : MAXACTVP
			this._writer.WriteBitShort(this._header.MaxViewportCount);
			//BS : ISOLINES
			this._writer.WriteBitShort(this._header.SurfaceIsolineCount);
			//BS : CMLJUST
			this._writer.WriteBitShort((short)this._header.CurrentMultilineJustification);
			//BS : TEXTQLTY
			this._writer.WriteBitShort(this._header.TextQuality);
			//BD : LTSCALE
			this._writer.WriteBitDouble(this._header.LineTypeScale);
			//BD : TEXTSIZE
			this._writer.WriteBitDouble(this._header.TextHeightDefault);
			//BD : TRACEWID
			this._writer.WriteBitDouble(this._header.TraceWidthDefault);
			//BD : SKETCHINC
			this._writer.WriteBitDouble(this._header.SketchIncrement);
			//BD : FILLETRAD
			this._writer.WriteBitDouble(this._header.FilletRadius);
			//BD : THICKNESS
			this._writer.WriteBitDouble(this._header.ThicknessDefault);
			//BD : ANGBASE
			this._writer.WriteBitDouble(this._header.AngleBase);
			//BD : PDSIZE
			this._writer.WriteBitDouble(this._header.PointDisplaySize);
			//BD : PLINEWID
			this._writer.WriteBitDouble(this._header.PolylineWidthDefault);
			//BD : USERR1
			this._writer.WriteBitDouble(this._header.UserDouble1);
			//BD : USERR2
			this._writer.WriteBitDouble(this._header.UserDouble2);
			//BD : USERR3
			this._writer.WriteBitDouble(this._header.UserDouble3);
			//BD : USERR4
			this._writer.WriteBitDouble(this._header.UserDouble4);
			//BD : USERR5
			this._writer.WriteBitDouble(this._header.UserDouble5);
			//BD : CHAMFERA
			this._writer.WriteBitDouble(this._header.ChamferDistance1);
			//BD : CHAMFERB
			this._writer.WriteBitDouble(this._header.ChamferDistance2);
			//BD : CHAMFERC
			this._writer.WriteBitDouble(this._header.ChamferLength);
			//BD : CHAMFERD
			this._writer.WriteBitDouble(this._header.ChamferAngle);
			//BD : FACETRES
			this._writer.WriteBitDouble(this._header.FacetResolution);
			//BD : CMLSCALE
			this._writer.WriteBitDouble(this._header.CurrentMultilineScale);
			//BD : CELTSCALE
			this._writer.WriteBitDouble(this._header.CurrentEntityLinetypeScale);

			//TV: MENUNAME
			this._writer.WriteVariableText(this._header.MenuFileName);

			//Common:
			//BL: TDCREATE(Julian day)
			//BL: TDCREATE(Milliseconds into the day)
			this._writer.WriteDateTime(this._header.CreateDateTime);
			//BL: TDUPDATE(Julian day)
			//BL: TDUPDATE(Milliseconds into the day)
			this._writer.WriteDateTime(this._header.UpdateDateTime);

			//R2004 +:
			if (this.R2004Plus)
			{
				//BL : Unknown
				this._writer.WriteBitLong(0);
				//BL : Unknown
				this._writer.WriteBitLong(0);
				//BL : Unknown
				this._writer.WriteBitLong(0);
			}

			//Common:
			//BL: TDINDWG(Days)
			//BL: TDINDWG(Milliseconds into the day)
			this._writer.WriteTimeSpan(this._header.TotalEditingTime);
			//BL: TDUSRTIMER(Days)
			//BL: TDUSRTIMER(Milliseconds into the day)
			this._writer.WriteTimeSpan(this._header.UserElapsedTimeSpan);

			//CMC : CECOLOR
			this._writer.WriteCmColor(this._header.CurrentEntityColor);

			//H : HANDSEED The next handle, with an 8-bit length specifier preceding the handle
			//bytes (standard hex handle form) (code 0). The HANDSEED is not part of the handle
			//stream, but of the normal data stream (relevant for R21 and later).
			this._writer.Main.HandleReference(this._header.HandleSeed);

			//H : CLAYER (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.CurrentLayer);

			//H: TEXTSTYLE(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.CurrentTextStyle);

			//H: CELTYPE(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.CurrentLineType);

			//R2007 + Only:
			if (this.R2007Plus)
			{
				//H: CMATERIAL(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//Common:
			//H: DIMSTYLE (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.DimensionStyleOverrides);

			//H: CMLSTYLE (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);

			//R2000+ Only:
			if (this.R2000Plus)
			{
				//BD: PSVPSCALE
				this._writer.WriteBitDouble(this._header.ViewportDefaultViewScaleFactor);
			}

			//Common:
			//3BD: INSBASE(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceInsertionBase);
			//3BD: EXTMIN(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceExtMin);
			//3BD: EXTMAX(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceExtMax);
			//2RD: LIMMIN(PSPACE)
			this._writer.Write2RawDouble(this._header.PaperSpaceLimitsMin);
			//2RD: LIMMAX(PSPACE)
			this._writer.Write2RawDouble(this._header.PaperSpaceLimitsMax);
			//BD: ELEVATION(PSPACE)
			this._writer.WriteBitDouble(this._header.PaperSpaceElevation);
			//3BD: UCSORG(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceUcsOrigin);
			//3BD: UCSXDIR(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceUcsXAxis);
			//3BD: UCSYDIR(PSPACE)
			this._writer.Write3BitDouble(this._header.PaperSpaceUcsYAxis);

			//H: UCSNAME (PSPACE) (hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.PaperSpaceUcs);

			//R2000+ Only:
			if (this.R2000Plus)
			{
				//H : PUCSORTHOREF (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//BS : PUCSORTHOVIEW	??
				this._writer.WriteBitShort(0);

				//H: PUCSBASE(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//3BD: PUCSORGTOP
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicTopDOrigin);
				//3BD: PUCSORGBOTTOM
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicBottomDOrigin);
				//3BD: PUCSORGLEFT
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicLeftDOrigin);
				//3BD: PUCSORGRIGHT
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicRightDOrigin);
				//3BD: PUCSORGFRONT
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicFrontDOrigin);
				//3BD: PUCSORGBACK
				this._writer.Write3BitDouble(this._header.PaperSpaceOrthographicBackDOrigin);
			}

			//Common:
			//3BD: INSBASE(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceInsertionBase);
			//3BD: EXTMIN(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceExtMin);
			//3BD: EXTMAX(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceExtMax);
			//2RD: LIMMIN(MSPACE)
			this._writer.Write2RawDouble(this._header.ModelSpaceLimitsMin);
			//2RD: LIMMAX(MSPACE)
			this._writer.Write2RawDouble(this._header.ModelSpaceLimitsMax);
			//BD: ELEVATION(MSPACE)
			this._writer.WriteBitDouble(this._header.Elevation);
			//3BD: UCSORG(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceOrigin);
			//3BD: UCSXDIR(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceXAxis);
			//3BD: UCSYDIR(MSPACE)
			this._writer.Write3BitDouble(this._header.ModelSpaceYAxis);

			//H: UCSNAME(MSPACE)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.ModelSpace);

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//H: UCSORTHOREF(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//BS: UCSORTHOVIEW	??
				this._writer.WriteBitShort(0);

				//H : UCSBASE(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//3BD: UCSORGTOP
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicTopDOrigin);
				//3BD: UCSORGBOTTOM
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicBottomDOrigin);
				//3BD: UCSORGLEFT
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicLeftDOrigin);
				//3BD: UCSORGRIGHT
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicRightDOrigin);
				//3BD: UCSORGFRONT
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicFrontDOrigin);
				//3BD: UCSORGBACK
				this._writer.Write3BitDouble(this._header.ModelSpaceOrthographicBackDOrigin);

				//TV : DIMPOST
				this._writer.WriteVariableText(this._header.DimensionPostFix);
				//TV : DIMAPOST
				this._writer.WriteVariableText(this._header.DimensionAlternateDimensioningSuffix);
			}

			//R13-R14 Only:
			if (this.R13_14Only)
			{
				//B: DIMTOL
				this._writer.WriteBit(this._header.DimensionGenerateTolerances);
				//B : DIMLIM
				this._writer.WriteBit(this._header.DimensionLimitsGeneration);
				//B : DIMTIH
				this._writer.WriteBit(this._header.DimensionTextInsideHorizontal);
				//B : DIMTOH
				this._writer.WriteBit(this._header.DimensionTextOutsideHorizontal);
				//B : DIMSE1
				this._writer.WriteBit(this._header.DimensionSuppressFirstExtensionLine);
				//B : DIMSE2
				this._writer.WriteBit(this._header.DimensionSuppressSecondExtensionLine);
				//B : DIMALT
				this._writer.WriteBit(this._header.DimensionAlternateUnitDimensioning);
				//B : DIMTOFL
				this._writer.WriteBit(this._header.DimensionTextOutsideExtensions);
				//B : DIMSAH
				this._writer.WriteBit(this._header.DimensionSeparateArrowBlocks);
				//B : DIMTIX
				this._writer.WriteBit(this._header.DimensionTextInsideExtensions);
				//B : DIMSOXD
				this._writer.WriteBit(this._header.DimensionSuppressOutsideExtensions);
				//RC : DIMALTD
				this._writer.WriteByte((byte)this._header.DimensionAlternateUnitDecimalPlaces);
				//RC : DIMZIN
				this._writer.WriteByte((byte)this._header.DimensionZeroHandling);
				//B : DIMSD1
				this._writer.WriteBit(this._header.DimensionSuppressFirstDimensionLine);
				//B : DIMSD2
				this._writer.WriteBit(this._header.DimensionSuppressSecondDimensionLine);
				//RC : DIMTOLJ
				this._writer.WriteByte((byte)this._header.DimensionToleranceAlignment);
				//RC : DIMJUST
				this._writer.WriteByte((byte)this._header.DimensionTextHorizontalAlignment);
				//RC : DIMFIT
				this._writer.WriteByte((byte)this._header.DimensionFit);
				//B : DIMUPT
				this._writer.WriteBit(this._header.DimensionCursorUpdate);
				//RC : DIMTZIN
				this._writer.WriteByte((byte)this._header.DimensionToleranceZeroHandling);
				//RC: DIMALTZ
				this._writer.WriteByte((byte)this._header.DimensionAlternateUnitZeroHandling);
				//RC : DIMALTTZ
				this._writer.WriteByte((byte)this._header.DimensionAlternateUnitToleranceZeroHandling);
				//RC : DIMTAD
				this._writer.WriteByte((byte)this._header.DimensionTextVerticalAlignment);
				//BS : DIMUNIT
				this._writer.WriteBitShort(this._header.DimensionUnit);
				//BS : DIMAUNIT
				this._writer.WriteBitShort(this._header.DimensionAngularDimensionDecimalPlaces);
				//BS : DIMDEC
				this._writer.WriteBitShort(this._header.DimensionDecimalPlaces);
				//BS : DIMTDEC
				this._writer.WriteBitShort(this._header.DimensionToleranceDecimalPlaces);
				//BS : DIMALTU
				this._writer.WriteBitShort((short)this._header.DimensionAlternateUnitFormat);
				//BS : DIMALTTD
				this._writer.WriteBitShort(this._header.DimensionAlternateUnitToleranceDecimalPlaces);

				//H : DIMTXSTY(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, this._header.DimensionStyleOverrides);
			}

			//Common:
			//BD: DIMSCALE
			this._writer.WriteBitDouble(this._header.DimensionScaleFactor);
			//BD : DIMASZ
			this._writer.WriteBitDouble(this._header.DimensionArrowSize);
			//BD : DIMEXO
			this._writer.WriteBitDouble(this._header.DimensionExtensionLineOffset);
			//BD : DIMDLI
			this._writer.WriteBitDouble(this._header.DimensionLineIncrement);
			//BD : DIMEXE
			this._writer.WriteBitDouble(this._header.DimensionExtensionLineExtension);
			//BD : DIMRND
			this._writer.WriteBitDouble(this._header.DimensionRounding);
			//BD : DIMDLE
			this._writer.WriteBitDouble(this._header.DimensionLineExtension);
			//BD : DIMTP
			this._writer.WriteBitDouble(this._header.DimensionPlusTolerance);
			//BD : DIMTM
			this._writer.WriteBitDouble(this._header.DimensionMinusTolerance);

			//R2007 + Only:
			if (this.R2007Plus)
			{
				//BD: DIMFXL
				this._writer.WriteBitDouble(this._header.DimensionFixedExtensionLineLength);
				//BD : DIMJOGANG
				this._writer.WriteBitDouble(this._header.DimensionJoggedRadiusDimensionTransverseSegmentAngle);
				//BS : DIMTFILL
				this._writer.WriteBitShort((short)this._header.DimensionTextBackgroundFillMode);
				//CMC : DIMTFILLCLR
				this._writer.WriteCmColor(this._header.DimensionTextBackgroundColor);
			}

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//B: DIMTOL
				this._writer.WriteBit(this._header.DimensionGenerateTolerances);
				//B : DIMLIM
				this._writer.WriteBit(this._header.DimensionLimitsGeneration);
				//B : DIMTIH
				this._writer.WriteBit(this._header.DimensionTextInsideHorizontal);
				//B : DIMTOH
				this._writer.WriteBit(this._header.DimensionTextOutsideHorizontal);
				//B : DIMSE1
				this._writer.WriteBit(this._header.DimensionSuppressFirstExtensionLine);
				//B : DIMSE2
				this._writer.WriteBit(this._header.DimensionSuppressSecondExtensionLine);
				//BS : DIMTAD
				this._writer.WriteBitShort((short)this._header.DimensionTextVerticalAlignment);
				//BS : DIMZIN
				this._writer.WriteBitShort((short)this._header.DimensionZeroHandling);
				//BS : DIMAZIN
				this._writer.WriteBitShort((short)this._header.DimensionAngularZeroHandling);
			}

			//R2007 + Only:
			if (this.R2007Plus)
			{
				//BS: DIMARCSYM
				this._writer.WriteBitShort((short)this._header.DimensionArcLengthSymbolPosition);
			}

			//Common:
			//BD: DIMTXT
			this._writer.WriteBitDouble(this._header.DimensionTextHeight);
			//BD : DIMCEN
			this._writer.WriteBitDouble(this._header.DimensionCenterMarkSize);
			//BD: DIMTSZ
			this._writer.WriteBitDouble(this._header.DimensionTickSize);
			//BD : DIMALTF
			this._writer.WriteBitDouble(this._header.DimensionAlternateUnitScaleFactor);
			//BD : DIMLFAC
			this._writer.WriteBitDouble(this._header.DimensionLinearScaleFactor);
			//BD : DIMTVP
			this._writer.WriteBitDouble(this._header.DimensionTextVerticalPosition);
			//BD : DIMTFAC
			this._writer.WriteBitDouble(this._header.DimensionToleranceScaleFactor);
			//BD : DIMGAP
			this._writer.WriteBitDouble(this._header.DimensionLineGap);

			//R13 - R14 Only:
			if (this.R13_14Only)
			{
				//T: DIMPOST
				this._writer.WriteVariableText(this._header.DimensionPostFix);
				//T : DIMAPOST
				this._writer.WriteVariableText(this._header.DimensionAlternateDimensioningSuffix);
				//T : DIMBLK
				this._writer.WriteVariableText(this._header.DimensionBlockName);
				//T : DIMBLK1
				this._writer.WriteVariableText(this._header.DimensionBlockNameFirst);
				//T : DIMBLK2
				this._writer.WriteVariableText(this._header.DimensionBlockNameSecond);
			}

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//BD: DIMALTRND
				this._writer.WriteBitDouble(this._header.DimensionAlternateUnitRounding);
				//B : DIMALT
				this._writer.WriteBit(this._header.DimensionAlternateUnitDimensioning);
				//BS : DIMALTD
				this._writer.WriteBitShort(this._header.DimensionAlternateUnitDecimalPlaces);
				//B : DIMTOFL
				this._writer.WriteBit(this._header.DimensionTextOutsideExtensions);
				//B : DIMSAH
				this._writer.WriteBit(this._header.DimensionSeparateArrowBlocks);
				//B : DIMTIX
				this._writer.WriteBit(this._header.DimensionTextInsideExtensions);
				//B : DIMSOXD
				this._writer.WriteBit(this._header.DimensionSuppressOutsideExtensions);
			}

			//Common:
			//CMC: DIMCLRD
			this._writer.WriteCmColor(this._header.DimensionLineColor);
			//CMC : DIMCLRE
			this._writer.WriteCmColor(this._header.DimensionExtensionLineColor);
			//CMC : DIMCLRT
			this._writer.WriteCmColor(this._header.DimensionTextColor);

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//BS: DIMADEC
				this._writer.WriteBitShort(this._header.DimensionAngularDimensionDecimalPlaces);
				//BS : DIMDEC
				this._writer.WriteBitShort(this._header.DimensionDecimalPlaces);
				//BS : DIMTDEC
				this._writer.WriteBitShort(this._header.DimensionToleranceDecimalPlaces);
				//BS : DIMALTU
				this._writer.WriteBitShort((short)this._header.DimensionAlternateUnitFormat);
				//BS : DIMALTTD
				this._writer.WriteBitShort(this._header.DimensionAlternateUnitToleranceDecimalPlaces);
				//BS : DIMAUNIT
				this._writer.WriteBitShort((short)this._header.DimensionAngularUnit);
				//BS : DIMFRAC
				this._writer.WriteBitShort((short)this._header.DimensionFractionFormat);
				//BS : DIMLUNIT
				this._writer.WriteBitShort((short)this._header.DimensionLinearUnitFormat);
				//BS : DIMDSEP
				this._writer.WriteBitShort((short)this._header.DimensionDecimalSeparator);
				//BS : DIMTMOVE
				this._writer.WriteBitShort((short)this._header.DimensionTextMovement);
				//BS : DIMJUST
				this._writer.WriteBitShort((short)this._header.DimensionTextHorizontalAlignment);
				//B : DIMSD1
				this._writer.WriteBit(this._header.DimensionSuppressFirstExtensionLine);
				//B : DIMSD2
				this._writer.WriteBit(this._header.DimensionSuppressSecondExtensionLine);
				//BS : DIMTOLJ
				this._writer.WriteBitShort((short)this._header.DimensionToleranceAlignment);
				//BS : DIMTZIN
				this._writer.WriteBitShort((short)this._header.DimensionToleranceZeroHandling);
				//BS: DIMALTZ
				this._writer.WriteBitShort((short)this._header.DimensionAlternateUnitZeroHandling);
				//BS : DIMALTTZ
				this._writer.WriteBitShort((short)this._header.DimensionAlternateUnitToleranceZeroHandling);
				//B : DIMUPT
				this._writer.WriteBit(this._header.DimensionCursorUpdate);
				//BS : DIMATFIT
				this._writer.WriteBitShort(this._header.DimensionDimensionTextArrowFit);
			}

			//R2007 + Only:
			if (this.R2007Plus)
			{
				//B: DIMFXLON
				this._writer.WriteBit(this._header.DimensionIsExtensionLineLengthFixed);
			}

			//R2010 + Only:
			if (this.R2010Plus)
			{
				//B: DIMTXTDIRECTION
				this._writer.WriteBit(this._header.DimensionTextDirection == Tables.TextDirection.RightToLeft);
				//BD : DIMALTMZF
				this._writer.WriteBitDouble(this._header.DimensionAltMzf);
				//T : DIMALTMZS
				this._writer.WriteVariableText(this._header.DimensionAltMzs);
				//BD : DIMMZF
				this._writer.WriteBitDouble(this._header.DimensionMzf);
				//T : DIMMZS
				this._writer.WriteVariableText(this._header.DimensionMzs);
			}

			//R2000 + Only:
			if (this.R2000Plus)
			{
				//H: DIMTXSTY(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLDRBLK(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK1(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK2(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007+ Only:
			if (this.R2007Plus)
			{
				//H : DIMLTYPE (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLTEX1(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLTEX2(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2000+ Only:
			if (this.R2000Plus)
			{
				//BS: DIMLWD
				this._writer.WriteBitShort((short)this._header.DimensionLineWeight);
				//BS : DIMLWE
				this._writer.WriteBitShort((short)this._header.ExtensionLineWeight);
			}

			//H: BLOCK CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.BlockRecords);
			//H: LAYER CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.Layers);
			//H: STYLE CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.TextStyles);
			//H: LINETYPE CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.LineTypes);
			//H: VIEW CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.Views);
			//H: UCS CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.UCSs);
			//H: VPORT CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.VPorts);
			//H: APPID CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.AppIds);
			//H: DIMSTYLE CONTROL OBJECT(hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.DimensionStyles);

			//R13 - R15 Only:
			if (this.R13_15Only)
			{
				//H: VIEWPORT ENTITY HEADER CONTROL OBJECT(hard owner)
				this._writer.HandleReference(DwgReferenceType.HardOwnership, null);
			}

			//Common:
			//H: DICTIONARY(ACAD_GROUP)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			//H: DICTIONARY(ACAD_MLINESTYLE)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, null);

			//H : DICTIONARY (NAMED OBJECTS) (hard owner)
			this._writer.HandleReference(DwgReferenceType.HardOwnership, this._document.RootDictionary);

			//R2000+ Only:
			if (this.R2000Plus)
			{
				//BS: TSTACKALIGN, default = 1(not present in DXF)
				this._writer.WriteBitShort(this._header.StackedTextAlignment);
				//BS: TSTACKSIZE, default = 70(not present in DXF)
				this._writer.WriteBitShort(this._header.StackedTextSizePercentage);

				//TV: HYPERLINKBASE
				this._writer.WriteVariableText(this._header.HyperLinkBase);
				//TV : STYLESHEET
				this._writer.WriteVariableText(this._header.StyleSheetName);

				//H : DICTIONARY(LAYOUTS)(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.RootDictionary[CadDictionary.AcadLayout]);
				//H: DICTIONARY(PLOTSETTINGS)(hard pointer)
				//_writer.HandleReference(DwgReferenceType.HardPointer, _document.RootDictionary[CadDictionary.AcadPlotSettings]);
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DICTIONARY(PLOTSTYLES)(hard pointer)
				//_writer.HandleReference(DwgReferenceType.HardPointer, _document.RootDictionary[CadDictionary.AcadPlotStyleName]);
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2004 +:
			if (this.R2004Plus)
			{
				//H: DICTIONARY (MATERIALS) (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DICTIONARY (COLORS) (hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007 +:
			if (this.R2007Plus)
			{
				//H: DICTIONARY(VISUALSTYLE)(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//R2013+:
				if (this.R2013Plus)
				{
					//H : UNKNOWN (hard pointer)	//DICTIONARY_VISUALSTYLE
					this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				}
			}

			//R2000 +:
			if (this.R2000Plus)
			{
				//BL: Flags:

				//CELWEIGHT Flags & 0x001F
				int flags = ((int)this._header.CurrentEntityLineWeight & 0x1F) |
							//ENDCAPS Flags & 0x0060
							(this._header.EndCaps << 0x5) |
							//JOINSTYLE Flags & 0x0180
							(this._header.JoinStyle << 0x7);

				//LWDISPLAY!(Flags & 0x0200)
				if (!this._header.DisplayLineWeight)
				{
					flags |= 0x200;
				}
				//XEDIT!(Flags & 0x0400)
				if (!this._header.XEdit)
				{
					flags |= 0x400;
				}
				//EXTNAMES Flags & 0x0800
				if (this._header.ExtendedNames)
				{
					flags |= 0x800;
				}
				//PSTYLEMODE Flags & 0x2000
				if (this._header.PlotStyleMode == 1)
				{
					flags |= 0x2000;
				}
				//OLESTARTUP Flags & 0x4000
				if (this._header.LoadOLEObject)
				{
					flags |= 0x4000;
				}

				this._writer.WriteBitLong(flags);

				//BS: INSUNITS
				this._writer.WriteBitShort((short)this._header.InsUnits);
				//BS : CEPSNTYPE
				this._writer.WriteBitShort(this._header.CurrentEntityPlotStyleType);

				if (this._header.CurrentEntityPlotStyleType == 3)
				{
					//H: CPSNID(present only if CEPSNTYPE == 3) (hard pointer)
					this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				}

				//TV: FINGERPRINTGUID
				this._writer.WriteVariableText(this._header.FingerPrintGuid);
				//TV : VERSIONGUID
				this._writer.WriteVariableText(this._header.VersionGuid);
			}

			//R2004 +:
			if (this.R2004Plus)
			{
				//RC: SORTENTS
				this._writer.WriteByte((byte)this._header.EntitySortingFlags);
				//RC : INDEXCTL
				this._writer.WriteByte(this._header.IndexCreationFlags);
				//RC : HIDETEXT
				this._writer.WriteByte(this._header.HideText);
				//RC : XCLIPFRAME, before R2010 the value can be 0 or 1 only.
				this._writer.WriteByte(this._header.ExternalReferenceClippingBoundaryType);
				//RC : DIMASSOC
				this._writer.WriteByte((byte)this._header.DimensionAssociativity);
				//RC : HALOGAP
				this._writer.WriteByte(this._header.HaloGapPercentage);
				//BS : OBSCUREDCOLOR
				this._writer.WriteBitShort(this._header.ObscuredColor.Index);
				//BS : INTERSECTIONCOLOR
				this._writer.WriteBitShort(this._header.InterfereColor.Index);
				//RC : OBSCUREDLTYPE
				this._writer.WriteByte(this._header.ObscuredType);
				//RC: INTERSECTIONDISPLAY
				this._writer.WriteByte(this._header.IntersectionDisplay);

				//TV : PROJECTNAME
				this._writer.WriteVariableText(this._header.ProjectName);
			}

			//Common:
			//H: BLOCK_RECORD(*PAPER_SPACE)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.PaperSpace);
			//H: BLOCK_RECORD(*MODEL_SPACE)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.ModelSpace);
			//H: LTYPE(BYLAYER)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.LineTypes["ByLayer"]);
			//H: LTYPE(BYBLOCK)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.LineTypes["ByBlock"]);
			//H: LTYPE(CONTINUOUS)(hard pointer)
			this._writer.HandleReference(DwgReferenceType.HardPointer, this._document.LineTypes["Continuous"]);

			//R2007 +:
			if (this.R2007Plus)
			{
				//B: CAMERADISPLAY
				this._writer.WriteBit(this._header.CameraDisplayObjects);

				//BL : unknown
				this._writer.WriteBitLong(0);
				//BL : unknown
				this._writer.WriteBitLong(0);
				//BD : unknown
				this._writer.WriteBitDouble(0);

				//BD : STEPSPERSEC
				this._writer.WriteBitDouble(this._header.StepsPerSecond);
				//BD : STEPSIZE
				this._writer.WriteBitDouble(this._header.StepSize);
				//BD : 3DDWFPREC
				this._writer.WriteBitDouble(this._header.Dw3DPrecision);
				//BD : LENSLENGTH
				this._writer.WriteBitDouble(this._header.LensLength);
				//BD : CAMERAHEIGHT
				this._writer.WriteBitDouble(this._header.CameraHeight);
				//RC : SOLIDHIST
				this._writer.WriteByte((byte)this._header.SolidsRetainHistory);
				//RC : SHOWHIST
				this._writer.WriteByte((byte)this._header.ShowSolidsHistory);
				//BD : PSOLWIDTH
				this._writer.WriteBitDouble(this._header.SweptSolidWidth);
				//BD : PSOLHEIGHT
				this._writer.WriteBitDouble(this._header.SweptSolidHeight);
				//BD : LOFTANG1
				this._writer.WriteBitDouble(this._header.DraftAngleFirstCrossSection);
				//BD : LOFTANG2
				this._writer.WriteBitDouble(this._header.DraftAngleSecondCrossSection);
				//BD : LOFTMAG1
				this._writer.WriteBitDouble(this._header.DraftMagnitudeFirstCrossSection);
				//BD : LOFTMAG2
				this._writer.WriteBitDouble(this._header.DraftMagnitudeSecondCrossSection);
				//BS : LOFTPARAM
				this._writer.WriteBitShort(this._header.SolidLoftedShape);
				//RC : LOFTNORMALS
				this._writer.WriteByte((byte)this._header.LoftedObjectNormals);
				//BD : LATITUDE
				this._writer.WriteBitDouble(this._header.Latitude);
				//BD : LONGITUDE
				this._writer.WriteBitDouble(this._header.Longitude);
				//BD : NORTHDIRECTION
				this._writer.WriteBitDouble(this._header.NorthDirection);
				//BL : TIMEZONE
				this._writer.WriteBitLong(this._header.TimeZone);
				//RC : LIGHTGLYPHDISPLAY
				this._writer.WriteByte((byte)this._header.DisplayLightGlyphs);
				//RC : TILEMODELIGHTSYNCH	??
				this._writer.WriteByte((byte)'0');
				//RC : DWFFRAME
				this._writer.WriteByte((byte)this._header.DwgUnderlayFramesVisibility);
				//RC : DGNFRAME
				this._writer.WriteByte((byte)this._header.DgnUnderlayFramesVisibility);

				//B : unknown
				this._writer.WriteBit(false);

				//CMC : INTERFERECOLOR
				this._writer.WriteCmColor(this._header.InterfereColor);

				//H : INTERFEREOBJVS(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: INTERFEREVPVS(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DRAGVS(hard pointer)
				this._writer.HandleReference(DwgReferenceType.HardPointer, null);

				//RC: CSHADOW
				this._writer.WriteByte(this._header.ShadowMode);
				//BD : unknown
				this._writer.WriteBitDouble(this._header.ShadowPlaneLocation);
			}

			//R14 +:
			if (this._header.Version >= ACadVersion.AC1014)
			{
				//BS : unknown short(type 5 / 6 only) these do not seem to be required,
				this._writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only) even for type 5.
				this._writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only)
				this._writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only)
				this._writer.WriteBitShort(-1);

				if (this.R2004Plus)
				{
					//This file versions seem to finish with this values
					this._writer.WriteBitLong(0);
					this._writer.WriteBitLong(0);
					this._writer.WriteBit(false);
				}
			}

			this._writer.WriteSpearShift();

			//Write the size and merge the streams
			this.writeSizeAndCrc();
		}

		private void writeSizeAndCrc()
		{
			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			this._startWriter.WriteBytes(DwgSectionDefinition.StartSentinels[SectionName]);

			CRC8StreamHandler crc = new CRC8StreamHandler(this._startWriter.Stream, 0xC0C1);
			StreamIO swriter = new StreamIO(crc);

			//RL : Size of the section.
			swriter.Write((int)this._msmain.Length);

			//R2010/R2013 (only present if the maintenance version is greater than 3!) or R2018+:
			if (R2010Plus && _header.MaintenanceVersion > 3 || R2018Plus)
			{
				//Unknown (4 byte long), might be part of a 64-bit size.
				swriter.Write<int>(0);
			}

			crc.Write(this._msmain.GetBuffer(), 0, (int)this._msmain.Length);

			//Common:
			//RS : CRC for the data section, starting after the sentinel.Use 0xC0C1 for the initial value.
			swriter.Write<ushort>(crc.Seed);

			//Ending sentinel: 0x30,0x84,0xE0,0xDC,0x02,0x21,0xC7,0x56,0xA0,0x83,0x97,0x47,0xB1,0x92,0xCC,0xA0
			this._startWriter.WriteBytes(DwgSectionDefinition.EndSentinels[SectionName]);
		}
	}
}
