using ACadSharp.Header;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderWriter : DwgSectionIO
	{
		private MemoryStream _msbegin;
		private MemoryStream _msmain;

		private IDwgStreamWriter _swbegin;
		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;
		private CadHeader header;

		public DwgHeaderWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;
			this.header = document.Header;

			this._msbegin = new MemoryStream();
			this._msmain = new MemoryStream();

			this._swbegin = DwgStreamWriter.GetStreamHandler(document.Header.Version, this._msbegin, Encoding.Default);
			this._writer = DwgStreamWriter.GetStreamHandler(document.Header.Version, _msmain, Encoding.Default);
		}

		public void Write()
		{
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
			if (R13_14Only)
			{
				//BS : Unknown short, default value 0
				this._writer.WriteBitShort(0);
			}

			//Pre-2004 Only:
			if (R2004Pre)
			{
				//H : Handle of the current viewport entity header (hard pointer)
			}

			//Common:
			//B: DIMASO
			_writer.WriteBit(header.AssociatedDimensions);
			//B: DIMSHO
			_writer.WriteBit(header.UpdateDimensionsWhileDragging);

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B : DIMSAV Undocumented.
				_writer.WriteBit(header.DIMSAV);
			}

			//Common:
			//B: PLINEGEN
			_writer.WriteBit(header.PolylineLineTypeGeneration);
			//B : ORTHOMODE
			_writer.WriteBit(header.OrthoMode);
			//B: REGENMODE
			_writer.WriteBit(header.RegenerationMode);
			//B : FILLMODE
			_writer.WriteBit(header.FillMode);
			//B : QTEXTMODE
			_writer.WriteBit(header.QuickTextMode);
			//B : PSLTSCALE
			_writer.WriteBit(header.PaperSpaceLineTypeScaling == SpaceLineTypeScaling.Normal);
			//B : LIMCHECK
			_writer.WriteBit(header.LimitCheckingOn);

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
				//B : BLIPMODE
				_writer.WriteBit(header.BlipMode);

			//R2004+:
			if (R2004Plus)
				//B : Undocumented
				_writer.WriteBit(false);

			//Common:
			//B: USRTIMER(User timer on / off).
			_writer.WriteBit(header.UserTimer);
			//B : SKPOLY
			_writer.WriteBit(header.SketchPolylines);
			//B : ANGDIR
			_writer.WriteBit(header.AngularDirection != Types.Units.AngularDirection.CounterClockWise);
			//B : SPLFRAME
			_writer.WriteBit(header.ShowSplineControlPoints);

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//B : ATTREQ
				_writer.WriteBit(false);
				//B : ATTDIA
				_writer.WriteBit(false);
			}

			//Common:
			//B: MIRRTEXT
			_writer.WriteBit(header.MirrorText);
			//B : WORLDVIEW
			_writer.WriteBit(header.WorldView);

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//B: WIREFRAME Undocumented.
				_writer.WriteBit(false);
			}

			//Common:
			//B: TILEMODE
			_writer.WriteBit(header.ShowModelSpace);
			//B : PLIMCHECK
			_writer.WriteBit(header.PaperSpaceLimitsChecking);
			//B : VISRETAIN
			_writer.WriteBit(header.RetainXRefDependentVisibilitySettings);

			//R13 - R14 Only(stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//B : DELOBJ
				_writer.WriteBit(false);
			}

			//Common:
			//B: DISPSILH
			_writer.WriteBit(header.DisplaySilhouetteCurves);
			//B : PELLIPSE(not present in DXF)
			_writer.WriteBit(header.CreateEllipseAsPolyline);
			//BS: PROXYGRAPHICS
			_writer.WriteBitShort((short)(this.header.ProxyGraphics ? 1 : 0));

			//R13-R14 Only (stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS : DRAGMODE
				_writer.WriteBitShort(0);
			}

			//Common:
			//BS: TREEDEPTH
			_writer.WriteBitShort(header.SpatialIndexMaxTreeDepth);
			//BS : LUNITS
			_writer.WriteBitShort((short)header.LinearUnitFormat);
			//BS : LUPREC
			_writer.WriteBitShort(header.LinearUnitPrecision);
			//BS : AUNITS
			_writer.WriteBitShort((short)header.AngularUnit);
			//BS : AUPREC
			_writer.WriteBitShort(header.AngularUnitPrecision);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
				//BS: OSMODE
				_writer.WriteBitShort((short)header.ObjectSnapMode);

			//Common:
			//BS: ATTMODE
			_writer.WriteBitShort((short)header.AttributeVisibility);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS: COORDS
				_writer.WriteBitShort(0);
			}

			//Common:
			//BS: PDMODE
			_writer.WriteBitShort(header.PointDisplayMode);

			//R13 - R14 Only Only(stored in registry from R15 onwards):
			if (R13_14Only)
			{
				//BS: PICKSTYLE
				_writer.WriteBitShort(0);
			}

			//R2004 +:
			if (R2004Plus)
			{
				//BL: Unknown
				_writer.WriteBitLong(0);
				//BL: Unknown
				_writer.WriteBitLong(0);
				//BL: Unknown
				_writer.WriteBitLong(0);
			}

			//Common:
			//BS : USERI1
			_writer.WriteBitShort(header.UserShort1);
			//BS : USERI2
			_writer.WriteBitShort(header.UserShort2);
			//BS : USERI3
			_writer.WriteBitShort(header.UserShort3);
			//BS : USERI4
			_writer.WriteBitShort(header.UserShort4);
			//BS : USERI5
			_writer.WriteBitShort(header.UserShort5);

			//BS: SPLINESEGS
			_writer.WriteBitShort(header.NumberOfSplineSegments);
			//BS : SURFU
			_writer.WriteBitShort(header.SurfaceDensityU);
			//BS : SURFV
			_writer.WriteBitShort(header.SurfaceDensityV);
			//BS : SURFTYPE
			_writer.WriteBitShort(header.SurfaceType);
			//BS : SURFTAB1
			_writer.WriteBitShort(header.SurfaceMeshTabulationCount1);
			//BS : SURFTAB2
			_writer.WriteBitShort(header.SurfaceMeshTabulationCount2);
			//BS : SPLINETYPE
			_writer.WriteBitShort((short)header.SplineType);
			//BS : SHADEDGE
			_writer.WriteBitShort((short)header.ShadeEdge);
			//BS : SHADEDIF
			_writer.WriteBitShort(header.ShadeDiffuseToAmbientPercentage);
			//BS: UNITMODE
			_writer.WriteBitShort(header.UnitMode);
			//BS : MAXACTVP
			_writer.WriteBitShort(header.MaxViewportCount);
			//BS : ISOLINES
			_writer.WriteBitShort(header.SurfaceIsolineCount);
			//BS : CMLJUST
			_writer.WriteBitShort((short)header.CurrentMultilineJustification);
			//BS : TEXTQLTY
			_writer.WriteBitShort(header.TextQuality);
			//BD : LTSCALE
			_writer.WriteBitDouble(header.LineTypeScale);
			//BD : TEXTSIZE
			_writer.WriteBitDouble(header.TextHeightDefault);
			//BD : TRACEWID
			_writer.WriteBitDouble(header.TraceWidthDefault);
			//BD : SKETCHINC
			_writer.WriteBitDouble(header.SketchIncrement);
			//BD : FILLETRAD
			_writer.WriteBitDouble(header.FilletRadius);
			//BD : THICKNESS
			_writer.WriteBitDouble(header.ThicknessDefault);
			//BD : ANGBASE
			_writer.WriteBitDouble(header.AngleBase);
			//BD : PDSIZE
			_writer.WriteBitDouble(header.PointDisplaySize);
			//BD : PLINEWID
			_writer.WriteBitDouble(header.PolylineWidthDefault);
			//BD : USERR1
			_writer.WriteBitDouble(header.UserDouble1);
			//BD : USERR2
			_writer.WriteBitDouble(header.UserDouble2);
			//BD : USERR3
			_writer.WriteBitDouble(header.UserDouble3);
			//BD : USERR4
			_writer.WriteBitDouble(header.UserDouble4);
			//BD : USERR5
			_writer.WriteBitDouble(header.UserDouble5);
			//BD : CHAMFERA
			_writer.WriteBitDouble(header.ChamferDistance1);
			//BD : CHAMFERB
			_writer.WriteBitDouble(header.ChamferDistance2);
			//BD : CHAMFERC
			_writer.WriteBitDouble(header.ChamferLength);
			//BD : CHAMFERD
			_writer.WriteBitDouble(header.ChamferAngle);
			//BD : FACETRES
			_writer.WriteBitDouble(header.FacetResolution);
			//BD : CMLSCALE
			_writer.WriteBitDouble(header.CurrentMultilineScale);
			//BD : CELTSCALE
			_writer.WriteBitDouble(header.CurrentEntityLinetypeScale);

			//TV: MENUNAME
			_writer.WriteVariableText(header.MenuFileName);

			//Common:
			//BL: TDCREATE(Julian day)
			//BL: TDCREATE(Milliseconds into the day)
			_writer.WriteDateTime(header.CreateDateTime);
			//BL: TDUPDATE(Julian day)
			//BL: TDUPDATE(Milliseconds into the day)
			_writer.WriteDateTime(header.UpdateDateTime);

			//R2004 +:
			if (R2004Plus)
			{
				//BL : Unknown
				_writer.WriteBitLong(0);
				//BL : Unknown
				_writer.WriteBitLong(0);
				//BL : Unknown
				_writer.WriteBitLong(0);
			}

			//Common:
			//BL: TDINDWG(Days)
			//BL: TDINDWG(Milliseconds into the day)
			_writer.WriteTimeSpan(header.TotalEditingTime);
			//BL: TDUSRTIMER(Days)
			//BL: TDUSRTIMER(Milliseconds into the day)
			_writer.WriteTimeSpan(header.UserElapsedTimeSpan);

			//CMC : CECOLOR
			_writer.WriteCmColor(header.CurrentEntityColor);

			//H : HANDSEED The next handle, with an 8-bit length specifier preceding the handle
			//bytes (standard hex handle form) (code 0). The HANDSEED is not part of the handle
			//stream, but of the normal data stream (relevant for R21 and later).
			_writer.HandleReference(header.HandleSeed);

			//H : CLAYER (hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.CurrentLayer);

			//H: TEXTSTYLE(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.CurrentTextStyle);

			//H: CELTYPE(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.CurrentLineType);

			//R2007 + Only:
			if (R2007Plus)
			{
				//H: CMATERIAL(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//Common:
			//H: DIMSTYLE (hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.DimensionStyleOverrides);

			//H: CMLSTYLE (hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, null);

			//R2000+ Only:
			if (R2000Plus)
			{
				//BD: PSVPSCALE
				_writer.WriteBitDouble(header.ViewportDefaultViewScaleFactor);
			}

			//Common:
			//3BD: INSBASE(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceInsertionBase);
			//3BD: EXTMIN(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceExtMin);
			//3BD: EXTMAX(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceExtMax);
			//2RD: LIMMIN(PSPACE)
			_writer.Write2RawDouble(header.PaperSpaceLimitsMin);
			//2RD: LIMMAX(PSPACE)
			_writer.Write2RawDouble(header.PaperSpaceLimitsMax);
			//BD: ELEVATION(PSPACE)
			_writer.WriteBitDouble(header.PaperSpaceElevation);
			//3BD: UCSORG(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceUcsOrigin);
			//3BD: UCSXDIR(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceUcsXAxis);
			//3BD: UCSYDIR(PSPACE)
			_writer.Write3BitDouble(header.PaperSpaceUcsYAxis);

			//H: UCSNAME (PSPACE) (hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.PaperSpaceUcs);

			//R2000+ Only:
			if (R2000Plus)
			{
				//H : PUCSORTHOREF (hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//BS : PUCSORTHOVIEW	??
				_writer.WriteBitShort(0);

				//H: PUCSBASE(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//3BD: PUCSORGTOP
				_writer.Write3BitDouble(header.PaperSpaceOrthographicTopDOrigin);
				//3BD: PUCSORGBOTTOM
				_writer.Write3BitDouble(header.PaperSpaceOrthographicBottomDOrigin);
				//3BD: PUCSORGLEFT
				_writer.Write3BitDouble(header.PaperSpaceOrthographicLeftDOrigin);
				//3BD: PUCSORGRIGHT
				_writer.Write3BitDouble(header.PaperSpaceOrthographicRightDOrigin);
				//3BD: PUCSORGFRONT
				_writer.Write3BitDouble(header.PaperSpaceOrthographicFrontDOrigin);
				//3BD: PUCSORGBACK
				_writer.Write3BitDouble(header.PaperSpaceOrthographicBackDOrigin);
			}

			//Common:
			//3BD: INSBASE(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceInsertionBase);
			//3BD: EXTMIN(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceExtMin);
			//3BD: EXTMAX(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceExtMax);
			//2RD: LIMMIN(MSPACE)
			_writer.Write2RawDouble(header.ModelSpaceLimitsMin);
			//2RD: LIMMAX(MSPACE)
			_writer.Write2RawDouble(header.ModelSpaceLimitsMax);
			//BD: ELEVATION(MSPACE)
			_writer.WriteBitDouble(header.Elevation);
			//3BD: UCSORG(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceOrigin);
			//3BD: UCSXDIR(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceXAxis);
			//3BD: UCSYDIR(MSPACE)
			_writer.Write3BitDouble(header.ModelSpaceYAxis);

			//H: UCSNAME(MSPACE)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, header.ModelSpace);

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: UCSORTHOREF(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//BS: UCSORTHOVIEW	??
				_writer.WriteBitShort(0);

				//H : UCSBASE(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//3BD: UCSORGTOP
				_writer.Write3BitDouble(header.ModelSpaceOrthographicTopDOrigin);
				//3BD: UCSORGBOTTOM
				_writer.Write3BitDouble(header.ModelSpaceOrthographicBottomDOrigin);
				//3BD: UCSORGLEFT
				_writer.Write3BitDouble(header.ModelSpaceOrthographicLeftDOrigin);
				//3BD: UCSORGRIGHT
				_writer.Write3BitDouble(header.ModelSpaceOrthographicRightDOrigin);
				//3BD: UCSORGFRONT
				_writer.Write3BitDouble(header.ModelSpaceOrthographicFrontDOrigin);
				//3BD: UCSORGBACK
				_writer.Write3BitDouble(header.ModelSpaceOrthographicBackDOrigin);

				//TV : DIMPOST
				_writer.WriteVariableText(header.DimensionPostFix);
				//TV : DIMAPOST
				_writer.WriteVariableText(header.DimensionAlternateDimensioningSuffix);
			}

			//R13-R14 Only:
			if (R13_14Only)
			{
				//B: DIMTOL
				_writer.WriteBit(header.DimensionGenerateTolerances);
				//B : DIMLIM
				_writer.WriteBit(header.DimensionLimitsGeneration);
				//B : DIMTIH
				_writer.WriteBit(header.DimensionTextInsideHorizontal);
				//B : DIMTOH
				_writer.WriteBit(header.DimensionTextOutsideHorizontal);
				//B : DIMSE1
				_writer.WriteBit(header.DimensionSuppressFirstExtensionLine);
				//B : DIMSE2
				_writer.WriteBit(header.DimensionSuppressSecondExtensionLine);
				//B : DIMALT
				_writer.WriteBit(header.DimensionAlternateUnitDimensioning);
				//B : DIMTOFL
				_writer.WriteBit(header.DimensionTextOutsideExtensions);
				//B : DIMSAH
				_writer.WriteBit(header.DimensionSeparateArrowBlocks);
				//B : DIMTIX
				_writer.WriteBit(header.DimensionTextInsideExtensions);
				//B : DIMSOXD
				_writer.WriteBit(header.DimensionSuppressOutsideExtensions);
				//RC : DIMALTD
				_writer.WriteByte((byte)header.DimensionAlternateUnitDecimalPlaces);
				//RC : DIMZIN
				_writer.WriteByte((byte)header.DimensionZeroHandling);
				//B : DIMSD1
				_writer.WriteBit(header.DimensionSuppressFirstDimensionLine);
				//B : DIMSD2
				_writer.WriteBit(header.DimensionSuppressSecondDimensionLine);
				//RC : DIMTOLJ
				_writer.WriteByte((byte)header.DimensionToleranceAlignment);
				//RC : DIMJUST
				_writer.WriteByte((byte)header.DimensionTextHorizontalAlignment);
				//RC : DIMFIT
				_writer.WriteByte((byte)header.DimensionFit);
				//B : DIMUPT
				_writer.WriteBit(header.DimensionCursorUpdate);
				//RC : DIMTZIN
				_writer.WriteByte((byte)header.DimensionToleranceZeroHandling);
				//RC: DIMALTZ
				_writer.WriteByte((byte)header.DimensionAlternateUnitZeroHandling);
				//RC : DIMALTTZ
				_writer.WriteByte((byte)header.DimensionAlternateUnitToleranceZeroHandling);
				//RC : DIMTAD
				_writer.WriteByte((byte)header.DimensionTextVerticalAlignment);
				//BS : DIMUNIT
				_writer.WriteBitShort(header.DimensionUnit);
				//BS : DIMAUNIT
				_writer.WriteBitShort(header.DimensionAngularDimensionDecimalPlaces);
				//BS : DIMDEC
				_writer.WriteBitShort(header.DimensionDecimalPlaces);
				//BS : DIMTDEC
				_writer.WriteBitShort(header.DimensionToleranceDecimalPlaces);
				//BS : DIMALTU
				_writer.WriteBitShort((short)header.DimensionAlternateUnitFormat);
				//BS : DIMALTTD
				_writer.WriteBitShort(header.DimensionAlternateUnitToleranceDecimalPlaces);

				//H : DIMTXSTY(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, header.DimensionStyleOverrides);
			}

			//Common:
			//BD: DIMSCALE
			_writer.WriteBitDouble(header.DimensionScaleFactor);
			//BD : DIMASZ
			_writer.WriteBitDouble(header.DimensionArrowSize);
			//BD : DIMEXO
			_writer.WriteBitDouble(header.DimensionExtensionLineOffset);
			//BD : DIMDLI
			_writer.WriteBitDouble(header.DimensionLineIncrement);
			//BD : DIMEXE
			_writer.WriteBitDouble(header.DimensionExtensionLineExtension);
			//BD : DIMRND
			_writer.WriteBitDouble(header.DimensionRounding);
			//BD : DIMDLE
			_writer.WriteBitDouble(header.DimensionLineExtension);
			//BD : DIMTP
			_writer.WriteBitDouble(header.DimensionPlusTolerance);
			//BD : DIMTM
			_writer.WriteBitDouble(header.DimensionMinusTolerance);

			//R2007 + Only:
			if (R2007Plus)
			{
				//BD: DIMFXL
				_writer.WriteBitDouble(header.DimensionFixedExtensionLineLength);
				//BD : DIMJOGANG
				_writer.WriteBitDouble(header.DimensionJoggedRadiusDimensionTransverseSegmentAngle);
				//BS : DIMTFILL
				_writer.WriteBitShort((short)header.DimensionTextBackgroundFillMode);
				//CMC : DIMTFILLCLR
				_writer.WriteCmColor(header.DimensionTextBackgroundColor);
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//B: DIMTOL
				_writer.WriteBit(header.DimensionGenerateTolerances);
				//B : DIMLIM
				_writer.WriteBit(header.DimensionLimitsGeneration);
				//B : DIMTIH
				_writer.WriteBit(header.DimensionTextInsideHorizontal);
				//B : DIMTOH
				_writer.WriteBit(header.DimensionTextOutsideHorizontal);
				//B : DIMSE1
				_writer.WriteBit(header.DimensionSuppressFirstExtensionLine);
				//B : DIMSE2
				_writer.WriteBit(header.DimensionSuppressSecondExtensionLine);
				//BS : DIMTAD
				_writer.WriteBitShort((short)header.DimensionTextVerticalAlignment);
				//BS : DIMZIN
				_writer.WriteBitShort((short)header.DimensionZeroHandling);
				//BS : DIMAZIN
				_writer.WriteBitShort((short)header.DimensionAngularZeroHandling);
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//BS: DIMARCSYM
				_writer.WriteBitShort((short)header.DimensionArcLengthSymbolPosition);
			}

			//Common:
			//BD: DIMTXT
			_writer.WriteBitDouble(header.DimensionTextHeight);
			//BD : DIMCEN
			_writer.WriteBitDouble(header.DimensionCenterMarkSize);
			//BD: DIMTSZ
			_writer.WriteBitDouble(header.DimensionTickSize);
			//BD : DIMALTF
			_writer.WriteBitDouble(header.DimensionAlternateUnitScaleFactor);
			//BD : DIMLFAC
			_writer.WriteBitDouble(header.DimensionLinearScaleFactor);
			//BD : DIMTVP
			_writer.WriteBitDouble(header.DimensionTextVerticalPosition);
			//BD : DIMTFAC
			_writer.WriteBitDouble(header.DimensionToleranceScaleFactor);
			//BD : DIMGAP
			_writer.WriteBitDouble(header.DimensionLineGap);

			//R13 - R14 Only:
			if (R13_14Only)
			{
				//T: DIMPOST
				_writer.WriteVariableText(header.DimensionPostFix);
				//T : DIMAPOST
				_writer.WriteVariableText(header.DimensionAlternateDimensioningSuffix);
				//T : DIMBLK
				_writer.WriteVariableText(header.DimensionBlockName);
				//T : DIMBLK1
				_writer.WriteVariableText(header.DimensionBlockNameFirst);
				//T : DIMBLK2
				_writer.WriteVariableText(header.DimensionBlockNameSecond);
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//BD: DIMALTRND
				_writer.WriteBitDouble(header.DimensionAlternateUnitRounding);
				//B : DIMALT
				_writer.WriteBit(header.DimensionAlternateUnitDimensioning);
				//BS : DIMALTD
				_writer.WriteBitShort(header.DimensionAlternateUnitDecimalPlaces);
				//B : DIMTOFL
				_writer.WriteBit(header.DimensionTextOutsideExtensions);
				//B : DIMSAH
				_writer.WriteBit(header.DimensionSeparateArrowBlocks);
				//B : DIMTIX
				_writer.WriteBit(header.DimensionTextInsideExtensions);
				//B : DIMSOXD
				_writer.WriteBit(header.DimensionSuppressOutsideExtensions);
			}

			//Common:
			//CMC: DIMCLRD
			_writer.WriteCmColor(header.DimensionLineColor);
			//CMC : DIMCLRE
			_writer.WriteCmColor(header.DimensionExtensionLineColor);
			//CMC : DIMCLRT
			_writer.WriteCmColor(header.DimensionTextColor);

			//R2000 + Only:
			if (R2000Plus)
			{
				//BS: DIMADEC
				_writer.WriteBitShort(header.DimensionAngularDimensionDecimalPlaces);
				//BS : DIMDEC
				_writer.WriteBitShort(header.DimensionDecimalPlaces);
				//BS : DIMTDEC
				_writer.WriteBitShort(header.DimensionToleranceDecimalPlaces);
				//BS : DIMALTU
				_writer.WriteBitShort((short)header.DimensionAlternateUnitFormat);
				//BS : DIMALTTD
				_writer.WriteBitShort(header.DimensionAlternateUnitToleranceDecimalPlaces);
				//BS : DIMAUNIT
				_writer.WriteBitShort((short)header.DimensionAngularUnit);
				//BS : DIMFRAC
				_writer.WriteBitShort((short)header.DimensionFractionFormat);
				//BS : DIMLUNIT
				_writer.WriteBitShort((short)header.DimensionLinearUnitFormat);
				//BS : DIMDSEP
				_writer.WriteBitShort((short)header.DimensionDecimalSeparator);
				//BS : DIMTMOVE
				_writer.WriteBitShort((short)header.DimensionTextMovement);
				//BS : DIMJUST
				_writer.WriteBitShort((short)header.DimensionTextHorizontalAlignment);
				//B : DIMSD1
				_writer.WriteBit(header.DimensionSuppressFirstExtensionLine);
				//B : DIMSD2
				_writer.WriteBit(header.DimensionSuppressSecondExtensionLine);
				//BS : DIMTOLJ
				_writer.WriteBitShort((short)header.DimensionToleranceAlignment);
				//BS : DIMTZIN
				_writer.WriteBitShort((short)header.DimensionToleranceZeroHandling);
				//BS: DIMALTZ
				_writer.WriteBitShort((short)header.DimensionAlternateUnitZeroHandling);
				//BS : DIMALTTZ
				_writer.WriteBitShort((short)header.DimensionAlternateUnitToleranceZeroHandling);
				//B : DIMUPT
				_writer.WriteBit(header.DimensionCursorUpdate);
				//BS : DIMATFIT
				_writer.WriteBitShort(header.DimensionDimensionTextArrowFit);
			}

			//R2007 + Only:
			if (R2007Plus)
			{
				//B: DIMFXLON
				_writer.WriteBit(header.DimensionIsExtensionLineLengthFixed);
			}

			//R2010 + Only:
			if (R2010Plus)
			{
				//B: DIMTXTDIRECTION
				_writer.WriteBit(header.DimensionTextDirection == Tables.TextDirection.RightToLeft);
				//BD : DIMALTMZF
				_writer.WriteBitDouble(header.DimensionAltMzf);
				//T : DIMALTMZS
				_writer.WriteVariableText(header.DimensionAltMzs);
				//BD : DIMMZF
				_writer.WriteBitDouble(header.DimensionMzf);
				//T : DIMMZS
				_writer.WriteVariableText(header.DimensionMzs);
			}

			//R2000 + Only:
			if (R2000Plus)
			{
				//H: DIMTXSTY(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLDRBLK(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK1(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMBLK2(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007+ Only:
			if (R2007Plus)
			{
				//H : DIMLTYPE (hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLTEX1(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DIMLTEX2(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: DIMLWD
				_writer.WriteBitShort((short)header.DimensionLineWeight);
				//BS : DIMLWE
				_writer.WriteBitShort((short)header.ExtensionLineWeight);
			}

			//H: BLOCK CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.BlockRecords);
			//H: LAYER CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.Layers);
			//H: STYLE CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.TextStyles);
			//H: LINETYPE CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.LineTypes);
			//H: VIEW CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.Views);
			//H: UCS CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.UCSs);
			//H: VPORT CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.VPorts);
			//H: APPID CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.AppIds);
			//H: DIMSTYLE CONTROL OBJECT(hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.DimensionStyles);

			//R13 - R15 Only:
			if (R13_15Only)
			{
				//H: VIEWPORT ENTITY HEADER CONTROL OBJECT(hard owner)
				_writer.HandleReference(DwgReferenceType.HardOwnership, null);
			}

			//Common:
			//H: DICTIONARY(ACAD_GROUP)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, null);
			//H: DICTIONARY(ACAD_MLINESTYLE)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, null);

			//H : DICTIONARY (NAMED OBJECTS) (hard owner)
			_writer.HandleReference(DwgReferenceType.HardOwnership, _document.RootDictionary);

			//R2000+ Only:
			if (R2000Plus)
			{
				//BS: TSTACKALIGN, default = 1(not present in DXF)
				_writer.WriteBitShort(header.StackedTextAlignment);
				//BS: TSTACKSIZE, default = 70(not present in DXF)
				_writer.WriteBitShort(header.StackedTextSizePercentage);

				//TV: HYPERLINKBASE
				_writer.WriteVariableText(header.HyperLinkBase);
				//TV : STYLESHEET
				_writer.WriteVariableText(header.StyleSheetName);

				//H : DICTIONARY(LAYOUTS)(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, _document.RootDictionary[CadDictionary.AcadLayout]);
				//H: DICTIONARY(PLOTSETTINGS)(hard pointer)
				//_writer.HandleReference(DwgReferenceType.HardPointer, _document.RootDictionary[CadDictionary.AcadPlotSettings]);
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DICTIONARY(PLOTSTYLES)(hard pointer)
				//_writer.HandleReference(DwgReferenceType.HardPointer, _document.RootDictionary[CadDictionary.AcadPlotStyleName]);
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2004 +:
			if (R2004Plus)
			{
				//H: DICTIONARY (MATERIALS) (hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DICTIONARY (COLORS) (hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2007 +:
			if (R2007Plus)
			{
				//H: DICTIONARY(VISUALSTYLE)(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//R2013+:
				if (this.R2013Plus)
					//H : UNKNOWN (hard pointer)	//DICTIONARY_VISUALSTYLE
					_writer.HandleReference(DwgReferenceType.HardPointer, null);
			}

			//R2000 +:
			if (R2000Plus)
			{
				//BL: Flags:

				//CELWEIGHT Flags & 0x001F
				int flags = ((int)this.header.CurrentEntityLineWeight & 0x1F) |
							//ENDCAPS Flags & 0x0060
							(this.header.EndCaps << 0x5) |
							//JOINSTYLE Flags & 0x0180
							(this.header.JoinStyle << 0x7);

				//LWDISPLAY!(Flags & 0x0200)
				if (!this.header.DisplayLineWeight)
				{
					flags |= 0x200;
				}
				//XEDIT!(Flags & 0x0400)
				if (!this.header.XEdit)
				{
					flags |= 0x400;
				}
				//EXTNAMES Flags & 0x0800
				if (this.header.ExtendedNames)
				{
					flags |= 0x800;
				}
				//PSTYLEMODE Flags & 0x2000
				if (this.header.PlotStyleMode == 1)
				{
					flags |= 0x2000;
				}
				//OLESTARTUP Flags & 0x4000
				if (this.header.LoadOLEObject)
				{
					flags |= 0x4000;
				}

				this._writer.WriteBitLong(flags);

				//BS: INSUNITS
				_writer.WriteBitShort((short)header.InsUnits);
				//BS : CEPSNTYPE
				_writer.WriteBitShort(header.CurrentEntityPlotStyleType);

				if (header.CurrentEntityPlotStyleType == 3)
				{
					//H: CPSNID(present only if CEPSNTYPE == 3) (hard pointer)
					_writer.HandleReference(DwgReferenceType.HardPointer, null);
				}

				//TV: FINGERPRINTGUID
				_writer.WriteVariableText(header.FingerPrintGuid);
				//TV : VERSIONGUID
				_writer.WriteVariableText(header.VersionGuid);
			}

			//R2004 +:
			if (R2004Plus)
			{
				//RC: SORTENTS
				_writer.WriteByte((byte)header.EntitySortingFlags);
				//RC : INDEXCTL
				_writer.WriteByte(header.IndexCreationFlags);
				//RC : HIDETEXT
				_writer.WriteByte(header.HideText);
				//RC : XCLIPFRAME, before R2010 the value can be 0 or 1 only.
				_writer.WriteByte(header.ExternalReferenceClippingBoundaryType);
				//RC : DIMASSOC
				_writer.WriteByte((byte)header.DimensionAssociativity);
				//RC : HALOGAP
				_writer.WriteByte(header.HaloGapPercentage);
				//BS : OBSCUREDCOLOR
				_writer.WriteBitShort(header.ObscuredColor.Index);
				//BS : INTERSECTIONCOLOR
				_writer.WriteBitShort(header.InterfereColor.Index);
				//RC : OBSCUREDLTYPE
				_writer.WriteByte(header.ObscuredType);
				//RC: INTERSECTIONDISPLAY
				_writer.WriteByte(header.IntersectionDisplay);

				//TV : PROJECTNAME
				_writer.WriteVariableText(header.ProjectName);
			}

			//Common:
			//H: BLOCK_RECORD(*PAPER_SPACE)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, _document.PaperSpace);
			//H: BLOCK_RECORD(*MODEL_SPACE)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, _document.ModelSpace);
			//H: LTYPE(BYLAYER)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, _document.LineTypes["ByLayer"]);
			//H: LTYPE(BYBLOCK)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, _document.LineTypes["ByBlock"]);
			//H: LTYPE(CONTINUOUS)(hard pointer)
			_writer.HandleReference(DwgReferenceType.HardPointer, _document.LineTypes["Continuous"]);

			//R2007 +:
			if (R2007Plus)
			{
				//B: CAMERADISPLAY
				_writer.WriteBit(header.CameraDisplayObjects);

				//BL : unknown
				_writer.WriteBitLong(0);
				//BL : unknown
				_writer.WriteBitLong(0);
				//BD : unknown
				_writer.WriteBitDouble(0);

				//BD : STEPSPERSEC
				_writer.WriteBitDouble(header.StepsPerSecond);
				//BD : STEPSIZE
				_writer.WriteBitDouble(header.StepSize);
				//BD : 3DDWFPREC
				_writer.WriteBitDouble(header.Dw3DPrecision);
				//BD : LENSLENGTH
				_writer.WriteBitDouble(header.LensLength);
				//BD : CAMERAHEIGHT
				_writer.WriteBitDouble(header.CameraHeight);
				//RC : SOLIDHIST
				_writer.WriteByte((byte)header.SolidsRetainHistory);
				//RC : SHOWHIST
				_writer.WriteByte((byte)header.ShowSolidsHistory);
				//BD : PSOLWIDTH
				_writer.WriteBitDouble(header.SweptSolidWidth);
				//BD : PSOLHEIGHT
				_writer.WriteBitDouble(header.SweptSolidHeight);
				//BD : LOFTANG1
				_writer.WriteBitDouble(header.DraftAngleFirstCrossSection);
				//BD : LOFTANG2
				_writer.WriteBitDouble(header.DraftAngleSecondCrossSection);
				//BD : LOFTMAG1
				_writer.WriteBitDouble(header.DraftMagnitudeFirstCrossSection);
				//BD : LOFTMAG2
				_writer.WriteBitDouble(header.DraftMagnitudeSecondCrossSection);
				//BS : LOFTPARAM
				_writer.WriteBitShort(header.SolidLoftedShape);
				//RC : LOFTNORMALS
				_writer.WriteByte((byte)header.LoftedObjectNormals);
				//BD : LATITUDE
				_writer.WriteBitDouble(header.Latitude);
				//BD : LONGITUDE
				_writer.WriteBitDouble(header.Longitude);
				//BD : NORTHDIRECTION
				_writer.WriteBitDouble(header.NorthDirection);
				//BL : TIMEZONE
				_writer.WriteBitLong(header.TimeZone);
				//RC : LIGHTGLYPHDISPLAY
				_writer.WriteByte((byte)header.DisplayLightGlyphs);
				//RC : TILEMODELIGHTSYNCH	??
				_writer.WriteByte((byte)'0');
				//RC : DWFFRAME
				_writer.WriteByte((byte)header.DwgUnderlayFramesVisibility);
				//RC : DGNFRAME
				_writer.WriteByte((byte)header.DgnUnderlayFramesVisibility);

				//B : unknown
				_writer.WriteBit(false);

				//CMC : INTERFERECOLOR
				_writer.WriteCmColor(header.InterfereColor);

				//H : INTERFEREOBJVS(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: INTERFEREVPVS(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);
				//H: DRAGVS(hard pointer)
				_writer.HandleReference(DwgReferenceType.HardPointer, null);

				//RC: CSHADOW
				_writer.WriteByte(header.ShadowMode);
				//BD : unknown
				_writer.WriteBitDouble(header.ShadowPlaneLocation);
			}

			//R14 +:
			if (this.header.Version >= ACadVersion.AC1014)
			{
				//BS : unknown short(type 5 / 6 only) these do not seem to be required,
				_writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only) even for type 5.
				_writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only)
				_writer.WriteBitShort(-1);
				//BS : unknown short(type 5 / 6 only)
				_writer.WriteBitShort(-1);

				if (this.R2004Plus)
				{
					//This file versions seem to finish with this values
					_writer.WriteBitLong(0);
					_writer.WriteBitLong(0);
					_writer.WriteBit(false);
				}
			}

			this._writer.WriteSpearShift();

			//Common:
			//RS : CRC for the data section, starting after the sentinel.Use 0xC0C1 for the initial value.
			_writer.WriteRawShort(0xC0C1);

			//Ending sentinel: 0x30,0x84,0xE0,0xDC,0x02,0x21,0xC7,0x56,0xA0,0x83,0x97,0x47,0xB1,0x92,0xCC,0xA0
			_swbegin.WriteBytes(DwgSectionDefinition.EndSentinels[DwgSectionDefinition.Header]);

			//Write the size and merge the streams
			this.writeSectionBegin();
			this.mergeStreams();
		}

		private void writeSectionBegin()
		{
			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			_swbegin.WriteBytes(DwgSectionDefinition.StartSentinels[DwgSectionDefinition.Header]);

			//RL : Size of the section.
			_swbegin.WriteRawLong(this._msmain.Length);
		}

		private void mergeStreams()
		{
			this._stream.Write(this._msbegin.GetBuffer(), 0, (int)_msbegin.Length);
			this._stream.Write(this._msmain.GetBuffer(), 0, (int)_msmain.Length);
		}
	}
}
