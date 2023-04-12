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

		public ACadVersion Version { get; set; } = ACadVersion.AC1018;

		/// <summary>
		/// Maintenance version number(should be ignored)
		/// </summary>
		/// <remarks>
		/// System variable ACADMAINTVER.
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$ACADMAINTVER", 70)]
		public short MaintenanceVersion { get; internal set; } = 0;

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
		[CadSystemVariable(DxfReferenceType.Ignored, "$LASTSAVEDBY", 3)]
		public string LastSavedBy { get; set; } = "ACadSharp";

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
		/// Fill mode on if nonzero
		/// </summary>
		/// <remarks>
		/// System variable FILLMODE.
		/// </remarks>
		[CadSystemVariable("$FILLMODE", DxfCode.Int16)]
		public bool FillMode { get; set; } = true;

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
		/// Specifies the maximum depth of the spatial index
		/// </summary>
		/// <remarks>
		/// System variable TREEDEPTH
		/// </remarks>
		[CadSystemVariable("$TREEDEPTH", 70)]
		public short SpatialIndexMaxTreeDepth { get; set; } = 3020;

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
		public AttributeVisibilityMode AttributeVisibility { get; set; } = AttributeVisibilityMode.Normal;

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
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable SPLINESEGS
		/// </remarks>
		[CadSystemVariable("$SPLINESEGS", 70)]
		public short NumberOfSplineSegments { get; set; } = 8;

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
			get { return this.CurrentTextStyle.Name; }
			set
			{
				if (this.Document != null)
				{
					this.CurrentTextStyle = this.Document.TextStyles[value];
				}
				else
				{
					this.CurrentTextStyle = new TextStyle(value);
				}
			}
		}

		public TextStyle CurrentTextStyle { get; private set; } = TextStyle.Default;

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

		/// <summary>
		/// Entity linetype name, or BYBLOCK or BYLAYER
		/// </summary>
		/// <remarks>
		/// System variable CELTYPE
		/// </remarks>
		[CadSystemVariable("$CELTYPE", 6)]
		public string LineTypeName
		{
			get { return this.CurrentLineType.Name; }
			set
			{
				if (this.Document != null)
				{
					this.CurrentLineType = this.Document.LineTypes[value];
				}
				else
				{
					this.CurrentLineType = new LineType(value);
				}
			}
		}

		public LineType CurrentLineType { get; private set; } = LineType.ByLayer;

		/// <summary>
		/// Current multiline style name
		/// </summary>
		/// <remarks>
		/// System variable CMLSTYLE
		/// </remarks>
		[CadSystemVariable("$CMLSTYLE", 2)]
		public string MultilineStyleName { get; set; } = "Standard";

		//TODO: Header MLStyle
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
		public double PointDisplaySize { get; set; } = 0.0d;

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
		public string MenuFileName { get; set; } = ".";

		/// <summary>
		/// Next available handle
		/// </summary>
		/// <remarks>
		/// System variable HANDSEED
		/// </remarks>
		[CadSystemVariable("$HANDSEED", 5)]
		public ulong HandleSeed { get; internal set; } = 0x0;

		/// <summary>
		/// Local date/time of drawing creation (see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDCREATE
		/// </remarks>
		[CadSystemVariable("$TDCREATE", 40)]
		public DateTime CreateDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// Universal date/time the drawing was created(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUCREATE
		/// </remarks>
		[CadSystemVariable("$TDUCREATE", 40)]
		public DateTime UniversalCreateDateTime { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Local date/time of last drawing update(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUPDATE
		/// </remarks>
		[CadSystemVariable("$TDUPDATE", 40)]
		public DateTime UpdateDateTime { get; set; } = DateTime.Now;

		/// <summary>
		/// Universal date/time of the last update/save(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDUUPDATE
		/// </remarks>
		[CadSystemVariable("$TDUUPDATE", 40)]
		public DateTime UniversalUpdateDateTime { get; set; } = DateTime.UtcNow;

		/// <summary>
		/// Cumulative editing time for this drawing(see Special Handling of Date/Time Variables)
		/// </summary>
		/// <remarks>
		/// System variable TDINDWG
		/// </remarks>
		[CadSystemVariable("$TDINDWG", 40)]
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
		/// Insertion base set by BASE command(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PINSBASE
		/// </remarks>
		[CadSystemVariable("$PINSBASE", 10, 20, 30)]
		public XYZ PaperSpaceInsertionBase { get; set; } = XYZ.Zero;

		/// <summary>
		/// X, Y, and Z drawing extents lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PEXTMIN
		/// </remarks>
		[CadSystemVariable("$PEXTMIN", 10, 20, 30)]
		public XYZ PaperSpaceExtMin { get; set; }

		/// <summary>
		/// X, Y, and Z drawing extents upper-right corner(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PEXTMAX
		/// </remarks>
		[CadSystemVariable("$PEXTMAX", 10, 20, 30)]
		public XYZ PaperSpaceExtMax { get; set; }

		/// <summary>
		/// XY drawing limits lower-left corner(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PLIMMIN
		/// </remarks>
		[CadSystemVariable("$PLIMMIN", 10, 20)]
		public XY PaperSpaceLimitsMin { get; set; }

		/// <summary>
		/// XY drawing limits upper-right corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable PLIMMAX
		/// </remarks>
		[CadSystemVariable("$PLIMMAX", 10, 20)]
		public XY PaperSpaceLimitsMax { get; set; }

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
		/// Point which becomes the new UCS origin after changing paper space UCS to TOP when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGTOP
		/// </remarks>
		[CadSystemVariable("$PUCSORGTOP", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicTopDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to BOTTOM when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGBOTTOM
		/// </remarks>
		[CadSystemVariable("$PUCSORGBOTTOM", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicBottomDOrigin { get; set; }

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
		/// Point which becomes the new UCS origin after changing paper space UCS to FRONT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGFRONT
		/// </remarks>
		[CadSystemVariable("$PUCSORGFRONT", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicFrontDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing paper space UCS to BACK when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable PUCSORGBACK
		/// </remarks>
		[CadSystemVariable("$PUCSORGBACK", 10, 20, 30)]
		public XYZ PaperSpaceOrthographicBackDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to TOP when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGTOP
		/// </remarks>
		[CadSystemVariable("$UCSORGTOP", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicTopDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to BOTTOM when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGBOTTOM
		/// </remarks>
		[CadSystemVariable("$UCSORGBOTTOM", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicBottomDOrigin { get; set; }

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
		/// Point which becomes the new UCS origin after changing model space UCS to FRONT when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGFRONT
		/// </remarks>
		[CadSystemVariable("$UCSORGFRONT", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicFrontDOrigin { get; set; }

		/// <summary>
		/// Point which becomes the new UCS origin after changing model space UCS to BACK when PUCSBASE is set to WORLD
		/// </summary>
		/// <remarks>
		/// System variable UCSORGBACK
		/// </remarks>
		[CadSystemVariable("$UCSORGBACK", 10, 20, 30)]
		public XYZ ModelSpaceOrthographicBackDOrigin { get; set; }

		/// <summary>
		/// Insertion base set by BASE command(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable INSBASE
		/// </remarks>
		[CadSystemVariable("$INSBASE", 10, 20, 30)]
		public XYZ ModelSpaceInsertionBase { get; set; }

		/// <summary>
		/// X, Y, and Z drawing extents lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable EXTMIN
		/// </remarks>
		[CadSystemVariable("$EXTMIN", 10, 20, 30)]
		public XYZ ModelSpaceExtMin { get; set; }

		/// <summary>
		/// X, Y, and Z drawing extents upper-right corner(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable EXTMAX
		/// </remarks>
		[CadSystemVariable("$EXTMAX", 10, 20, 30)]
		public XYZ ModelSpaceExtMax { get; set; }

		/// <summary>
		/// XY drawing limits lower-left corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMIN
		/// </remarks>
		[CadSystemVariable("$LIMMIN", 10, 20)]
		public XY ModelSpaceLimitsMin { get; set; }

		/// <summary>
		/// XY drawing limits upper-right corner (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable LIMMAX
		/// </remarks>
		[CadSystemVariable("$LIMMAX", 10, 20)]
		public XY ModelSpaceLimitsMax { get; set; }

		/// <summary>
		/// Current elevation set by ELEV command
		/// </summary>
		/// <remarks>
		/// System variable ELEVATION
		/// </remarks>
		[CadSystemVariable("$ELEVATION", 40)]
		public double Elevation
		{
			get { return this.ModelSpace.Elevation; }
			set
			{
				this.ModelSpace.Elevation = value;
			}
		}

		/// <summary>
		/// Origin of current UCS(in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSORG
		/// </remarks>
		[CadSystemVariable("$UCSORG", 10, 20, 30)]
		public XYZ ModelSpaceOrigin
		{
			get { return this.ModelSpace.Origin; }
			set
			{
				this.ModelSpace.Origin = value;
			}
		}

		/// <summary>
		/// Direction of the current UCS X axis (in WCS)
		/// </summary>
		/// <remarks>
		/// System variable UCSXDIR
		/// </remarks>
		[CadSystemVariable("$UCSXDIR", 10, 20, 30)]
		public XYZ ModelSpaceXAxis
		{
			get { return this.ModelSpace.XAxis; }
			set
			{
				this.ModelSpace.XAxis = value;
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
			get { return this.ModelSpace.YAxis; }
			set
			{
				this.ModelSpace.YAxis = value;
			}
		}

		/// <summary>
		/// Arrow block name
		/// </summary>
		/// <remarks>
		/// System variable DIMBLK
		/// </remarks>
		[CadSystemVariable("$DIMBLK", 1)]
		public string DimensionBlockName { get; set; } = string.Empty;

		/// <summary>
		/// Arrow block name for leaders
		/// </summary>
		/// <remarks>
		/// System variable DIMLDRBLK
		/// </remarks>
		[CadSystemVariable("$DIMLDRBLK", 1)]
		public string ArrowBlockName { get; set; } = string.Empty;

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

		public bool XEdit { get; set; }

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
		public bool LoadOLEObject { get; set; }

		/// <summary>
		/// Default drawing units for AutoCAD DesignCenter blocks
		/// </summary>
		/// <remarks>
		/// System variable INSUNITS
		/// </remarks>
		[CadSystemVariable("$INSUNITS", 70)]
		public UnitsType InsUnits { get; set; } = UnitsType.Unitless;

		public short CurrentEntityPlotStyleType { get; set; }

		public string FingerPrintGuid { get; set; } = Guid.NewGuid().ToString();

		public string VersionGuid { get; set; } = Guid.NewGuid().ToString();

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
		/// Alternate unit dimensioning performed if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMALT
		/// </remarks>
		[CadSystemVariable("$DIMALT", 70)]
		public bool DimensionAlternateUnitDimensioning
		{
			get { return this.DimensionStyleOverrides.AlternateUnitDimensioning; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitDimensioning = value;
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
		/// Alternate unit scale factor
		/// </summary>
		/// <remarks>
		/// System variable DIMALTF
		/// </remarks>
		[CadSystemVariable("$DIMALTF", 40)]
		public double DimensionAlternateUnitScaleFactor
		{
			get { return this.DimensionStyleOverrides.AlternateUnitScaleFactor; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitScaleFactor = value;
			}
		}

		/// <summary>
		/// Extension line offset
		/// </summary>
		/// <remarks>
		/// System variable DIMEXO
		/// </remarks>
		[CadSystemVariable("$DIMEXO", 40)]
		public double DimensionExtensionLineOffset
		{
			get { return this.DimensionStyleOverrides.ExtensionLineOffset; }
			set
			{
				this.DimensionStyleOverrides.ExtensionLineOffset = value;
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
			get { return this.DimensionStyleOverrides.ScaleFactor; }
			set
			{
				this.DimensionStyleOverrides.ScaleFactor = value;
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
			get { return this.DimensionStyleOverrides.AlternateUnitDecimalPlaces; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitDecimalPlaces = value;
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
		public bool DimensionSuppressFirstDimensionLine
		{
			get { return this.DimensionStyleOverrides.SuppressFirstDimensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressFirstDimensionLine = value;
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
			get { return this.DimensionStyleOverrides.SuppressSecondDimensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressSecondDimensionLine = value;
			}
		}

		/// <summary>
		/// Vertical justification for tolerance values
		/// </summary>
		/// <remarks>
		/// System variable DIMTOL
		/// </remarks>
		[CadSystemVariable("$DIMTOL", 70)]
		public bool DimensionGenerateTolerances
		{
			get { return this.DimensionStyleOverrides.GenerateTolerances; }
			set
			{
				this.DimensionStyleOverrides.GenerateTolerances = value;
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
		/// Controls suppression of zeros for primary unit values
		/// </summary>
		/// <remarks>
		/// System variable DIMZIN
		/// </remarks>
		[CadSystemVariable("$DIMZIN", 70)]
		public ZeroHandling DimensionZeroHandling
		{
			get { return this.DimensionStyleOverrides.ZeroHandling; }
			set
			{
				this.DimensionStyleOverrides.ZeroHandling = value;
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

		/// <remarks>
		/// System variable DIMFIT
		/// </remarks>
		[CadSystemVariable("$DIMFIT", 70)]
		public char DimensionFit
		{
			get { return this.DimensionStyleOverrides.DimensionFit; }
			set
			{
				this.DimensionStyleOverrides.DimensionFit = value;
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

		/// <summary>
		/// Determines rounding of alternate units
		/// </summary>
		/// <remarks>
		/// System variable DIMALTRND
		/// </remarks>
		[CadSystemVariable("$DIMALTRND", 40)]
		public double DimensionAlternateUnitRounding
		{
			get { return this.DimensionStyleOverrides.AlternateUnitRounding; }
			set
			{
				this.DimensionStyleOverrides.AlternateUnitRounding = value;
			}
		}

		/// <summary>
		/// Alternate dimensioning suffix
		/// </summary>
		/// <remarks>
		/// System variable DIMAPOST
		/// </remarks>
		[CadSystemVariable("$DIMAPOST", 1)]
		public string DimensionAlternateDimensioningSuffix
		{
			get { return this.DimensionStyleOverrides.AlternateDimensioningSuffix; }
			set
			{
				this.DimensionStyleOverrides.AlternateDimensioningSuffix = value;
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
			get { return this.DimensionStyleOverrides.ArrowSize; }
			set
			{
				this.DimensionStyleOverrides.ArrowSize = value;
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
			get { return this.DimensionStyleOverrides.AngularZeroHandling; }
			set
			{
				this.DimensionStyleOverrides.AngularZeroHandling = value;
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
			get { return this.DimensionStyleOverrides.ArcLengthSymbolPosition; }
			set
			{
				this.DimensionStyleOverrides.ArcLengthSymbolPosition = value;
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
			get { return this.DimensionStyleOverrides.SeparateArrowBlocks; }
			set
			{
				this.DimensionStyleOverrides.SeparateArrowBlocks = value;
			}
		}

		/// <summary>
		/// Size of center mark/lines
		/// </summary>
		/// <remarks>
		/// System variable DIMCEN
		/// </remarks>
		[CadSystemVariable("$DIMCEN", 40)]
		public double DimensionCenterMarkSize
		{
			get { return this.DimensionStyleOverrides.CenterMarkSize; }
			set
			{
				this.DimensionStyleOverrides.CenterMarkSize = value;
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
			get { return this.DimensionStyleOverrides.TickSize; }
			set
			{
				this.DimensionStyleOverrides.TickSize = value;
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
			get { return this.DimensionStyleOverrides.DimensionLineColor; }
			set
			{
				this.DimensionStyleOverrides.DimensionLineColor = value;
			}
		}

		/// <summary>
		/// Dimension extension line color
		/// </summary>
		/// <remarks>
		/// System variable DIMCLRE
		/// </remarks>
		[CadSystemVariable("$DIMCLRE", 70)]
		public Color DimensionExtensionLineColor
		{
			get { return this.DimensionStyleOverrides.ExtensionLineColor; }
			set
			{
				this.DimensionStyleOverrides.ExtensionLineColor = value;
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
			get { return this.DimensionStyleOverrides.TextColor; }
			set
			{
				this.DimensionStyleOverrides.TextColor = value;
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
			get { return this.DimensionStyleOverrides.DimensionLineExtension; }
			set
			{
				this.DimensionStyleOverrides.DimensionLineExtension = value;
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
			get { return this.DimensionStyleOverrides.DimensionLineIncrement; }
			set
			{
				this.DimensionStyleOverrides.DimensionLineIncrement = value;
			}
		}

		/// <summary>
		/// Extension line extension
		/// </summary>
		/// <remarks>
		/// System variable DIMEXE
		/// </remarks>
		[CadSystemVariable("$DIMEXE", 40)]
		public double DimensionExtensionLineExtension
		{
			get { return this.DimensionStyleOverrides.ExtensionLineExtension; }
			set
			{
				this.DimensionStyleOverrides.ExtensionLineExtension = value;
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
			get { return this.DimensionStyleOverrides.IsExtensionLineLengthFixed; }
			set
			{
				this.DimensionStyleOverrides.IsExtensionLineLengthFixed = value;
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
			get { return this.DimensionStyleOverrides.FixedExtensionLineLength; }
			set
			{
				this.DimensionStyleOverrides.FixedExtensionLineLength = value;
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
			get { return this.DimensionStyleOverrides.JoggedRadiusDimensionTransverseSegmentAngle; }
			set
			{
				this.DimensionStyleOverrides.JoggedRadiusDimensionTransverseSegmentAngle = value;
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
			get { return this.DimensionStyleOverrides.TextBackgroundFillMode; }
			set
			{
				this.DimensionStyleOverrides.TextBackgroundFillMode = value;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		/// <remarks>
		/// System variable DIMTFILLCLR
		/// </remarks>
		[CadSystemVariable(DxfReferenceType.Ignored, "$DIMTFILLCLR", 62)]
		public Color DimensionTextBackgroundColor
		{
			get { return this.DimensionStyleOverrides.TextBackgroundColor; }
			set
			{
				this.DimensionStyleOverrides.TextBackgroundColor = value;
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
			get { return this.DimensionStyleOverrides.DimensionLineGap; }
			set
			{
				this.DimensionStyleOverrides.DimensionLineGap = value;
			}
		}

		/// <summary>
		/// Linear measurements scale factor
		/// </summary>
		/// <remarks>
		/// System variable DIMLFAC
		/// </remarks>
		[CadSystemVariable("$DIMLFAC", 40)]
		public double DimensionLinearScaleFactor
		{
			get { return this.DimensionStyleOverrides.LinearScaleFactor; }
			set
			{
				this.DimensionStyleOverrides.LinearScaleFactor = value;
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
			get { return this.DimensionStyleOverrides.TextVerticalPosition; }
			set
			{
				this.DimensionStyleOverrides.TextVerticalPosition = value;
			}
		}

		/// <summary>
		/// Dimension line lineweight
		/// </summary>
		/// <remarks>
		/// System variable DIMLWD
		/// </remarks>
		[CadSystemVariable("$DIMLWD", 70)]
		public LineweightType DimensionLineWeight
		{
			get { return this.DimensionStyleOverrides.DimensionLineWeight; }
			set
			{
				this.DimensionStyleOverrides.DimensionLineWeight = value;
			}
		}

		/// <summary>
		/// Extension line lineweight
		/// </summary>
		/// <remarks>
		/// System variable DIMLWE
		/// </remarks>
		[CadSystemVariable("$DIMLWE", 70)]
		public LineweightType ExtensionLineWeight
		{
			get { return this.DimensionStyleOverrides.ExtensionLineWeight; }
			set
			{
				this.DimensionStyleOverrides.ExtensionLineWeight = value;
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
			get { return this.DimensionStyleOverrides.PostFix; }
			set
			{
				this.DimensionStyleOverrides.PostFix = value;
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
			get { return this.DimensionStyleOverrides.Rounding; }
			set
			{
				this.DimensionStyleOverrides.Rounding = value;
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
			get { return this.DimensionStyleOverrides.SuppressFirstExtensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressFirstExtensionLine = value;
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
			get { return this.DimensionStyleOverrides.SuppressSecondExtensionLine; }
			set
			{
				this.DimensionStyleOverrides.SuppressSecondExtensionLine = value;
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
			get { return this.DimensionStyleOverrides.SuppressOutsideExtensions; }
			set
			{
				this.DimensionStyleOverrides.SuppressOutsideExtensions = value;
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
			get { return this.DimensionStyleOverrides.TextVerticalAlignment; }
			set
			{
				this.DimensionStyleOverrides.TextVerticalAlignment = value;
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
			get { return this.DimensionStyleOverrides.DimensionUnit; }
			set
			{
				this.DimensionStyleOverrides.DimensionUnit = value;
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
			get { return this.DimensionStyleOverrides.ToleranceScaleFactor; }
			set
			{
				this.DimensionStyleOverrides.ToleranceScaleFactor = value;
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
			get { return this.DimensionStyleOverrides.TextInsideHorizontal; }
			set
			{
				this.DimensionStyleOverrides.TextInsideHorizontal = value;
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
			get { return this.DimensionStyleOverrides.TextInsideExtensions; }
			set
			{
				this.DimensionStyleOverrides.TextInsideExtensions = value;
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
			get { return this.DimensionStyleOverrides.MinusTolerance; }
			set
			{
				this.DimensionStyleOverrides.MinusTolerance = value;
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
			get { return this.DimensionStyleOverrides.TextOutsideExtensions; }
			set
			{
				this.DimensionStyleOverrides.TextOutsideExtensions = value;
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
			get { return this.DimensionStyleOverrides.TextOutsideHorizontal; }
			set
			{
				this.DimensionStyleOverrides.TextOutsideHorizontal = value;
			}
		}

		/// <summary>
		/// Dimension limits generated if nonzero
		/// </summary>
		/// <remarks>
		/// System variable DIMLIM
		/// </remarks>
		[CadSystemVariable("$DIMLIM", 70)]
		public bool DimensionLimitsGeneration
		{
			get { return this.DimensionStyleOverrides.LimitsGeneration; }
			set
			{
				this.DimensionStyleOverrides.LimitsGeneration = value;
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
			get { return this.DimensionStyleOverrides.PlusTolerance; }
			set
			{
				this.DimensionStyleOverrides.PlusTolerance = value;
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
			get { return this.DimensionStyleOverrides.TextHeight; }
			set
			{
				this.DimensionStyleOverrides.TextHeight = value;
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
			get { return this.DimensionStyleOverrides.TextDirection; }
			set
			{
				this.DimensionStyleOverrides.TextDirection = value;
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
			get { return this.DimensionStyleOverrides.AltMzf; }
			set
			{
				this.DimensionStyleOverrides.AltMzf = value;
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
			get { return this.DimensionStyleOverrides.AltMzs; }
			set
			{
				this.DimensionStyleOverrides.AltMzs = value;
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
			get { return this.DimensionStyleOverrides.Mzf; }
			set
			{
				this.DimensionStyleOverrides.Mzf = value;
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
			get { return this.DimensionStyleOverrides.Mzs; }
			set
			{
				this.DimensionStyleOverrides.Mzs = value;
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

		public Layer CurrentLayer { get; private set; } = Layer.Default;

		public DimensionStyle DimensionStyleOverrides { get; private set; } = DimensionStyle.Default;

		public UCS ModelSpace { get; private set; } = new UCS();

		public UCS PaperSpaceUcs { get; private set; } = new UCS();

		private readonly static PropertyExpression<CadHeader, CadSystemVariableAttribute> _propertyCache;

		public CadHeader() { }

		public CadHeader(ACadVersion version)
		{
			this.Version = version;
		}
		static CadHeader()
		{
			_propertyCache = new PropertyExpression<CadHeader, CadSystemVariableAttribute>(
				(info, attribute) => attribute.Name);
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
			var prop = _propertyCache.GetProperty(systemvar);

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
				prop.Setter(this, Convert.ChangeType(values.First(), prop.Property.PropertyType));
			}
			else
			{
				prop.Setter(this, Activator.CreateInstance(prop.Property.PropertyType, values));
			}
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