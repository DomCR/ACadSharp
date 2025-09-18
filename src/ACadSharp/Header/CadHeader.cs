using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp.Header
{
	public class CadHeader
	{
		/// <summary>
		/// Sets the zero (0) base angle with respect to the current UCS.
		/// </summary>
		/// <remarks>
		/// System variable ANGBASE
		/// </remarks>
		[CadSystemVariable("$ANGBASE", 50)]
		public double AngleBase { get; set; } = 0.0d;

		/// <summary>
		/// Sets the direction of positive angles.
		/// </summary>
		/// <remarks>
		/// System variable ANGDIR.
		/// </remarks>
		[CadSystemVariable("$ANGDIR", 70)]
		public AngularDirection AngularDirection { get; set; } = AngularDirection.ClockWise;

		/// <summary>
		/// Sets units for angles.
		/// </summary>
		/// <remarks>
		/// System variable AUNITS.
		/// </remarks>
		[CadSystemVariable("$AUNITS", 70)]
		public AngularUnitFormat AngularUnit { get; set; } = AngularUnitFormat.DecimalDegrees;

		/// <summary>
		/// Sets the display precision for angular units and coordinates.
		/// </summary>
		/// <remarks>
		/// System variable AUPREC.
		/// </remarks>
		/// <value>
		/// Valid values are from 0 to 8.
		/// </value>
		[CadSystemVariable("$AUPREC", 70)]
		public short AngularUnitPrecision
		{
			get
			{
				return this._angularUnitPrecision;
			}
			set
			{
				ObjectExtensions.InRange(value, 0, 8, "AUPREC valid values are from 0 to 8");
				this._angularUnitPrecision = value;
			}
		}

		/// <summary>
		/// Arrow block name for leaders
		/// </summary>
		/// <remarks>
		/// System variable DIMLDRBLK
		/// </remarks>
		[CadSystemVariable("$DIMLDRBLK", 1)]
		public string ArrowBlockName { get; set; } = string.Empty;

		/// <summary>
		/// </summary>
		/// <remarks>
		/// System variable DIMASO <br/>
		/// Obsolete; see DIMASSOC
		/// </remarks>
		[CadSystemVariable("$DIMASO", 70)]
		public bool AssociatedDimensions { get; set; } = true;

		/// <summary>
		/// Controls display of attributes.
		/// </summary>
		/// <remarks>
		/// System variable ATTMODE.
		/// </remarks>
		[CadSystemVariable("$ATTMODE", 70)]
		public AttributeVisibilityMode AttributeVisibility { get; set; } = AttributeVisibilityMode.Normal;

		/// <summary>
		/// System variable BLIPMODE	??
		/// </summary>
		[CadSystemVariable("$BLIPMODE", 70)]
		public bool BlipMode { get; set; }

		/// <remarks>
		/// System variable CAMERADISPLAY
		/// </remarks>
		[CadSystemVariable("$CAMERADISPLAY", 290)]
		public bool CameraDisplayObjects { get; set; }

		/// <remarks>
		/// System variable CAMERAHEIGHT
		/// </remarks>
		[CadSystemVariable("$CAMERAHEIGHT", 40)]
		public double CameraHeight { get; set; }

		/// <summary>
		/// Sets the chamfer angle when CHAMMODE is set to 1.
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERD.
		/// </remarks>
		[CadSystemVariable("$CHAMFERD", 40)]
		public double ChamferAngle { get; set; } = 0.0d;

		/// <summary>
		/// Sets the first chamfer distance when CHAMMODE is set to 0.
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERA.
		/// </remarks>
		[CadSystemVariable("$CHAMFERA", 40)]
		public double ChamferDistance1 { get; set; } = 0.0d;

		/// <summary>
		/// Sets the second chamfer distance when CHAMMODE is set to 0.
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERB.
		/// </remarks>
		[CadSystemVariable("$CHAMFERB", 40)]
		public double ChamferDistance2 { get; set; } = 0.0d;

		/// <summary>
		/// Sets the chamfer length when CHAMMODE is set to 1.
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERC.
		/// </remarks>
		[CadSystemVariable("$CHAMFERC", 40)]
		public double ChamferLength { get; set; } = 0.0d;

		/// <summary>
		/// Drawing code page.
		/// </summary>
		/// <remarks>
		/// System variable DWGCODEPAGE
		/// </remarks>
		[CadSystemVariable("$DWGCODEPAGE", 3)]
		public string CodePage { get; set; } = "ANSI_1252";

		/// <summary>
		/// Local date/time of drawing creation (see Special Handling of Date/Time Variables).
		/// </summary>
		/// <remarks>
		/// System variable TDCREATE.
		/// </remarks>
		[CadSystemVariable("$TDCREATE", 40)]
		public DateTime CreateDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// Controls the ellipse type created with ELLIPSE.
		/// </summary>
		/// <remarks>
		/// System variable PELLIPSE.
		/// </remarks>
		[CadSystemVariable("$PELLIPSE", 70)]
		public bool CreateEllipseAsPolyline { get; set; } = false;

		/// <summary>
		/// Current entity color number.
		/// </summary>
		/// <remarks>
		/// System variable CECOLOR.
		/// </remarks>
		[CadSystemVariable("$CECOLOR", 62)]
		public Color CurrentEntityColor { get; set; } = Color.ByLayer;

		/// <summary>
		/// Current entity linetype scale.
		/// </summary>
		/// <remarks>
		/// System variable CELTSCALE.
		/// </remarks>
		[CadSystemVariable("$CELTSCALE", 40)]
		public double CurrentEntityLinetypeScale { get; set; } = 1.0d;

		/// <summary>
		/// Line weight of new objects
		/// </summary>
		/// <remarks>
		/// System variable CELWEIGHT
		/// </remarks>
		[CadSystemVariable("$CELWEIGHT", 370)]
		public LineWeightType CurrentEntityLineWeight { get; set; } = LineWeightType.ByLayer;

		/// <summary>
		/// Plot style type of new objects
		/// </summary>
		/// <remarks>
		/// System variable CEPSNTYPE
		/// </remarks>
		[CadSystemVariable("$CEPSNTYPE", 380)]
		public EntityPlotStyleType CurrentEntityPlotStyle { get; set; }

		/// <summary>
		/// Gets the current layer associated with the document. If the document is null, returns the default layer.
		/// </summary>
		/// <remarks>This property retrieves the layer based on the current layer name from the document's layer
		/// collection, if a document is available. If no document is associated, it returns the default layer stored
		/// internally.</remarks>
		public Layer CurrentLayer
		{
			get
			{
				if (this.Document == null)
				{
					return this._currentLayer;
				}
				else
				{
					return this.Document.Layers[this.CurrentLayerName];
				}
			}
			private set
			{
				this._currentLayer = value;
			}
		}

		/// <summary>
		/// Sets the current layer.
		/// </summary>
		/// <remarks>
		/// System variable CLAYER
		/// </remarks>
		[CadSystemVariable("$CLAYER", true, 8)]
		public string CurrentLayerName
		{
			get { return this._currentLayer.Name; }
			set
			{
				if (this.Document != null)
				{
					this._currentLayer = this.Document.Layers[value];
				}
				else
				{
					this._currentLayer = new Layer(value);
				}
			}
		}

		/// <summary>
		/// Gets the current line type associated with the document or the default line type if no document is set.
		/// </summary>
		public LineType CurrentLineType
		{
			get
			{
				if (this.Document == null)
				{
					return this._currentLineType;
				}
				else
				{
					return this.Document.LineTypes[this.CurrentLineTypeName];
				}
			}
			private set
			{
				_currentLineType = value;
			}
		}

		/// <summary>
		/// Sets the linetype of new objects.
		/// </summary>
		/// <remarks>
		/// System variable CELTYPE.
		/// </remarks>
		[CadSystemVariable("$CELTYPE", true, 6)]
		public string CurrentLineTypeName
		{
			get { return this._currentLineType.Name; }
			set
			{
				if (this.Document != null)
				{
					this._currentLineType = this.Document.LineTypes[value];
				}
				else
				{
					this._currentLineType = new LineType(value);
				}
			}
		}

		public MLineStyle CurrentMLineStyle
		{
			get
			{
				if (this.Document == null)
				{
					return this._currentMLineStyle;
				}
				else
				{
					return this.Document.MLineStyles[this.CurrentMLineStyleName];
				}
			}
			private set
			{
				this._currentMLineStyle = value;
			}
		}

		/// <summary>
		/// Current multi line justification.
		/// </summary>
		/// <remarks>
		/// System variable CMLJUST.
		/// </remarks>
		[CadSystemVariable("$CMLJUST", 70)]
		public VerticalAlignmentType CurrentMultiLineJustification { get; set; } = VerticalAlignmentType.Top;

		/// <summary>
		/// Current multiline scale.
		/// </summary>
		/// <remarks>
		/// System variable CMLSCALE.
		/// </remarks>
		[CadSystemVariable("$CMLSCALE", 40)]
		public double CurrentMultilineScale { get; set; } = 20.0d;

		/// <summary>
		/// Current multiline style name.
		/// </summary>
		/// <remarks>
		/// System variable CMLSTYLE.
		/// </remarks>
		[CadSystemVariable("$CMLSTYLE", true, 2)]
		public string CurrentMLineStyleName
		{
			get { return this._currentMLineStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this._currentMLineStyle = this.Document.MLineStyles[value];
				}
				else
				{
					this._currentMLineStyle = new MLineStyle(value);
				}
			}
		}

		/// <summary>
		/// Gets the current text style applied to the document or the default text style if no document is loaded.
		/// </summary>
		public TextStyle CurrentTextStyle
		{
			get
			{
				if (this.Document == null)
				{
					return this._currentTextStyle;
				}
				else
				{
					return this.Document.TextStyles[this.CurrentTextStyleName];
				}
			}
			private set
			{
				this._currentTextStyle = value;
			}
		}

		/// <remarks>
		/// System variable DGNFRAME
		/// </remarks>
		[CadSystemVariable("$DGNFRAME", 280)]
		public char DgnUnderlayFramesVisibility { get; set; }

		/// <summary>
		/// Alternate dimensioning suffix
		/// </summary>
		/// <remarks>
		/// System variable DIMAPOST
		/// </remarks>
		[CadSystemVariable("$DIMAPOST", 1)]
		public string DimensionAlternateDimensioningSuffix
		{
			get { return this._dimensionStyleOverrides.AlternateDimensioningSuffix; }
			set
			{
				this._dimensionStyleOverrides.AlternateDimensioningSuffix = value;
			}
		}

		/// <summary>
		/// Alternate unit decimal places
		/// </summary>
		/// <remarks>
		/// System variable DIMALTD
		/// </remarks>
		[CadSystemVariable("$DIMALTD", 70)]
		public short DimensionAlternateUnitDecimalPlaces
		{
			get { return this._dimensionStyleOverrides.AlternateUnitDecimalPlaces; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Alternate unit dimensioning performed if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMALT
		/// </remarks>
		[CadSystemVariable("$DIMALT", 70)]
		public bool DimensionAlternateUnitDimensioning
		{
			get { return this._dimensionStyleOverrides.AlternateUnitDimensioning; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitDimensioning = value;
			}
		}

		/// <summary>
		/// Units format for alternate units of all dimension style family members except angular
		/// </summary>
		/// <remarks>
		/// System variable DIMALTU
		/// </remarks>
		[CadSystemVariable("$DIMALTU", 70)]
		public LinearUnitFormat DimensionAlternateUnitFormat
		{
			get { return this._dimensionStyleOverrides.AlternateUnitFormat; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitFormat = value;
			}
		}

		/// <summary>
		/// Determines rounding of alternate units
		/// </summary>
		/// <remarks>
		/// System variable DIMALTRND
		/// </remarks>
		[CadSystemVariable("$DIMALTRND", 40)]
		public double DimensionAlternateUnitRounding
		{
			get { return this._dimensionStyleOverrides.AlternateUnitRounding; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitRounding = value;
			}
		}

		/// <summary>
		/// Alternate unit scale factor
		/// </summary>
		/// <remarks>
		/// System variable DIMALTF
		/// </remarks>
		[CadSystemVariable("$DIMALTF", 40)]
		public double DimensionAlternateUnitScaleFactor
		{
			get { return this._dimensionStyleOverrides.AlternateUnitScaleFactor; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitScaleFactor = value;
			}
		}

		/// <summary>
		/// Number of decimal places for tolerance values of an alternate units dimension
		/// </summary>
		/// <remarks>
		/// System variable DIMALTTD
		/// </remarks>
		[CadSystemVariable("$DIMALTTD", 70)]
		public short DimensionAlternateUnitToleranceDecimalPlaces
		{
			get { return this._dimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for alternate tolerance values
		/// </summary>
		/// <remarks>
		/// System variable DIMALTTZ
		/// </remarks>
		[CadSystemVariable("$DIMALTTZ", 70)]
		public ZeroHandling DimensionAlternateUnitToleranceZeroHandling
		{
			get { return this._dimensionStyleOverrides.AlternateUnitToleranceZeroHandling; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitToleranceZeroHandling = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for alternate unit dimension values
		/// </summary>
		/// <remarks>
		/// System variable DIMALTZ
		/// </remarks>
		[CadSystemVariable("$DIMALTZ", 70)]
		public ZeroHandling DimensionAlternateUnitZeroHandling
		{
			get { return this._dimensionStyleOverrides.AlternateUnitZeroHandling; }
			set
			{
				this._dimensionStyleOverrides.AlternateUnitZeroHandling = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMALTMZF
		/// </remarks>
		[CadSystemVariable("$DIMALTMZF", 40)]
		public double DimensionAltMzf
		{
			get { return this._dimensionStyleOverrides.AltMzf; }
			set
			{
				this._dimensionStyleOverrides.AltMzf = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMALTMZS
		/// </remarks>
		[CadSystemVariable("$DIMALTMZS", 6)]
		public string DimensionAltMzs
		{
			get { return this._dimensionStyleOverrides.AltMzs; }
			set
			{
				this._dimensionStyleOverrides.AltMzs = value;
			}
		}

		/// <summary>
		/// Number of precision places displayed in angular dimensions
		/// </summary>
		/// <remarks>
		/// System variable DIMADEC
		/// </remarks>
		[CadSystemVariable("$DIMADEC", 70)]
		public short DimensionAngularDimensionDecimalPlaces
		{
			get { return this._dimensionStyleOverrides.AngularDecimalPlaces; }
			set
			{
				this._dimensionStyleOverrides.AngularDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Angle format for angular dimensions
		/// </summary>
		/// <remarks>
		/// System variable DIMAUNIT
		/// </remarks>
		[CadSystemVariable("$DIMAUNIT", 70)]
		public AngularUnitFormat DimensionAngularUnit
		{
			get { return this._dimensionStyleOverrides.AngularUnit; }
			set
			{
				this._dimensionStyleOverrides.AngularUnit = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for angular dimensions
		/// </summary>
		/// <remarks>
		/// System variable DIMAZIN
		/// </remarks>
		[CadSystemVariable("$DIMAZIN", 70)]
		public ZeroHandling DimensionAngularZeroHandling
		{
			get { return this._dimensionStyleOverrides.AngularZeroHandling; }
			set
			{
				this._dimensionStyleOverrides.AngularZeroHandling = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMARCSYM
		/// </remarks>
		[CadSystemVariable("$DIMARCSYM", 70)]
		public ArcLengthSymbolPosition DimensionArcLengthSymbolPosition
		{
			get { return this._dimensionStyleOverrides.ArcLengthSymbolPosition; }
			set
			{
				this._dimensionStyleOverrides.ArcLengthSymbolPosition = value;
			}
		}

		/// <summary>
		/// Dimensioning arrow size
		/// </summary>
		/// <remarks>
		/// System variable DIMASZ
		/// </remarks>
		[CadSystemVariable("$DIMASZ", 40)]
		public double DimensionArrowSize
		{
			get { return this._dimensionStyleOverrides.ArrowSize; }
			set
			{
				this._dimensionStyleOverrides.ArrowSize = value;
			}
		}

		/// <summary>
		/// Controls the associativity of dimension objects
		/// </summary>
		/// <remarks>
		/// System variable DIMASSOC
		/// </remarks>
		[CadSystemVariable("$DIMASSOC", 280)]
		public DimensionAssociation DimensionAssociativity { get; set; } = DimensionAssociation.CreateAssociativeDimensions;

		/// <summary>
		/// Arrow block name
		/// </summary>
		/// <remarks>
		/// System variable DIMBLK
		/// </remarks>
		[CadSystemVariable("$DIMBLK", 1)]
		public string DimensionBlockName { get; set; } = string.Empty;

		/// <summary>
		/// First arrow block name
		/// </summary>
		/// <remarks>
		/// System variable DIMBLK1
		/// </remarks>
		[CadSystemVariable("$DIMBLK1", 1)]
		public string DimensionBlockNameFirst { get; set; }

		/// <summary>
		/// Second arrow block name
		/// </summary>
		/// <remarks>
		/// System variable DIMBLK2
		/// </remarks>
		[CadSystemVariable("$DIMBLK2", 1)]
		public string DimensionBlockNameSecond { get; set; }

		/// <summary>
		/// Size of center mark/lines
		/// </summary>
		/// <remarks>
		/// System variable DIMCEN
		/// </remarks>
		[CadSystemVariable("$DIMCEN", 40)]
		public double DimensionCenterMarkSize
		{
			get { return this._dimensionStyleOverrides.CenterMarkSize; }
			set
			{
				this._dimensionStyleOverrides.CenterMarkSize = value;
			}
		}

		/// <summary>
		/// Cursor functionality for user-positioned text
		/// </summary>
		/// <remarks>
		/// System variable DIMUPT
		/// </remarks>
		[CadSystemVariable("$DIMUPT", 70)]
		public bool DimensionCursorUpdate
		{
			get { return this._dimensionStyleOverrides.CursorUpdate; }
			set
			{
				this._dimensionStyleOverrides.CursorUpdate = value;
			}
		}

		/// <summary>
		/// Number of decimal places for the tolerance values of a primary units dimension
		/// </summary>
		/// <remarks>
		/// System variable DIMDEC
		/// </remarks>
		[CadSystemVariable("$DIMDEC", 70)]
		public short DimensionDecimalPlaces
		{
			get { return this._dimensionStyleOverrides.DecimalPlaces; }
			set
			{
				this._dimensionStyleOverrides.DecimalPlaces = value;
			}
		}

		/// <summary>
		/// Single-character decimal separator used when creating dimensions whose unit format is decimal
		/// </summary>
		/// <remarks>
		/// System variable DIMLUNIT
		/// </remarks>
		[CadSystemVariable("$DIMDSEP", 70)]
		public char DimensionDecimalSeparator
		{
			get { return this._dimensionStyleOverrides.DecimalSeparator; }
			set
			{
				this._dimensionStyleOverrides.DecimalSeparator = value;
			}
		}

		/// <summary>
		/// Controls dimension text and arrow placement when space is not sufficient to place both within the extension lines
		/// </summary>
		/// <remarks>
		/// System variable DIMATFIT
		/// </remarks>
		[CadSystemVariable("$DIMATFIT", 70)]
		public TextArrowFitType DimensionDimensionTextArrowFit
		{
			get { return this._dimensionStyleOverrides.DimensionTextArrowFit; }
			set
			{
				this._dimensionStyleOverrides.DimensionTextArrowFit = value;
			}
		}

		/// <summary>
		/// Dimension extension line color override.
		/// </summary>
		/// <remarks>
		/// System variable DIMCLRE
		/// </remarks>
		[CadSystemVariable("$DIMCLRE", 70)]
		public Color DimensionExtensionLineColor
		{
			get { return this._dimensionStyleOverrides.ExtensionLineColor; }
			set
			{
				this._dimensionStyleOverrides.ExtensionLineColor = value;
			}
		}

		/// <summary>
		/// Extension line extension override.
		/// </summary>
		/// <remarks>
		/// System variable DIMEXE
		/// </remarks>
		[CadSystemVariable("$DIMEXE", 40)]
		public double DimensionExtensionLineExtension
		{
			get { return this._dimensionStyleOverrides.ExtensionLineExtension; }
			set
			{
				this._dimensionStyleOverrides.ExtensionLineExtension = value;
			}
		}

		/// <summary>
		/// Extension line offset override.
		/// </summary>
		/// <remarks>
		/// System variable DIMEXO
		/// </remarks>
		[CadSystemVariable("$DIMEXO", 40)]
		public double DimensionExtensionLineOffset
		{
			get { return this._dimensionStyleOverrides.ExtensionLineOffset; }
			set
			{
				this._dimensionStyleOverrides.ExtensionLineOffset = value;
			}
		}

		/// <remarks>
		/// System variable DIMFIT
		/// </remarks>
		[CadSystemVariable("$DIMFIT", 70)]
		public short DimensionFit
		{
			get { return this._dimensionStyleOverrides.DimensionFit; }
			set
			{
				this._dimensionStyleOverrides.DimensionFit = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMFXL
		/// </remarks>
		[CadSystemVariable("$DIMFXL", 40)]
		public double DimensionFixedExtensionLineLength
		{
			get { return this._dimensionStyleOverrides.FixedExtensionLineLength; }
			set
			{
				this._dimensionStyleOverrides.FixedExtensionLineLength = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMFRAC
		/// </remarks>
		[CadSystemVariable("$DIMFRAC", 70)]
		public FractionFormat DimensionFractionFormat
		{
			get { return this._dimensionStyleOverrides.FractionFormat; }
			set
			{
				this._dimensionStyleOverrides.FractionFormat = value;
			}
		}

		/// <summary>
		/// Vertical justification for tolerance values override.
		/// </summary>
		/// <remarks>
		/// System variable DIMTOL
		/// </remarks>
		[CadSystemVariable("$DIMTOL", 70)]
		public bool DimensionGenerateTolerances
		{
			get { return this._dimensionStyleOverrides.GenerateTolerances; }
			set
			{
				this._dimensionStyleOverrides.GenerateTolerances = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMFXLON
		/// </remarks>
		[CadSystemVariable("$DIMFXLON", 70)]
		public bool DimensionIsExtensionLineLengthFixed
		{
			get { return this._dimensionStyleOverrides.IsExtensionLineLengthFixed; }
			set
			{
				this._dimensionStyleOverrides.IsExtensionLineLengthFixed = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMJOGANG
		/// </remarks>
		[CadSystemVariable("$DIMJOGANG", 40)]
		public double DimensionJoggedRadiusDimensionTransverseSegmentAngle
		{
			get { return this._dimensionStyleOverrides.JoggedRadiusDimensionTransverseSegmentAngle; }
			set
			{
				this._dimensionStyleOverrides.JoggedRadiusDimensionTransverseSegmentAngle = value;
			}
		}

		/// <summary>
		/// Dimension limits generated if nonzero override.
		/// </summary>
		/// <remarks>
		/// System variable DIMLIM
		/// </remarks>
		[CadSystemVariable("$DIMLIM", 70)]
		public bool DimensionLimitsGeneration
		{
			get { return this._dimensionStyleOverrides.LimitsGeneration; }
			set
			{
				this._dimensionStyleOverrides.LimitsGeneration = value;
			}
		}

		/// <summary>
		/// Linear measurements scale factor override.
		/// </summary>
		/// <remarks>
		/// System variable DIMLFAC
		/// </remarks>
		[CadSystemVariable("$DIMLFAC", 40)]
		public double DimensionLinearScaleFactor
		{
			get { return this._dimensionStyleOverrides.LinearScaleFactor; }
			set
			{
				this._dimensionStyleOverrides.LinearScaleFactor = value;
			}
		}

		/// <summary>
		/// Sets units for all dimension types except Angular
		/// </summary>
		/// <remarks>
		/// System variable DIMLUNIT
		/// </remarks>
		[CadSystemVariable("$DIMLUNIT", 70)]
		public LinearUnitFormat DimensionLinearUnitFormat
		{
			get { return this._dimensionStyleOverrides.LinearUnitFormat; }
			set
			{
				this._dimensionStyleOverrides.LinearUnitFormat = value;
			}
		}

		/// <summary>
		/// Dimension line color
		/// </summary>
		/// <remarks>
		/// System variable DIMCLRD
		/// </remarks>
		[CadSystemVariable("$DIMCLRD", 70)]
		public Color DimensionLineColor
		{
			get { return this._dimensionStyleOverrides.DimensionLineColor; }
			set
			{
				this._dimensionStyleOverrides.DimensionLineColor = value;
			}
		}

		/// <summary>
		/// Dimension line extension
		/// </summary>
		/// <remarks>
		/// System variable DIMDLE
		/// </remarks>
		[CadSystemVariable("$DIMDLE", 40)]
		public double DimensionLineExtension
		{
			get { return this._dimensionStyleOverrides.DimensionLineExtension; }
			set
			{
				this._dimensionStyleOverrides.DimensionLineExtension = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMGAP
		/// </remarks>
		[CadSystemVariable("$DIMGAP", 40)]
		public double DimensionLineGap
		{
			get { return this._dimensionStyleOverrides.DimensionLineGap; }
			set
			{
				this._dimensionStyleOverrides.DimensionLineGap = value;
			}
		}

		/// <summary>
		/// Dimension line increment
		/// </summary>
		/// <remarks>
		/// System variable DIMDLI
		/// </remarks>
		[CadSystemVariable("$DIMDLI", 40)]
		public double DimensionLineIncrement
		{
			get { return this._dimensionStyleOverrides.DimensionLineIncrement; }
			set
			{
				this._dimensionStyleOverrides.DimensionLineIncrement = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMLTYPE
		/// </remarks>
		[CadSystemVariable("$DIMLTYPE", 6)]
		public string DimensionLineType { get; set; } = "ByBlock";

		/// <summary>
		/// Dimension line lineweight
		/// </summary>
		/// <remarks>
		/// System variable DIMLWD
		/// </remarks>
		[CadSystemVariable("$DIMLWD", 70)]
		public LineWeightType DimensionLineWeight
		{
			get { return this._dimensionStyleOverrides.DimensionLineWeight; }
			set
			{
				this._dimensionStyleOverrides.DimensionLineWeight = value;
			}
		}

		/// <summary>
		/// Minus tolerance
		/// </summary>
		/// <remarks>
		/// System variable DIMTM
		/// </remarks>
		[CadSystemVariable("$DIMTM", 40)]
		public double DimensionMinusTolerance
		{
			get { return this._dimensionStyleOverrides.MinusTolerance; }
			set
			{
				this._dimensionStyleOverrides.MinusTolerance = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMMZF
		/// </remarks>
		[CadSystemVariable("$DIMMZF", 40)]
		public double DimensionMzf
		{
			get { return this._dimensionStyleOverrides.Mzf; }
			set
			{
				this._dimensionStyleOverrides.Mzf = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMMZS
		/// </remarks>
		[CadSystemVariable("$DIMMZS", 6)]
		public string DimensionMzs
		{
			get { return this._dimensionStyleOverrides.Mzs; }
			set
			{
				this._dimensionStyleOverrides.Mzs = value;
			}
		}

		/// <summary>
		/// Plus tolerance
		/// </summary>
		/// <remarks>
		/// System variable DIMTP
		/// </remarks>
		[CadSystemVariable("$DIMTP", 40)]
		public double DimensionPlusTolerance
		{
			get { return this._dimensionStyleOverrides.PlusTolerance; }
			set
			{
				this._dimensionStyleOverrides.PlusTolerance = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMPOST
		/// </remarks>
		[CadSystemVariable("$DIMPOST", 1)]
		public string DimensionPostFix
		{
			get { return this._dimensionStyleOverrides.PostFix; }
			set
			{
				this._dimensionStyleOverrides.PostFix = value;
			}
		}

		/// <summary>
		/// Rounding value for dimension distances
		/// </summary>
		/// <remarks>
		/// System variable DIMRND
		/// </remarks>
		[CadSystemVariable("$DIMRND", 40)]
		public double DimensionRounding
		{
			get { return this._dimensionStyleOverrides.Rounding; }
			set
			{
				this._dimensionStyleOverrides.Rounding = value;
			}
		}

		/// <summary>
		/// Overall dimensioning scale factor
		/// </summary>
		/// <remarks>
		/// System variable DIMSCALE
		/// </remarks>
		[CadSystemVariable("$DIMSCALE", 40)]
		public double DimensionScaleFactor
		{
			get { return this._dimensionStyleOverrides.ScaleFactor; }
			set
			{
				this._dimensionStyleOverrides.ScaleFactor = value;
			}
		}

		/// <summary>
		/// Use separate arrow blocks if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMSAH
		/// </remarks>
		[CadSystemVariable("$DIMSAH", 70)]
		public bool DimensionSeparateArrowBlocks
		{
			get { return this._dimensionStyleOverrides.SeparateArrowBlocks; }
			set
			{
				this._dimensionStyleOverrides.SeparateArrowBlocks = value;
			}
		}

		/// <summary>
		/// Gets the current dimension style applied to the document or the default dimension style if no document is loaded.
		/// </summary>
		public DimensionStyle CurrentDimensionStyle
		{
			get
			{
				if (this.Document == null)
				{
					return this._currentDimensionStyle;
				}
				else
				{
					return this.Document.DimensionStyles[this.CurrentDimensionStyleName];
				}
			}
			private set
			{
				this._currentDimensionStyle = value;
			}
		}

		/// <summary>
		/// Dimension style name.
		/// </summary>
		/// <remarks>
		/// System variable DIMSTYLE
		/// </remarks>
		[CadSystemVariable("$DIMSTYLE", true, 2)]
		public string CurrentDimensionStyleName
		{
			get { return this._currentDimensionStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this._currentDimensionStyle = this.Document.DimensionStyles[value];
				}
				else
				{
					this._currentDimensionStyle = new DimensionStyle(value);
				}
			}
		}

		/// <summary>
		/// Suppression of first extension line.
		/// </summary>
		/// <remarks>
		/// System variable DIMSD1
		/// </remarks>
		[CadSystemVariable("$DIMSD1", 70)]
		public bool DimensionSuppressFirstDimensionLine
		{
			get { return this._dimensionStyleOverrides.SuppressFirstDimensionLine; }
			set
			{
				this._dimensionStyleOverrides.SuppressFirstDimensionLine = value;
			}
		}

		/// <summary>
		/// First extension line suppressed if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMSE1
		/// </remarks>
		[CadSystemVariable("$DIMSE1", 70)]
		public bool DimensionSuppressFirstExtensionLine
		{
			get { return this._dimensionStyleOverrides.SuppressFirstExtensionLine; }
			set
			{
				this._dimensionStyleOverrides.SuppressFirstExtensionLine = value;
			}
		}

		/// <summary>
		/// Suppress outside-extensions dimension lines if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMSOXD
		/// </remarks>
		[CadSystemVariable("$DIMSOXD", 70)]
		public bool DimensionSuppressOutsideExtensions
		{
			get { return this._dimensionStyleOverrides.SuppressOutsideExtensions; }
			set
			{
				this._dimensionStyleOverrides.SuppressOutsideExtensions = value;
			}
		}

		/// <summary>
		/// Suppression of second extension line
		/// </summary>
		/// <remarks>
		/// System variable DIMSD2
		/// </remarks>
		[CadSystemVariable("$DIMSD2", 70)]
		public bool DimensionSuppressSecondDimensionLine
		{
			get { return this._dimensionStyleOverrides.SuppressSecondDimensionLine; }
			set
			{
				this._dimensionStyleOverrides.SuppressSecondDimensionLine = value;
			}
		}

		/// <summary>
		/// Second extension line suppressed if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMSE2
		/// </remarks>
		[CadSystemVariable("$DIMSE2", 70)]
		public bool DimensionSuppressSecondExtensionLine
		{
			get { return this._dimensionStyleOverrides.SuppressSecondExtensionLine; }
			set
			{
				this._dimensionStyleOverrides.SuppressSecondExtensionLine = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMLTEX1
		/// </remarks>
		[CadSystemVariable("$DIMLTEX1", 6)]
		public string DimensionTex1 { get; set; } = "ByBlock";

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMLTEX2
		/// </remarks>
		[CadSystemVariable("$DIMLTEX2", 6)]
		public string DimensionTex2 { get; set; } = "ByBlock";

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMTFILLCLR
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$DIMTFILLCLR", 62)]
		public Color DimensionTextBackgroundColor
		{
			get { return this._dimensionStyleOverrides.TextBackgroundColor; }
			set
			{
				this._dimensionStyleOverrides.TextBackgroundColor = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMTFILL
		/// </remarks>
		[CadSystemVariable("$DIMTFILL", 70)]
		public DimensionTextBackgroundFillMode DimensionTextBackgroundFillMode
		{
			get { return this._dimensionStyleOverrides.TextBackgroundFillMode; }
			set
			{
				this._dimensionStyleOverrides.TextBackgroundFillMode = value;
			}
		}

		/// <summary>
		/// Dimension text color
		/// </summary>
		/// <remarks>
		/// System variable DIMCLRT
		/// </remarks>
		[CadSystemVariable("$DIMCLRT", 70)]
		public Color DimensionTextColor
		{
			get { return this._dimensionStyleOverrides.TextColor; }
			set
			{
				this._dimensionStyleOverrides.TextColor = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMTXTDIRECTION
		/// </remarks>
		[CadSystemVariable("$DIMTXTDIRECTION", 70)]
		public TextDirection DimensionTextDirection
		{
			get { return this._dimensionStyleOverrides.TextDirection; }
			set
			{
				this._dimensionStyleOverrides.TextDirection = value;
			}
		}

		/// <summary>
		/// Dimensioning text height
		/// </summary>
		/// <remarks>
		/// System variable DIMTXT
		/// </remarks>
		[CadSystemVariable("$DIMTXT", 40)]
		public double DimensionTextHeight
		{
			get { return this._dimensionStyleOverrides.TextHeight; }
			set
			{
				this._dimensionStyleOverrides.TextHeight = value;
			}
		}

		/// <summary>
		/// Horizontal dimension text position
		/// </summary>
		/// <remarks>
		/// System variable DIMJUST
		/// </remarks>
		[CadSystemVariable("$DIMJUST", 70)]
		public DimensionTextHorizontalAlignment DimensionTextHorizontalAlignment
		{
			get { return this._dimensionStyleOverrides.TextHorizontalAlignment; }
			set
			{
				this._dimensionStyleOverrides.TextHorizontalAlignment = value;
			}
		}

		/// <summary>
		/// Force text inside extensions if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMTIX
		/// </remarks>
		[CadSystemVariable("$DIMTIX", 70)]
		public bool DimensionTextInsideExtensions
		{
			get { return this._dimensionStyleOverrides.TextInsideExtensions; }
			set
			{
				this._dimensionStyleOverrides.TextInsideExtensions = value;
			}
		}

		/// <summary>
		/// Text inside horizontal if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMTIH
		/// </remarks>
		[CadSystemVariable("$DIMTIH", 70)]
		public bool DimensionTextInsideHorizontal
		{
			get { return this._dimensionStyleOverrides.TextInsideHorizontal; }
			set
			{
				this._dimensionStyleOverrides.TextInsideHorizontal = value;
			}
		}

		/// <summary>
		/// Dimension text movement rules decimal
		/// </summary>
		/// <remarks>
		/// System variable DIMTMOVE
		/// </remarks>
		[CadSystemVariable("$DIMTMOVE", 70)]
		public TextMovement DimensionTextMovement
		{
			get { return this._dimensionStyleOverrides.TextMovement; }
			set
			{
				this._dimensionStyleOverrides.TextMovement = value;
			}
		}

		/// <summary>
		/// If text is outside the extension lines, dimension lines are forced between the extension lines if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMTOFL
		/// </remarks>
		[CadSystemVariable("$DIMTOFL", 70)]
		public bool DimensionTextOutsideExtensions
		{
			get { return this._dimensionStyleOverrides.TextOutsideExtensions; }
			set
			{
				this._dimensionStyleOverrides.TextOutsideExtensions = value;
			}
		}

		/// <summary>
		/// Text outside horizontal if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMTOH
		/// </remarks>
		[CadSystemVariable("$DIMTOH", 70)]
		public bool DimensionTextOutsideHorizontal
		{
			get { return this._dimensionStyleOverrides.TextOutsideHorizontal; }
			set
			{
				this._dimensionStyleOverrides.TextOutsideHorizontal = value;
			}
		}

		/// <summary>
		/// Gets the current dimension text style applied to the document or the default dimension text style if no document is loaded.
		/// </summary>
		public TextStyle DimensionTextStyle
		{
			get
			{
				if (this.Document == null)
				{
					return this._dimensionTextStyle;
				}
				else
				{
					return this.Document.TextStyles[this.DimensionTextStyleName];
				}
			}
			private set
			{
				this._dimensionTextStyle = value;
			}
		}

		/// <summary>
		/// Dimension text style
		/// </summary>
		/// <remarks>
		/// System variable DIMTXSTY
		/// </remarks>
		[CadSystemVariable("$DIMTXSTY", true, 7)]
		public string DimensionTextStyleName
		{
			get { return this._dimensionTextStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this._dimensionTextStyle = this.Document.TextStyles[value];
				}
				else
				{
					this._dimensionTextStyle = new TextStyle(value);
				}
			}
		}

		/// <summary>
		/// Text above dimension line if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMTAD
		/// </remarks>
		[CadSystemVariable("$DIMTAD", 70)]
		public DimensionTextVerticalAlignment DimensionTextVerticalAlignment
		{
			get { return this._dimensionStyleOverrides.TextVerticalAlignment; }
			set
			{
				this._dimensionStyleOverrides.TextVerticalAlignment = value;
			}
		}

		/// <summary>
		/// Text vertical position
		/// </summary>
		/// <remarks>
		/// System variable DIMTVP
		/// </remarks>
		[CadSystemVariable("$DIMTVP", 40)]
		public double DimensionTextVerticalPosition
		{
			get { return this._dimensionStyleOverrides.TextVerticalPosition; }
			set
			{
				this._dimensionStyleOverrides.TextVerticalPosition = value;
			}
		}

		/// <summary>
		/// Dimensioning tick size
		/// </summary>
		/// <remarks>
		/// System variable DIMTSZ
		/// </remarks>
		[CadSystemVariable("$DIMTSZ", 40)]
		public double DimensionTickSize
		{
			get { return this._dimensionStyleOverrides.TickSize; }
			set
			{
				this._dimensionStyleOverrides.TickSize = value;
			}
		}

		/// <summary>
		/// Vertical justification for tolerance values
		/// </summary>
		/// <remarks>
		/// System variable DIMTOLJ
		/// </remarks>
		[CadSystemVariable("$DIMTOLJ", 70)]
		public ToleranceAlignment DimensionToleranceAlignment
		{
			get { return this._dimensionStyleOverrides.ToleranceAlignment; }
			set
			{
				this._dimensionStyleOverrides.ToleranceAlignment = value;
			}
		}

		/// <summary>
		/// Number of decimal places to display the tolerance values
		/// </summary>
		/// <remarks>
		/// System variable DIMTDEC
		/// </remarks>
		[CadSystemVariable("$DIMTDEC", 70)]
		public short DimensionToleranceDecimalPlaces
		{
			get { return this._dimensionStyleOverrides.ToleranceDecimalPlaces; }
			set
			{
				this._dimensionStyleOverrides.ToleranceDecimalPlaces = value;
			}
		}

		/// <summary>
		/// Dimension tolerance display scale factor
		/// </summary>
		/// <remarks>
		/// System variable DIMTFAC
		/// </remarks>
		[CadSystemVariable("$DIMTFAC", 40)]
		public double DimensionToleranceScaleFactor
		{
			get { return this._dimensionStyleOverrides.ToleranceScaleFactor; }
			set
			{
				this._dimensionStyleOverrides.ToleranceScaleFactor = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for tolerance values
		/// </summary>
		/// <remarks>
		/// System variable DIMTZIN
		/// </remarks>
		[CadSystemVariable("$DIMTZIN", 70)]
		public ZeroHandling DimensionToleranceZeroHandling
		{
			get { return this._dimensionStyleOverrides.ToleranceZeroHandling; }
			set
			{
				this._dimensionStyleOverrides.ToleranceZeroHandling = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for alternate unit dimension values
		/// </summary>
		/// <remarks>
		/// System variable DIMUNIT
		/// </remarks>
		[CadSystemVariable("$DIMUNIT", 70)]
		public short DimensionUnit
		{
			get { return this._dimensionStyleOverrides.DimensionUnit; }
			set
			{
				this._dimensionStyleOverrides.DimensionUnit = value;
			}
		}

		/// <summary>
		/// Controls suppression of zeros for primary unit values
		/// </summary>
		/// <remarks>
		/// System variable DIMZIN
		/// </remarks>
		[CadSystemVariable("$DIMZIN", 70)]
		public ZeroHandling DimensionZeroHandling
		{
			get { return this._dimensionStyleOverrides.ZeroHandling; }
			set
			{
				this._dimensionStyleOverrides.ZeroHandling = value;
			}
		}

		/// <summary>
		/// Get the <see cref="DimensionStyle"/> override for this document.
		/// </summary>
		public DimensionStyle DimensionstyleOverrides { get { return this._dimensionStyleOverrides; } }

		/// <remarks>
		/// System variable LIGHTGLYPHDISPLAY
		/// </remarks>
		[CadSystemVariable("$LIGHTGLYPHDISPLAY", 280)]
		public char DisplayLightGlyphs { get; set; }

		/// <summary>
		/// Controls whether the lineweights of objects are displayed.<br/>
		/// 0 = Lineweight is not displayed<br/>
		/// 1 = Lineweight is displayed
		/// </summary>
		/// <remarks>
		/// System variable LWDISPLAY
		/// </remarks>
		[CadSystemVariable("$LWDISPLAY", 290)]
		public bool DisplayLineWeight { get; set; } = false;

		/// <summary>
		/// Controls display of silhouette edges of 3D solid and surface objects in the Wireframe or 2D Wireframe visual styles.
		/// </summary>
		/// <remarks>
		/// System variable DISPSILH.
		/// </remarks>
		[CadSystemVariable("$DISPSILH", 70)]
		public bool DisplaySilhouetteCurves { get; set; } = false;

		/// <summary>
		/// Document where this header resides
		/// </summary>
		public CadDocument Document { get; internal set; }

		/// <remarks>
		/// System variable LOFTANG1
		/// </remarks>
		[CadSystemVariable("$LOFTANG1", 40)]
		public double DraftAngleFirstCrossSection { get; set; }

		/// <remarks>
		/// System variable LOFTANG2
		/// </remarks>
		[CadSystemVariable("$LOFTANG2", 40)]
		public double DraftAngleSecondCrossSection { get; set; }

		/// <remarks>
		/// System variable LOFTMAG1
		/// </remarks>
		[CadSystemVariable("$LOFTMAG1", 40)]
		public double DraftMagnitudeFirstCrossSection { get; set; }

		/// <remarks>
		/// System variable LOFTMAG2
		/// </remarks>
		[CadSystemVariable("$LOFTMAG2", 40)]
		public double DraftMagnitudeSecondCrossSection { get; set; }

		/// <remarks>
		/// System variable 3DDWFPREC
		/// </remarks>
		[CadSystemVariable("$3DDWFPREC", 40)]
		public double Dw3DPrecision { get; set; }

		/// <remarks>
		/// System variable DWFFRAME
		/// </remarks>
		[CadSystemVariable("$DWFFRAME", 280)]
		public char DwgUnderlayFramesVisibility { get; set; }

		/// <summary>
		/// Current elevation set by ELEV command
		/// </summary>
		/// <remarks>
		/// System variable ELEVATION
		/// </remarks>
		[CadSystemVariable("$ELEVATION", 40)]
		public double Elevation
		{
			get { return this.ModelSpaceUcs.Elevation; }
			set
			{
				this.ModelSpaceUcs.Elevation = value;
			}
		}

		/// <summary>
		/// Line weight end-caps setting for new objects
		/// </summary>
		/// <remarks>
		/// System variable ENDCAPS
		/// </remarks>
		[CadSystemVariable("$ENDCAPS", 280)]
		public short EndCaps { get; set; }

		/// <summary>
		/// Controls the object sorting methods
		/// </summary>
		/// <remarks>
		/// System variable SORTENTS
		/// </remarks>
		[CadSystemVariable("$SORTENTS", 280)]
		public ObjectSortingFlags EntitySortingFlags { get; set; }

		/// <summary>
		/// Controls symbol table naming
		/// </summary>
		/// <remarks>
		/// System variable EXTNAMES
		/// </remarks>
		[CadSystemVariable("$EXTNAMES", 290)]
		public bool ExtendedNames { get; set; } = true;

		/// <summary>
		/// Extension line lineweight
		/// </summary>
		/// <remarks>
		/// System variable DIMLWE
		/// </remarks>
		[CadSystemVariable("$DIMLWE", 70)]
		public LineWeightType ExtensionLineWeight
		{
			get { return this._dimensionStyleOverrides.ExtensionLineWeight; }
			set
			{
				this._dimensionStyleOverrides.ExtensionLineWeight = value;
			}
		}

		/// <summary>
		/// Determines whether xref clipping boundaries are visible or plotted in the current drawing.
		/// </summary>
		/// <remarks>
		/// System variable XCLIPFRAME
		/// </remarks>
		[CadSystemVariable("$XCLIPFRAME", 280)]
		public XClipFrameType ExternalReferenceClippingBoundaryType { get; set; } = XClipFrameType.DisplayNotPlot;

		/// <summary>
		/// Adjusts the smoothness of shaded and rendered objects, rendered shadows, and objects with hidden lines removed.
		/// </summary>
		/// <remarks>
		/// System variable FACETRES.
		/// </remarks>
		/// <value>
		/// Valid values are from 0.01 to 10.0.
		/// </value>
		[CadSystemVariable("$FACETRES", 40)]
		public double FacetResolution
		{
			get
			{
				return this._facetResolution;
			}
			set
			{
				ObjectExtensions.InRange(value, 0.01, 10, "FACETRES valid values are from 0.01 to 10.0");
				this._facetResolution = value;
			}
		}

		/// <summary>
		/// Stores the current fillet radius for 2D objects.
		/// </summary>
		/// <remarks>
		/// System variable FILLETRAD.
		/// </remarks>
		[CadSystemVariable("$FILLETRAD", 40)]
		public double FilletRadius { get; set; } = 0.0d;

		/// <summary>
		/// Specifies whether hatches and fills, 2D solids, and wide polylines are filled in.
		/// </summary>
		/// <remarks>
		/// System variable FILLMODE.
		/// </remarks>
		[CadSystemVariable("$FILLMODE", 70)]
		public bool FillMode { get; set; } = true;

		/// <summary>
		/// Set at creation time, uniquely identifies a particular drawing
		/// </summary>
		/// <remarks>
		/// System variable FINGERPRINTGUID
		/// </remarks>
		[CadSystemVariable("$FINGERPRINTGUID", 2)]
		public string FingerPrintGuid { get; internal set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// Specifies a gap to be displayed where an object is hidden by another object; the value is specified as a percent of one unit and is independent of the zoom level.A haloed line is shortened at the point where it is hidden when HIDE or the Hidden option of SHADEMODE is used
		/// </summary>
		/// <remarks>
		/// System variable HALOGAP
		/// </remarks>
		[CadSystemVariable("$HALOGAP", 280)]
		public byte HaloGapPercentage { get; set; } = 0;

		/// <summary>
		/// Next available handle.
		/// </summary>
		/// <remarks>
		/// System variable HANDSEED.
		/// </remarks>
		[CadSystemVariable("$HANDSEED", 5)]
		public ulong HandleSeed { get; internal set; } = 0x01;

		/// <summary>
		/// Specifies HIDETEXT system variable
		/// </summary>
		/// <remarks>
		/// System variable HIDETEXT
		/// </remarks>
		[CadSystemVariable("$HIDETEXT", 280)]   //note: mismatch with docs, code 290
		public byte HideText { get; set; }

		/// <summary>
		/// Path for all relative hyperlinks in the drawing. If null, the drawing path is used
		/// </summary>
		/// <remarks>
		/// System variable HYPERLINKBASE
		/// </remarks>
		[CadSystemVariable("$HYPERLINKBASE", 1)]
		public string HyperLinkBase { get; set; }

		/// <summary>
		/// Controls whether layer and spatial indexes are created and saved in drawing files
		/// </summary>
		/// <remarks>
		/// System variable INDEXCTL
		/// </remarks>
		[CadSystemVariable("$INDEXCTL", 280)]
		public IndexCreationFlags IndexCreationFlags { get; set; }

		/// <summary>
		/// Default drawing units for blocks
		/// </summary>
		/// <remarks>
		/// System variable INSUNITS
		/// </remarks>
		[CadSystemVariable("$INSUNITS", 70)]
		public UnitsType InsUnits { get; set; } = UnitsType.Unitless;

		/// <summary>
		/// Represents the ACI color index of the "interference objects" created during the INTERFERE command. Default value is 1
		/// </summary>
		/// <remarks>
		/// System variable INTERFERECOLOR
		/// </remarks>
		[CadSystemVariable("$INTERFERECOLOR", 62)]
		public Color InterfereColor { get; set; } = new Color(1);

		public byte IntersectionDisplay { get; set; }

		/// <summary>
		/// Line weight joint setting for new objects
		/// </summary>
		/// <remarks>
		/// System variable JOINSTYLE
		/// </remarks>
		[CadSystemVariable("$JOINSTYLE", 280)]
		public short JoinStyle { get; set; }

		/// <summary>
		/// Displays the name of the last person who modified the file
		/// </summary>
		/// <remarks>
		/// System variable LASTSAVEDBY
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$LASTSAVEDBY", 3)]
		public string LastSavedBy { get; set; } = "ACadSharp";

		/// <summary>
		/// Specifies the latitude of the drawing model in decimal format.
		/// </summary>
		/// <remarks>
		/// System variable LATITUDE
		/// </remarks>
		[CadSystemVariable("$LATITUDE", 40)]
		public double Latitude { get; set; } = 37.7950d;

		/// <remarks>
		/// System variable LENSLENGTH
		/// </remarks>
		[CadSystemVariable("$LENSLENGTH", 40)]
		public double LensLength { get; set; }

		/// <summary>
		/// Controls whether you can create objects outside the grid limits.
		/// </summary>
		/// <remarks>
		/// System variable LIMCHECK.
		/// </remarks>
		[CadSystemVariable("$LIMCHECK", 70)]
		public bool LimitCheckingOn { get; set; } = false;

		/// <summary>
		/// Sets the linear units format for creating objects.
		/// </summary>
		/// <remarks>
		/// System variable LUNITS.
		/// </remarks>
		[CadSystemVariable("$LUNITS", 70)]
		public LinearUnitFormat LinearUnitFormat { get; set; } = LinearUnitFormat.Decimal;

		/// <summary>
		/// Sets the display precision for linear units and coordinates.
		/// </summary>
		/// <remarks>
		/// System variable LUPREC.
		/// </remarks>
		/// <value>
		/// Valid values are from 0 to 8.
		/// </value>
		[CadSystemVariable("$LUPREC", 70)]
		public short LinearUnitPrecision
		{
			get
			{
				return this._linearUnitPrecision;
			}
			set
			{
				ObjectExtensions.InRange(value, 0, 8, "LUPREC valid values are from 0 to 8");
				this._linearUnitPrecision = value;
			}
		}

		/// <summary>
		/// Sets the global linetype scale factor.
		/// </summary>
		/// <remarks>
		/// System variable LTSCALE.
		/// </remarks>
		[CadSystemVariable("$LTSCALE", 40)]
		public double LineTypeScale { get; set; } = 1.0d;

		/// <remarks>
		/// System variable OLESTARTUP
		/// </remarks>
		//[CadSystemVariable("$OLESTARTUP", 290)]
		public bool LoadOLEObject { get; set; }

		/// <remarks>
		/// System variable LOFTNORMALS
		/// </remarks>
		[CadSystemVariable("$LOFTNORMALS", 280)]
		public char LoftedObjectNormals { get; set; }

		/// <summary>
		/// Specifies the longitude of the drawing model in decimal format.
		/// </summary>
		/// <remarks>
		/// System variable LONGITUDE
		/// </remarks>
		[CadSystemVariable("$LONGITUDE", 40)]
		public double Longitude { get; set; } = -122.394d;

		/// <summary>
		/// Maintenance version number(should be ignored)
		/// </summary>
		/// <remarks>
		/// System variable ACADMAINTVER.
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$ACADMAINTVER", 70)]
		public short MaintenanceVersion { get; internal set; }

		/// <summary>
		/// Sets the maximum number of viewports that can be active at one time in a layout.
		/// </summary>
		/// <remarks>
		/// System variable MAXACTVP.
		/// </remarks>
		[CadSystemVariable("$MAXACTVP", 70)]
		public short MaxViewportCount { get; set; } = 64;

		/// <summary>
		/// Controls whether the current drawing uses imperial or metric hatch pattern and linetype files.
		/// </summary>
		/// <remarks>
		/// System variable MEASUREMENT
		/// </remarks>
		[CadSystemVariable("$MEASUREMENT", 70)]
		public MeasurementUnits MeasurementUnits { get; set; } = MeasurementUnits.Metric;

		/// <summary>
		/// Name of menu file.
		/// </summary>
		/// <remarks>
		/// System variable MENU.
		/// </remarks>
		[CadSystemVariable("$MENU", 1)]
		public string MenuFileName { get; set; } = ".";

		/// <summary>
		/// Controls how MIRROR reflects text.
		/// </summary>
		/// <remarks>
		/// System variable MIRRTEXT.
		/// </remarks>
		[CadSystemVariable("$MIRRTEXT", 70)]
		public bool MirrorText { get; set; } = false;

		/// <summary>
		/// X, Y, and Z drawing extents upper-right corner(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable EXTMAX
		/// </remarks>
		[CadSystemVariable("$EXTMAX", 10, 20, 30)]
		public XYZ ModelSpaceExtMax { get; set; }

		/// <summary>
		/// X, Y, and Z drawing extents lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable EXTMIN
		/// </remarks>
		[CadSystemVariable("$EXTMIN", 10, 20, 30)]
		public XYZ ModelSpaceExtMin { get; set; }

		/// <summary>
		/// Insertion base set by BASE command(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable INSBASE
		/// </remarks>
		[CadSystemVariable("$INSBASE", 10, 20, 30)]
		public XYZ ModelSpaceInsertionBase { get; set; } = XYZ.Zero;

		/// <summary>
		/// XY drawing limits upper-right corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMAX
		/// </remarks>
		[CadSystemVariable("$LIMMAX", 10, 20)]
		public XY ModelSpaceLimitsMax { get; set; }

		/// <summary>
		/// XY drawing limits lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMIN
		/// </remarks>
		[CadSystemVariable("$LIMMIN", 10, 20)]
		public XY ModelSpaceLimitsMin { get; set; }

		/// <summary>
		/// Origin of current UCS(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSORG
		/// </remarks>
		[CadSystemVariable("$UCSORG", 10, 20, 30)]
		public XYZ ModelSpaceOrigin
		{
			get { return this.ModelSpaceUcs.Origin; }
			set
			{
				this.ModelSpaceUcs.Origin = value;
			}
		}

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to BACK when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGBACK
		/// </remarks>
		[CadSystemVariable("$UCSORGBACK", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicBackDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to BOTTOM when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGBOTTOM
		/// </remarks>
		[CadSystemVariable("$UCSORGBOTTOM", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicBottomDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to FRONT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGFRONT
		/// </remarks>
		[CadSystemVariable("$UCSORGFRONT", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicFrontDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to LEFT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGLEFT
		/// </remarks>
		[CadSystemVariable("$UCSORGLEFT", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicLeftDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to RIGHT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGRIGHT
		/// </remarks>
		[CadSystemVariable("$UCSORGRIGHT", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicRightDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to TOP when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGTOP
		/// </remarks>
		[CadSystemVariable("$UCSORGTOP", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicTopDOrigin { get; set; }

		//TODO: How header UCS work??
		public UCS ModelSpaceUcs { get; private set; } = new UCS();

		public UCS ModelSpaceUcsBase { get; private set; } = new UCS();

		/// <summary>
		/// Direction of the current UCS X axis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSXDIR
		/// </remarks>
		[CadSystemVariable("$UCSXDIR", 10, 20, 30)]
		public XYZ ModelSpaceXAxis
		{
			get { return this.ModelSpaceUcs.XAxis; }
			set
			{
				this.ModelSpaceUcs.XAxis = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS Y axis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSYDIR
		/// </remarks>
		[CadSystemVariable("$UCSYDIR", 10, 20, 30)]
		public XYZ ModelSpaceYAxis
		{
			get { return this.ModelSpaceUcs.YAxis; }
			set
			{
				this.ModelSpaceUcs.YAxis = value;
			}
		}

		/// <remarks>
		/// System variable NORTHDIRECTION
		/// </remarks>
		[CadSystemVariable("$NORTHDIRECTION", 40)]
		public double NorthDirection { get; set; }

		/// <summary>
		/// Sets the number of line segments to be generated for each spline-fit polyline generated by the Spline option of the PEDIT command.
		/// </summary>
		/// <remarks>
		/// System variable SPLINESEGS.
		/// </remarks>
		[CadSystemVariable("$SPLINESEGS", 70)]
		public short NumberOfSplineSegments { get; set; } = 8;

		/// <summary>
		/// Sets running object snaps.
		/// </summary>
		/// <remarks>
		/// System variable OSMODE.
		/// </remarks>
		[CadSystemVariable("$OSMODE", 70)]
		public ObjectSnapMode ObjectSnapMode { get; set; } = (ObjectSnapMode)4133;

		public Color ObscuredColor { get; set; }

		/// <remarks>
		/// System variable OBSCUREDLTYPE
		/// </remarks>
		public byte ObscuredType { get; set; }

		/// <summary>
		/// Constrains cursor movement to the perpendicular.
		/// </summary>
		/// <remarks>
		/// System variable ORTHOMODE.
		/// </remarks>
		[CadSystemVariable("$ORTHOMODE", 70)]
		public bool OrthoMode { get; set; } = false;

		/// <summary>
		/// Name of the UCS that defines the origin and orientation of orthographic UCS settings (paper space only)
		/// </summary>
		/// <remarks>
		/// System variable PUCSBASE
		/// </remarks>
		[CadSystemVariable("$PUCSBASE", true, 2)]
		public string PaperSpaceBaseName
		{
			get { return this.PaperSpaceUcsBase.Name; }
			set
			{
				this.PaperSpaceUcsBase.Name = value;
			}
		}

		/// <summary>
		/// Current elevation set by ELEV command
		/// </summary>
		/// <remarks>
		/// System variable PELEVATION
		/// </remarks>
		[CadSystemVariable("$PELEVATION", 40)]
		public double PaperSpaceElevation
		{
			get { return this.PaperSpaceUcs.Elevation; }
			set
			{
				this.PaperSpaceUcs.Elevation = value;
			}
		}

		/// <summary>
		/// X, Y, and Z drawing extents upper-right corner(in WCS).
		/// </summary>
		/// <remarks>
		/// System variable PEXTMAX.
		/// </remarks>
		[CadSystemVariable("$PEXTMAX", 10, 20, 30)]
		public XYZ PaperSpaceExtMax { get; set; } = XYZ.Zero;

		/// <summary>
		/// X, Y, and Z drawing extents lower-left corner (in WCS).
		/// </summary>
		/// <remarks>
		/// System variable PEXTMIN.
		/// </remarks>
		[CadSystemVariable("$PEXTMIN", 10, 20, 30)]
		public XYZ PaperSpaceExtMin { get; set; } = XYZ.Zero;

		/// <summary>
		/// Paper space insertion base point.
		/// </summary>
		/// <remarks>
		/// System variable PINSBASE.
		/// </remarks>
		[CadSystemVariable("$PINSBASE", 10, 20, 30)]
		public XYZ PaperSpaceInsertionBase { get; set; } = XYZ.Zero;

		/// <summary>
		/// Limits checking in paper space when nonzero.
		/// </summary>
		/// <remarks>
		/// System variable PLIMCHECK.
		/// </remarks>
		[CadSystemVariable("$PLIMCHECK", 70)]
		public bool PaperSpaceLimitsChecking { get; set; } = false;

		/// <summary>
		/// XY drawing limits upper-right corner (in WCS).
		/// </summary>
		/// <remarks>
		/// System variable PLIMMAX.
		/// </remarks>
		[CadSystemVariable("$PLIMMAX", 10, 20)]
		public XY PaperSpaceLimitsMax { get; set; } = XY.Zero;

		/// <summary>
		/// XY drawing limits lower-left corner(in WCS).
		/// </summary>
		/// <remarks>
		/// System variable PLIMMIN.
		/// </remarks>
		[CadSystemVariable("$PLIMMIN", 10, 20)]
		public XY PaperSpaceLimitsMin { get; set; } = XY.Zero;

		/// <summary>
		/// Controls paper space linetype scaling.
		/// </summary>
		/// <remarks>
		/// System variable PSLTSCALE.
		/// </remarks>
		[CadSystemVariable("$PSLTSCALE", 70)]
		public SpaceLineTypeScaling PaperSpaceLineTypeScaling { get; set; } = SpaceLineTypeScaling.Normal;

		/// <summary>
		/// Current paper space UCS name
		/// </summary>
		/// <remarks>
		/// System variable PUCSNAME
		/// </remarks>
		[CadSystemVariable("$PUCSNAME", true, 2)]
		public string PaperSpaceName
		{
			get { return this.PaperSpaceUcs.Name; }
			set
			{
				this.PaperSpaceUcs.Name = value;
			}
		}

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to BACK when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGBACK
		/// </remarks>
		[CadSystemVariable("$PUCSORGBACK", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicBackDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to BOTTOM when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGBOTTOM
		/// </remarks>
		[CadSystemVariable("$PUCSORGBOTTOM", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicBottomDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to FRONT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGFRONT
		/// </remarks>
		[CadSystemVariable("$PUCSORGFRONT", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicFrontDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to LEFT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGLEFT
		/// </remarks>
		[CadSystemVariable("$PUCSORGLEFT", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicLeftDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to RIGHT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGRIGHT
		/// </remarks>
		[CadSystemVariable("$PUCSORGRIGHT", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicRightDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to TOP when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGTOP
		/// </remarks>
		[CadSystemVariable("$PUCSORGTOP", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicTopDOrigin { get; set; }

		public UCS PaperSpaceUcs { get; private set; } = new UCS();

		public UCS PaperSpaceUcsBase { get; private set; } = new UCS();

		/// <summary>
		/// Origin of current UCS (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PUCSORG
		/// </remarks>
		[CadSystemVariable("$PUCSORG", 10, 20, 30)]
		public XYZ PaperSpaceUcsOrigin
		{
			get { return this.PaperSpaceUcs.Origin; }
			set
			{
				this.PaperSpaceUcs.Origin = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS X axis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PUCSXDIR
		/// </remarks>
		[CadSystemVariable("$PUCSXDIR", 10, 20, 30)]
		public XYZ PaperSpaceUcsXAxis
		{
			get { return this.PaperSpaceUcs.XAxis; }
			set
			{
				this.PaperSpaceUcs.XAxis = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS Y aYis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PUCSYDIR
		/// </remarks>
		[CadSystemVariable("$PUCSYDIR", 10, 20, 30)]
		public XYZ PaperSpaceUcsYAxis
		{
			get { return this.PaperSpaceUcs.YAxis; }
			set
			{
				this.PaperSpaceUcs.YAxis = value;
			}
		}

		/// <summary>
		/// Indicates whether the current drawing is in a Color-Dependent or Named Plot Style mode
		/// </summary>
		/// <remarks>
		/// System variable PSTYLEMODE
		/// </remarks>
		[CadSystemVariable("$PSTYLEMODE", 290)]
		public short PlotStyleMode { get; set; }

		/// <summary>
		/// Controls how point objects are displayed.
		/// </summary>
		/// <remarks>
		/// System variable PDMODE.
		/// </remarks>
		[CadSystemVariable("$PDMODE", 70)]
		public short PointDisplayMode { get; set; } = 0;

		/// <summary>
		/// Sets the display size for point objects.
		/// </summary>
		/// <remarks>
		/// System variable PDSIZE.
		/// </remarks>
		[CadSystemVariable("$PDSIZE", 40)]
		public double PointDisplaySize { get; set; } = 0.0d;

		/// <summary>
		/// Governs the generation of linetype patterns around the vertices of a 2D polyline:<br/>
		/// 1 = Linetype is generated in a continuous pattern around vertices of the polyline<br/>
		/// 0 = Each segment of the polyline starts and ends with a dash
		/// </summary>
		/// <remarks>
		/// System variable PLINEGEN
		/// </remarks>
		[CadSystemVariable("$PLINEGEN", 70)]
		public bool PolylineLineTypeGeneration { get; set; } = false;

		/// <summary>
		/// Stores the default polyline width.
		/// </summary>
		/// <remarks>
		/// System variable PLINEWID.
		/// </remarks>
		[CadSystemVariable("$PLINEWID", 40)]
		public double PolylineWidthDefault { get; set; } = 0.0d;

		/// <summary>
		/// Assigns a project name to the current drawing. Used when an external reference or image is not found on its original path. The project name points to a section in the registry that can contain one or more search paths for each project name defined. Project names and their search directories are created from the Files tab of the Options dialog box
		/// </summary>
		/// <remarks>
		/// System variable PROJECTNAME
		/// </remarks>
		[CadSystemVariable("$PROJECTNAME", 1)]
		public string ProjectName { get; set; }

		/// <summary>
		/// Specifies whether images of proxy objects are saved in the drawing.
		/// </summary>
		/// <remarks>
		/// System variable PROXYGRAPHICS.
		/// </remarks>
		[CadSystemVariable("$PROXYGRAPHICS", 70)]
		public bool ProxyGraphics { get; set; } = true;

		/// <summary>
		/// Quick Text mode on if nonzero
		/// </summary>
		/// <remarks>
		/// System variable QTEXTMODE.
		/// </remarks>
		[CadSystemVariable("$QTEXTMODE", 70)]
		public bool QuickTextMode { get; set; } = false;

		/// <summary>
		/// Obsolete. Controls automatic regeneration of the drawing.
		/// </summary>
		/// <remarks>
		/// System variable REGENMODE.
		/// </remarks>
		[CadSystemVariable("$REGENMODE", 70)]
		public bool RegenerationMode { get; set; } = true;

		/// <summary>
		/// The default value is 0.
		/// Read only.
		/// </summary>
		/// <remarks>
		/// System variable REQUIREDVERSIONS <br/>
		/// Only in <see cref="ACadVersion.AC1024"/> or above
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$REQUIREDVERSIONS", 70)]
		public long RequiredVersions { get; set; }

		/// <summary>
		/// Controls the properties of xref-dependent layers: <br/>
		/// 0 = Don't retain xref-dependent visibility settings <br/>
		/// 1 = Retain xref-dependent visibility settings <br/>
		/// </summary>
		/// <remarks>
		/// System variable VISRETAIN.
		/// </remarks>
		[CadSystemVariable("$VISRETAIN", 70)]
		public bool RetainXRefDependentVisibilitySettings { get; set; } = true;

		/// <summary>
		/// Sets the ratio of diffuse reflective light to ambient light.
		/// </summary>
		/// <remarks>
		/// System variable SHADEDIF.
		/// </remarks>
		/// <value>
		/// range 1-100
		/// </value>
		[CadSystemVariable("$SHADEDIF", 70)]
		public short ShadeDiffuseToAmbientPercentage { get; set; } = 70;

		/// <summary>
		/// Controls the shading of edges.
		/// </summary>
		/// <remarks>
		/// System variable SHADEDGE.
		/// </remarks>
		[CadSystemVariable("$SHADEDGE", 70)]
		public ShadeEdgeType ShadeEdge { get; set; } = ShadeEdgeType.FacesInEntityColorEdgesInBlack;

		/// <summary>
		/// Shadow mode for a 3D object
		/// </summary>
		/// <remarks>
		/// System variable CSHADOW
		/// </remarks>
		[CadSystemVariable("$CSHADOW", 280)]
		public ShadowMode ShadowMode { get; set; }

		/// <summary>
		/// Location of the ground shadow plane. This is a Z axis ordinate
		/// </summary>
		/// <remarks>
		/// System variable SHADOWPLANELOCATION
		/// </remarks>
		[CadSystemVariable("$SHADOWPLANELOCATION", 40)]
		public double ShadowPlaneLocation { get; set; }

		/// <summary>
		/// Determines whether the Model tab or the most-recently accessed named layout tab is active.
		/// </summary>
		/// <remarks>
		/// System variable TILEMODE.
		/// </remarks>
		[CadSystemVariable("$TILEMODE", 70)]
		public bool ShowModelSpace { get; set; }

		/// <remarks>
		/// System variable SHOWHIST
		/// </remarks>
		[CadSystemVariable("$SHOWHIST", 280)]
		public char ShowSolidsHistory { get; set; }

		/// <summary>
		/// Controls the display of helixes and smoothed mesh objects.
		/// </summary>
		/// <remarks>
		/// System variable SPLFRAME.
		/// </remarks>
		[CadSystemVariable("$SPLFRAME", 70)]
		public bool ShowSplineControlPoints { get; set; } = false;

		/// <summary>
		/// Sketch record increment.
		/// </summary>
		/// <remarks>
		/// System variable SKETCHINC.
		/// </remarks>
		[CadSystemVariable("$SKETCHINC", 40)]
		public double SketchIncrement { get; set; } = 1.0d;

		/// <summary>
		/// Determines the object type created by the SKETCH command.
		/// </summary>
		/// <remarks>
		/// System variable SKPOLY.
		/// </remarks>
		[CadSystemVariable("$SKPOLY", 70)]
		public bool SketchPolylines { get; set; } = false;

		/// <remarks>
		/// System variable LOFTPARAM
		/// </remarks>
		[CadSystemVariable("$LOFTPARAM", 70)]
		public short SolidLoftedShape { get; set; }

		/// <remarks>
		/// System variable SOLIDHIST
		/// </remarks>
		[CadSystemVariable("$SOLIDHIST", 280)]
		public char SolidsRetainHistory { get; set; }

		/// <summary>
		/// Specifies the maximum depth, that is, the number of times the tree-structured spatial index can divide into branches.
		/// </summary>
		/// <remarks>
		/// System variable TREEDEPTH.
		/// </remarks>
		[CadSystemVariable("$TREEDEPTH", 70)]
		public short SpatialIndexMaxTreeDepth { get; set; } = 3020;

		/// <summary>
		/// Sets the type of curve generated by the Spline option of the PEDIT command.
		/// </summary>
		/// <remarks>
		/// System variable SPLINETYPE.
		/// </remarks>
		[CadSystemVariable("$SPLINETYPE", 70)]
		public SplineType SplineType { get; set; } = SplineType.CubicBSpline;

		/// <remarks>
		/// System variable TSTACKALIGN, default = 1(not present in DXF)
		/// </remarks>
		public short StackedTextAlignment { get; internal set; } = 1;

		/// <remarks>
		/// TSTACKSIZE, default = 70(not present in DXF)
		/// </remarks>
		public short StackedTextSizePercentage { get; internal set; } = 70;

		/// <summary>
		/// Specifies the size of each step when in walk or fly mode, in drawing units.
		/// </summary>
		/// <remarks>
		/// System variable STEPSIZE
		/// </remarks>
		[CadSystemVariable("$STEPSIZE", 40)]
		public double StepSize { get; set; } = 6.0d;

		/// <summary>
		/// Specifies the number of steps taken per second when you are in walk or fly mode.
		/// </summary>
		/// <remarks>
		/// System variable STEPSPERSEC
		/// </remarks>
		/// <value>
		/// Valid values are from 1 to 30.
		/// </value>
		[CadSystemVariable("$STEPSPERSEC", 40)]
		public double StepsPerSecond
		{
			get
			{
				return this._stepsPerSecond;
			}
			set
			{
				ObjectExtensions.InRange(value, 1, 30, "STEPSPERSEC valid values are from 1 to 30");
				this._stepsPerSecond = value;
			}
		}

		/// <remarks>
		/// System variable STYLESHEET
		/// </remarks>
		[CadSystemVariable("$STYLESHEET", 1)]
		public string StyleSheetName { get; set; }

		/// <summary>
		/// Surface density (for PEDIT Smooth) in M direction.
		/// </summary>
		/// <remarks>
		/// System variable SURFU.
		/// </remarks>
		[CadSystemVariable("$SURFU", 70)]
		public short SurfaceDensityU { get; set; } = 6;

		/// <summary>
		/// Surface density(for PEDIT Smooth) in N direction.
		/// </summary>
		/// <remarks>
		/// System variable SURFV.
		/// </remarks>
		[CadSystemVariable("$SURFV", 70)]
		public short SurfaceDensityV { get; set; } = 6;

		/// <summary>
		/// Specifies the number of contour lines displayed on the curved surfaces of 3D solids.
		/// </summary>
		/// <remarks>
		/// System variable ISOLINES.
		/// </remarks>
		/// <value>
		/// Valid values are from 0 to 2047.
		/// </value>
		public short SurfaceIsolineCount
		{
			get
			{
				return this._surfaceIsolineCount;
			}
			set
			{
				ObjectExtensions.InRange(value, 0, 2047, "ISOLINES valid values are from 0 to 2047");
				this._surfaceIsolineCount = value;
			}
		}

		/// <summary>
		/// Number of mesh tabulations in first direction.
		/// </summary>
		/// <remarks>
		/// System variable SURFTAB1.
		/// </remarks>
		[CadSystemVariable("$SURFTAB1", 70)]
		public short SurfaceMeshTabulationCount1 { get; set; } = 6;

		/// <summary>
		/// Number of mesh tabulations in second direction.
		/// </summary>
		/// <remarks>
		/// System variable SURFTAB2.
		/// </remarks>
		[CadSystemVariable("$SURFTAB2", 70)]
		public short SurfaceMeshTabulationCount2 { get; set; } = 6;

		/// <summary>
		/// Surface type for PEDIT Smooth.
		/// </summary>
		/// <remarks>
		/// System variable SURFTYPE.
		/// </remarks>
		[CadSystemVariable("$SURFTYPE", 70)]
		public short SurfaceType { get; set; } = 6;

		/// <remarks>
		/// System variable PSOLHEIGHT
		/// </remarks>
		[CadSystemVariable("$PSOLHEIGHT", 40)]
		public double SweptSolidHeight { get; set; }

		/// <remarks>
		/// System variable PSOLWIDTH
		/// </remarks>
		[CadSystemVariable("$PSOLWIDTH", 40)]
		public double SweptSolidWidth { get; set; }

		/// <summary>
		/// Sets the default text height when creating new text objects.
		/// </summary>
		/// <remarks>
		/// System variable TEXTSIZE.
		/// </remarks>
		[CadSystemVariable("$TEXTSIZE", 40)]
		public double TextHeightDefault { get; set; } = 2.5d;

		/// <summary>
		/// Sets the resolution of TrueType text for plotting and rendering.
		/// </summary>
		/// <remarks>
		/// System variable TEXTQLTY.
		/// </remarks>
		/// <value>
		/// Valid values are from 1 to 100.
		/// </value>
		public short TextQuality
		{
			get
			{
				return this._textQuality;
			}
			set
			{
				ObjectExtensions.InRange(value, 0, 100, "TEXTQLTY valid values are from 0 to 100");
				this._textQuality = value;
			}
		}

		/// <summary>
		/// Sets the name of the current text style.
		/// </summary>
		/// <remarks>
		/// System variable TEXTSTYLE.
		/// </remarks>
		[CadSystemVariable("$TEXTSTYLE", true, 7)]
		public string CurrentTextStyleName
		{
			get { return this._currentTextStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this._currentTextStyle = this.Document.TextStyles[value];
				}
				else
				{
					this._currentTextStyle = new TextStyle(value);
				}
			}
		}

		/// <summary>
		/// Sets the default 3D thickness property when creating 2D geometric objects.
		/// </summary>
		/// <remarks>
		/// System variable THICKNESS.
		/// </remarks>
		[CadSystemVariable("$THICKNESS", 40)]
		public double ThicknessDefault { get; set; } = 0.0d;

		/// <summary>
		/// Sets the time zone for the sun in the drawing.
		/// </summary>
		/// <remarks>
		/// The values in the table are expressed as hours and minutes away from Greenwich Mean Time. You can also change this value in the Geographic Location dialog box when you set or edit geographic location information for the drawing file.
		/// <br/>
		/// System variable TIMEZONE
		/// </remarks>
		[CadSystemVariable("$TIMEZONE", 70)]
		public int TimeZone { get; set; } = 0;

		/// <summary>
		/// Cumulative editing time for this drawing(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDINDWG
		/// </remarks>
		[CadSystemVariable("$TDINDWG", 40)]
		public TimeSpan TotalEditingTime { get; set; } = new TimeSpan();

		/// <summary>
		/// Default trace width.
		/// </summary>
		/// <remarks>
		/// System variable TRACEWID.
		/// </remarks>
		[CadSystemVariable("$TRACEWID", 40)]
		public double TraceWidthDefault { get; set; }

		/// <summary>
		/// Name of the UCS that defines the origin and orientation of orthographic UCS settings
		/// </summary>
		/// <remarks>
		/// System variable UCSBASE
		/// </remarks>
		[CadSystemVariable("$UCSBASE", true, 2)]
		public string UcsBaseName
		{
			get { return this.ModelSpaceUcsBase.Name; }
			set
			{
				this.ModelSpaceUcsBase.Name = value;
			}
		}

		/// <summary>
		/// Name of current UCS
		/// </summary>
		/// <remarks>
		/// System variable UCSNAME
		/// </remarks>
		[CadSystemVariable("$UCSNAME", true, 2)]
		public string UcsName
		{
			get { return this.ModelSpaceUcs.Name; }
			set
			{
				this.ModelSpaceUcs.Name = value;
			}
		}

		/// <summary>
		/// Controls the display format for units.
		/// </summary>
		/// <remarks>
		/// System variable UNITMODE.
		/// </remarks>
		[CadSystemVariable("$UNITMODE", 70)]
		public short UnitMode { get; set; } = 0;

		/// <summary>
		/// Universal date/time the drawing was created(see Special Handling of Date/Time Variables).
		/// </summary>
		/// <remarks>
		/// System variable TDUCREATE.
		/// </remarks>
		[CadSystemVariable("$TDUCREATE", 40)]
		public DateTime UniversalCreateDateTime { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Universal date/time of the last update/save(see Special Handling of Date/Time Variables).
		/// </summary>
		/// <remarks>
		/// System variable TDUUPDATE.
		/// </remarks>
		[CadSystemVariable("$TDUUPDATE", 40)]
		public DateTime UniversalUpdateDateTime { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Local date/time of last drawing update(see Special Handling of Date/Time Variables).
		/// </summary>
		/// <remarks>
		/// System variable TDUPDATE.
		/// </remarks>
		[CadSystemVariable("$TDUPDATE", 40)]
		public DateTime UpdateDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// System variable DIMSHO
		/// </summary>
		[CadSystemVariable("$DIMSHO", 70)]
		public bool UpdateDimensionsWhileDragging { get; set; } = true;

		/// <summary>
		/// Real variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERR1.
		/// </remarks>
		[CadSystemVariable("$USERR1", 40)]
		public double UserDouble1 { get; set; }

		/// <summary>
		/// Real variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERR2.
		/// </remarks>
		[CadSystemVariable("$USERR2", 40)]
		public double UserDouble2 { get; set; }

		/// <summary>
		/// Real variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERR3.
		/// </remarks>
		[CadSystemVariable("$USERR3", 40)]
		public double UserDouble3 { get; set; }

		/// <summary>
		/// Real variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERR4.
		/// </remarks>
		[CadSystemVariable("$USERR4", 40)]
		public double UserDouble4 { get; set; }

		/// <summary>
		/// Real variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERR5.
		/// </remarks>
		[CadSystemVariable("$USERR5", 40)]
		public double UserDouble5 { get; set; }

		/// <summary>
		/// User-elapsed timer
		/// </summary>
		/// <remarks>
		/// System variable TDUSRTIMER
		/// </remarks>
		[CadSystemVariable("$TDUSRTIMER", 40)]
		public TimeSpan UserElapsedTimeSpan { get; set; }

		/// <summary>
		/// Integer variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERI1.
		/// </remarks>
		[CadSystemVariable("$USERI1", 70)]
		public short UserShort1 { get; set; } = 0;

		/// <summary>
		/// Integer variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERI2.
		/// </remarks>
		[CadSystemVariable("$USERI2", 70)]
		public short UserShort2 { get; set; } = 0;

		/// <summary>
		/// Integer variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERI3.
		/// </remarks>
		[CadSystemVariable("$USERI3", 70)]
		public short UserShort3 { get; set; } = 0;

		/// <summary>
		/// Integer variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERI4.
		/// </remarks>
		[CadSystemVariable("$USERI4", 70)]
		public short UserShort4 { get; set; } = 0;

		/// <summary>
		/// Integer variable intended for use by third-party developers.
		/// </summary>
		/// <remarks>
		/// System variable USERI5.
		/// </remarks>
		[CadSystemVariable("$USERI5", 70)]
		public short UserShort5 { get; set; } = 0;

		/// <summary>
		/// Controls the user timer for the drawing.
		/// </summary>
		/// <remarks>
		/// System variable USRTIMER.
		/// </remarks>
		[CadSystemVariable("$USRTIMER", 70)]
		public bool UserTimer { get; set; } = false;

		public ACadVersion Version
		{
			get { return this._version; }
			set
			{
				this._version = value;

				//Values are relevant for the dwgWriter, manually checked form dxf
				switch (value)
				{
					case ACadVersion.AC1015:
						this.MaintenanceVersion = 20;
						break;
					case ACadVersion.AC1018:
						this.MaintenanceVersion = 104;
						break;
					case ACadVersion.AC1021:
						this.MaintenanceVersion = 50;
						break;
					case ACadVersion.AC1024:
						this.MaintenanceVersion = 226;
						break;
					case ACadVersion.AC1027:
						this.MaintenanceVersion = 125;
						break;
					case ACadVersion.AC1032:
						this.MaintenanceVersion = 228;
						break;
					default:
						this.MaintenanceVersion = 0;
						break;
				}
			}
		}

		/// <summary>
		/// Uniquely identifies a particular version of a drawing. Updated when the drawing is modified
		/// </summary>
		/// <remarks>
		/// System variable VERSIONGUID
		/// </remarks>
		[CadSystemVariable("$VERSIONGUID", 2)]
		public string VersionGuid { get; internal set; } = Guid.NewGuid().ToString();

		/// <summary>
		/// The Drawing database version number.
		/// </summary>
		/// <remarks>
		/// System variable ACADVER.
		/// </remarks>
		[CadSystemVariable("$ACADVER", DxfCode.Text)]
		public string VersionString
		{
			get { return this.Version.ToString(); }
			set
			{
				this.Version = CadUtils.GetVersionFromName(value);
			}
		}

		/// <summary>
		/// View scale factor for new viewports.
		/// </summary>
		/// <remarks>
		/// System variable PSVPSCALE.
		/// </remarks>
		[CadSystemVariable("$PSVPSCALE", 40)]
		public double ViewportDefaultViewScaleFactor { get; set; }

		/// <summary>
		/// Determines whether input to the DVIEW and VPOINT commands is relative to the WCS (default) or the current UCS.
		/// </summary>
		/// <remarks>
		/// System variable WORLDVIEW.
		/// </remarks>
		[CadSystemVariable("$WORLDVIEW", 70)]
		public bool WorldView { get; set; } = true;

		/// <summary>
		/// Controls whether the current drawing can be edited in-place when being referenced by another drawing
		/// </summary>
		/// <remarks>
		/// System variable XEDIT
		/// </remarks>
		[CadSystemVariable("$XEDIT", 290)]
		public bool XEdit { get; set; }

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMSAV
		/// </remarks>
		internal bool DIMSAV { get; set; }

		//Create enum for variable
		private static readonly PropertyExpression<CadHeader, CadSystemVariableAttribute> _propertyCache;

		private short _angularUnitPrecision = 0;
		private Layer _currentLayer = Layer.Default;
		private LineType _currentLineType = LineType.ByLayer;
		private MLineStyle _currentMLineStyle = MLineStyle.Default;
		private TextStyle _currentTextStyle = TextStyle.Default;
		private DimensionStyle _dimensionStyleOverrides = new DimensionStyle("override");
		private DimensionStyle _currentDimensionStyle = DimensionStyle.Default;
		private TextStyle _dimensionTextStyle = TextStyle.Default;
		private double _facetResolution = 0.5;
		private short _linearUnitPrecision = 4;
		private double _stepsPerSecond = 2.0d;
		private short _surfaceIsolineCount = 4;
		private short _textQuality = 50;
		private ACadVersion _version = ACadVersion.AC1032;

		static CadHeader()
		{
			_propertyCache = new PropertyExpression<CadHeader, CadSystemVariableAttribute>(
				(info, attribute) => attribute.Name);
		}

		public CadHeader() : this(ACadVersion.AC1032)
		{
		}

		public CadHeader(CadDocument document) : this(ACadVersion.AC1032)
		{
			this.Document = document;
		}

		public CadHeader(ACadVersion version)
		{
			this.Version = version;
		}

		/// <summary>
		/// Gets a map of all the system variables and it's codes
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, CadSystemVariable> GetHeaderMap()
		{
			Dictionary<string, CadSystemVariable> map = new Dictionary<string, CadSystemVariable>();
			foreach (PropertyInfo p in typeof(CadHeader).GetProperties())
			{
				CadSystemVariableAttribute att = p.GetCustomAttribute<CadSystemVariableAttribute>();
				if (att == null)
					continue;

				map.Add(att.Name, new CadSystemVariable(p));
			}

			return map;
		}

		public object GetValue(string systemvar)
		{
			var prop = _propertyCache.GetProperty(systemvar);
			return prop.Getter(this);
		}

		/// <summary>
		/// Get the primitive values in each dxf code
		/// </summary>
		/// <param name="systemvar"></param>
		/// <returns>dictionary with the codes and values</returns>
		public Dictionary<DxfCode, object> GetValues(string systemvar)
		{
			Dictionary<DxfCode, object> value = null;

			foreach (PropertyInfo p in this.GetType().GetProperties())
			{
				CadSystemVariableAttribute att = p.GetCustomAttribute<CadSystemVariableAttribute>();
				if (att == null)
					continue;

				if (att.Name == systemvar)
				{
					value = new Dictionary<DxfCode, object>();

					if (att.ValueCodes.Length == 1)
					{
						value.Add(att.ValueCodes[0], p.GetValue(this));
					}
					else
					{
						IVector vector = (IVector)p.GetValue(this);
						for (int i = 0; i < vector.Dimension; i++)
						{
							value.Add(att.ValueCodes[i], vector[i]);
						}
					}

					break;
				}
			}

			return value;
		}

		/// <summary>
		/// Set a value of a system variable by name
		/// </summary>
		/// <param name="systemvar">name of the system var</param>
		/// <param name="values">parameters for the constructor of the value</param>
		public void SetValue(string systemvar, params object[] values)
		{
			PropertyExpression<CadHeader, CadSystemVariableAttribute>.Prop prop = _propertyCache.GetProperty(systemvar);

			ConstructorInfo constr = prop.Property.PropertyType.GetConstructor(values.Select(o => o.GetType()).ToArray());

			if (prop.Property.PropertyType.IsEnum)
			{
				int v = Convert.ToInt32(values.First());
				prop.Setter(this, Enum.ToObject(prop.Property.PropertyType, v));
			}
			else if (prop.Property.PropertyType.IsEquivalentTo(typeof(DateTime)))
			{
				double jvalue = (double)values.First();

				prop.Setter(this, CadUtils.FromJulianCalendar((double)values.First()));
			}
			else if (prop.Property.PropertyType.IsEquivalentTo(typeof(TimeSpan)))
			{
				double jvalue = (double)values.First();

				prop.Setter(this, CadUtils.EditingTime((double)values.First()));
			}
			else if (constr == null)
			{
				if (prop.Attribute.IsName && values.First() is string name)
				{
					if (!name.IsNullOrEmpty())
					{
						prop.Setter(this, Convert.ChangeType(values.First(), prop.Property.PropertyType));
					}
				}
				else
				{
					prop.Setter(this, Convert.ChangeType(values.First(), prop.Property.PropertyType));
				}
			}
			else
			{
				prop.Setter(this, Activator.CreateInstance(prop.Property.PropertyType, values));
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{this.Version}";
		}
	}
}