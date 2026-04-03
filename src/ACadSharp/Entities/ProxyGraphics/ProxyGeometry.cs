using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using CSMath;
using System.Collections.Generic;

namespace ACadSharp.Entities.ProxyGraphics;

public class ProxyGeometry
{
	public static IEnumerable<IProxyGeometry> ReadGeometries(byte[] arr)
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
					geometries.Add(readPophModelTransform(stream));
					break;
				case GraphicsType.PolylineWithNormal:
					geometries.Add(readPolylineWithNormal(stream));
					break;
				case GraphicsType.LwPolyine:
					geometries.Add(readLwPolyine(stream));
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
					break;
			}

			if (stream.Position == pos)
			{
				//jump not implemented proxies
				stream.ReadBytes(objSize - 8);
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
		throw new NotImplementedException();
	}

	private static IProxyGeometry readCircularArc(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readCircularArc3Pt(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readExtents(StreamIO stream)
	{
		ProxyExtents extents = new ProxyExtents();

		extents.Min = readPoint(stream);
		extents.Max = readPoint(stream);

		return extents;
	}

	private static IProxyGeometry readLwPolyine(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readMesh(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static XYZ readPoint(StreamIO stream)
	{
		var x = stream.ReadDouble();
		var y = stream.ReadDouble();
		var z = stream.ReadDouble();
		return new XYZ(x, y, z);
	}

	private static IProxyGeometry readPolygon(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPolyline(StreamIO stream)
	{
		ProxyPolyline pline = new ProxyPolyline();
		var pointCount = stream.ReadInt();
		for (int j = 0; j < pointCount; j++)
		{
			pline.Points.Add(readPoint(stream));
		}
		return pline;
	}

	private static IProxyGeometry readPolylineWithNormal(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPopClip(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPophModelTransform(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPushClip(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPushModelTransform(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readPushModelTransform2(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readRay(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readShell(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentColor(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentFillon(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubEntityMapper(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubEntityMaterial(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentLayer(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentLineType(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentLineTypeScale(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentLineWeight(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentMarker(StreamIO stream)
	{
		stream.ReadInt();
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentPlotStyleName(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentThickness(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readSubentTrueColor(StreamIO stream)
	{
		stream.ReadByte();
		stream.ReadByte();
		stream.ReadByte();
		//Missing alpha
		stream.ReadByte();

		throw new NotImplementedException();
	}

	private static IProxyGeometry readText(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readText2(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readUnicodeText(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readUnicodeText2(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readUnknown37(StreamIO stream)
	{
		throw new NotImplementedException();
	}

	private static IProxyGeometry readXLine(StreamIO stream)
	{
		throw new NotImplementedException();
	}
}
