using ACadSharp.Attributes;
using ACadSharp.Entities;
using ACadSharp.Types.Units;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
		public string VersionString { get; set; }
		public ACadVersion Version { get; set; }    //TODO: Fix the string version

		/// <summary>
		/// System variable ACADMAINTVER.
		/// Maintenance version number(should be ignored)
		/// </summary>
		[CadSystemVariable("$ACADMAINTVER", DxfCode.Int16)]
		public short MaintenanceVersion { get; set; }

		/// <summary>
		/// Drawing code page; set to the system code page when a new drawing is created,
		/// but not otherwise maintained by AutoCAD
		/// </summary>
		/// <remarks>
		/// System variable DWGCODEPAGE
		/// </remarks>
		[CadSystemVariable("$DWGCODEPAGE", 3)]
		public string CodePage { get; set; }

		/// <summary>
		/// Displays the name of the last person who modified the file
		/// </summary>
		/// <remarks>
		/// System variable LASTSAVEDBY
		/// </remarks>
		[CadSystemVariable("$LASTSAVEDBY", 3)]
		public string LastSavedBy { get; set; }

		/// <summary>
		/// System variable REQUIREDVERSIONS.
		/// The default value is 0.
		/// Read only.
		/// </summary>
		/// <remarks>Only in <see cref="ACadVersion.AC1024"/> or above.</remarks>
		[CadSystemVariable("$REQUIREDVERSIONS", DxfCode.Int16)]
		public long RequiredVersions { get; set; }

		/// <summary>
		/// System variable DIMASO.
		/// </summary>
		/// <remarks>
		/// Obsolete; see DIMASSOC.
		/// </remarks>
		[CadSystemVariable("$DIMASO", DxfCode.Int16)]
		public bool AssociatedDimensions { get; set; } = true;

		/// <summary>
		/// System variable DIMSHO.
		/// </summary>
		[CadSystemVariable("$DIMSHO", DxfCode.Int16)]
		public bool UpdateDimensionsWhileDragging { get; set; } = true;

		/// <summary>
		/// Undocumented
		/// </summary>
		public bool DIMSAV { get; set; }

		/// <summary>
		/// System variable PLINEGEN.
		/// </summary>
		[CadSystemVariable("$PLINEGEN", DxfCode.Int16)]
		public bool PolylineLineTypeGeneration { get; set; }

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
		public SpaceLineTypeScaling PaperSpaceLineTypeScaling { get; set; }

		/// <summary>
		/// Nonzero if limits checking is on
		/// System variable LIMCHECK.
		/// </summary>
		[CadSystemVariable("$LIMCHECK", DxfCode.Int16)]
		public bool LimitCheckingOn { get; set; }

		/// <summary>
		/// System variable BLIPMODE.	??
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
		public AngularDirection AngularDirection { get; set; }

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
		public bool MirrorText { get; set; }

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
		/// System variable DISPSILH
		/// </summary>
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
		/// System variable LUNITS
		/// </summary>
		[CadSystemVariable("$LUNITS", DxfCode.Int16)]
		public LinearUnitFormat LinearUnitFormat { get; set; }

		/// <summary>
		/// 
		/// System variable LUPREC
		/// </summary>
		public short LinearUnitPrecision { get; set; }

		/// <summary>
		/// 
		/// System variable AUNITS
		/// </summary>
		public AngularUnitFormat AngularUnit { get; set; }

		/// <summary>
		/// 
		/// System variable AUPREC
		/// </summary>
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
		/// 
		/// System variable PDMODE
		/// </summary>
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
		/// 
		/// System variable SHADEDIF
		/// </summary>
		public short ShadeDiffuseToAmbientPercentage { get; set; }
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
		/// Current multiline justification.
		/// System variable CMLJUST
		/// </summary>
		public TextVerticalAlignment CurrentMultilineJustification { get; set; }

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
		public double LineTypeScale { get; set; }

		/// <summary>
		/// Default text height
		/// </summary>
		/// <remarks>
		/// System variable TEXTSIZE
		/// </remarks>
		[CadSystemVariable("$TEXTSIZE", 40)]
		public double TextHeightDefault { get; set; }

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
		/// 
		/// System variable 
		/// </summary>
		public double AngleBase { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
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
		/// 
		/// System variable 
		/// </summary>
		public double ChamferDistance1 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ChamferDistance2 { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ChamferLength { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ChamferAngle { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double FacetResolution { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double CurrentMultilineScale { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double CurrentEntityLinetypeScale { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public string MenuFileName { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public DateTime CreateDateTime { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public DateTime UpdateDateTime { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public TimeSpan TotalEditingTime { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public TimeSpan UserElapsedTimeSpan { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public Color CurrentEntityColor { get; set; }
		/// <summary>
		/// 
		/// System variable 
		/// </summary>
		public double ViewportDefaultViewScaleFactor { get; set; }
		/// <summary>
		/// PSPACE
		/// </summary>
		public UCS PaperSpaceUcs { get; set; } = new UCS();
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
		/// System variable EXTMAX.
		/// X, Y, and Z drawing extents upper-right corner(in WCS)
		/// </summary>
		[CadSystemVariable("$EXTMAX", DxfCode.XCoordinate, DxfCode.YCoordinate, DxfCode.ZCoordinate)]
		public XYZ ExtMax { get; set; }
		/// <summary>
		/// System variable LIMMIN.
		/// XY drawing limits lower-left corner (in WCS)
		/// </summary>
		[CadSystemVariable("$LIMMIN", DxfCode.XCoordinate, DxfCode.YCoordinate)]
		public XY LimitsMin { get; set; }
		/// <summary>
		/// System variable LIMMAX.
		/// XY drawing limits upper-right corner (in WCS)
		/// </summary>
		[CadSystemVariable("$LIMMAX", DxfCode.XCoordinate, DxfCode.YCoordinate)]
		public XY LimitsMax { get; set; }
		public double Elevation { get; set; }
		public string DimensionBlockName { get; set; }
		public string DimensionBlockNameFirst { get; set; }
		public string DimensionBlockNameSecond { get; set; }
		public short StackedTextAlignment { get; set; }
		public short StackedTextSizePercentage { get; set; }
		public string HyperLinkBase { get; set; }
		public short CurrentEntityLineWeight { get; set; }
		public short EndCaps { get; set; }
		public short JoinStyle { get; set; }
		public short DisplayLineWeight { get; set; }
		public short XEdit { get; set; }
		public short ExtendedNames { get; set; }
		public short PlotStyleMode { get; set; }
		public short LoadOLEObject { get; set; }
		public short InsUnits { get; set; }
		public short CurrentEntityPlotStyleType { get; set; }
		public string FingerPrintGuid { get; set; }
		public string VersionGuid { get; set; }
		public ObjectSortingFlags EntitySortingFlags { get; set; }
		public byte IndexCreationFlags { get; set; }
		public byte HideText { get; set; }
		public byte ExternalReferenceClippingBoundaryType { get; set; }
		/// <summary>
		/// System variable DIMASSOC.
		/// Controls the associativity of dimension objects
		/// </summary>
		[CadSystemVariable("$DIMASSOC", DxfCode.Int8)]
		public DimensionAssociation DimensionAssociativity { get; set; }
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

		public UCS Ucs { get; set; } = new UCS();

		public DimensionStyle DimensionStyleOverrides { get; set; } = new DimensionStyle();
		#endregion

		internal ulong HandleSeed { get; set; }

		public CadHeader() { }
		public CadHeader(ACadVersion version)
		{
			this.Version = version;
		}

		public static Dictionary<string, DxfCode[]> GetHeaderMap()
		{
			Dictionary<string, DxfCode[]> map = new Dictionary<string, DxfCode[]>();
			foreach (PropertyInfo p in typeof(CadHeader).GetProperties())
			{
				CadSystemVariableAttribute att = p.GetCustomAttribute<CadSystemVariableAttribute>();
				if (att == null)
					continue;

				map.Add(att.Name, att.ValueCodes);
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
	}
}
