using System;
using System.Collections.Generic;
using System.Text;

namespace ACadSharp
{
	public static class DxfFileToken
	{
		public const string Undefined = "";

		public const string BeginSection = "SECTION";
		public const string EndSection = "ENDSEC";
		public const string EndSequence = "SEQEND";
		public const string EndOfFile = "EOF";

		public const string HeaderSection = "HEADER";

		public const string ClassesSection = "CLASSES";
		public const string ClassEntry = "CLASS";

		#region Tables
		public const string TablesSection = "TABLES";
		public const string TableEntry = "TABLE";
		public const string EndTable = "ENDTAB";
		public const string TableAppId = "APPID";
		public const string TableBlockRecord = "BLOCK_RECORD";
		public const string TableDimstyle = "DIMSTYLE";
		public const string TableLayer = "LAYER";
		public const string TableLinetype = "LTYPE";
		public const string TableStyle = "STYLE";
		public const string TableUcs = "UCS";
		public const string TableView = "VIEW";
		public const string TableVport = "VPORT"; 
		#endregion

		public const string BlocksSection = "BLOCKS";
		public const string Block = "BLOCK";

		#region Entities
		public const string EntitiesSection = "ENTITIES";
		public const string Entity3DFace = "3DFACE";
		public const string Entity3DSolid = "3DSOLID";
		public const string EntityProxyEntity = "ACAD_PROXY_ENTITY";
		public const string EntityArc = "ARC";
		public const string EntityAttributeDefinition = "ATTDEF";
		public const string EntityAttribute = "ATTRIB";
		public const string EntityBody = "BODY";
		public const string EntityCircle = "CIRCLE";
		public const string EntityCoordinationModel = "COORDINATION MODEL";
		public const string EntityDimension = "DIMENSION";
		public const string EntityEllipse = "ELLIPSE";
		public const string EntityHatch = "HATCH";
		public const string EntityHelix = "HELIX";
		public const string EntityImage = "IMAGE";
		public const string EntityInsert = "INSERT";
		public const string EntityLeader = "LEADER";
		public const string EntityLight = "LIGHT";
		public const string EntityLine = "LINE";
		public const string EntityLwPolyline = "LWPOLYLINE";
		public const string EntityMesh = "MESH";
		public const string EntityMLeader = "MLEADER";
		public const string EntityMLeaderStyle = "MLEADERSTYLE";
		public const string EntityMLine = "MLINE";
		public const string EntityMText = "MTEXT";
		public const string EntityOleFrame = "OLEFRAME";
		public const string EntityOle2Frame = "OLE2FRAME";
		public const string EntityPoint = "POINT";
		public const string EntityPolyline = "POLYLINE";
		public const string EntityRay = "RAY";
		public const string EntityRegion = "REGION";
		public const string EntitySection = "SECTION";
		public const string EntitySeqend = "SEQEND";
		public const string EntityShape = "SHAPE";
		public const string EntitySolid = "SOLID";
		public const string EntitySpline = "SPLINE";
		public const string EntitySun = "SUN";
		public const string EntitySurface = "SURFACE";
		public const string EntityTable = "TABLE";
		public const string EntityText = "TEXT";
		public const string EntityTolerance = "TOLERANCE";
		public const string EntityTrace = "TRACE";
		public const string EntityUnderlay = "UNDERLAY";
		public const string EntityVertex = "VERTEX";
		public const string EntityViewport = "VIEWPORT";
		public const string EntityWipeout = "WIPEOUT";
		public const string EntityXline = "XLINE";
		#endregion

		public const string ObjectsSection = "OBJECTS";
		public const string ObjectDictionary = "DICTIONARY";
	}

	public static class DxfSubclassMarker
	{
		public const string ApplicationId = "AcDbRegAppTableRecord";
		public const string Table = "AcDbSymbolTable";
		public const string TableRecord = "AcDbSymbolTableRecord";
		public const string Layer = "AcDbLayerTableRecord";
		public const string VPort = "AcDbViewportTableRecord";
		public const string View = "AcDbViewTableRecord";
		public const string Linetype = "AcDbLinetypeTableRecord";
		public const string TextStyle = "AcDbTextStyleTableRecord";
		public const string MLineStyle = "AcDbMlineStyle";
		public const string DimensionStyleTable = "AcDbDimStyleTable";
		public const string DimensionStyle = "AcDbDimStyleTableRecord";
		public const string Ucs = "AcDbUCSTableRecord";
		public const string Dimension = "AcDbDimension";
		public const string AlignedDimension = "AcDbAlignedDimension";
		public const string LinearDimension = "AcDbRotatedDimension";
		public const string RadialDimension = "AcDbRadialDimension";
		public const string DiametricDimension = "AcDbDiametricDimension";
		public const string Angular3PointDimension = "AcDb3PointAngularDimension";
		public const string Angular2LineDimension = "AcDb2LineAngularDimension";
		public const string OrdinateDimension = "AcDbOrdinateDimension";
		public const string BlockRecord = "AcDbBlockTableRecord";
		public const string BlockBegin = "AcDbBlockBegin";
		public const string BlockEnd = "AcDbBlockEnd";
		public const string Entity = "AcDbEntity";
		public const string Arc = "AcDbArc";
		public const string Circle = "AcDbCircle";
		public const string Ellipse = "AcDbEllipse";
		public const string Spline = "AcDbSpline";
		public const string Face3d = "AcDbFace";
		public const string Helix = "AcDbHelix";
		public const string Insert = "AcDbBlockReference";
		public const string Line = "AcDbLine";
		public const string Ray = "AcDbRay";
		public const string XLine = "AcDbXline";
		public const string MLine = "AcDbMline";
		public const string Point = "AcDbPoint";
		public const string Vertex = "AcDbVertex";
		public const string Polyline = "AcDb2dPolyline";
		public const string Leader = "AcDbLeader";
		public const string LwPolyline = "AcDbPolyline";
		public const string PolylineVertex = "AcDb2dVertex ";
		public const string Polyline3d = "AcDb3dPolyline";
		public const string Polyline3dVertex = "AcDb3dPolylineVertex";
		public const string PolyfaceMesh = "AcDbPolyFaceMesh";
		public const string PolyfaceMeshVertex = "AcDbPolyFaceMeshVertex";
		public const string PolyfaceMeshFace = "AcDbFaceRecord";
		public const string Shape = "AcDbShape";
		public const string Solid = "AcDbTrace";
		public const string Trace = "AcDbTrace";
		public const string Text = "AcDbText";
		public const string Tolerance = "AcDbFcf";
		public const string Wipeout = "AcDbWipeout";
		public const string Mesh = "AcDbSubDMesh";
		public const string MText = "AcDbMText";
		public const string Hatch = "AcDbHatch";
		public const string Underlay = "AcDbUnderlayReference";
		public const string UnderlayDefinition = "AcDbUnderlayDefinition";
		public const string Viewport = "AcDbViewport";
		public const string Attribute = "AcDbAttribute";
		public const string AttributeDefinition = "AcDbAttributeDefinition";
		public const string Dictionary = "AcDbDictionary";
		public const string XRecord = "AcDbXrecord";
		public const string RasterImage = "AcDbRasterImage";
		public const string RasterImageDef = "AcDbRasterImageDef";
		public const string RasterImageDefReactor = "AcDbRasterImageDefReactor";
		public const string RasterVariables = "AcDbRasterVariables";
		public const string Group = "AcDbGroup";
		public const string Layout = "AcDbLayout";
		public const string PlotSettings = "AcDbPlotSettings";
	}
}
