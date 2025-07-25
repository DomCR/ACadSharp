﻿using ACadSharp.Entities;
using ACadSharp.Objects;
using CSMath;
using System;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ACadSharp.IO.DXF
{
	internal abstract partial class DxfSectionWriterBase
	{
		protected void writeEntity<T>(T entity)
			where T : Entity
		{
			//TODO: Implement complex entities in a separated branch
			switch (entity)
			{
				case TableEntity:
				case Solid3D:
				case UnknownEntity:
					this.notify($"Entity type not implemented : {entity.GetType().FullName}", NotificationType.NotImplemented);
					return;
			}

			this._writer.Write(DxfCode.Start, entity.ObjectName);

			this.writeCommonObjectData(entity);

			this.writeCommonEntityData(entity);

			switch (entity)
			{
				case Arc arc:
					this.writeArc(arc);
					break;
				case Circle circle:
					this.writeCircle(circle);
					break;
				case Dimension dimension:
					this.writeDimension(dimension);
					break;
				case Ellipse ellipse:
					this.writeEllipse(ellipse);
					break;
				case Face3D face3D:
					this.writeFace3D(face3D);
					break;
				case Hatch hatch:
					this.writeHatch(hatch);
					break;
				case Insert insert:
					this.writeInsert(insert);
					break;
				case Leader leader:
					this.writeLeader(leader);
					break;
				case Line line:
					this.writeLine(line);
					break;
				case LwPolyline lwPolyline:
					this.writeLwPolyline(lwPolyline);
					break;
				case Mesh mesh:
					this.writeMesh(mesh);
					break;
				case MLine mline:
					this.writeMLine(mline);
					break;
				case MText mtext:
					this.writeMText(mtext);
					break;
				case MultiLeader multiLeader:
					this.writeMultiLeader(multiLeader);
					break;
				case PdfUnderlay pdfUnderlay:
					this.writePdfUnderlay<PdfUnderlay, PdfUnderlayDefinition>(pdfUnderlay);
					break;
				case Point point:
					this.writePoint(point);
					break;
				case Polyline polyline:
					this.writePolyline(polyline);
					break;
				case RasterImage rasterImage:
					this.writeCadImage(rasterImage);
					break;
				case Ray ray:
					this.writeRay(ray);
					break;
				case Shape shape:
					this.writeShape(shape);
					break;
				case Solid solid:
					this.writeSolid(solid);
					break;
				case Spline spline:
					this.writeSpline(spline);
					break;
				case TextEntity text:
					this.writeTextEntity(text);
					break;
				case Tolerance tolerance:
					this.writeTolerance(tolerance);
					break;
				case Vertex vertex:
					this.writeVertex(vertex);
					break;
				case Viewport viewport:
					this.writeViewport(viewport);
					break;
				case Wipeout wipeout:
					this.writeCadImage(wipeout);
					break;
				case XLine xline:
					this.writeXLine(xline);
					break;
				default:
					throw new NotImplementedException($"Entity not implemented {entity.GetType().FullName}");
			}

			this.writeExtendedData(entity.ExtendedData);
		}

		private void writeArc(Arc arc)
		{
			DxfClassMap map = DxfClassMap.Create<Arc>();

			this.writeCircle(arc);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Arc);

			this._writer.Write(50, arc.StartAngle, map);
			this._writer.Write(51, arc.EndAngle, map);
		}

		private void writeCircle(Circle circle)
		{
			DxfClassMap map = DxfClassMap.Create<Circle>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Circle);

			this._writer.Write(10, circle.Center, map);

			this._writer.Write(39, circle.Thickness, map);
			this._writer.Write(40, circle.Radius, map);

			this._writer.Write(210, circle.Normal, map);
		}

		private void writeDimension(Dimension dim)
		{
			DxfClassMap map = DxfClassMap.Create<Dimension>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Dimension);

			this._writer.WriteName(2, dim.Block, map);

			this._writer.Write(10, dim.DefinitionPoint, map);
			this._writer.Write(11, dim.TextMiddlePoint, map);

			this._writer.Write(53, dim.TextRotation, map);
			this._writer.Write(70, (short)dim.Flags, map);
			this._writer.Write(71, (short)dim.AttachmentPoint, map);
			this._writer.Write(72, (short)dim.LineSpacingStyle, map);
			this._writer.Write(41, dim.LineSpacingFactor, map);

			if (string.IsNullOrEmpty(dim.Text))
			{
				this._writer.Write(1, dim.Text, map);
			}

			this._writer.Write(210, dim.Normal, map);

			this._writer.WriteName(3, dim.Style, map);

			switch (dim)
			{
				case DimensionAligned aligned:
					this.writeDimensionAligned(aligned);
					break;
				case DimensionRadius radius:
					this.writeDimensionRadius(radius);
					break;
				case DimensionDiameter diameter:
					this.writeDimensionDiameter(diameter);
					break;
				case DimensionAngular2Line angular2Line:
					this.writeDimensionAngular2Line(angular2Line);
					break;
				case DimensionAngular3Pt angular3Pt:
					this.writeDimensionAngular3Pt(angular3Pt);
					break;
				case DimensionOrdinate ordinate:
					this.writeDimensionOrdinate(ordinate);
					break;
				default:
					throw new NotImplementedException($"Dimension type not implemented {dim.GetType().FullName}");
			}
		}

		private void writeDimensionAligned(DimensionAligned aligned)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionAligned>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.AlignedDimension);

			this._writer.Write(13, aligned.FirstPoint, map);
			this._writer.Write(14, aligned.SecondPoint, map);

			if (aligned is DimensionLinear linear)
			{
				this.writeDimensionLinear(linear);
			}
		}

		private void writeDimensionLinear(DimensionLinear linear)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionLinear>();

			this._writer.Write(50, linear.Rotation, map);

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.LinearDimension);
		}

		private void writeDimensionRadius(DimensionRadius radius)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionRadius>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.RadialDimension);

			this._writer.Write(15, radius.AngleVertex, map);

			this._writer.Write(40, radius.LeaderLength, map);
		}

		private void writeDimensionDiameter(DimensionDiameter diameter)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionDiameter>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.DiametricDimension);

			this._writer.Write(15, diameter.AngleVertex, map);

			this._writer.Write(40, diameter.LeaderLength, map);
		}

		private void writeDimensionAngular2Line(DimensionAngular2Line angular2Line)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionAngular2Line>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Angular2LineDimension);

			this._writer.Write(13, angular2Line.FirstPoint, map);
			this._writer.Write(14, angular2Line.SecondPoint, map);
			this._writer.Write(15, angular2Line.AngleVertex, map);
			this._writer.Write(16, angular2Line.DimensionArc, map);
		}

		private void writeDimensionAngular3Pt(DimensionAngular3Pt angular3Pt)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionAngular3Pt>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Angular3PointDimension);

			this._writer.Write(13, angular3Pt.FirstPoint, map);
			this._writer.Write(14, angular3Pt.SecondPoint, map);
			this._writer.Write(15, angular3Pt.AngleVertex, map);
		}

		private void writeDimensionOrdinate(DimensionOrdinate ordinate)
		{
			DxfClassMap map = DxfClassMap.Create<DimensionOrdinate>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.OrdinateDimension);

			this._writer.Write(13, ordinate.FeatureLocation, map);
			this._writer.Write(14, ordinate.LeaderEndpoint, map);
		}

		private void writeHatch(Hatch hatch)
		{
			DxfClassMap map = DxfClassMap.Create<Hatch>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Hatch);

			this._writer.Write(10, 0, map);
			this._writer.Write(20, 0, map);
			this._writer.Write(30, hatch.Elevation, map);

			this._writer.Write(210, hatch.Normal, map);

			this._writer.Write(2, hatch.Pattern.Name, map);

			this._writer.Write(70, hatch.IsSolid ? (short)1 : (short)0, map);
			this._writer.Write(71, hatch.IsAssociative ? (short)1 : (short)0, map);

			this._writer.Write(91, hatch.Paths.Count, map);
			foreach (var path in hatch.Paths)
			{
				this.writeBoundaryPath(path);
			}

			this.writeHatchPattern(hatch, hatch.Pattern);

			if (hatch.PixelSize != 0)
			{
				this._writer.Write(47, hatch.PixelSize, map);
			}

			this._writer.Write(98, hatch.SeedPoints.Count);
			foreach (XY spoint in hatch.SeedPoints)
			{
				this._writer.Write(10, spoint);
			}

			//TODO: Implement HatchGradientPattern
		}

		private void writeBoundaryPath(Hatch.BoundaryPath path)
		{
			this._writer.Write(92, (int)path.Flags);

			if (!path.Flags.HasFlag(BoundaryPathFlags.Polyline))
			{
				this._writer.Write(93, path.Edges.Count);
			}

			foreach (Hatch.BoundaryPath.Edge edge in path.Edges)
			{
				this.writeHatchBoundaryPathEdge(edge);
			}

			this._writer.Write(97, path.Entities.Count);
			foreach (Entity entity in path.Entities)
			{
				this._writer.WriteHandle(330, entity);
			}
		}

		private void writeHatchBoundaryPathEdge(Hatch.BoundaryPath.Edge edge)
		{
			if (edge is not Hatch.BoundaryPath.Polyline)
			{
				this._writer.Write(72, edge.Type);
			}

			switch (edge)
			{
				case Hatch.BoundaryPath.Arc arc:
					this._writer.Write(10, arc.Center);
					this._writer.Write(40, arc.Radius);
					this._writer.Write(50, arc.StartAngle);
					this._writer.Write(51, arc.EndAngle);
					this._writer.Write(73, arc.CounterClockWise ? (short)1 : (short)0);
					break;
				case Hatch.BoundaryPath.Ellipse ellipse:
					this._writer.Write(10, ellipse.Center);
					this._writer.Write(11, ellipse.MajorAxisEndPoint);
					this._writer.Write(40, ellipse.MinorToMajorRatio);
					this._writer.Write(50, ellipse.StartAngle);
					this._writer.Write(51, ellipse.EndAngle);
					this._writer.Write(73, ellipse.IsCounterclockwise ? (short)1 : (short)0);
					break;
				case Hatch.BoundaryPath.Line line:
					this._writer.Write(10, line.Start);
					this._writer.Write(11, line.End);
					break;
				case Hatch.BoundaryPath.Polyline poly:
					this._writer.Write(72, poly.HasBulge ? (short)1 : (short)0);
					this._writer.Write(73, poly.IsClosed ? (short)1 : (short)0);
					this._writer.Write(93, poly.Vertices.Count);
					for (int i = 0; i < poly.Vertices.Count; i++)
					{
						this._writer.Write(10, (XY)poly.Vertices[i]);
						if (poly.HasBulge)
						{
							this._writer.Write(42, poly.Bulges.ElementAtOrDefault(i));
						}
					}
					break;
				case Hatch.BoundaryPath.Spline spline:
					this._writer.Write(73, spline.Rational ? (short)1 : (short)0);
					this._writer.Write(74, spline.Periodic ? (short)1 : (short)0);

					this._writer.Write(94, (int)spline.Degree);
					this._writer.Write(95, spline.Knots.Count);
					this._writer.Write(96, spline.ControlPoints.Count);

					foreach (double knot in spline.Knots)
					{
						this._writer.Write(40, knot);
					}

					foreach (var point in spline.ControlPoints)
					{
						this._writer.Write(10, point.X);
						this._writer.Write(20, point.Y);
						if (spline.Rational)
						{
							this._writer.Write(42, point.Z);
						}
					}
					break;
				default:
					throw new ArgumentException($"Unknown Hatch.BoundaryPath.Edge type {edge.GetType().FullName}");
			}
		}

		private void writeHatchPattern(Hatch hatch, HatchPattern pattern)
		{
			this._writer.Write(75, (short)hatch.Style);
			this._writer.Write(76, (short)hatch.PatternType);

			if (!hatch.IsSolid)
			{
				this._writer.Write(52, MathHelper.RadToDeg(hatch.PatternAngle));
				this._writer.Write(41, hatch.PatternScale);
				this._writer.Write(77, (short)(hatch.IsDouble ? 1 : 0));
				this._writer.Write(78, (short)pattern.Lines.Count);
				foreach (HatchPattern.Line line in pattern.Lines)
				{
					this._writer.Write(53, MathHelper.RadToDeg(line.Angle));
					this._writer.Write(43, line.BasePoint.X);
					this._writer.Write(44, line.BasePoint.Y);
					this._writer.Write(45, line.Offset.X);
					this._writer.Write(46, line.Offset.Y);
					this._writer.Write(79, (short)line.DashLengths.Count);
					foreach (double dashLength in line.DashLengths)
					{
						this._writer.Write(49, dashLength);
					}
				}
			}
		}

		private void writeEllipse(Ellipse ellipse)
		{
			DxfClassMap map = DxfClassMap.Create<Ellipse>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Ellipse);

			this._writer.Write(10, ellipse.Center, map);

			this._writer.Write(11, ellipse.EndPoint, map);

			this._writer.Write(210, ellipse.Normal, map);

			this._writer.Write(39, ellipse.Thickness, map);
			this._writer.Write(40, ellipse.RadiusRatio, map);
			this._writer.Write(41, ellipse.StartParameter, map);
			this._writer.Write(42, ellipse.EndParameter, map);
		}

		private void writeFace3D(Face3D face)
		{
			DxfClassMap map = DxfClassMap.Create<Face3D>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Face3d);

			this._writer.Write(10, face.FirstCorner, map);
			this._writer.Write(11, face.SecondCorner, map);
			this._writer.Write(12, face.ThirdCorner, map);
			this._writer.Write(13, face.FourthCorner, map);

			this._writer.Write(70, (short)face.Flags, map);
		}

		private void writeInsert(Insert insert)
		{
			DxfClassMap map = DxfClassMap.Create<Insert>();

			this._writer.Write(DxfCode.Subclass, insert.SubclassMarker);

			this._writer.WriteName(2, insert.Block, map);

			this._writer.Write(10, insert.InsertPoint, map);

			this._writer.Write(41, insert.XScale, map);
			this._writer.Write(42, insert.YScale, map);
			this._writer.Write(43, insert.ZScale, map);

			this._writer.Write(50, insert.Rotation, map);


			this._writer.Write(70, (short)insert.ColumnCount);
			this._writer.Write(71, (short)insert.RowCount);

			this._writer.Write(44, insert.ColumnSpacing);
			this._writer.Write(45, insert.RowSpacing);

			this._writer.Write(210, insert.Normal, map);

			if (insert.HasAttributes)
			{
				this._writer.Write(66, 1);

				//WARNING: Write extended data before attributes

				foreach (var att in insert.Attributes)
				{
					this.writeEntity(att);
				}

				this.writeSeqend(insert.Attributes.Seqend);
			}
		}

		private void writeLeader(Leader leader)
		{
			DxfClassMap map = DxfClassMap.Create<Leader>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Leader);

			this._writer.WriteName(3, leader.Style, map);

			this._writer.Write(71, leader.ArrowHeadEnabled ? (short)1 : (short)0, map);
			this._writer.Write(72, (short)leader.PathType, map);
			this._writer.Write(73, (short)leader.CreationType, map);
			this._writer.Write(74, leader.HookLineDirection == HookLineDirection.Same ? (short)1 : (short)0, map);
			this._writer.Write(75, leader.HasHookline ? (short)1 : (short)0, map);

			this._writer.Write(40, leader.TextHeight, map);
			this._writer.Write(41, leader.TextWidth, map);

			this._writer.Write(76, leader.Vertices.Count, map);
			foreach (var vertex in leader.Vertices)
			{
				this._writer.Write(10, vertex, map);
			}

			//this._writer.Write(77, leader,map);
			//this._writer.Write(340, leader.Annotation,map);

			this._writer.Write(210, leader.Normal, map);

			this._writer.Write(211, leader.HorizontalDirection, map);
			this._writer.Write(212, leader.BlockOffset, map);
			this._writer.Write(213, leader.AnnotationOffset, map);
		}

		private void writeLine(Line line)
		{
			DxfClassMap map = DxfClassMap.Create<Line>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Line);

			this._writer.Write(10, line.StartPoint, map);

			this._writer.Write(11, line.EndPoint, map);

			this._writer.Write(39, line.Thickness, map);

			this._writer.Write(210, line.Normal, map);
		}

		private void writeLwPolyline(LwPolyline polyline)
		{
			DxfClassMap map = DxfClassMap.Create<LwPolyline>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.LwPolyline);

			this._writer.Write(90, polyline.Vertices.Count);
			this._writer.Write(70, (short)polyline.Flags);

			this._writer.Write(38, polyline.Elevation);
			this._writer.Write(39, polyline.Thickness);

			foreach (LwPolyline.Vertex v in polyline.Vertices)
			{
				this._writer.Write(10, v.Location);
				this._writer.Write(40, v.StartWidth);
				this._writer.Write(41, v.EndWidth);
				this._writer.Write(42, v.Bulge);
			}

			this._writer.Write(210, polyline.Normal, map);
		}

		private void writeMesh(Mesh mesh)
		{
			DxfClassMap map = DxfClassMap.Create<Mesh>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Mesh);

			this._writer.Write(71, (short)mesh.Version, map);
			this._writer.Write(72, (short)(mesh.BlendCrease ? 1 : 0), map);

			this._writer.Write(91, mesh.SubdivisionLevel, map);

			this._writer.Write(92, mesh.Vertices.Count, map);
			foreach (XYZ vertex in mesh.Vertices)
			{
				this._writer.Write(10, vertex, map);
			}

			int nFaces = mesh.Faces.Count;
			nFaces += mesh.Faces.Sum(f => f.Length);

			this._writer.Write(93, nFaces);
			foreach (int[] face in mesh.Faces)
			{
				this._writer.Write(90, face.Length);
				foreach (int index in face)
				{
					this._writer.Write(90, index);
				}
			}

			this._writer.Write(94, mesh.Edges.Count, map);
			foreach (Mesh.Edge edge in mesh.Edges)
			{
				this._writer.Write(90, edge.Start);
				this._writer.Write(90, edge.End);
			}

			this._writer.Write(95, mesh.Edges.Count, map);
			foreach (Mesh.Edge edge in mesh.Edges)
			{
				this._writer.Write(140, edge.Crease);
			}

			this._writer.Write(90, 0);
		}

		private void writeMLine(MLine mLine)
		{
			DxfClassMap map = DxfClassMap.Create<MLine>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.MLine);

			//Style has two references
			this._writer.WriteName(2, mLine.Style, map);
			this._writer.WriteHandle(340, mLine.Style, map);

			this._writer.Write(40, mLine.ScaleFactor);

			this._writer.Write(70, (short)mLine.Justification);
			this._writer.Write(71, (short)mLine.Flags);
			this._writer.Write(72, (short)mLine.Vertices.Count);

			if (mLine.Style != null)
			{
				this._writer.Write(73, (short)mLine.Style.Elements.Count);
			}

			this._writer.Write(10, mLine.StartPoint, map);

			this._writer.Write(210, mLine.Normal);

			foreach (var v in mLine.Vertices)
			{
				this._writer.Write(11, v.Position, map);
				this._writer.Write(12, v.Direction, map);
				this._writer.Write(13, v.Miter, map);

				foreach (var s in v.Segments)
				{
					this._writer.Write(74, (short)s.Parameters.Count);
					foreach (double parameter in s.Parameters)
					{
						this._writer.Write(41, parameter);
					}
					this._writer.Write(75, (short)s.AreaFillParameters.Count);
					foreach (double areaFillParameter in s.AreaFillParameters)
					{
						this._writer.Write(42, areaFillParameter);
					}
				}
			}
		}

		private void writeMText(MText mtext, bool writeSubclass = true)
		{
			DxfClassMap map = DxfClassMap.Create<MText>();

			if (writeSubclass)
			{
				this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.MText);
			}

			this._writer.Write(10, mtext.InsertPoint, map);

			this._writer.Write(40, mtext.Height, map);
			this._writer.Write(41, mtext.RectangleWidth, map);
			this._writer.Write(44, mtext.LineSpacing, map);

			if (this.Version >= ACadVersion.AC1021)
			{
				this._writer.Write(46, mtext.RectangleHeight, map);
			}

			this._writer.Write(71, (short)mtext.AttachmentPoint, map);
			this._writer.Write(72, (short)mtext.DrawingDirection, map);

			this.writeLongTextValue(1, 3, mtext.Value);

			this._writer.WriteName(7, mtext.Style);

			this._writer.Write(73, (short)mtext.LineSpacingStyle, map);

			this._writer.Write(11, mtext.AlignmentPoint, map);

			this._writer.Write(210, mtext.Normal, map);
		}

		private void writeMultiLeader(MultiLeader multiLeader)
		{
			MultiLeaderAnnotContext contextData = multiLeader.ContextData;

			this._writer.Write(100, "AcDbMLeader");

			//	version
			//	if (Version > ACadVersion.
			this._writer.Write(270, 2);

			writeMultiLeaderAnnotContext(contextData);

			//	MultiLeader properties
			this._writer.WriteHandle(340, multiLeader.Style);
			this._writer.Write(90, multiLeader.PropertyOverrideFlags);
			this._writer.Write(170, (short)multiLeader.PathType);

			this._writer.WriteCmColor(91, multiLeader.LineColor);

			this._writer.WriteHandle(341, multiLeader.LineType);
			this._writer.Write(171, (short)multiLeader.LeaderLineWeight);
			this._writer.Write(290, multiLeader.EnableLanding);
			this._writer.Write(291, multiLeader.EnableDogleg);
			this._writer.Write(41, multiLeader.LandingDistance);
			this._writer.Write(42, multiLeader.ArrowheadSize);
			this._writer.Write(172, (short)multiLeader.ContentType);
			this._writer.WriteHandle(343, multiLeader.TextStyle);
			this._writer.Write(173, (short)multiLeader.TextLeftAttachment);
			this._writer.Write(95, (short)multiLeader.TextRightAttachment);
			this._writer.Write(174, (short)multiLeader.TextAngle);
			this._writer.Write(175, (short)multiLeader.TextAlignment);

			this._writer.WriteCmColor(92, multiLeader.TextColor);

			this._writer.Write(292, multiLeader.TextFrame);

			this._writer.WriteCmColor(93, multiLeader.BlockContentColor);

			this._writer.Write(10, multiLeader.BlockContentScale);

			this._writer.Write(43, multiLeader.BlockContentRotation);
			this._writer.Write(176, (short)multiLeader.BlockContentConnection);
			this._writer.Write(293, multiLeader.EnableAnnotationScale);
			this._writer.Write(294, multiLeader.TextDirectionNegative);
			this._writer.Write(178, multiLeader.TextAligninIPE);
			this._writer.Write(179, multiLeader.TextAttachmentPoint);
			this._writer.Write(45, multiLeader.ScaleFactor);
			this._writer.Write(271, multiLeader.TextAttachmentDirection);
			this._writer.Write(272, multiLeader.TextBottomAttachment);
			this._writer.Write(273, multiLeader.TextTopAttachment);
			this._writer.Write(295, 0);
		}

		private void writeMultiLeaderAnnotContext(MultiLeaderAnnotContext contextData)
		{
			this._writer.Write(300, "CONTEXT_DATA{");
			this._writer.Write(40, contextData.ScaleFactor);
			this._writer.Write(10, contextData.ContentBasePoint);
			this._writer.Write(41, contextData.TextHeight);
			this._writer.Write(140, contextData.ArrowheadSize);
			this._writer.Write(145, contextData.LandingGap);
			this._writer.Write(174, (short)contextData.TextLeftAttachment);
			this._writer.Write(175, (short)contextData.TextRightAttachment);
			this._writer.Write(176, (short)contextData.TextAlignment);
			this._writer.Write(177, (short)contextData.BlockContentConnection);
			this._writer.Write(290, contextData.HasTextContents);
			this._writer.Write(304, contextData.TextLabel);

			this._writer.Write(11, contextData.TextNormal);

			this._writer.WriteHandle(340, contextData.TextStyle);

			this._writer.Write(12, contextData.TextLocation);

			this._writer.Write(13, contextData.Direction);

			this._writer.Write(42, contextData.TextRotation);
			this._writer.Write(43, contextData.BoundaryWidth);
			this._writer.Write(44, contextData.BoundaryHeight);
			this._writer.Write(45, contextData.LineSpacingFactor);
			this._writer.Write(170, (short)contextData.LineSpacing);

			this._writer.WriteCmColor(90, contextData.TextColor);

			this._writer.Write(171, (short)contextData.TextAttachmentPoint);
			this._writer.Write(172, (short)contextData.FlowDirection);

			this._writer.WriteCmColor(91, contextData.BackgroundFillColor);

			this._writer.Write(141, contextData.BackgroundScaleFactor);
			this._writer.Write(92, contextData.BackgroundTransparency);
			this._writer.Write(291, contextData.BackgroundFillEnabled);
			this._writer.Write(292, contextData.BackgroundMaskFillOn);
			this._writer.Write(173, contextData.ColumnType);
			this._writer.Write(293, contextData.TextHeightAutomatic);
			this._writer.Write(142, contextData.ColumnWidth);
			this._writer.Write(143, contextData.ColumnGutter);
			this._writer.Write(294, contextData.ColumnFlowReversed);
			this._writer.Write(295, contextData.WordBreak);

			this._writer.Write(296, contextData.HasContentsBlock);

			this._writer.Write(110, contextData.BasePoint);

			this._writer.Write(111, contextData.BaseDirection);

			this._writer.Write(112, contextData.BaseVertical);

			this._writer.Write(297, contextData.NormalReversed);

			foreach (MultiLeaderAnnotContext.LeaderRoot leaderRoot in contextData.LeaderRoots)
			{
				writeLeaderRoot(leaderRoot);
			}

			this._writer.Write(272, (short)contextData.TextBottomAttachment);
			this._writer.Write(273, (short)contextData.TextTopAttachment);
			this._writer.Write(301, "}");       //	CONTEXT_DATA
		}

		private void writeLeaderRoot(MultiLeaderAnnotContext.LeaderRoot leaderRoot)
		{
			this._writer.Write(302, "LEADER{");

			// TODO: true is placeholder
			this._writer.Write(290, true ? (short)1 : (short)0); // Has Set Last Leader Line Point
			this._writer.Write(291, true ? (short)1 : (short)0); // Has Set Dogleg Vector

			this._writer.Write(10, leaderRoot.ConnectionPoint);

			this._writer.Write(11, leaderRoot.Direction);

			this._writer.Write(90, leaderRoot.LeaderIndex);
			this._writer.Write(40, leaderRoot.LandingDistance);

			foreach (MultiLeaderAnnotContext.LeaderLine leaderLine in leaderRoot.Lines)
			{
				writeLeaderLine(leaderLine);
			}

			this._writer.Write(271, 0);
			this._writer.Write(303, "}");   //	LEADER
		}

		private void writeLeaderLine(MultiLeaderAnnotContext.LeaderLine leaderLine)
		{
			this._writer.Write(304, "LEADER_LINE{");

			foreach (XYZ point in leaderLine.Points)
			{
				this._writer.Write(10, point);
			}
			this._writer.Write(91, leaderLine.Index);

			this._writer.Write(305, "}");   //	LEADER_Line
		}

		private void writePdfUnderlay<T,R>(T underlay)
			where T : UnderlayEntity<R>
			where R : UnderlayDefinition
		{
			DxfClassMap map = DxfClassMap.Create<T>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Underlay);

			this._writer.WriteHandle(340, underlay.Definition, map);

			this._writer.Write(10, underlay.InsertPoint, map);

			this._writer.Write(280, underlay.Flags, map);
			this._writer.Write(281, underlay.Contrast, map);
			this._writer.Write(282, underlay.Fade, map);

			foreach (XY bv in underlay.ClipBoundaryVertices)
			{
				this._writer.Write(11, bv, map);
			}

		}

		private void writePoint(Point point)
		{
			DxfClassMap map = DxfClassMap.Create<Point>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Point);

			this._writer.Write(10, point.Location, map);

			this._writer.Write(39, point.Thickness, map);

			this._writer.Write(210, point.Normal, map);

			this._writer.Write(50, point.Rotation, map);
		}

		private void writePolyline(Polyline polyline)
		{
			DxfClassMap map;

			switch (polyline)
			{
				case Polyline2D:
					map = DxfClassMap.Create<Polyline2D>();
					break;
				case Polyline3D:
					map = DxfClassMap.Create<Polyline3D>();
					break;
				case PolyfaceMesh:
					map = DxfClassMap.Create<PolyfaceMesh>();
					break;
				default:
					throw new NotImplementedException($"Polyline not implemented {polyline.GetType().FullName}");
			}

			this._writer.Write(DxfCode.Subclass, polyline.SubclassMarker);

			this._writer.Write(DxfCode.XCoordinate, 0);
			this._writer.Write(DxfCode.YCoordinate, 0);
			this._writer.Write(DxfCode.ZCoordinate, polyline.Elevation);

			this._writer.Write(70, (short)polyline.Flags, map);
			this._writer.Write(75, (short)polyline.SmoothSurface, map);

			this._writer.Write(210, polyline.Normal, map);

			if (polyline.Vertices.Any())
			{
				foreach (Vertex v in polyline.Vertices)
				{
					this.writeEntity(v);
				}

				this.writeSeqend(polyline.Vertices.Seqend);
			}
		}

		private void writeSeqend(Seqend seqend)
		{
			this._writer.Write(0, seqend.ObjectName);
			this._writer.Write(5, seqend.Handle);
			this._writer.Write(330, seqend.Owner.Handle);
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Entity);
			this._writer.Write(8, seqend.Layer.Name);
		}

		private void writeRay(Ray ray)
		{
			DxfClassMap map = DxfClassMap.Create<Ray>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Ray);

			this._writer.Write(10, ray.StartPoint, map);

			this._writer.Write(11, ray.Direction, map);
		}

		private void writeShape(Shape shape)
		{
			DxfClassMap map = DxfClassMap.Create<Shape>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Shape);

			this._writer.Write(39, shape.Thickness, map);

			this._writer.Write(10, shape.InsertionPoint, map);

			this._writer.Write(40, shape.Size, map);

			this._writer.WriteName(2, shape.ShapeStyle, map);

			this._writer.Write(50, shape.Rotation, map);

			this._writer.Write(41, shape.RelativeXScale, map);
			this._writer.Write(51, shape.ObliqueAngle, map);

			this._writer.Write(210, shape.Normal, map);
		}

		private void writeSolid(Solid solid)
		{
			DxfClassMap map = DxfClassMap.Create<Solid>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Solid);

			this._writer.Write(10, solid.FirstCorner, map);
			this._writer.Write(11, solid.SecondCorner, map);
			this._writer.Write(12, solid.ThirdCorner, map);
			this._writer.Write(13, solid.FourthCorner, map);

			this._writer.Write(39, solid.Thickness, map);

			this._writer.Write(210, solid.Normal, map);
		}

		private void writeSpline(Spline spline)
		{
			DxfClassMap map = DxfClassMap.Create<Spline>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Spline);

			if (spline.Flags.HasFlag(SplineFlags.Planar))
			{
				this._writer.Write(210, spline.Normal, map);
			}

			this._writer.Write(70, (short)spline.Flags, map);
			this._writer.Write(71, (short)spline.Degree, map);
			this._writer.Write(72, (short)spline.Knots.Count, map);
			this._writer.Write(73, (short)spline.ControlPoints.Count, map);

			if (spline.FitPoints.Any())
			{
				this._writer.Write(74, (short)spline.FitPoints.Count, map);
			}

			this._writer.Write(42, spline.KnotTolerance, map);
			this._writer.Write(43, spline.ControlPointTolerance, map);
			this._writer.Write(44, spline.FitTolerance, map);

			this._writer.Write(12, spline.StartTangent, map);
			this._writer.Write(13, spline.EndTangent, map);

			foreach (double knot in spline.Knots)
			{
				this._writer.Write(40, knot, map);
			}
			foreach (double weight in spline.Weights)
			{
				this._writer.Write(41, weight, map);
			}
			foreach (var cp in spline.ControlPoints)
			{
				this._writer.Write(10, cp, map);
			}
			foreach (var fp in spline.FitPoints)
			{
				this._writer.Write(11, fp, map);
			}
		}

		private void writeTextEntity(TextEntity text)
		{
			DxfClassMap map = DxfClassMap.Create<TextEntity>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Text);

			this._writer.Write(1, text.Value, map);

			this._writer.Write(10, text.InsertPoint, map);

			this._writer.Write(40, text.Height, map);

			if (text.WidthFactor != 1.0)
			{
				this._writer.Write(41, text.WidthFactor, map);
			}

			if (text.Rotation != 0.0)
			{
				this._writer.Write(50, text.Rotation, map);
			}

			if (text.ObliqueAngle != 0.0)
			{
				this._writer.Write(51, text.ObliqueAngle, map);
			}

			this._writer.Write(7, text.Style.Name);

			this._writer.Write(11, text.AlignmentPoint, map);

			this._writer.Write(210, text.Normal, map);

			if (text.Mirror != 0)
			{
				this._writer.Write(71, text.Mirror, map);
			}
			if (text.HorizontalAlignment != 0)
			{
				this._writer.Write(72, text.HorizontalAlignment, map);
			}

			if (text.GetType() == typeof(TextEntity))
			{
				this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Text);

				if (text.VerticalAlignment != 0)
				{
					this._writer.Write(73, text.VerticalAlignment, map);
				}
			}

			if (text is AttributeBase)
			{
				switch (text)
				{
					case AttributeEntity att:
						this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Attribute);
						this.writeAttributeBase(att);
						break;
					case AttributeDefinition attdef:
						this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.AttributeDefinition);
						this._writer.Write(3, attdef.Prompt, DxfClassMap.Create<AttributeDefinition>());
						this.writeAttributeBase(attdef);
						break;
					default:
						throw new ArgumentException($"Unknown AttributeBase type {text.GetType().FullName}");
				}
			}
		}

		private void writeTolerance(Tolerance tolerance)
		{
			DxfClassMap map = DxfClassMap.Create<Tolerance>();

			this._writer.Write(DxfCode.Subclass, tolerance.SubclassMarker);

			this._writer.WriteName(3, tolerance.Style, map);

			this._writer.Write(10, tolerance.InsertionPoint, map);
			this._writer.Write(11, tolerance.Direction, map);
			this._writer.Write(210, tolerance.Normal, map);
			this._writer.Write(1, tolerance.Text, map);
		}

		private void writeAttributeBase(AttributeBase att)
		{
			this._writer.Write(2, att.Tag);

			this._writer.Write(70, (short)att.Flags);
			this._writer.Write(73, (short)0);

			if (att.VerticalAlignment != 0)
			{
				this._writer.Write(74, (short)att.VerticalAlignment);
			}

			if (this.Version > ACadVersion.AC1027 && att.MText != null)
			{
				this._writer.Write(71, (short)att.AttributeType);
				this._writer.Write(72, (short)0);
				this._writer.Write(11, att.AlignmentPoint);

				this.writeMText(att.MText, false);
			}
		}

		private void writeVertex(Vertex v)
		{
			DxfClassMap map = DxfClassMap.Create<Vertex>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Vertex);
			this._writer.Write(DxfCode.Subclass, v.SubclassMarker);

			this._writer.Write(10, v.Location, map);

			this._writer.Write(40, v.StartWidth, map);
			this._writer.Write(41, v.EndWidth, map);
			this._writer.Write(42, v.Bulge, map);

			this._writer.Write(70, v.Flags, map);

			this._writer.Write(50, v.CurveTangent, map);
		}

		private void writeViewport(Viewport vp)
		{
			DxfClassMap map = DxfClassMap.Create<Viewport>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Viewport);

			this._writer.Write(10, vp.Center, map);

			this._writer.Write(40, vp.Width, map);
			this._writer.Write(41, vp.Height, map);

			this._writer.Write(69, vp.Id, map);

			this._writer.Write(12, vp.ViewCenter, map);

			this._writer.Write(13, vp.SnapBase, map);

			this._writer.Write(14, vp.SnapSpacing, map);

			this._writer.Write(15, vp.GridSpacing, map);

			this._writer.Write(16, vp.ViewDirection, map);

			this._writer.Write(17, vp.ViewTarget, map);

			this._writer.Write(42, vp.LensLength, map);

			this._writer.Write(43, vp.FrontClipPlane, map);
			this._writer.Write(44, vp.BackClipPlane, map);
			this._writer.Write(45, vp.ViewHeight, map);

			this._writer.Write(50, vp.SnapAngle, map);
			this._writer.Write(51, vp.TwistAngle, map);

			this._writer.Write(72, vp.CircleZoomPercent, map);

			foreach (var layer in vp.FrozenLayers)
			{
				this._writer.Write(331, layer.Handle, map);
			}

			this._writer.Write(90, (int)vp.Status, map);

			if (vp.Boundary != null)
			{
				this._writer.Write(340, vp.Boundary.Handle, map);
			}

			this._writer.Write(110, vp.UcsOrigin, map);

			this._writer.Write(111, vp.UcsXAxis, map);

			this._writer.Write(112, vp.UcsYAxis, map);
		}

		private void writeCadImage<T>(T image)
			where T : CadWipeoutBase
		{
			DxfClassMap map = DxfClassMap.Create<T>();

			this._writer.Write(DxfCode.Subclass, image.SubclassMarker);

			this._writer.Write(90, image.ClassVersion, map);

			this._writer.Write(10, image.InsertPoint, map);
			this._writer.Write(11, image.UVector, map);
			this._writer.Write(12, image.VVector, map);
			this._writer.Write(13, image.Size, map);

			this._writer.WriteHandle(340, image.Definition, map);

			this._writer.Write(70, (short)image.Flags, map);

			this._writer.Write(280, image.ClippingState, map);
			this._writer.Write(281, image.Brightness, map);
			this._writer.Write(282, image.Contrast, map);
			this._writer.Write(283, image.Fade, map);

			//this._writer.WriteHandle(360, image.DefinitionReactor, map);

			this._writer.Write(71, (short)image.ClipType, map);

			if (image.ClipType == ClipType.Polygonal)
			{
				this._writer.Write(91, image.ClipBoundaryVertices.Count + 1, map);
				foreach (XY bv in image.ClipBoundaryVertices)
				{
					this._writer.Write(14, bv, map);
				}

				this._writer.Write(14, image.ClipBoundaryVertices.First(), map);
			}
			else
			{
				this._writer.Write(91, image.ClipBoundaryVertices.Count, map);
				foreach (XY bv in image.ClipBoundaryVertices)
				{
					this._writer.Write(14, bv, map);
				}
			}
		}

		private void writeXLine(XLine xline)
		{
			DxfClassMap map = DxfClassMap.Create<XLine>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XLine);

			this._writer.Write(10, xline.FirstPoint, map);
			this._writer.Write(11, xline.Direction, map);
		}
	}
}
