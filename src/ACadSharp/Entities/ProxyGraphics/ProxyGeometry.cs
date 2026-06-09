using ACadSharp.IO;
using ACadSharp.IO.DWG;
using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ACadSharp.Entities.ProxyGraphics;

// "TODO: VALIDATE" comments used to indicate type parser that have not yet been tested

public class ProxyGeometry
{
	internal static IEnumerable<IProxyGeometry> ReadGeometries(CadDocumentBuilder builder, byte[] arr)
	{
		List<IProxyGeometry> geometries = new();

		StreamIO stream = new StreamIO(arr);
		stream.EndianConverter = new LittleEndianConverter();
		var size = stream.ReadInt();
		var count = stream.ReadInt();

		for (int i = 0; i < count; i++)
		{
			var objSize = stream.ReadInt(); //Includes size and type
			GraphicsType type = (GraphicsType)stream.ReadInt();

			var pos = stream.Position;

			switch (type)
			{
				case GraphicsType.Extents:
					geometries.Add(readExtents(stream));
					break;
				case GraphicsType.Circle:
					geometries.Add(readCircle(stream));
					break;
				case GraphicsType.CirclePt3:
					geometries.Add(readCirclePt3(stream));
					break;
				case GraphicsType.CircularArc:
					geometries.Add(readCircularArc(stream));
					break;
				case GraphicsType.CircularArc3Pt:
					geometries.Add(readCircularArc3Pt(stream));
					break;
				case GraphicsType.Polyline:
					geometries.Add(readPolyline(stream));
					break;
				case GraphicsType.Polygon:
					geometries.Add(readPolygon(stream));
					break;
				case GraphicsType.Mesh:
					geometries.Add(readMesh(stream));
					break;
				case GraphicsType.Shell:
					geometries.Add(readShell(stream));
					break;
				case GraphicsType.Text:
					geometries.Add(readText(stream));
					break;
				case GraphicsType.Text2:
					geometries.Add(readText2(stream));
					break;
				case GraphicsType.XLine:
					geometries.Add(readXLine(stream));
					break;
				case GraphicsType.Ray:
					geometries.Add(readRay(stream));
					break;
				case GraphicsType.SubentColor:
					geometries.Add(readSubentColor(stream));
					break;
				case GraphicsType.SubentLayer:
					geometries.Add(readSubentLayer(stream));
					break;
				case GraphicsType.SubentLineType:
					geometries.Add(readSubentLineType(stream));
					break;
				case GraphicsType.SubentMarker:
					geometries.Add(readSubentMarker(stream));
					break;
				case GraphicsType.SubentFillon:
					geometries.Add(readSubentFillon(stream));
					break;
				case GraphicsType.SubentTrueColor:
					geometries.Add(readSubentTrueColor(stream));
					break;
				case GraphicsType.SubentLineWeight:
					geometries.Add(readSubentLineWeight(stream));
					break;
				case GraphicsType.SubentLineTypeScale:
					geometries.Add(readSubentLineTypeScale(stream));
					break;
				case GraphicsType.SubentThickness:
					geometries.Add(readSubentThickness(stream));
					break;
				case GraphicsType.SubentPlotStyleName:
					geometries.Add(readSubentPlotStyleName(stream));
					break;
				case GraphicsType.PushClip:
					geometries.Add(readPushClip(stream));
					break;
				case GraphicsType.PopClip:
					geometries.Add(readPopClip(stream));
					break;
				case GraphicsType.PushModelTransform:
					geometries.Add(readPushModelTransform(stream));
					break;
				case GraphicsType.PushModelTransform2:
					geometries.Add(readPushModelTransform2(stream));
					break;
				case GraphicsType.PophModelTransform:
					geometries.Add(readPopModelTransform(stream));
					break;
				case GraphicsType.PolylineWithNormal:
					geometries.Add(readPolylineWithNormal(stream));
					break;
				case GraphicsType.LwPolyine:
					geometries.Add(readLwPolyine(builder, stream));
					break;
				case GraphicsType.SubEntityMaterial:
					geometries.Add(readSubEntityMaterial(stream));
					break;
				case GraphicsType.SubEntityMapper:
					geometries.Add(readSubEntityMapper(stream));
					break;
				case GraphicsType.UnicodeText:
					geometries.Add(readUnicodeText(stream));
					break;
				case GraphicsType.Unknown37:
					geometries.Add(readUnknown37(stream));
					break;
				case GraphicsType.UnicodeText2:
					geometries.Add(readUnicodeText2(stream));
					break;
				case GraphicsType.Unknown:
				default:
					// Type 51 (size 12, data: zeros)
					break;
			}

			long readDiff = objSize - (stream.Position - pos) - 8;
			if (readDiff > 0)
			{
				//jump not implemented proxies
				stream.ReadBytes((int) readDiff);
			}
		}

		return geometries;
	}

	private static IProxyGeometry readCircle(StreamIO stream)
	{
		ProxyCircle circle = new ProxyCircle();
		circle.Center = readPoint(stream);
		circle.Radius = stream.ReadDouble();
		circle.Normal = readPoint(stream);
		return circle;
	}

	private static IProxyGeometry readCirclePt3(StreamIO stream)
	{
		ProxyCirclePt3 circle = new ProxyCirclePt3();

		circle.Point1 = readPoint(stream);
		circle.Point2 = readPoint(stream);
		circle.Point3 = readPoint(stream);

		Debugger.Break();	// TODO: VALIDATE
		return circle;
	}

	private static IProxyGeometry readCircularArc(StreamIO stream)
	{
		ProxyCircularArc arc = new ProxyCircularArc();

		arc.Center = readPoint(stream);
		arc.Radius = stream.ReadDouble();
		arc.Normal = readPoint(stream);
		arc.StartVectorDirection = readPoint(stream);
		arc.SweepAngle = stream.ReadDouble();
		arc.ArcType = stream.ReadInt();

		return arc;
	}

	private static IProxyGeometry readCircularArc3Pt(StreamIO stream)
	{
		ProxyCircularArc3Pt arc = new ProxyCircularArc3Pt();

		arc.Point1 = readPoint(stream);
		arc.Point2 = readPoint(stream);
		arc.Point3 = readPoint(stream);
		arc.ArcType = stream.ReadInt();

		return arc;
	}

	private static IProxyGeometry readExtents(StreamIO stream)
	{
		ProxyExtents extents = new ProxyExtents();

		extents.Min = readPoint(stream);
		extents.Max = readPoint(stream);

		return extents;
	}

	private static IProxyGeometry readLwPolyine(CadDocumentBuilder builder, StreamIO stream)
	{
		int dataSize = stream.ReadInt();
		ProxyLwPolyine polyline = new ProxyLwPolyine();

		IDwgStreamReader reader = DwgStreamReaderBase.GetStreamHandler(builder.Version, stream.Stream);

		polyline.Entity = new LwPolyline();
		DwgObjectReader.readLWPolyline(reader, builder.Version, polyline.Entity);
		// TODO: Read LwPolyline entity w/o common data

		polyline.Unknown1 = stream.ReadByte();
		polyline.Unknown2 = stream.ReadByte();
		polyline.Unknown3 = stream.ReadByte();

		return polyline;
	}

	private static IProxyGeometry readMesh(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPolygon(StreamIO stream)
	{
		ProxyPolygon polygon = new ProxyPolygon();
		polygon.Points = [];

		int pointCount = stream.ReadInt();
		for (int i = 0; i < pointCount; i++)
		{
			polygon.Points.Add(readPoint(stream));
		}

		return polygon;
	}

	private static IProxyGeometry readPolyline(StreamIO stream)
	{
		ProxyPolyline polyline = new ProxyPolyline();
		int pointCount = stream.ReadInt();
		for (int i = 0; i < pointCount; i++)
		{
			polyline.Points.Add(readPoint(stream));
		}
		return polyline;
	}

	private static IProxyGeometry readPolylineWithNormal(StreamIO stream)
	{
		ProxyPolylineWithNormal polyline = new ProxyPolylineWithNormal();

		int pointCount = stream.ReadInt();
		for (int i = 0; i < pointCount; i++)
		{
			polyline.Points.Add(readPoint(stream));
		}
		polyline.Normal = readPoint(stream);

		return polyline;
	}

	private static IProxyGeometry readPopClip(StreamIO stream)
	{
		return new ProxyPopClip();
	}

	private static IProxyGeometry readPopModelTransform(StreamIO stream)
	{
		return new ProxyPopModelTransform();
	}

	private static IProxyGeometry readPushClip(StreamIO stream)
	{
		ProxyPushClip clip = new ProxyPushClip();

		clip.Extrusion = readPoint(stream);
		clip.ClipBoundaryOrigin = readPoint(stream);
		clip.PointCount = stream.ReadInt();
		clip.Points = [];
		for (int i = 0; i < clip.PointCount; i++)
		{
			clip.Points.Add(new XY(stream.ReadDouble(), stream.ReadDouble()));
		}
		clip.ClipBoundaryTransformMatrix = readMatrix(stream);
		clip.InverseBlockTransformMatrix = readMatrix(stream);
		clip.FrontClipOn = stream.ReadInt();
		clip.BackClipOn = stream.ReadInt();
		clip.FrontClip = stream.ReadDouble();
		clip.BackClip = stream.ReadDouble();
		clip.DrawBoundary = stream.ReadInt() == 1;

		Debugger.Break();    // TODO: VALIDATE
		return clip;
	}

	private static IProxyGeometry readPushModelTransform(StreamIO stream)
	{
		ProxyPushModelTransform modelTransform = new ProxyPushModelTransform();

		modelTransform.TransformationMatrix = readMatrix(stream);
		return modelTransform;
	}

	private static IProxyGeometry readPushModelTransform2(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readRay(StreamIO stream)
	{
		ProxyRay ray = new ProxyRay();

		ray.ConstructionLinePoint = readPoint(stream);
		ray.Point2 = readPoint(stream);

		Debugger.Break();    // TODO: VALIDATE
		return ray;
	}

	private static IProxyGeometry readShell(StreamIO stream)
	{
		ProxyShell shell = new ProxyShell();

		Func<int, bool> adHasPrimTraits = a => (a & 0xFFFFL) != 0;
		Func<int, bool> adPrimsHaveColors = a => (a & 0x0001L) != 0;
		Func<int, bool> adPrimsHaveLayers = a => (a & 0x0002L) != 0;
		Func<int, bool> adPrimsHaveLinetypes = a => (a & 0x0004L) != 0;
		Func<int, bool> adPrimsHaveMarkers = a => (a & 0x0020L) != 0;
		Func<int, bool> adPrimsHaveVisibilities = a => (a & 0x0040L) != 0;
		Func<int, bool> adPrimsHaveNormals = a => (a & 0x0080L) != 0;
		Func<int, bool> adPrimsHaveOrientation = a => (a & 0x0400L) != 0;

		shell.PointCount = stream.ReadInt();
		shell.Vertices = [];
		for (int i = 0; i < shell.PointCount; i++) {
			shell.Vertices.Add(readPoint(stream));
		}
		shell.FaceCount = stream.ReadInt();
		shell.Faces = [];
		List<int> face = [];
		for (int i = 0; i < shell.FaceCount; i++) {
			int value = stream.ReadInt();
			if (value < 0) {
				int vertexCount = value * -1;
				shell.Faces.Add(face);
				face = [];
				for (int j = 0; j < vertexCount; j++) {
					face.Add(stream.ReadInt());
				}
				shell.Faces.Add(face);
				face = [];
			}
			else {
				// TODO: Check if this ever happens and if so, if this is correct
				face.Add(value);
			}
		}
		if (face.Count > 0) {
			shell.Faces.Add(face);
		}

		// TODO: Validate and fix the below logic

		int edgePrimitiveFlags = stream.ReadInt();
		if (adHasPrimTraits(edgePrimitiveFlags)) {
			// TODO: Actually get edge count
			if (adPrimsHaveColors(edgePrimitiveFlags)) {
				int edgeColor = stream.ReadInt();		// color for each edge
			}
			if (adPrimsHaveLayers(edgePrimitiveFlags)) {
				int layerIds = stream.ReadInt();		// layer ids, 1 for each edge
			}
			if (adPrimsHaveLinetypes(edgePrimitiveFlags)) {
				int linetypeIds = stream.ReadInt();		// linetype ids, 1 for each edge
			}
			if (adPrimsHaveMarkers(edgePrimitiveFlags)) {
				int markerIndices = stream.ReadInt();	// marker indices, 1 for each edge
			}
			if (adPrimsHaveVisibilities(edgePrimitiveFlags)) {
				for (int i = 0; i < shell.Faces.SelectMany(x => x).Where(x => x >= 0 && x < shell.Vertices.Count).Distinct().Count(); i++) {
					int visibilityIndicator = stream.ReadInt();		// visibility indicator, 1 for each edge
				}
			}
		}

		int facePrimitiveFlags = stream.ReadInt();
		if (adHasPrimTraits(facePrimitiveFlags)) {
			if (adPrimsHaveColors(facePrimitiveFlags)) {
				int edgeColor = stream.ReadInt();		// color for each face
			}
			if (adPrimsHaveLayers(facePrimitiveFlags)) {
				int layerIds = stream.ReadInt();        // layer ids, 1 for each face
			}
			if (adPrimsHaveLinetypes(facePrimitiveFlags)) {
				int linetypeIds = stream.ReadInt();     // linetype ids, 1 for each face
			}
			if (adPrimsHaveMarkers(facePrimitiveFlags)) {
				int markerIndices = stream.ReadInt();   // marker indices, 1 for each face
			}
			if (adPrimsHaveVisibilities(facePrimitiveFlags)) {
				int visibilityIndicator = stream.ReadInt();     // visibility indicator, 1 for each face
			}
		}

		int vertexPrimitiveFlags = stream.ReadInt();
		if (adHasPrimTraits(vertexPrimitiveFlags)) {
			int vertexCount = /* TODO: rows * cols */ 0;
			if (adPrimsHaveNormals(vertexPrimitiveFlags)) {
				XYZ normal = readPoint(stream);			// normal, 1 for each vertex
			}
			if (adPrimsHaveOrientation(vertexPrimitiveFlags)) {
				int orientation = stream.ReadInt();		// orientation indicator, 1 ONLY
			}
		}

		return shell;
	}

	private static IProxyGeometry readSubentColor(StreamIO stream)
	{
		ProxySubentColor color = new ProxySubentColor();

		color.ColorIndex = stream.ReadInt();

		if (color.ColorIndex > 256) Debugger.Break();	// seen 257

		return color;
	}

	private static IProxyGeometry readSubentFillon(StreamIO stream)
	{
		ProxySubentFillon fillOn = new ProxySubentFillon();

		int fillValue = stream.ReadInt();
		fillOn.IsOn = fillValue == 1;	// Also seen value 2 as well

		return fillOn;
	}

	private static IProxyGeometry readSubEntityMapper(StreamIO stream)
	{
		ProxySubEntityMapper mapper = new ProxySubEntityMapper();

		mapper.DummyValue1 = stream.ReadInt();
		mapper.DummyValue2 = stream.ReadInt();
		mapper.Projection = stream.ReadInt();
		mapper.UTiling = stream.ReadInt();
		mapper.VTiling = stream.ReadInt();
		mapper.AutoTransform = stream.ReadInt();
		mapper.DummyValue3 = stream.ReadInt();

		Debugger.Break();    // TODO: VALIDATE
		return mapper;
	}

	private static IProxyGeometry readSubEntityMaterial(StreamIO stream)
	{
		ProxySubEntityMaterial material = new ProxySubEntityMaterial();

		//material.MaterialHandle = new DwgStreamReaderAC24(stream.Stream, false).HandleReference();
		material.MaterialHandle = (ulong) stream.ReadInt();
		int unknown1 = stream.ReadInt();

		return material;
	}

	private static IProxyGeometry readSubentLayer(StreamIO stream)
	{
		ProxySubentLayer layer = new ProxySubentLayer();

		layer.LayerIndex = stream.ReadInt();

		return layer;
	}

	private static IProxyGeometry readSubentLineType(StreamIO stream)
	{
		ProxySubentLineType lineType = new ProxySubentLineType();

		lineType.LineTypeIndex = stream.ReadUInt();

		return lineType;
	}

	private static IProxyGeometry readSubentLineTypeScale(StreamIO stream)
	{
		ProxySubentLineTypeScale scale = new ProxySubentLineTypeScale();

		scale.LineTypeScale = stream.ReadDouble();

		return scale;
	}

	private static IProxyGeometry readSubentLineWeight(StreamIO stream)
	{
		ProxySubentLineWeight lineWeight= new ProxySubentLineWeight();

		lineWeight.LineWeight = (LineWeightType) stream.ReadInt();

		return lineWeight;
	}

	private static IProxyGeometry readSubentMarker(StreamIO stream)
	{
		ProxySubentMarker marker = new ProxySubentMarker();

		marker.MarkerIndex = stream.ReadInt();

		return marker;
	}

	private static IProxyGeometry readSubentPlotStyleName(StreamIO stream)
	{
		ProxySubentPlotStyleName psName = new ProxySubentPlotStyleName();

		psName.Type = (ProxyPlotStyleType) stream.ReadInt();
		psName.PlotStyleIndex = stream.ReadInt();

		return psName;
	}

	private static IProxyGeometry readSubentThickness(StreamIO stream)
	{
		ProxySubentThickness thickness = new ProxySubentThickness();

		thickness.Thickness = stream.ReadDouble();

		return thickness;
	}

	private static IProxyGeometry readSubentTrueColor(StreamIO stream)
	{
		ProxySubentTrueColor trueColor = new ProxySubentTrueColor();

		// The first three bytes are the value and the fourth byte indicates how
		// those 3 previous bytes should be interpreted (see https://help.autodesk.com/view/OARX/2025/ENU/?guid=OARX-RefGuide-AcGiSubEntityTraits__setTrueColor_AcCmEntityColor_)

		byte b1 = stream.ReadByte();
		byte b2 = stream.ReadByte();
		byte b3 = stream.ReadByte();
		ProxyColorMethod method = (ProxyColorMethod) stream.ReadByte();

		switch (method) 
		{
			case ProxyColorMethod.ByLayer:
				trueColor.Color = Color.ByLayer;
				break;
			case ProxyColorMethod.ByBlock:
				trueColor.Color = Color.ByBlock;
				break;
			case ProxyColorMethod.ByColor:
				trueColor.Color = new Color(b1, b2, b3);
				break;
			case ProxyColorMethod.ByACI:
				trueColor.Color = new Color(b1);
				break;
			case ProxyColorMethod.Foreground:
				trueColor.Color = new Color(7);	// TODO: Check if this is what foreground actually means
				break;
			case ProxyColorMethod.None:
				break;
			default:
				break;
		}

		return trueColor;
	}

	private static IProxyGeometry readText(StreamIO stream)
	{
		ProxyText text = new ProxyText();

		text.StartPoint = readPoint(stream);
		text.Normal = readPoint(stream);
		text.TextDirection = readPoint(stream);
		text.Height = stream.ReadDouble();
		text.WidthFactor = stream.ReadDouble();
		text.ObliqueAngle = stream.ReadDouble();
		text.Text = readPaddedString(stream);

		// Padding to align the text to the next 4-byte boundary
		if ((text.Text.Length + 1) % 4 != 0)
		{
			stream.ReadBytes(4 - (text.Text.Length + 1) % 4);
		}

		return text;
	}

	private static IProxyGeometry readText2(StreamIO stream)
	{
		ProxyText2 text = new ProxyText2();
		text.StartPoint = readPoint(stream);
		text.Normal = readPoint(stream);
		text.TextDirection = readPoint(stream);
		text.Text = readPaddedString(stream);

		// Padding to align the text to the next 4-byte boundary
		if ((text.Text.Length + 1) % 4 != 0)
		{
			stream.ReadBytes(4 - (text.Text.Length + 1) % 4);
		}

		text.TextLength = stream.ReadInt();
		text.IsRaw = stream.ReadInt() == 0;
		text.Height = stream.ReadDouble();
		text.WidthFactor = stream.ReadDouble();
		text.ObliqueAngle = stream.ReadDouble();
		text.TrackingPercentage = stream.ReadDouble();

		text.IsBackwards = stream.ReadInt() == 1;
		text.IsUpsideDown = stream.ReadInt() == 1;
		text.IsVertical = stream.ReadInt() == 1;
		text.IsUnderlined = stream.ReadInt() == 1;
		text.IsOverlined = stream.ReadInt() == 1;

		text.FontFilename = readPaddedString(stream);

		// Padding to align the text to the next 4-byte boundary
		if ((text.FontFilename.Length + 1) % 4 != 0)
		{
			stream.ReadBytes(4 - (text.FontFilename.Length + 1) % 4);
		}

		text.BigFontFilename = readPaddedString(stream);

		// Padding to align the text to the next 4-byte boundary
		if ((text.BigFontFilename.Length + 1) % 4 != 0)
		{
			stream.ReadBytes(4 - (text.BigFontFilename.Length + 1) % 4);
		}

		return text;
	}

	private static IProxyGeometry readUnicodeText(StreamIO stream)
	{
		ProxyUnicodeText text = new ProxyUnicodeText();

		text.StartPoint = readPoint(stream);
		text.Normal = readPoint(stream);
		text.TextDirection = readPoint(stream);
		text.Height = stream.ReadDouble();
		text.WidthFactor = stream.ReadDouble();
		text.ObliqueAngle = stream.ReadDouble();
		text.Text = readPaddedUnicodeString(stream);

		// Padding to align the text to the next 4-byte boundary
		if (text.Text.Length % 2 == 0)
		{
			stream.ReadShort();
		}

		return text;
	}

	private static IProxyGeometry readUnicodeText2(StreamIO stream)
	{
		ProxyUnicodeText2 text = new ProxyUnicodeText2();
		text.StartPoint = readPoint(stream);
		text.Normal = readPoint(stream);
		text.TextDirection = readPoint(stream);
		text.Text = readPaddedUnicodeString(stream);

		// Padding to align the text to the next 4-byte boundary
		if (text.Text.Length % 2 == 0)
		{
			stream.ReadShort();
		}

		text.TextLength = stream.ReadInt();
		text.IsRaw = stream.ReadInt() == 0;
		text.Height = stream.ReadDouble();
		text.WidthFactor = stream.ReadDouble();
		text.ObliqueAngle = stream.ReadDouble();
		text.TrackingPercentage = stream.ReadDouble();

		text.IsBackwards = stream.ReadInt() == 1;
		text.IsUpsideDown = stream.ReadInt() == 1;
		text.IsVertical = stream.ReadInt() == 1;
		text.IsUnderlined = stream.ReadInt() == 1;
		text.IsOverlined = stream.ReadInt() == 1;

		TrueTypeFontDescriptor fontDescriptor = new TrueTypeFontDescriptor();
		fontDescriptor.IsBold = stream.ReadInt() == 1;
		fontDescriptor.IsItalic = stream.ReadInt() == 1;
		fontDescriptor.Charset = (byte)stream.ReadInt();
		fontDescriptor.PitchAndFamily = (byte)stream.ReadInt();
		fontDescriptor.Typeface = readPaddedUnicodeString(stream);

		// Padding to align the text to the next 4-byte boundary
		if (fontDescriptor.Typeface.Length % 2 == 0)
		{
			stream.ReadShort();
		}

		fontDescriptor.FontFilename = readPaddedUnicodeString(stream);

		// Padding to align the text to the next 4-byte boundary
		if (fontDescriptor.FontFilename.Length % 2 == 0)
		{
			stream.ReadShort();
		}

		text.FontDescriptor = fontDescriptor;
		text.BigFontFilename = readPaddedUnicodeString(stream);

		// Padding to align the text to the next 4-byte boundary
		if (fontDescriptor.FontFilename.Length % 2 == 0)
		{
			stream.ReadShort();
		}

		return text;
	}

	private static IProxyGeometry readUnknown37(StreamIO stream)
	{
		Debugger.Break();    // TODO: VALIDATE
		return new ProxyUnknown37();
	}

	private static IProxyGeometry readXLine(StreamIO stream)
	{
		ProxyXLine xline = new ProxyXLine();

		xline.Point1 = readPoint(stream);
		xline.Point2 = readPoint(stream);

		return xline;
	}

	private static XYZ readPoint(StreamIO stream)
	{
		var x = stream.ReadDouble();
		var y = stream.ReadDouble();
		var z = stream.ReadDouble();
		return new XYZ(x, y, z);
	}

	private static Matrix4 readMatrix(StreamIO stream)
	{
		Matrix4 matrix = Matrix4.Identity;
		matrix.M00 = stream.ReadDouble();
		matrix.M01 = stream.ReadDouble();
		matrix.M02 = stream.ReadDouble();
		matrix.M03 = stream.ReadDouble();
		matrix.M10 = stream.ReadDouble();
		matrix.M11 = stream.ReadDouble();
		matrix.M12 = stream.ReadDouble();
		matrix.M13 = stream.ReadDouble();
		matrix.M20 = stream.ReadDouble();
		matrix.M21 = stream.ReadDouble();
		matrix.M22 = stream.ReadDouble();
		matrix.M23 = stream.ReadDouble();
		matrix.M30 = stream.ReadDouble();
		matrix.M31 = stream.ReadDouble();
		matrix.M32 = stream.ReadDouble();
		matrix.M33 = stream.ReadDouble();
		return matrix;
	}

	/// <summary>
	/// PUS : Padded Unicode string, The bytes are encoded using Unicode encoding. 
	/// The bytes consist of byte pairs and the string is terminated by 2 zero bytes.
	/// </summary>
	/// <returns></returns>
	private static string readPaddedUnicodeString(StreamIO stream)
	{
		byte[] nullTerminator = new byte[] { 0, 0 };

		List<byte> stringBytes = new List<byte>();
		byte[] character;
		while (!(character = stream.ReadBytes(2)).AsSpan().SequenceEqual(nullTerminator))
		{
			stringBytes.AddRange(character);
		}
		return Encoding.Unicode.GetString(stringBytes.ToArray());
	}

	/// <summary>
	/// PS : Padded string. This is a string, terminated with a zero byte.
	/// The file’s text encoding (code page) is used to encode / decode 
	/// the bytes into a string
	/// </summary>
	/// <returns></returns>
	private static string readPaddedString(StreamIO stream)
	{
		List<byte> stringBytes = new List<byte>();
		byte character;
		while ((character = stream.ReadByte()) != 0)
		{
			stringBytes.Add(character);
		}
		return stream.Encoding.GetString(stringBytes.ToArray());
	}

	public enum ProxyColorMethod : byte
	{
		ByLayer = 0xC0,
		ByBlock = 0xC1,
		ByColor = 0xC2,
		ByACI = 0xC3,
		Foreground = 0xC5,
		None = 0xC8
	}
}
