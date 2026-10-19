using ACadSharp.IO.DWG;
using CSMath;
using CSUtilities.Converters;
using CSUtilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ACadSharp.Entities.ProxyGraphics;

internal class ProxyGeometryWriter
{
	private readonly Encoding _encoding;

	private readonly ACadVersion _version;

	private StreamIO _stream;

	private ProxyGeometryWriter(ACadVersion version, Encoding encoding)
	{
		this._version = version;
		this._encoding = encoding;
	}

	public static byte[] WriteGeometries(ACadVersion version, Encoding encoding, IEnumerable<IProxyGeometry> geometries)
	{
		ProxyGeometryWriter writer = new ProxyGeometryWriter(version, encoding);

		List<byte[]> chunks = new();
		foreach (IProxyGeometry geometry in geometries)
		{
			byte[] data = writer.writeGeometry(geometry);
			if (data != null)
			{
				chunks.Add(data);
			}
		}

		int size = 8 + chunks.Sum(c => c.Length + 8);

		MemoryStream ms = new MemoryStream();
		StreamIO stream = new StreamIO(ms);
		stream.Write<int, LittleEndianConverter>(size);
		stream.Write<int, LittleEndianConverter>(chunks.Count);

		for (int i = 0; i < chunks.Count; i++)
		{
			byte[] chunk = chunks[i];
			//Size includes the size and type
			stream.Write<int, LittleEndianConverter>(chunk.Length + 4);
			stream.WriteBytes(chunk);
		}

		return ms.ToArray();
	}

	private void writeCircle(ProxyCircle circle)
	{
		this.writePoint(circle.Center);
		this._stream.Write<double, LittleEndianConverter>(circle.Radius);
		this.writePoint(circle.Normal);
	}

	private void writeCirclePt3(ProxyCirclePt3 circlePt3)
	{
		this.writePoint(circlePt3.Point1);
		this.writePoint(circlePt3.Point2);
		this.writePoint(circlePt3.Point3);
	}

	private void writeCircularArc(ProxyCircularArc arc)
	{
		this.writePoint(arc.Center);
		this._stream.Write<double, LittleEndianConverter>(arc.Radius);
		this.writePoint(arc.Normal);
		this.writePoint(arc.StartVectorDirection);
		this._stream.Write<double, LittleEndianConverter>(arc.SweepAngle);
		this._stream.Write<int, LittleEndianConverter>(arc.ArcType);
	}

	private void writeCircularArc3Pt(ProxyCircularArc3Pt arc3Pt)
	{
		this.writePoint(arc3Pt.Point1);
		this.writePoint(arc3Pt.Point2);
		this.writePoint(arc3Pt.Point3);
		this._stream.Write<int, LittleEndianConverter>(arc3Pt.ArcType);
	}

	private void writeExtents(ProxyExtents extents)
	{
		this.writePoint(extents.Min);
		this.writePoint(extents.Max);
	}

	private byte[] writeGeometry(IProxyGeometry geometry)
	{
		MemoryStream ms = new MemoryStream();
		this._stream = new StreamIO(ms);

		this._stream.Write<int, LittleEndianConverter>((int)geometry.GraphicsType);

		switch (geometry)
		{
			case ProxyExtents extents:
				this.writeExtents(extents);
				break;
			case ProxyCircle circle:
				this.writeCircle(circle);
				break;
			case ProxyCirclePt3 circlePt3:
				this.writeCirclePt3(circlePt3);
				break;
			case ProxyCircularArc arc:
				this.writeCircularArc(arc);
				break;
			case ProxyCircularArc3Pt arc3Pt:
				this.writeCircularArc3Pt(arc3Pt);
				break;
			case ProxyPolylineWithNormal polylineWithNormal:
				this.writePolylineWithNormal(polylineWithNormal);
				break;
			case ProxyPolyline polyline:
				this.writePolyline(polyline);
				break;
			case ProxyPolygon polygon:
				this.writePolygon(polygon);
				break;
			case ProxyMesh mesh:
				this.writeMesh(mesh);
				break;
			case ProxyShell shell:
				this.writeShell(shell);
				break;
			case ProxyText text:
				this.writeText(text);
				break;
			case ProxyText2 text2:
				this.writeText2(text2);
				break;
			case ProxyXLine xline:
				this.writeXLine(xline);
				break;
			case ProxyRay ray:
				this.writeRay(ray);
				break;
			case ProxySubentColor color:
				this.writeSubentColor(color);
				break;
			case ProxySubentLayer layer:
				this.writeSubentLayer(layer);
				break;
			case ProxySubentLineType lineType:
				this.writeSubentLineType(lineType);
				break;
			case ProxySubentMarker marker:
				this.writeSubentMarker(marker);
				break;
			case ProxySubentFillon fillOn:
				this.writeSubentFillon(fillOn);
				break;
			case ProxySubentTrueColor trueColor:
				this.writeSubentTrueColor(trueColor);
				break;
			case ProxySubentLineWeight lineWeight:
				this.writeSubentLineWeight(lineWeight);
				break;
			case ProxySubentLineTypeScale scale:
				this.writeSubentLineTypeScale(scale);
				break;
			case ProxySubentThickness thickness:
				this.writeSubentThickness(thickness);
				break;
			case ProxySubentPlotStyleName psName:
				this.writeSubentPlotStyleName(psName);
				break;
			case ProxyPushClip clip:
				this.writePushClip(clip);
				break;
			case ProxyPopClip:
			case ProxyPopModelTransform:
			case ProxyUnknown37:
				break;
			case ProxyPushModelTransform transform:
				this.writePushModelTransform(transform);
				break;
			case ProxyPushModelTransform2 transform2:
				this.writePushModelTransform2(transform2);
				break;
			case ProxyLwPolyine lwPolyline:
				this.writeLwPolyline(lwPolyline);
				break;
			case ProxySubEntityMaterial material:
				this.writeSubEntityMaterial(material);
				break;
			case ProxySubEntityMapper mapper:
				this.writeSubEntityMapper(mapper);
				break;
			case ProxyUnicodeText unicodeText:
				this.writeUnicodeText(unicodeText);
				break;
			case ProxyUnicodeText2 unicodeText2:
				this.writeUnicodeText2(unicodeText2);
				break;
			default:
				return null;
		}

		return ms.ToArray();
	}

	private void writeLwPolyline(ProxyLwPolyine proxy)
	{
		LwPolyline pline = proxy.Entity ?? new LwPolyline();

		MemoryStream ms = new MemoryStream();
		IDwgStreamWriter writer = DwgStreamWriterBase.GetStreamWriter(this._version, ms, this._encoding);

		// The following writes the LwPolyline entity like the DwgObjectWriter
		bool hasBulges = pline.Vertices.Any(v => v.Bulge != 0.0);
		bool hasIds = pline.Vertices.Any(v => v.Id != 0);
		bool hasWidths = pline.Vertices.Any(v => v.StartWidth != 0.0 || v.EndWidth != 0.0);

		short flags = 0;
		if (pline.Flags.HasFlag(LwPolylineFlags.Plinegen))
			flags |= 0x100;
		if (pline.Flags.HasFlag(LwPolylineFlags.Closed))
			flags |= 0x200;
		if (pline.ConstantWidth != 0.0)
			flags |= 0x4;
		if (pline.Elevation != 0.0)
			flags |= 0x8;
		if (pline.Thickness != 0.0)
			flags |= 0x2;
		if (pline.Normal != XYZ.AxisZ)
			flags |= 0x1;
		if (hasBulges)
			flags |= 0x10;
		if (hasIds)
			flags |= 0x400;
		if (hasWidths)
			flags |= 0x20;

		writer.WriteBitShort(flags);

		if ((flags & 0x4) != 0)
			writer.WriteBitDouble(pline.ConstantWidth);
		if ((flags & 0x8) != 0)
			writer.WriteBitDouble(pline.Elevation);
		if ((flags & 0x2) != 0)
			writer.WriteBitDouble(pline.Thickness);
		if ((flags & 0x1) != 0)
			writer.Write3BitDouble(pline.Normal);

		writer.WriteBitLong(pline.Vertices.Count);

		if (hasBulges)
			writer.WriteBitLong(pline.Vertices.Count);
		if (hasIds)
			writer.WriteBitLong(pline.Vertices.Count);
		if (hasWidths)
			writer.WriteBitLong(pline.Vertices.Count);

		if (this._version == ACadVersion.AC1014 || this._version == ACadVersion.AC1012)
		{
			foreach (LwPolyline.Vertex v in pline.Vertices)
			{
				writer.Write2RawDouble(v.Location);
			}
		}

		if (this._version >= ACadVersion.AC1015 && pline.Vertices.Count > 0)
		{
			writer.Write2RawDouble(pline.Vertices[0].Location);
			for (int i = 1; i < pline.Vertices.Count; i++)
			{
				writer.Write2BitDoubleWithDefault(pline.Vertices[i - 1].Location, pline.Vertices[i].Location);
			}
		}

		if (hasBulges)
		{
			foreach (LwPolyline.Vertex v in pline.Vertices)
				writer.WriteBitDouble(v.Bulge);
		}

		if (hasIds)
		{
			foreach (LwPolyline.Vertex v in pline.Vertices)
				writer.WriteBitLong(v.Id);
		}

		if (hasWidths)
		{
			foreach (LwPolyline.Vertex v in pline.Vertices)
			{
				writer.WriteBitDouble(v.StartWidth);
				writer.WriteBitDouble(v.EndWidth);
			}
		}

		writer.WriteSpearShift();

		byte[] data = ms.ToArray();
		this._stream.Write<int, LittleEndianConverter>(data.Length);
		this._stream.WriteBytes(data);

		// Additional unknown data after regular LwPolyline Entity data
		this._stream.WriteBytes(new byte[] { proxy.Unknown1, proxy.Unknown2, proxy.Unknown3 });
	}

	private void writeMatrix(Matrix4 matrix)
	{
		this._stream.Write<double, LittleEndianConverter>(matrix.M00);
		this._stream.Write<double, LittleEndianConverter>(matrix.M01);
		this._stream.Write<double, LittleEndianConverter>(matrix.M02);
		this._stream.Write<double, LittleEndianConverter>(matrix.M03);
		this._stream.Write<double, LittleEndianConverter>(matrix.M10);
		this._stream.Write<double, LittleEndianConverter>(matrix.M11);
		this._stream.Write<double, LittleEndianConverter>(matrix.M12);
		this._stream.Write<double, LittleEndianConverter>(matrix.M13);
		this._stream.Write<double, LittleEndianConverter>(matrix.M20);
		this._stream.Write<double, LittleEndianConverter>(matrix.M21);
		this._stream.Write<double, LittleEndianConverter>(matrix.M22);
		this._stream.Write<double, LittleEndianConverter>(matrix.M23);
		this._stream.Write<double, LittleEndianConverter>(matrix.M30);
		this._stream.Write<double, LittleEndianConverter>(matrix.M31);
		this._stream.Write<double, LittleEndianConverter>(matrix.M32);
		this._stream.Write<double, LittleEndianConverter>(matrix.M33);
	}

	private void writeMesh(ProxyMesh mesh)
	{
		this._stream.Write<int, LittleEndianConverter>(mesh.RowCount);
		this._stream.Write<int, LittleEndianConverter>(mesh.ColumnCount);

		int vertexCount = mesh.RowCount * mesh.ColumnCount;
		this.writeValues(mesh.Vertices, vertexCount, this.writePoint);

		int edgeCount = (mesh.RowCount - 1) * mesh.ColumnCount + (mesh.ColumnCount - 1) * mesh.RowCount;
		int faceCount = (mesh.RowCount - 1) * (mesh.ColumnCount - 1);

		this.writeTraits(mesh.EdgeTraits, edgeCount, true);
		this.writeTraits(mesh.FaceTraits, faceCount, false);
		this.writeVertexTraits(mesh.VertexTraits, vertexCount);
	}

	private void writePaddedString(string value)
	{
		// PS : Padded string, terminated with a zero byte and aligned to the next 4-byte boundary
		byte[] bytes = this._encoding.GetBytes(value ?? string.Empty);
		this._stream.WriteBytes(bytes);
		this._stream.WriteBytes(new byte[] { 0 });

		int length = bytes.Length + 1;
		if (length % 4 != 0)
		{
			this._stream.WriteBytes(new byte[4 - length % 4]);
		}
	}

	private void writePaddedUnicodeString(string value)
	{
		// PUS : Padded Unicode string, terminated by 2 zero bytes and aligned to the next 4-byte boundary
		value ??= string.Empty;
		this._stream.WriteBytes(Encoding.Unicode.GetBytes(value));
		this._stream.WriteBytes(new byte[] { 0, 0 });

		if (value.Length % 2 == 0)
		{
			this._stream.WriteBytes(new byte[] { 0, 0 });
		}
	}

	private void writePoint(XYZ point)
	{
		this._stream.Write<double, LittleEndianConverter>(point.X);
		this._stream.Write<double, LittleEndianConverter>(point.Y);
		this._stream.Write<double, LittleEndianConverter>(point.Z);
	}

	private void writePoints(List<XYZ> points)
	{
		points ??= new List<XYZ>();
		this._stream.Write<int, LittleEndianConverter>(points.Count);
		foreach (XYZ point in points)
		{
			this.writePoint(point);
		}
	}

	private void writePolygon(ProxyPolygon polygon)
	{
		this.writePoints(polygon.Points);
	}

	private void writePolyline(ProxyPolyline polyline)
	{
		this.writePoints(polyline.Points);
	}

	private void writePolylineWithNormal(ProxyPolylineWithNormal polylineWithNormal)
	{
		this.writePoints(polylineWithNormal.Points);
		this.writePoint(polylineWithNormal.Normal);
	}

	private void writePushClip(ProxyPushClip clip)
	{
		this.writePoint(clip.Extrusion);
		this.writePoint(clip.ClipBoundaryOrigin);

		List<XY> boundary = clip.ClipBoundary ?? new List<XY>();
		this._stream.Write<int, LittleEndianConverter>(boundary.Count);
		foreach (XY point in boundary)
		{
			this._stream.Write<double, LittleEndianConverter>(point.X);
			this._stream.Write<double, LittleEndianConverter>(point.Y);
		}

		this.writeMatrix(clip.ClipBoundaryTransformMatrix);
		this.writeMatrix(clip.InverseBlockTransformMatrix);

		this._stream.Write<int, LittleEndianConverter>(clip.FrontClipOn ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(clip.BackClipOn ? 1 : 0);
		this._stream.Write<double, LittleEndianConverter>(clip.FrontClip);
		this._stream.Write<double, LittleEndianConverter>(clip.BackClip);
		this._stream.Write<int, LittleEndianConverter>(clip.DrawBoundary ? 1 : 0);
	}

	private void writePushModelTransform(ProxyPushModelTransform transform)
	{
		this.writeMatrix(transform.TransformationMatrix);
	}

	private void writePushModelTransform2(ProxyPushModelTransform2 transform2)
	{
		this.writeMatrix(transform2.TransformationMatrix);
	}

	private void writeRay(ProxyRay ray)
	{
		this.writePoint(ray.ConstructionLinePoint);
		this.writePoint(ray.Point2);
	}

	private void writeShell(ProxyShell shell)
	{
		this.writePoints(shell.Vertices);

		List<int[]> faces = shell.Faces ?? new List<int[]>();

		// Face list size, includes the count entry of each face
		this._stream.Write<int, LittleEndianConverter>(faces.Sum(f => f.Length + 1));

		int edgeCount = 0;
		foreach (int[] face in faces)
		{
			this._stream.Write<int, LittleEndianConverter>(face.Length);
			foreach (int index in face)
			{
				this._stream.Write<int, LittleEndianConverter>(index);
			}
			edgeCount += face.Length;
		}

		this.writeTraits(shell.EdgeTraits, edgeCount, true);
		this.writeTraits(shell.FaceTraits, faces.Count, false);
		this.writeVertexTraits(shell.VertexTraits, shell.Vertices?.Count ?? 0);
	}

	private void writeSubentColor(ProxySubentColor color)
	{
		this._stream.Write<int, LittleEndianConverter>(color.ColorIndex);
	}

	private void writeSubentFillon(ProxySubentFillon fillOn)
	{
		this._stream.Write<int, LittleEndianConverter>(fillOn.IsOn ? 1 : 0);
	}

	private void writeSubEntityMapper(ProxySubEntityMapper mapper)
	{
		this._stream.Write<int, LittleEndianConverter>(mapper.DummyValue1);
		this._stream.Write<int, LittleEndianConverter>(mapper.DummyValue2);
		this._stream.Write<int, LittleEndianConverter>(mapper.Projection);
		this._stream.Write<int, LittleEndianConverter>(mapper.UTiling);
		this._stream.Write<int, LittleEndianConverter>(mapper.VTiling);
		this._stream.Write<int, LittleEndianConverter>(mapper.AutoTransform);
		this._stream.Write<int, LittleEndianConverter>(mapper.DummyValue3);
	}

	private void writeSubEntityMaterial(ProxySubEntityMaterial material)
	{
		this._stream.Write<int, LittleEndianConverter>((int)material.MaterialHandle);
		this._stream.Write<int, LittleEndianConverter>(0);
	}

	private void writeSubentLayer(ProxySubentLayer layer)
	{
		this._stream.Write<int, LittleEndianConverter>(layer.LayerIndex);
	}

	private void writeSubentLineType(ProxySubentLineType lineType)
	{
		this._stream.Write<uint, LittleEndianConverter>(lineType.LineTypeIndex);
	}

	private void writeSubentLineTypeScale(ProxySubentLineTypeScale scale)
	{
		this._stream.Write<double, LittleEndianConverter>(scale.LineTypeScale);
	}

	private void writeSubentLineWeight(ProxySubentLineWeight lineWeight)
	{
		this._stream.Write<int, LittleEndianConverter>((int)lineWeight.LineWeight);
	}

	private void writeSubentMarker(ProxySubentMarker marker)
	{
		this._stream.Write<int, LittleEndianConverter>(marker.MarkerIndex);
	}

	private void writeSubentPlotStyleName(ProxySubentPlotStyleName psName)
	{
		this._stream.Write<int, LittleEndianConverter>((int)psName.Type);
		this._stream.Write<int, LittleEndianConverter>(psName.PlotStyleIndex);
	}

	private void writeSubentThickness(ProxySubentThickness thickness)
	{
		this._stream.Write<double, LittleEndianConverter>(thickness.Thickness);
	}

	private void writeSubentTrueColor(ProxySubentTrueColor trueColor)
	{
		byte b1 = 0, b2 = 0, b3 = 0;

		switch (trueColor.ColorMethod)
		{
			case ProxyColorMethod.ByColor:
				b1 = trueColor.Color.R;
				b2 = trueColor.Color.G;
				b3 = trueColor.Color.B;
				break;
			case ProxyColorMethod.ByACI:
				b1 = (byte)trueColor.Color.Index;
				break;
		}

		this._stream.WriteBytes(new byte[] { b1, b2, b3, (byte)trueColor.ColorMethod });
	}

	private void writeText(ProxyText text)
	{
		this.writePoint(text.StartPoint);
		this.writePoint(text.Normal);
		this.writePoint(text.TextDirection);
		this._stream.Write<double, LittleEndianConverter>(text.Height);
		this._stream.Write<double, LittleEndianConverter>(text.WidthFactor);
		this._stream.Write<double, LittleEndianConverter>(text.ObliqueAngle);
		this.writePaddedString(text.Text);
	}

	private void writeText2(ProxyText2 text)
	{
		this.writePoint(text.StartPoint);
		this.writePoint(text.Normal);
		this.writePoint(text.TextDirection);
		this.writePaddedString(text.Text);

		this.writeTextStyleFlags(text.TextLength, text.IsRaw, text.Height, text.WidthFactor, text.ObliqueAngle, text.TrackingPercentage,
			text.IsBackwards, text.IsUpsideDown, text.IsVertical, text.IsUnderlined, text.IsOverlined);

		this.writePaddedString(text.FontFilename);
		this.writePaddedString(text.BigFontFilename);
	}

	private void writeTextStyleFlags(int textLength, bool isRaw, double height, double widthFactor, double obliqueAngle, double tracking,
		bool isBackwards, bool isUpsideDown, bool isVertical, bool isUnderlined, bool isOverlined)
	{
		this._stream.Write<int, LittleEndianConverter>(textLength);
		this._stream.Write<int, LittleEndianConverter>(isRaw ? 0 : 1);
		this._stream.Write<double, LittleEndianConverter>(height);
		this._stream.Write<double, LittleEndianConverter>(widthFactor);
		this._stream.Write<double, LittleEndianConverter>(obliqueAngle);
		this._stream.Write<double, LittleEndianConverter>(tracking);
		this._stream.Write<int, LittleEndianConverter>(isBackwards ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(isUpsideDown ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(isVertical ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(isUnderlined ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(isOverlined ? 1 : 0);
	}

	private void writeTraits(PrimitiveTraits traits, int count, bool isEdge)
	{
		if (traits == null)
		{
			this._stream.Write<int, LittleEndianConverter>((int)PrimitiveFlags.None);
			return;
		}

		PrimitiveFlags flags = PrimitiveFlags.None;
		if (traits.Colors.Count > 0)
			flags |= PrimitiveFlags.HasColors;
		if (traits.LayerHandles.Count > 0)
			flags |= PrimitiveFlags.HasLayers;
		if (isEdge && ((EdgeTraits)traits).LineTypeHandles.Count > 0)
			flags |= PrimitiveFlags.HasLineTypes;
		if (traits.MakerIds.Count > 0)
			flags |= PrimitiveFlags.HasMarkers;
		if (!isEdge && ((FaceTraits)traits).Normals.Count > 0)
			flags |= PrimitiveFlags.HasNormals;
		if (traits.VisibilityIndicators.Count > 0)
			flags |= PrimitiveFlags.HasVisibilities;

		this._stream.Write<int, LittleEndianConverter>((int)flags);

		if (flags.HasFlag(PrimitiveFlags.HasColors))
		{
			this.writeValues(traits.Colors, count, v => this._stream.Write<int, LittleEndianConverter>(v));
		}
		if (flags.HasFlag(PrimitiveFlags.HasLayers))
		{
			this.writeValues(traits.LayerHandles, count, v => this._stream.Write<int, LittleEndianConverter>((int)v));
		}
		if (flags.HasFlag(PrimitiveFlags.HasLineTypes))
		{
			this.writeValues(((EdgeTraits)traits).LineTypeHandles, count, v => this._stream.Write<int, LittleEndianConverter>((int)v));
		}
		if (flags.HasFlag(PrimitiveFlags.HasMarkers))
		{
			this.writeValues(traits.MakerIds, count, v => this._stream.Write<int, LittleEndianConverter>(v));
		}
		if (flags.HasFlag(PrimitiveFlags.HasNormals))
		{
			this.writeValues(((FaceTraits)traits).Normals, count, this.writePoint);
		}
		if (flags.HasFlag(PrimitiveFlags.HasVisibilities))
		{
			this.writeValues(traits.VisibilityIndicators, count, v => this._stream.Write<int, LittleEndianConverter>(v));
		}
	}

	private void writeUnicodeText(ProxyUnicodeText unicodeText)
	{
		this.writePoint(unicodeText.StartPoint);
		this.writePoint(unicodeText.Normal);
		this.writePoint(unicodeText.TextDirection);
		this._stream.Write<double, LittleEndianConverter>(unicodeText.Height);
		this._stream.Write<double, LittleEndianConverter>(unicodeText.WidthFactor);
		this._stream.Write<double, LittleEndianConverter>(unicodeText.ObliqueAngle);
		this.writePaddedUnicodeString(unicodeText.Text);
	}

	private void writeUnicodeText2(ProxyUnicodeText2 text)
	{
		this.writePoint(text.StartPoint);
		this.writePoint(text.Normal);
		this.writePoint(text.TextDirection);
		this.writePaddedUnicodeString(text.Text);

		this.writeTextStyleFlags(text.TextLength, text.IsRaw, text.Height, text.WidthFactor, text.ObliqueAngle, text.TrackingPercentage,
			text.IsBackwards, text.IsUpsideDown, text.IsVertical, text.IsUnderlined, text.IsOverlined);

		TrueTypeFontDescriptor font = text.FontDescriptor ?? new TrueTypeFontDescriptor();
		this._stream.Write<int, LittleEndianConverter>(font.IsBold ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(font.IsItalic ? 1 : 0);
		this._stream.Write<int, LittleEndianConverter>(font.Charset);
		this._stream.Write<int, LittleEndianConverter>(font.PitchAndFamily);
		this.writePaddedUnicodeString(font.Typeface);
		this.writePaddedUnicodeString(font.FontFilename);
		this.writePaddedUnicodeString(text.BigFontFilename);
	}

	private void writeValues<T>(List<T> values, int count, Action<T> write)
	{
		for (int i = 0; i < count; i++)
		{
			write(i < values.Count ? values[i] : default);
		}
	}

	private void writeVertexTraits(VertexTraits traits, int count)
	{
		if (traits == null)
		{
			this._stream.Write<int, LittleEndianConverter>((int)PrimitiveFlags.None);
			return;
		}

		PrimitiveFlags flags = PrimitiveFlags.HasOrientation;
		if (traits.Normals != null && traits.Normals.Count > 0)
		{
			flags |= PrimitiveFlags.HasNormals;
		}

		this._stream.Write<int, LittleEndianConverter>((int)flags);

		if (flags.HasFlag(PrimitiveFlags.HasNormals))
		{
			this.writeValues(traits.Normals, count, this.writePoint);
		}

		this._stream.Write<int, LittleEndianConverter>(traits.Orientation);
	}

	private void writeXLine(ProxyXLine xline)
	{
		this.writePoint(xline.Point1);
		this.writePoint(xline.Point2);
	}
}