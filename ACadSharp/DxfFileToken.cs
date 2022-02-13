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
		public const string TableGroup = "GROUP";
		public const string TableLayer = "LAYER";
		public const string TableLinetype = "LTYPE";
		public const string TableStyle = "STYLE";
		public const string TableUcs = "UCS";
		public const string TableView = "VIEW";
		public const string TableVport = "VPORT"; 
		public const string TableXRecord = "XRECORD"; 

		#endregion

		public const string BlocksSection = "BLOCKS";
		public const string Block = "BLOCK";
		public const string EndBlock = "ENDBLK";

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
		public const string ObjectPlotSettings = "PLOTSETTINGS";
		public const string ObjectLayout = "LAYOUT";
		public const string ObjectMLStyle = "MLINESTYLE";
	}
}
