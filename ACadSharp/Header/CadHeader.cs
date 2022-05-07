using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACadSharp.Header
{
	public enum AttributeVisibilityMode
	{
		None = 0,
		Normal = 1,
		All = 2
	}

	public class CadHeader
	{
		//https://help.autodesk.com/view/OARX/2021/ENU/?guid=GUID-A85E8E67-27CD-4C59-BE61-4DC9FADBE74A

		//TODO : Finish the header documentation

		#region Header System Variables

		/// <summary>
		/// The AutoCAD drawing database version number.
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
				/*
				 The AutoCAD drawing database version number:
				AC1006 = R10
				AC1009 = R11 and R12
				AC1012 = R13
				AC1014 = R14
				AC1015 = AutoCAD 2000
				AC1018 = AutoCAD 2004
				AC1021 = AutoCAD 2007
				AC1024 = AutoCAD 2010
				AC1027 = AutoCAD 2013
				AC1032 = AutoCAD 2018
				 */

				this.Version = CadUtils.GetVersionFromName(value);
			}
		}

		public ACadVersion Version { get; set; } = ACadVersion.AC1032;

		/// <summary>
		/// Maintenance version number(should be ignored)
		/// </summary>
		/// <remarks>
		/// System variable ACADMAINTVER.
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$ACADMAINTVER", 70)]
		public short MaintenanceVersion { get; set; }

		/// <summary>
		/// Drawing code page; set to the system code page when a new drawing is created,
		/// but not otherwise maintained by AutoCAD
		/// </summary>
		/// <remarks>
		/// System variable DWGCODEPAGE
		/// </remarks>
		[CadSystemVariable("$DWGCODEPAGE", 3)]
		public string CodePage { get; set; } = "ANSI_1252";

		/// <summary>
		/// Displays the name of the last person who modified the file
		/// </summary>
		/// <remarks>
		/// System variable LASTSAVEDBY
		/// </remarks>
		[CadSystemVariable("$LASTSAVEDBY", 3)]
		public string LastSavedBy { get; set; } = "ACadSharp";

		/// <summary>
		/// The default value is 0.
		/// Read only.
		/// </summary>
		/// <remarks>
		/// System variable REQUIREDVERSIONS <br/>
		/// Only in <see cref="ACadVersion.AC1024"/> or above
		/// </remarks>
		[CadSystemVariable("$REQUIREDVERSIONS", DxfCode.Int16)]
		public long RequiredVersions { get; set; }

		/// <summary>
		/// </summary>
		/// <remarks>
		/// System variable DIMASO <br/>
		/// Obsolete; see DIMASSOC
		/// </remarks>
		[CadSystemVariable("$DIMASO", DxfCode.Int16)]
		public bool AssociatedDimensions { get; set; } = true;

		/// <summary>
		/// System variable DIMSHO
		/// </summary>
		[CadSystemVariable("$DIMSHO", DxfCode.Int16)]
		public bool UpdateDimensionsWhileDragging { get; set; } = true;

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMSAV
		/// </remarks>
		public bool DIMSAV { get; set; }

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
		/// System variable ORTHOMODE.
		/// Ortho mode on if nonzero.
		/// </summary>
		[CadSystemVariable("$ORTHOMODE", DxfCode.Int16)]
		public bool OrthoMode { get; set; }

		/// <summary>
		/// System variable REGENMODE.
		/// REGENAUTO mode on if nonzero
		/// </summary>
		[CadSystemVariable("$REGENMODE", DxfCode.Int16)]
		public bool RegenerationMode { get; set; }

		/// <summary>
		/// System variable FILLMODE.
		/// Fill mode on if nonzero
		/// </summary>
		[CadSystemVariable("$FILLMODE", DxfCode.Int16)]
		public bool FillMode { get; set; }

		/// <summary>
		/// Quick Text mode on if nonzero
		/// </summary>
		/// <remarks>
		/// System variable QTEXTMODE.
		/// </remarks>
		[CadSystemVariable("$QTEXTMODE", DxfCode.Int16)]
		public bool QuickTextMode { get; set; }

		/// <summary>
		/// Controls paper space linetype scaling.
		/// </summary>
		/// <remarks>
		/// System variable PSLTSCALE.
		/// </remarks>
		[CadSystemVariable("$PSLTSCALE", DxfCode.Int16)]
		public SpaceLineTypeScaling PaperSpaceLineTypeScaling { get; set; } = SpaceLineTypeScaling.Normal;

		/// <summary>
		/// Nonzero if limits checking is on
		/// System variable LIMCHECK.
		/// </summary>
		[CadSystemVariable("$LIMCHECK", DxfCode.Int16)]
		public bool LimitCheckingOn { get; set; }

		/// <summary>
		/// System variable BLIPMODE	??
		/// </summary>
		[CadSystemVariable("$BLIPMODE", DxfCode.Int16)]
		public bool BlipMode { get; set; }

		/// <summary>
		/// Controls the user timer for the drawing
		/// System variable USRTIMER
		/// </summary>
		[CadSystemVariable("$USRTIMER", DxfCode.Int16)]
		public bool UserTimer { get; set; }

		/// <summary>
		/// Determines the object type created by the SKETCH command
		/// System variable SKPOLY
		/// </summary>
		[CadSystemVariable("$SKPOLY", DxfCode.Int16)]
		public bool SketchPolylines { get; set; }

		/// <summary>
		/// Represents angular direction.
		/// System variable ANGDIR
		/// </summary>
		[CadSystemVariable("$ANGDIR", DxfCode.Int16)]
		public AngularDirection AngularDirection { get; set; } = AngularDirection.ClockWise;

		/// <summary>
		/// Controls the display of helixes and smoothed mesh objects.
		/// System variable SPLFRAME
		/// </summary>
		[CadSystemVariable("$SPLFRAME", DxfCode.Int16)]
		public bool ShowSplineControlPoints { get; set; }

		/// <summary>
		/// Mirror text if nonzero <br/>
		/// System variable MIRRTEXT
		/// </summary>
		[CadSystemVariable("$MIRRTEXT", DxfCode.Int16)]
		public bool MirrorText { get; set; } = false;

		/// <summary>
		/// Determines whether input for the DVIEW and VPOINT command evaluated as relative to the WCS or current UCS <br/>
		/// System variable WORLDVIEW
		/// </summary>
		[CadSystemVariable("$WORLDVIEW", DxfCode.Int16)]
		public bool WorldView { get; set; }

		/// <summary>
		/// 1 for previous release compatibility mode; 0 otherwise <br/>
		/// System variable TILEMODE
		/// </summary>
		[CadSystemVariable("$TILEMODE", DxfCode.Int16)]
		public bool ShowModelSpace { get; set; }

		/// <summary>
		/// Limits checking in paper space when nonzero <br/>
		/// System variable PLIMCHECK
		/// </summary>
		[CadSystemVariable("$PLIMCHECK", DxfCode.Int16)]
		public bool PaperSpaceLimitsChecking { get; set; }

		/// <summary>
		/// Controls the properties of xref-dependent layers: <br/>
		/// 0 = Don't retain xref-dependent visibility settings <br/>
		/// 1 = Retain xref-dependent visibility settings <br/>
		/// System variable VISRETAIN
		/// </summary>
		[CadSystemVariable("$VISRETAIN", DxfCode.Int16)]
		public bool RetainXRefDependentVisibilitySettings { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// System variable DISPSILH
		/// </remarks>
		public bool DisplaySilhouetteCurves { get; set; }

		/// <summary>
		/// 
		/// System variable PELLIPSE (not present in DXF)
		/// </summary>
		public bool CreateEllipseAsPolyline { get; set; }

		/// <summary>
		/// 
		/// System variable PROXYGRAPHICS
		/// </summary>
		public bool ProxyGraphics { get; set; }

		/// <summary>
		/// 
		/// System variable TREEDEPTH
		/// </summary>
		public short SpatialIndexMaxTreeDepth { get; set; }

		/// <summary>
		/// Units format for coordinates and distances
		/// </summary>
		/// <remarks>
		/// System variable LUNITS
		/// </remarks>
		[CadSystemVariable("$LUNITS", 70)]
		public LinearUnitFormat LinearUnitFormat { get; set; } = LinearUnitFormat.Decimal;

		/// <summary>
		/// Units precision for coordinates and distances
		/// </summary>
		/// <remarks>
		/// System variable LUPREC
		/// </remarks>
		[CadSystemVariable("$LUPREC", 70)]
		public short LinearUnitPrecision { get; set; } = 4;

		/// <summary>
		/// Entity linetype name, or BYBLOCK or BYLAYER
		/// </summary>
		/// <remarks>
		/// System variable AUNITS
		/// </remarks>
		[CadSystemVariable("$AUNITS", 70)]
		public AngularUnitFormat AngularUnit { get; set; }

		/// <summary>
		/// Units precision for angles
		/// </summary>
		/// <remarks>
		/// System variable AUPREC
		/// </remarks>
		[CadSystemVariable("$AUPREC", 70)]
		public short AngularUnitPrecision { get; set; }

		/// <summary>
		/// 
		/// System variable OSMODE
		/// </summary>
		public ObjectSnapMode ObjectSnapMode { get; set; }

		/// <summary>
		/// Attribute visibility
		/// </summary>
		/// <remarks>
		/// System variable ATTMODE
		/// </remarks>
		[CadSystemVariable("$ATTMODE", 70)]
		public AttributeVisibilityMode AttributeVisibility { get; set; }

		/// <summary>
		/// Point display mode
		/// </summary>
		/// <remarks>
		/// System variable PDMODE
		/// </remarks>
		[CadSystemVariable("$PDMODE", 70)]
		public short PointDisplayMode { get; set; }

		/// <summary>
		/// 
		/// System variable USERI1
		/// </summary>
		public short UserShort1 { get; set; }
		/// <summary>
		/// 
		/// System variable USERI2
		/// </summary>
		public short UserShort2 { get; set; }
		/// <summary>
		/// 
		/// System variable USERI3
		/// </summary>
		public short UserShort3 { get; set; }
		/// <summary>
		/// 
		/// System variable USERI4
		/// </summary>
		public short UserShort4 { get; set; }
		/// <summary>
		/// 
		/// System variable USERI5
		/// </summary>
		public short UserShort5 { get; set; }
		/// <summary>
		/// 
		/// System variable SPLINESEGS
		/// </summary>
		public short NumberOfSplineSegments { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public short SurfaceDensityU { get; set; }
		/// <summary>
		/// 
		/// System variable SURFU
		/// </summary>
		public short SurfaceDensityV { get; set; }
		/// <summary>
		/// 
		/// System variable SURFTYPE
		/// </summary>
		public short SurfaceType { get; set; }
		/// <summary>
		/// 
		/// System variable SURFTAB1
		/// </summary>
		public short SurfaceMeshTabulationCount1 { get; set; }
		/// <summary>
		/// 
		/// System variable SURFTAB2
		/// </summary>
		public short SurfaceMeshTabulationCount2 { get; set; }

		/// <summary>
		/// 
		/// System variable SPLINETYPE
		/// </summary>
		public SplineType SplineType { get; set; }

		/// <summary>
		/// 
		/// System variable SHADEDGE
		/// </summary>
		public ShadeEdgeType ShadeEdge { get; set; }

		/// <summary>
		/// Percent ambient/diffuse light
		/// </summary>
		/// <remarks>
		/// System variable SHADEDIF
		/// </remarks>
		/// <value>
		/// range 1-100
		/// </value>
		[CadSystemVariable("$SHADEDIF", 70)]
		public short ShadeDiffuseToAmbientPercentage { get; set; } = 70;

		/// <summary>
		/// 
		/// System variable UNITMODE
		/// </summary>
		public short UnitMode { get; set; }
		/// <summary>
		/// 
		/// System variable MAXACTVP
		/// </summary>
		public short MaxViewportCount { get; set; }

		/// <summary>
		/// 
		/// System variable ISOLINES
		/// </summary>
		public short SurfaceIsolineCount { get; set; }

		/// <summary>
		/// Current multiline justification
		/// </summary>
		/// <remarks>
		/// System variable CMLJUST
		/// </remarks>
		[CadSystemVariable("$CMLJUST", 70)]
		public VerticalAlignmentType CurrentMultilineJustification { get; set; } = VerticalAlignmentType.Top;

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public short TextQuality { get; set; }

		/// <summary>
		/// Global linetype scale
		/// </summary>
		/// <remarks>
		/// System variable LTSCALE
		/// </remarks>
		[CadSystemVariable("$LTSCALE", 40)]
		public double LineTypeScale { get; set; } = 1.0d;

		/// <summary>
		/// Default text height
		/// </summary>
		/// <remarks>
		/// System variable TEXTSIZE
		/// </remarks>
		[CadSystemVariable("$TEXTSIZE", 40)]
		public double TextHeightDefault { get; set; } = 2.5d;

		/// <summary>
		/// Current text style name
		/// </summary>
		/// <remarks>
		/// System variable TEXTSTYLE
		/// </remarks>
		[CadSystemVariable("$TEXTSTYLE", 7)]
		public string TextStyleName
		{
			get { return this.TextStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this.TextStyle = this.Document.TextStyles[value];
				}
				else
				{
					this.TextStyle = new TextStyle(value);
				}
			}
		}

		public TextStyle TextStyle { get; private set; } = TextStyle.Default;

		/// <summary>
		/// Current layer name
		/// </summary>
		/// <remarks>
		/// System variable CLAYER
		/// </remarks>
		[CadSystemVariable("$CLAYER", 8)]
		public string LayerName
		{
			get { return this.CurrentLayer.Name; }
			set
			{
				if (this.Document != null)
				{
					this.CurrentLayer = this.Document.Layers[value];
				}
				else
				{
					this.CurrentLayer = new Layer(value);
				}
			}
		}

		public Layer CurrentLayer { get; private set; } = Layer.Default;

		/// <summary>
		/// Entity linetype name, or BYBLOCK or BYLAYER
		/// </summary>
		/// <remarks>
		/// System variable CELTYPE
		/// </remarks>
		[CadSystemVariable("$CELTYPE", 8)]
		public string LineTypeName
		{
			get { return this.CurrentLType.Name; }
			set
			{
				if (this.Document != null)
				{
					this.CurrentLType = this.Document.LineTypes[value];
				}
				else
				{
					this.CurrentLType = new LineType(value);
				}
			}
		}

		public LineType CurrentLType { get; private set; } = LineType.ByLayer;

		/// <summary>
		/// Current multiline style name
		/// </summary>
		/// <remarks>
		/// System variable CMLSTYLE
		/// </remarks>
		[CadSystemVariable("$CMLSTYLE", 2)]
		public string MultilineStyleName { get; set; } = "Standard";
		//{
		//	get { return this.CurrentLType.Name; }
		//	set
		//	{
		//		if (this.Document != null)
		//		{
		//			this.CurrentLType = this.Document.LineTypes[value];
		//		}
		//		else
		//		{
		//			this.CurrentLType = new LineType(value);
		//		}
		//	}
		//}

		//public MLStyle CurrentTextStyle { get; private set; } = MLStyle.Default;

		/// <summary>
		/// Default trace width
		/// </summary>
		/// <remarks>
		/// System variable TRACEWID
		/// </remarks>
		[CadSystemVariable("$TRACEWID", 40)]
		public double TraceWidthDefault { get; set; }

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double SketchIncrement { get; set; }

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double FilletRadius { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ThicknessDefault { get; set; }

		/// <summary>
		/// Angle 0 direction
		/// </summary>
		/// <remarks>
		/// System variable ANGBASE
		/// </remarks>
		[CadSystemVariable("$ANGBASE", 50)]
		public double AngleBase { get; set; }

		/// <summary>
		/// Point display size
		/// </summary>
		/// <remarks>
		/// System variable PDSIZE
		/// </remarks>
		[CadSystemVariable("$PDSIZE", 40)]
		public double PointDisplaySize { get; set; }

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double PolylineWidthDefault { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double UserDouble1 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double UserDouble2 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double UserDouble3 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double UserDouble4 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double UserDouble5 { get; set; }

		/// <summary>
		/// First chamfer distance
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERA
		/// </remarks>
		[CadSystemVariable("$CHAMFERA", 40)]
		public double ChamferDistance1 { get; set; }

		/// <summary>
		/// Second  chamfer distance
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERB
		/// </remarks>
		[CadSystemVariable("$CHAMFERB", 40)]
		public double ChamferDistance2 { get; set; }

		/// <summary>
		/// Chamfer length
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERC
		/// </remarks>
		[CadSystemVariable("$CHAMFERC", 40)]
		public double ChamferLength { get; set; }

		/// <summary>
		/// Chamfer angle
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERD
		/// </remarks>
		[CadSystemVariable("$CHAMFERD", 40)]
		public double ChamferAngle { get; set; }

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double FacetResolution { get; set; }

		/// <summary>
		/// Current multiline scale
		/// </summary>
		/// <remarks>
		/// System variable CMLSCALE
		/// </remarks>
		[CadSystemVariable("$CMLSCALE", 40)]
		public double CurrentMultilineScale { get; set; } = 20.0d;

		/// <summary>
		/// Current entity linetype scale
		/// </summary>
		/// <remarks>
		/// System variable CHAMFERD
		/// </remarks>
		[CadSystemVariable("$CELTSCALE", 40)]
		public double CurrentEntityLinetypeScale { get; set; } = 1.0d;

		/// <summary>
		/// Name of menu file
		/// </summary>
		/// <remarks>		
		/// System variable MENU
		/// </remarks>
		[CadSystemVariable("$MENU", 1)]
		public string MenuFileName { get; set; } = string.Empty;

		/// <summary>
		/// Next available handle
		/// </summary>
		/// <remarks>
		/// System variable HANDSEED
		/// </remarks>
		[CadSystemVariable("$HANDSEED", 5)]
		public ulong HandleSeed { get; internal set; } = 0x27;

		/// <summary>
		/// Local date/time of drawing creation (see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDCREATE
		/// </remarks>
		//[CadSystemVariable("$TDCREATE",40)]
		public DateTime CreateDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// Universal date/time the drawing was created(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUCREATE
		/// </remarks>
		//[CadSystemVariable("$TDUCREATE", 40)]
		public DateTime UniversalCreateDateTime { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Local date/time of last drawing update(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUPDATE
		/// </remarks>
		//[CadSystemVariable("$TDUPDATE", 40)]
		public DateTime UpdateDateTime { get; set; }

		/// <summary>
		/// Universal date/time of the last update/save(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUUPDATE
		/// </remarks>
		//[CadSystemVariable("$TDUUPDATE", 40)]
		public DateTime UniversalUpdateDateTime { get; set; }

		/// <summary>
		/// Cumulative editing time for this drawing(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDINDWG
		/// </remarks>
		//[CadSystemVariable("$TDINDWG", 40)]
		public TimeSpan TotalEditingTime { get; set; }

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public TimeSpan UserElapsedTimeSpan { get; set; }

		/// <summary>
		/// Current entity color number
		/// </summary>
		/// <remarks>
		/// System variable CECOLOR
		/// </remarks>
		[CadSystemVariable("$CECOLOR", 62)]
		public Color CurrentEntityColor { get; set; } = Color.ByLayer;

		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ViewportDefaultViewScaleFactor { get; set; }

		/// <summary>
		/// Origin of current UCS (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSORG
		/// </remarks>
		[CadSystemVariable("$UCSORG", 10, 20, 30)]
		public XYZ PaperSpaceUcsOrigin
		{
			get { return this._paperSpaceUcs.Origin; }
			set
			{
				this._paperSpaceUcs.Origin = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS X axis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSXDIR
		/// </remarks>
		[CadSystemVariable("$UCSXDIR", 10, 20, 30)]
		public XYZ PaperSpaceUcsXAxis
		{
			get { return this._paperSpaceUcs.XAxis; }
			set
			{
				this._paperSpaceUcs.XAxis = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS Y aYis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSYDIR
		/// </remarks>
		[CadSystemVariable("$UCSYDIR", 10, 20, 30)]
		public XYZ PaperSpaceUcsYAYis
		{
			get { return this._paperSpaceUcs.YAxis; }
			set
			{
				this._paperSpaceUcs.YAxis = value;
			}
		}

		[Obsolete("Will convert to private")]
		public UCS _paperSpaceUcs { get; set; } = new UCS();

		/// <summary>
		/// System variable INSBASE.
		/// Insertion base set by BASE command(in WCS)
		/// </summary>
		[CadSystemVariable("$INSBASE", DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ InsertionBase { get; set; } = new XYZ();

		/// <summary>
		/// System variable EXTMIN.
		/// X, Y, and Z drawing extents lower-left corner (in WCS)
		/// </summary>
		[CadSystemVariable("$EXTMIN", DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ ExtMin { get; set; }

		/// <summary>
		/// System variable EXTMAX
		/// X, Y, and Z drawing extents upper-right corner(in WCS)
		/// </summary>
		[CadSystemVariable("$EXTMAX", DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ ExtMax { get; set; }

		/// <summary>
		/// XY drawing limits lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMIN
		/// </remarks>
		[CadSystemVariable("$LIMMIN", DxfCode.XCoordinate, DxfCode.YCoordinate)]
		public XY LimitsMin { get; set; }

		/// <summary>
		/// XY drawing limits upper-right corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMAX
		/// </remarks>
		[CadSystemVariable("$LIMMAX", DxfCode.XCoordinate, DxfCode.YCoordinate)]
		public XY LimitsMax { get; set; }

		public double Elevation { get; set; }
		public string DimensionBlockName { get; set; }
		public string DimensionBlockNameFirst { get; set; }
		public string DimensionBlockNameSecond { get; set; }
		public short StackedTextAlignment { get; set; }
		public short StackedTextSizePercentage { get; set; }
		public string HyperLinkBase { get; set; }

		/// <summary>
		/// Lineweight of new objects
		/// </summary>
		/// <remarks>
		/// System variable CELWEIGHT
		/// </remarks>
		[CadSystemVariable("$CELWEIGHT", 370)]
		public LineweightType CurrentEntityLineWeight { get; set; } = LineweightType.ByLayer;

		public short EndCaps { get; set; }

		public short JoinStyle { get; set; }

		/// <summary>
		/// Controls the display of lineweights on the Model or Layout tab<br/>
		/// 0 = Lineweight is not displayed<br/>
		/// 1 = Lineweight is displayed
		/// </summary>
		/// <remarks>
		/// System variable LWDISPLAY
		/// </remarks>
		[CadSystemVariable("$LWDISPLAY", 290)]
		public bool DisplayLineWeight { get; set; } = false;

		public short XEdit { get; set; }

		/// <summary>
		/// Controls symbol table naming:<br/>
		/// 0 = AutoCAD Release 14 compatibility. Limits names to 31 characters in length. Names can include the letters A to Z, the numerals 0 to 9, and the special characters dollar sign ($), underscore (_), and hyphen (-).<br/>
		/// 1 = AutoCAD 2000. Names can be up to 255 characters in length, and can include the letters A to Z, the numerals 0 to 9, spaces, and any special characters not used for other purposes by Microsoft Windows and AutoCAD
		/// </summary>
		/// <remarks>
		/// System variable EXTNAMES
		/// </remarks>
		[CadSystemVariable("$EXTNAMES", 290)]
		public bool ExtendedNames { get; set; } = true;

		public short PlotStyleMode { get; set; }
		public short LoadOLEObject { get; set; }

		/// <summary>
		/// Default drawing units for AutoCAD DesignCenter blocks
		/// </summary>
		/// <remarks>
		/// System variable INSUNITS
		/// </remarks>
		[CadSystemVariable("$INSUNITS", 70)]
		public UnitsType InsUnits { get; set; } = UnitsType.Unitless;

		public short CurrentEntityPlotStyleType { get; set; }

		public string FingerPrintGuid { get; set; }

		public string VersionGuid { get; set; }

		public ObjectSortingFlags EntitySortingFlags { get; set; }

		public byte IndexCreationFlags { get; set; }

		public byte HideText { get; set; }

		public byte ExternalReferenceClippingBoundaryType { get; set; }

		/// <summary>
		/// Controls the associativity of dimension objects
		/// </summary>
		/// <remarks>
		/// System variable DIMASSOC
		/// </remarks>
		[CadSystemVariable("$DIMASSOC", DxfCode.Int8)]
		public DimensionAssociation DimensionAssociativity { get; set; } = DimensionAssociation.CreateExplodedDimensions;

		/// <remarks>
		/// System variable HALOGAP
		/// </remarks>
		public byte HaloGapPercentage { get; set; }
		public Color ObscuredColor { get; set; }
		public Color InterfereColor { get; set; }
		public byte ObscuredType { get; set; }
		public byte IntersectionDisplay { get; set; }
		public string ProjectName { get; set; }
		public bool CameraDisplayObjects { get; set; }
		public double StepsPerSecond { get; set; }
		public double StepSize { get; set; }
		public double Dw3DPrecision { get; set; }
		public double LensLength { get; set; }
		public double CameraHeight { get; set; }
		public char SolidsRetainHistory { get; set; }
		public char ShowSolidsHistory { get; set; }
		public double SweptSolidWidth { get; set; }
		public double SweptSolidHeight { get; set; }
		public double DraftAngleFirstCrossSection { get; set; }
		public double DraftAngleSecondCrossSection { get; set; }
		public double DraftMagnitudeFirstCrossSection { get; set; }
		public double DraftMagnitudeSecondCrossSection { get; set; }
		public short SolidLoftedShape { get; set; }
		public char LoftedObjectNormals { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public double NorthDirection { get; set; }
		public int TimeZone { get; set; }
		public char DisplayLightGlyphs { get; set; }
		public char DwgUnderlayFramesVisibility { get; set; }
		public char DgnUnderlayFramesVisibility { get; set; }
		public byte ShadowMode { get; set; }
		public double ShadowPlaneLocation { get; set; }
		public string StyleSheetName { get; set; }

		#endregion

		public CadDocument Document { get; internal set; }

		public UCS Ucs { get; set; } = new UCS();


		/// <summary>
		/// Dimension style name
		/// </summary>
		/// <remarks>
		/// System variable DIMSTYLE
		/// </remarks>
		[CadSystemVariable("$DIMSTYLE", 2)]
		public string DimensionStyleOverridesName
		{
			get { return this.DimensionStyleOverrides.Name; }
			set
			{
				if (this.Document != null)
				{
					this.DimensionStyleOverrides = this.Document.DimensionStyles[value];
				}
				else
				{
					this.DimensionStyleOverrides = new DimensionStyle(value);
				}
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
			get { return this.DimensionStyleOverrides.AngularDimensionDecimalPlaces; }
			set
			{
				this.DimensionStyleOverrides.AngularDimensionDecimalPlaces = value;
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
			get { return this.DimensionStyleOverrides.DecimalPlaces; }
			set
			{
				this.DimensionStyleOverrides.DecimalPlaces = value;
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
			get { return this.DimensionStyleOverrides.ToleranceDecimalPlaces; }
			set
			{
				this.DimensionStyleOverrides.ToleranceDecimalPlaces = value;
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
			get { return this.DimensionStyleOverrides.AlternateUnitFormat; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitFormat = value;
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
			get { return this.DimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitToleranceDecimalPlaces = value;
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
			get { return this.DimensionStyleOverrides.AngularUnit; }
			set
			{
				this.DimensionStyleOverrides.AngularUnit = value;
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
			get { return this.DimensionStyleOverrides.FractionFormat; }
			set
			{
				this.DimensionStyleOverrides.FractionFormat = value;
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
			get { return this.DimensionStyleOverrides.LinearUnitFormat; }
			set
			{
				this.DimensionStyleOverrides.LinearUnitFormat = value;
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
			get { return this.DimensionStyleOverrides.DecimalSeparator; }
			set
			{
				this.DimensionStyleOverrides.DecimalSeparator = value;
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
			get { return this.DimensionStyleOverrides.TextMovement; }
			set
			{
				this.DimensionStyleOverrides.TextMovement = value;
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
			get { return this.DimensionStyleOverrides.TextHorizontalAlignment; }
			set
			{
				this.DimensionStyleOverrides.TextHorizontalAlignment = value;
			}
		}

		/// <summary>
		/// Suppression of first extension line
		/// </summary>
		/// <remarks>
		/// System variable DIMSD1
		/// </remarks>
		[CadSystemVariable("$DIMSD1", 70)]
		public bool DimensionSuppressFirstExtensionLine
		{
			get { return this.DimensionStyleOverrides.SuppressFirstExtensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressFirstExtensionLine = value;
			}
		}

		/// <summary>
		/// Suppression of second extension line
		/// </summary>
		/// <remarks>
		/// System variable DIMSD2
		/// </remarks>
		[CadSystemVariable("$DIMSD2", 70)]
		public bool DimensionSuppressSecondExtensionLine
		{
			get { return this.DimensionStyleOverrides.SuppressSecondExtensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressSecondExtensionLine = value;
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
			get { return this.DimensionStyleOverrides.ToleranceAlignment; }
			set
			{
				this.DimensionStyleOverrides.ToleranceAlignment = value;
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
			get { return this.DimensionStyleOverrides.ToleranceZeroHandling; }
			set
			{
				this.DimensionStyleOverrides.ToleranceZeroHandling = value;
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
			get { return this.DimensionStyleOverrides.AlternateUnitZeroHandling; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitZeroHandling = value;
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
			get { return this.DimensionStyleOverrides.AlternateUnitToleranceZeroHandling; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitToleranceZeroHandling = value;
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
			get { return this.DimensionStyleOverrides.CursorUpdate; }
			set
			{
				this.DimensionStyleOverrides.CursorUpdate = value;
			}
		}

		/// <summary>
		/// Controls dimension text and arrow placement when space is not sufficient to place both within the extension lines
		/// </summary>
		/// <remarks>
		/// System variable DIMATFIT
		/// </remarks>
		[CadSystemVariable("$DIMATFIT", 70)]
		public short DimensionDimensionTextArrowFit
		{
			get { return this.DimensionStyleOverrides.DimensionTextArrowFit; }
			set
			{
				this.DimensionStyleOverrides.DimensionTextArrowFit = value;
			}
		}

		public DimensionStyle DimensionStyleOverrides { get; private set; } = DimensionStyle.Default;

		public CadHeader() { }

		public CadHeader(ACadVersion version)
		{
			this.Version = version;
		}

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

		public void SetValue(string systemvar, params object[] values)
		{
			foreach (PropertyInfo p in this.GetType().GetProperties())
			{
				CadSystemVariableAttribute att = p.GetCustomAttribute<CadSystemVariableAttribute>();
				if (att == null || att.Name != systemvar)
					continue;

				object build = null;
				ConstructorInfo constr = p.PropertyType.GetConstructor(values.Select(o => o.GetType()).ToArray());

				if (p.PropertyType.IsEnum)
				{
					build = Enum.ToObject(p.PropertyType, values.First());
				}
				else if (constr == null)
				{
					build = Convert.ChangeType(values.First(), p.PropertyType);
				}
				else
				{
					build = Activator.CreateInstance(p.PropertyType, values);
				}

				//Set the value if it has any
				if (build != null)
				{
					p.SetValue(this, build);
					break;
				}
			}
		}

		public object GetValue(string systemvar)
		{
			object value = null;

			foreach (PropertyInfo p in this.GetType().GetProperties())
			{
				CadSystemVariableAttribute att = p.GetCustomAttribute<CadSystemVariableAttribute>();
				if (att == null)
					continue;

				if (att.Name == systemvar)
				{
					value = p.GetValue(this);
					break;
				}
			}

			return value;
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
						var arr = vector.GetComponents();
						for (int i = 0; i < arr.Length; i++)
						{
							value.Add(att.ValueCodes[i], arr[i]);
						}
					}

					break;
				}
			}

			return value;
		}
	}
}
