using ACadSharp.Entities;
using System;

namespace ACadSharp.IO.DXF
{
	internal abstract partial class DxfSectionWriterBase
	{
		protected void writeEntity<T>(T entity)
			where T : Entity
		{
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
				case Ellipse ellipse:
					this.writeEllipse(ellipse);
					break;
				case Line line:
					this.writeLine(line);
					break;
				case Point point:
					this.writePoint(point);
					break;
				case Polyline polyline:
					this.writePolyline(polyline);
					break;
				default:
					throw new NotImplementedException($"Entity not implemented {entity.GetType().FullName}");
			}
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

		private void writeEllipse(Ellipse ellipse)
		{
			DxfClassMap map = DxfClassMap.Create<Ellipse>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Circle);

			this._writer.Write(10, ellipse.Center, map);

			this._writer.Write(11, ellipse.EndPoint, map);

			this._writer.Write(210, ellipse.Normal, map);

			this._writer.Write(39, ellipse.Thickness, map);
			this._writer.Write(40, ellipse.RadiusRatio, map);
			this._writer.Write(41, ellipse.StartParameter, map);
			this._writer.Write(42, ellipse.EndParameter, map);
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

		private void writePoint(Point line)
		{
			DxfClassMap map = DxfClassMap.Create<Point>();

			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Point);

			this._writer.Write(10, line.Location, map);

			this._writer.Write(39, line.Thickness, map);

			this._writer.Write(210, line.Normal, map);

			this._writer.Write(50, line.Rotation, map);
		}

		private void writePolyline(Polyline polyline)
		{
			DxfClassMap entityMap = DxfClassMap.Create<Entity>();
			DxfClassMap plineMap = null;

			this._writer.Write(DxfCode.Start, polyline.ObjectName);

			this.writeCommonObjectData(polyline);

			this.writeClassMap(entityMap, polyline);

			switch (polyline)
			{
				case Polyline2D:
					plineMap = DxfClassMap.Create<Polyline2D>();
					break;
				case Polyline3D:
					plineMap = DxfClassMap.Create<Polyline3D>();
					break;
			}

			//Remove elevation
			plineMap.DxfProperties.Remove(30);

			this.writeClassMap(plineMap, polyline);

			this._writer.Write(DxfCode.XCoordinate, 0);
			this._writer.Write(DxfCode.YCoordinate, 0);
			this._writer.Write(DxfCode.ZCoordinate, polyline.Elevation);

			this.writeCollection(polyline.Vertices);
		}
	}
}
