using CSUtilities.Converters;
using CSUtilities.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ACadSharp.Entities.ProxyGraphics;

public enum GraphicsType
{
	Unknown = 0,
	Extents = 1,
	Circle = 2,
	CirclePt3 = 3,
	CircularArc = 4,
	CircularArc3Pt = 5,
	Polyline = 6,
	Polygon = 7,
	Mesh = 8,
	Shell = 9,
	Text = 10,
	Text2 = 11,
	XLine = 12,
	Ray = 13,
	SubentColor = 14,
	SubentLayer = 16,
	SubentLineType = 18,
	SubentMarker = 19,
	SubentFillon = 20,
	SubentTrueColor = 22,
	SubentLineWeight = 23,
	SubentLineTypeScale = 24,
	SubentThickness = 25,
	SubentPlotStyleName = 26,
	PushClip = 27,
	PopClip = 28,
	PushModelTransform = 29,
	PushModelTransform2 = 30,
	PophModelTransform = 31,
	PolylineWithNormal = 32,
	LwPolyine = 33,
	SubEntityMaterial = 34,
	SubEntityMapper = 35,
	UnicodeText = 36,
	Unknown37 = 37,
	UnicodeText2 = 38,//No code specified
}

public interface IProxyGeometry
{
	public GraphicsType GraphicsType { get; }
}

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
				case GraphicsType.Unknown:
					break;
				case GraphicsType.Extents:
					stream.ReadDouble();
					stream.ReadDouble();
					stream.ReadDouble();

					stream.ReadDouble();
					stream.ReadDouble();
					stream.ReadDouble();
					break;
				case GraphicsType.Circle:
					break;
				case GraphicsType.CirclePt3:
					break;
				case GraphicsType.CircularArc:
					break;
				case GraphicsType.CircularArc3Pt:
					break;
				case GraphicsType.Polyline:
					var pointCount = stream.ReadInt();//4
					for (int j = 0; j < pointCount; j++)
					{
						//8
						stream.ReadDouble();
						stream.ReadDouble();
						stream.ReadDouble();
					}
					break;
				case GraphicsType.Polygon:
					break;
				case GraphicsType.Mesh:
					break;
				case GraphicsType.Shell:
					break;
				case GraphicsType.Text:
					break;
				case GraphicsType.Text2:
					break;
				case GraphicsType.XLine:
					break;
				case GraphicsType.Ray:
					break;
				case GraphicsType.SubentColor:
					break;
				case GraphicsType.SubentLayer:
					break;
				case GraphicsType.SubentLineType:
					break;
				case GraphicsType.SubentMarker:
					stream.ReadInt();
					break;
				case GraphicsType.SubentFillon:
					break;
				case GraphicsType.SubentTrueColor:
					stream.ReadByte();
					stream.ReadByte();
					stream.ReadByte();
					//Missing alpha
					stream.ReadByte();
					break;
				case GraphicsType.SubentLineWeight:
					break;
				case GraphicsType.SubentLineTypeScale:
					break;
				case GraphicsType.SubentThickness:
					break;
				case GraphicsType.SubentPlotStyleName:
					break;
				case GraphicsType.PushClip:
					break;
				case GraphicsType.PopClip:
					break;
				case GraphicsType.PushModelTransform:
					break;
				case GraphicsType.PushModelTransform2:
					break;
				case GraphicsType.PophModelTransform:
					break;
				case GraphicsType.PolylineWithNormal:
					break;
				case GraphicsType.LwPolyine:
					break;
				case GraphicsType.SubEntityMaterial:
					break;
				case GraphicsType.SubEntityMapper:
					break;
				case GraphicsType.UnicodeText:
					break;
				case GraphicsType.Unknown37:
					break;
				case GraphicsType.UnicodeText2:
					break;
			}

			if (stream.Position == pos)
			{
				//jump not implemented
				stream.ReadBytes(objSize - 8);
			}
		}

		return geometries;
	}
}

public class ProxyCircle
{

}