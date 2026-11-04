using ACadSharp.Entities;
using ACadSharp.Header;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using System;

namespace ACadSharp.IO.DWG;

/* Documentation:
 * The header variables of the drawings older than R13 follow the file header without any separation,
 * each one stored with a fixed size and in a fixed order.
 * R10 stores a prefix of the R11 variables, the R12 drawings add VISRETAIN at the end.
 *
 * The variables that reference a table entry store its index in the table,
 * the locations of the tables added after R10 are stored between the variables.
 *
 * Source: https://github.com/michal-josef-spacek/CAD-Format-DWG-AC1009 (dwg_ac1009.ksy)
 */
internal class DwgHeaderReaderAC09 : DwgSectionIO
{
	public override string SectionName { get { return DwgSectionDefinition.Header; } }

	private readonly DwgFileHeaderAC09 _fileHeader;

	private readonly CadHeader _header;

	private readonly DwgStreamReaderAC09 _reader;

	public DwgHeaderReaderAC09(ACadVersion version, DwgStreamReaderAC09 reader, DwgFileHeaderAC09 fileHeader, CadHeader header) : base(version)
	{
		this._reader = reader;
		this._fileHeader = fileHeader;
		this._header = header;
		this._header.Version = version;
	}

	public void Read(out DwgHeaderHandlesCollection headerHandles)
	{
		headerHandles = new DwgHeaderHandlesCollection();

		this._reader.Position = this._fileHeader.HeaderVariablesSeeker;

		//3RD : INSBASE
		this._header.ModelSpaceInsertionBase = this._reader.Read3RawDouble();
		//RS : PLINEGEN
		this._header.PolylineLineTypeGeneration = this._reader.ReadShort() != 0;
		//3RD : EXTMIN
		this._header.ModelSpaceExtMin = this._reader.Read3RawDouble();
		//3RD : EXTMAX
		this._header.ModelSpaceExtMax = this._reader.Read3RawDouble();
		//2RD : LIMMIN
		this._header.ModelSpaceLimitsMin = this._reader.Read2RawDouble();
		//2RD : LIMMAX
		this._header.ModelSpaceLimitsMax = this._reader.Read2RawDouble();
		//3RD : VIEWCTR
		XYZ viewCenter = this._reader.Read3RawDouble();
		//RD : VIEWSIZE
		double viewSize = this._reader.ReadDouble();
		//RS : SNAPMODE
		short snapMode = this._reader.ReadShort();
		//2RD : SNAPUNIT
		XY snapUnit = this._reader.Read2RawDouble();
		//2RD : SNAPBASE
		XY snapBase = this._reader.Read2RawDouble();
		//RD : SNAPANG
		double snapAngle = this._reader.ReadDouble();
		//RS : SNAPSTYLE
		short snapStyle = this._reader.ReadShort();
		//RS : SNAPISOPAIR
		short snapIsoPair = this._reader.ReadShort();
		//RS : GRIDMODE
		short gridMode = this._reader.ReadShort();
		//2RD : GRIDUNIT
		XY gridUnit = this._reader.Read2RawDouble();
		//RS : ORTHOMODE
		this._header.OrthoMode = this._reader.ReadShort() != 0;
		//RS : REGENMODE
		this._header.RegenerationMode = this._reader.ReadShort() != 0;
		//RS : FILLMODE
		this._header.FillMode = this._reader.ReadShort() != 0;
		//RS : QTEXTMODE
		this._header.QuickTextMode = this._reader.ReadShort() != 0;
		//RS : DRAGMODE
		short dragMode = this._reader.ReadShort();
		//RD : LTSCALE
		this._header.LineTypeScale = this._reader.ReadDouble();
		//RD : TEXTSIZE
		this._header.TextHeightDefault = this._reader.ReadDouble();
		//RD : TRACEWID
		this._header.TraceWidthDefault = this._reader.ReadDouble();
		//RS : CLAYER, index in the layer table
		short currentLayer = this._reader.ReadShort();
		//RL : Unknown
		int unknownrl1 = this._reader.ReadInt();
		//RL : Unknown
		int unknownrl2 = this._reader.ReadInt();
		//RS : Unknown
		short unknownrs1 = this._reader.ReadShort();
		//RS : PSLTSCALE
		short psltscale = this._reader.ReadShort();
		//RS : TREEDEPTH
		short treeDepth = this._reader.ReadShort();

		//R11+:
		if (this.R11Plus)
		{
			this._header.PaperSpaceLineTypeScaling = psltscale != 0 ? SpaceLineTypeScaling.Normal : SpaceLineTypeScaling.Viewport;
			this._header.SpatialIndexMaxTreeDepth = treeDepth;
		}

		//RS : Unknown
		short unknownrs2 = this._reader.ReadShort();
		//RD : Aspect ratio of the view
		double aspectRatio = this._reader.ReadDouble();
		//RS : LUNITS
		this._header.LinearUnitFormat = (LinearUnitFormat)this._reader.ReadShort();
		//RS : LUPREC
		this._header.LinearUnitPrecision = this._reader.ReadShort();
		//RS : AXISMODE
		short axisMode = this._reader.ReadShort();
		//2RD : AXISUNIT
		XY axisUnit = this._reader.Read2RawDouble();
		//RD : SKETCHINC
		this._header.SketchIncrement = this._reader.ReadDouble();
		//RD : FILLETRAD
		this._header.FilletRadius = this._reader.ReadDouble();
		//RS : AUNITS
		this._header.AngularUnit = (AngularUnitFormat)this._reader.ReadShort();
		//RS : AUPREC
		this._header.AngularUnitPrecision = this._reader.ReadShort();
		//RS : TEXTSTYLE, index in the style table
		short currentTextStyle = this._reader.ReadShort();
		//RS : OSMODE
		this._header.ObjectSnapMode = (ObjectSnapMode)this._reader.ReadShort();
		//RS : ATTMODE
		this._header.AttributeVisibility = (AttributeVisibilityMode)this._reader.ReadShort();
		//TF 15 : MENU
		this._header.MenuFileName = this._reader.ReadFixedText(15);

		//RD : DIMSCALE
		this._header.DimensionScaleFactor = this._reader.ReadDouble();
		//RD : DIMASZ
		this._header.DimensionArrowSize = this._reader.ReadDouble();
		//RD : DIMEXO
		this._header.DimensionExtensionLineOffset = this._reader.ReadDouble();
		//RD : DIMDLI
		this._header.DimensionLineIncrement = this._reader.ReadDouble();
		//RD : DIMEXE
		this._header.DimensionExtensionLineExtension = this._reader.ReadDouble();
		//RD : DIMTP
		this._header.DimensionPlusTolerance = this._reader.ReadDouble();
		//RD : DIMTM
		this._header.DimensionMinusTolerance = this._reader.ReadDouble();
		//RD : DIMTXT
		this._header.DimensionTextHeight = this._reader.ReadDouble();
		//RD : DIMCEN
		this._header.DimensionCenterMarkSize = this._reader.ReadDouble();
		//RD : DIMTSZ
		this._header.DimensionTickSize = this._reader.ReadDouble();
		//RC : DIMTOL
		this._header.DimensionGenerateTolerances = this._reader.ReadByte() != 0;
		//RC : DIMLIM
		this._header.DimensionLimitsGeneration = this._reader.ReadByte() != 0;
		//RC : DIMTIH
		this._header.DimensionTextInsideHorizontal = this._reader.ReadByte() != 0;
		//RC : DIMTOH
		this._header.DimensionTextOutsideHorizontal = this._reader.ReadByte() != 0;
		//RC : DIMSE1
		this._header.DimensionSuppressFirstExtensionLine = this._reader.ReadByte() != 0;
		//RC : DIMSE2
		this._header.DimensionSuppressSecondExtensionLine = this._reader.ReadByte() != 0;
		//RC : DIMTAD
		this._header.DimensionTextVerticalAlignment = (DimensionTextVerticalAlignment)this._reader.ReadByte();

		//RS : LIMCHECK
		this._header.LimitCheckingOn = this._reader.ReadShort() != 0;
		//TF 45 : Second part of the menu name
		string menuExtension = this._reader.ReadFixedText(45);
		//RD : ELEVATION
		this._header.Elevation = this._reader.ReadDouble();
		//RD : THICKNESS
		this._header.ThicknessDefault = this._reader.ReadDouble();
		//3RD : VIEWDIR
		XYZ viewDirection = this._reader.Read3RawDouble();
		//3RD x 6 : Unknown view points
		for (int i = 0; i < 6; i++)
		{
			this._reader.Read3RawDouble();
		}
		//RS : Flag 3d
		short flag3d = this._reader.ReadShort();
		//RS : BLIPMODE
		this._header.BlipMode = this._reader.ReadShort() != 0;
		//RC : DIMZIN
		this._header.DimensionZeroHandling = (ZeroHandling)this._reader.ReadByte();
		//RD : DIMRND
		this._header.DimensionRounding = this._reader.ReadDouble();
		//RD : DIMDLE
		this._header.DimensionLineExtension = this._reader.ReadDouble();
		//TF 33 : DIMBLK
		this._header.DimensionBlockName = this._reader.ReadFixedText(33);
		//RS : CIRCLEZOOM
		short circleZoom = this._reader.ReadShort();
		//RS : COORDS
		short coords = this._reader.ReadShort();
		//RS : CECOLOR
		this._header.CurrentEntityColor = new Color(this._reader.ReadShort());
		//RS : CELTYPE, index in the linetype table
		short currentLineType = this._reader.ReadShort();
		//RL + RL : TDCREATE, julian day and milliseconds
		this._header.CreateDateTime = this._reader.Read8BitJulianDate();
		//RL + RL : TDUPDATE, julian day and milliseconds
		this._header.UpdateDateTime = this._reader.Read8BitJulianDate();
		//RL + RL : TDINDWG, days and milliseconds
		this._header.TotalEditingTime = this.readTimeSpan();
		//RL + RL : TDUSRTIMER, days and milliseconds
		this._header.UserElapsedTimeSpan = this.readTimeSpan();
		//RS : USRTIMER
		this._header.UserTimer = this._reader.ReadShort() != 0;
		//RS : FASTZOOM
		short fastZoom = this._reader.ReadShort();
		//RS : SKPOLY
		this._header.SketchPolylines = this._reader.ReadShort() != 0;
		//14 bytes : Unknown date
		this._reader.ReadBytes(14);
		//RD : ANGBASE
		this._header.AngleBase = this._reader.ReadDouble();
		//RS : ANGDIR
		this._header.AngularDirection = (AngularDirection)this._reader.ReadShort();
		//RS : PDMODE
		this._header.PointDisplayMode = this._reader.ReadShort();
		//RD : PDSIZE
		this._header.PointDisplaySize = this._reader.ReadDouble();
		//RD : PLINEWID
		this._header.PolylineWidthDefault = this._reader.ReadDouble();
		//RS : USERI1-5
		this._header.UserShort1 = this._reader.ReadShort();
		this._header.UserShort2 = this._reader.ReadShort();
		this._header.UserShort3 = this._reader.ReadShort();
		this._header.UserShort4 = this._reader.ReadShort();
		this._header.UserShort5 = this._reader.ReadShort();
		//RD : USERR1-5
		this._header.UserDouble1 = this._reader.ReadDouble();
		this._header.UserDouble2 = this._reader.ReadDouble();
		this._header.UserDouble3 = this._reader.ReadDouble();
		this._header.UserDouble4 = this._reader.ReadDouble();
		this._header.UserDouble5 = this._reader.ReadDouble();
		//RC : DIMALT
		this._header.DimensionAlternateUnitDimensioning = this._reader.ReadByte() != 0;
		//RC : DIMALTD
		this._header.DimensionAlternateUnitDecimalPlaces = this._reader.ReadByte();
		//RC : DIMASO
		this._header.AssociatedDimensions = this._reader.ReadByte() != 0;
		//RC : DIMSHO
		this._header.UpdateDimensionsWhileDragging = this._reader.ReadByte() != 0;
		//TF 16 : DIMPOST
		this._header.DimensionPostFix = this._reader.ReadFixedText(16);
		//TF 16 : DIMAPOST
		this._header.DimensionAlternateDimensioningSuffix = this._reader.ReadFixedText(16);
		//RD : DIMALTF
		this._header.DimensionAlternateUnitScaleFactor = this._reader.ReadDouble();
		//RD : DIMLFAC
		this._header.DimensionLinearScaleFactor = this._reader.ReadDouble();
		//RS : SPLINESEGS
		this._header.NumberOfSplineSegments = this._reader.ReadShort();
		//RS : SPLFRAME
		this._header.ShowSplineControlPoints = this._reader.ReadShort() != 0;
		//RS : ATTREQ
		short attributeRequest = this._reader.ReadShort();
		//RS : ATTDIA
		short attributeDialog = this._reader.ReadShort();
		//RD : CHAMFERA
		this._header.ChamferDistance1 = this._reader.ReadDouble();
		//RD : CHAMFERB
		this._header.ChamferDistance2 = this._reader.ReadDouble();
		//RS : MIRRTEXT
		this._header.MirrorText = this._reader.ReadShort() != 0;

		//UCS table
		this.readTableDescriptor(DxfFileToken.TableUcs);

		//RS : DWGCODEPAGE, read with the file header
		this._reader.ReadShort();

		//3RD : UCSORG
		this._header.ModelSpaceOrigin = this._reader.Read3RawDouble();
		//3RD : UCSXDIR
		this._header.ModelSpaceXAxis = this._reader.Read3RawDouble();
		//3RD : UCSYDIR
		this._header.ModelSpaceYAxis = this._reader.Read3RawDouble();
		//3RD : TARGET
		XYZ target = this._reader.Read3RawDouble();
		//RD : LENSLENGTH
		double lensLength = this._reader.ReadDouble();
		//RD : VIEWTWIST
		double viewTwist = this._reader.ReadDouble();
		//RD : FRONTZ
		double frontZ = this._reader.ReadDouble();
		//RD : BACKZ
		double backZ = this._reader.ReadDouble();
		//RS : VIEWMODE
		short viewMode = this._reader.ReadShort();
		//RC : DIMTOFL
		this._header.DimensionTextOutsideExtensions = this._reader.ReadByte() != 0;
		//TF 33 : DIMBLK1
		this._header.DimensionBlockNameFirst = this._reader.ReadFixedText(33);
		//TF 33 : DIMBLK2
		this._header.DimensionBlockNameSecond = this._reader.ReadFixedText(33);
		//RC : DIMSAH
		this._header.DimensionSeparateArrowBlocks = this._reader.ReadByte() != 0;
		//RC : DIMTIX
		this._header.DimensionTextInsideExtensions = this._reader.ReadByte() != 0;
		//RC : DIMSOXD
		this._header.DimensionSuppressOutsideExtensions = this._reader.ReadByte() != 0;
		//RD : DIMTVP
		this._header.DimensionTextVerticalPosition = this._reader.ReadDouble();
		//TF 33 : Unknown text
		string unknowntf = this._reader.ReadFixedText(33);
		//RS : HANDLING
		short handling = this._reader.ReadShort();
		//8 bytes : HANDSEED, the reader assigns its own handles
		this._reader.ReadBytes(8);
		//RS : SURFU
		this._header.SurfaceDensityU = this._reader.ReadShort();
		//RS : SURFV
		this._header.SurfaceDensityV = this._reader.ReadShort();
		//RS : SURFTYPE
		this._header.SurfaceType = this._reader.ReadShort();
		//RS : SURFTAB1
		this._header.SurfaceMeshTabulationCount1 = this._reader.ReadShort();
		//RS : SURFTAB2
		this._header.SurfaceMeshTabulationCount2 = this._reader.ReadShort();

		//VPORT table
		this.readTableDescriptor(DxfFileToken.TableVport);

		//RS : FLATLAND
		short flatland = this._reader.ReadShort();
		//RS : SPLINETYPE
		this._header.SplineType = (SplineType)this._reader.ReadShort();
		//RS : UCSICON
		short ucsIcon = this._reader.ReadShort();
		//RS : UCSNAME, index in the ucs table
		short currentUcs = this._reader.ReadShort();

		if (this._fileHeader.HeaderVariablesCount >= 160)
		{
			//APPID table, only in the last R10 drawings
			this.readTableDescriptor(DxfFileToken.TableAppId);
			//RS : WORLDVIEW
			this._header.WorldView = this._reader.ReadShort() != 0;
		}

		short currentPaperUcs = -1;

		//R11+:
		if (this.R11Plus)
		{
			//RS : Unknown
			short unknownrs3 = this._reader.ReadShort();
			//RS : Unknown
			short unknownrs4 = this._reader.ReadShort();

			//DIMSTYLE table
			this.readTableDescriptor(DxfFileToken.TableDimstyle);

			//5 bytes : Unknown
			this._reader.ReadBytes(5);
			//RS : DIMCLRD
			this._header.DimensionLineColor = new Color(this._reader.ReadShort());
			//RS : DIMCLRE
			this._header.DimensionExtensionLineColor = new Color(this._reader.ReadShort());
			//RS : DIMCLRT
			this._header.DimensionTextColor = new Color(this._reader.ReadShort());
			//RS : SHADEDGE
			this._header.ShadeEdge = (ShadeEdgeType)this._reader.ReadShort();
			//RS : SHADEDIF
			this._header.ShadeDiffuseToAmbientPercentage = this._reader.ReadShort();
			//RS : Unknown
			short unknownrs5 = this._reader.ReadShort();
			//RS : UNITMODE
			this._header.UnitMode = this._reader.ReadShort();
			//4 RD : Unit ratios
			for (int i = 0; i < 4; i++)
			{
				this._reader.ReadDouble();
			}
			//4 TF 32 : Unit names
			for (int i = 0; i < 4; i++)
			{
				this._reader.ReadFixedText(32);
			}
			//RD : DIMTFAC
			this._header.DimensionToleranceScaleFactor = this._reader.ReadDouble();
			//3RD : PUCSORG
			this._header.PaperSpaceUcsOrigin = this._reader.Read3RawDouble();
			//3RD : PUCSXDIR
			this._header.PaperSpaceUcsXAxis = this._reader.Read3RawDouble();
			//3RD : PUCSYDIR
			this._header.PaperSpaceUcsYAxis = this._reader.Read3RawDouble();
			//RS : PUCSNAME, index in the ucs table
			currentPaperUcs = this._reader.ReadShort();
			//RS : TILEMODE
			this._header.ShowModelSpace = this._reader.ReadShort() != 0;
			//RS : PLIMCHECK
			this._header.PaperSpaceLimitsChecking = this._reader.ReadShort() != 0;
			//RS : Unknown
			short unknownrs6 = this._reader.ReadShort();
			//3RD : PEXTMIN
			this._header.PaperSpaceExtMin = this._reader.Read3RawDouble();
			//3RD : PEXTMAX
			this._header.PaperSpaceExtMax = this._reader.Read3RawDouble();
			//2RD : PLIMMIN
			this._header.PaperSpaceLimitsMin = this._reader.Read2RawDouble();
			//2RD : PLIMMAX
			this._header.PaperSpaceLimitsMax = this._reader.Read2RawDouble();
			//3RD : PINSBASE
			this._header.PaperSpaceInsertionBase = this._reader.Read3RawDouble();

			//VX table, internal table of the viewports
			this.readTableDescriptor("VX");

			//RS : MAXACTVP
			this._header.MaxViewportCount = this._reader.ReadShort();
			//RD : DIMGAP
			this._header.DimensionLineGap = this._reader.ReadDouble();
			//RD : PELEVATION
			this._header.PaperSpaceElevation = this._reader.ReadDouble();

			//R12:
			if (this._fileHeader.HeaderVariablesCount >= 205)
			{
				//RS : VISRETAIN
				this._header.RetainXRefDependentVisibilitySettings = this._reader.ReadShort() != 0;
			}
		}

		//The references to the tables are resolved with the handles assigned to the entries
		headerHandles.CLAYER = this.getEntryHandle(DxfFileToken.TableLayer, currentLayer);
		headerHandles.TEXTSTYLE = this.getEntryHandle(DxfFileToken.TableStyle, currentTextStyle);
		headerHandles.UCSNAME_MSPACE = this.getEntryHandle(DxfFileToken.TableUcs, currentUcs);
		headerHandles.UCSNAME_PSPACE = this.getEntryHandle(DxfFileToken.TableUcs, currentPaperUcs);
		headerHandles.MODEL_SPACE = this._fileHeader.ModelSpaceHandle;
		headerHandles.PAPER_SPACE = this._fileHeader.PaperSpaceHandle;

		//BYLAYER and BYBLOCK are outside of the table
		headerHandles.CELTYPE = this.getEntryHandle(DxfFileToken.TableLinetype, currentLineType);
		if (headerHandles.CELTYPE == null && IsByBlockLineType(this._version, currentLineType))
		{
			this._header.CurrentLineTypeName = LineType.ByBlockName;
		}
	}

	/// <summary>
	/// Check if the line type index of an entity or the header means BYBLOCK.
	/// </summary>
	/// <remarks>
	/// R11 uses 32767 for BYLAYER and 32766 for BYBLOCK, R10 stores the index of the entities in a byte with 255 for BYBLOCK.
	/// </remarks>
	public static bool IsByBlockLineType(ACadVersion version, int index)
	{
		return index == short.MaxValue - 1
			|| (version < ACadVersion.AC1009 && index == byte.MaxValue);
	}

	private ulong? getEntryHandle(string table, int index)
	{
		if (index < 0
			|| !this._fileHeader.Tables.TryGetValue(table, out DwgFileHeaderAC09.TableDescriptor descriptor)
			|| index >= descriptor.Count)
		{
			return null;
		}

		return this._fileHeader.GetEntryHandle(table, index);
	}

	private void readTableDescriptor(string name)
	{
		DwgFileHeaderAC09.TableDescriptor table = new DwgFileHeaderAC09.TableDescriptor();

		//RS : Size of the entries
		table.ItemSize = this._reader.ReadShort();
		//RS : Number of entries
		table.Count = this._reader.ReadShort();
		//RS : Unknown
		this._reader.ReadShort();
		//RL : Address of the table
		table.Seeker = this._reader.ReadInt();

		this._fileHeader.Tables[name] = table;
	}

	private TimeSpan readTimeSpan()
	{
		int days = this._reader.ReadInt();
		int milliseconds = this._reader.ReadInt();

		if (days < 0 || milliseconds < 0)
		{
			return TimeSpan.Zero;
		}

		return TimeSpan.FromDays(days) + TimeSpan.FromMilliseconds(milliseconds);
	}
}
