using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Tables;
using ACadSharp.Types.Units;
using CSMath;
using CSUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ACadSharp.IO.SVG
{
	internal class SvgXmlWriter : XmlTextWriter
	{
		public event NotificationEventHandler OnNotification;

		public SvgConfiguration Configuration { get; } = new();

		public Layout Layout { get; set; }

		public UnitsType Units { get; protected set; }

		public SvgXmlWriter(Stream stream, SvgConfiguration configuration) : this(stream, null, configuration)
		{
		}

		public SvgXmlWriter(Stream stream, Encoding? encoding, SvgConfiguration configuration) : base(stream, encoding)
		{
			this.Configuration = configuration;
		}

		public void WriteAttributeString(string localName, double value)
		{
			this.WriteAttributeString(localName, value, this.Units);
		}

		public void WriteAttributeString(string localName, double value, UnitsType units)
		{
			this.WriteAttributeString(localName, value.ToSvg(units));
		}

		public void WriteBlock(BlockRecord record)
		{
			this.Units = record.Units;

			BoundingBox box = record.GetBoundingBox();

			this.startDocument(box, box, this.Units);

			foreach (var e in record.Entities)
			{
				this.writeEntity(e);
			}

			this.endDocument();
		}

		public void WriteLayout(Layout layout)
		{
			this.Layout = layout;
			this.Units = layout.PaperUnits.ToUnits();

			double paperWidth = layout.PaperWidth;
			double paperHeight = layout.PaperHeight;

			switch (layout.PaperRotation)
			{
				case PlotRotation.Degrees90:
				case PlotRotation.Degrees270:
					paperWidth = layout.PaperHeight;
					paperHeight = layout.PaperWidth;
					break;
			}

			XYZ lowerCorner = XYZ.Zero;
			XYZ upperCorner = new XYZ(paperWidth, paperHeight, 0.0);
			BoundingBox paper = new BoundingBox(lowerCorner, upperCorner);

			XYZ lowerMargin = layout.UnprintableMargin.BottomLeftCorner.Convert<XYZ>();
			XYZ upperMargin = upperCorner - layout.UnprintableMargin.TopCorner.Convert<XYZ>();
			BoundingBox margins = new BoundingBox(
				lowerMargin,
				upperMargin);

			this.startDocument(paper, null, UnitsType.Millimeters);

			Transform transform = new Transform(
				lowerMargin.ToPixelSize(UnitsType.Millimeters),
				new XYZ(layout.PrintScale),
				XYZ.Zero);

			foreach (var e in layout.AssociatedBlock.Entities)
			{
				this.writeEntity(e, transform);
			}

			this.endDocument();
		}

		protected void notify(string message, NotificationType type, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, type, ex));
		}

		protected void triggerNotification(object sender, NotificationEventArgs e)
		{
			this.OnNotification?.Invoke(sender, e);
		}

		protected void writeEntity(Entity entity, Transform transform)
		{
			this.WriteComment($"{entity.ObjectName} | {entity.Handle}");

			switch (entity)
			{
				case Arc arc:
					this.writeArc(arc, transform);
					break;
				case Dimension dimension:
					this.writeDimension(dimension, transform);
					break;
				case Line line:
					this.writeLine(line, transform);
					break;
				case Point point:
					this.writePoint(point, transform);
					break;
				case Circle circle:
					this.writeCircle(circle, transform);
					break;
				case Ellipse ellipse:
					this.writeEllipse(ellipse, transform);
					break;
				case Hatch hatch:
					this.writeHatch(hatch, transform);
					break;
				case Insert insert:
					this.writeInsert(insert, transform);
					break;
				case IPolyline polyline:
					this.writePolyline(polyline, transform);
					break;
				case IText text:
					this.writeText(text, transform);
					break;
				//case Spline spline:
				//	this.writeSpline(spline, transform);
				//	break;
				case Solid solid:
					this.writeSolid(solid, transform);
					break;
				default:
					this.notify($"[{entity.ObjectName}] Entity not implemented.", NotificationType.NotImplemented);
					break;
			}
		}

		private string colorSvg(Color color)
		{
			if (this.Layout != null && color.Equals(Color.Default))
			{
				color = Color.Black;
			}

			return $"rgb({color.R},{color.G},{color.B})";
		}

		private string createPath(params IEnumerable<IPolyline> polylines)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var item in polylines)
			{
				var pts = item.GetPoints<XY>().ToArray();
				if (!pts.Any())
				{
					continue;
				}

				var pt = pts[0];
				sb.Append($"M {pt.ToPixelSize(this.Units).ToSvg()} ");
				for (int i = 1; i < pts.Length; i++)
				{
					pt = pts[i];
					sb.Append($"L {pt.ToPixelSize(this.Units).ToSvg()} ");
				}

				if (item.IsClosed)
				{
					sb.Append("Z");
				}
			}

			return sb.ToString();
		}

		private bool drawableLineType(LineType lineType)
		{
			return lineType.IsComplex && !lineType.HasShapes;
		}

		private void endDocument()
		{
			this.WriteEndElement();
			this.WriteEndDocument();
			this.Close();
		}

		private double getPointSize(IEntity entity)
		{
			return entity.GetActiveLineWeightType().GetLineWeightValue().ToPixelSize(this.Units);
		}

		private void startDocument(BoundingBox box, BoundingBox? viewBox, UnitsType units)
		{
			this.WriteStartDocument();

			this.WriteStartElement("svg");
			this.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");

			this.WriteAttributeString("width", box.Max.X - box.Min.X, units);
			this.WriteAttributeString("height", box.Max.Y - box.Min.Y, units);

			if (viewBox.HasValue)
			{
				var vb = viewBox.Value;
				this.WriteStartAttribute("viewBox");
				this.WriteValue(vb.Min.X.ToPixelSize(units));
				this.WriteValue(" ");
				this.WriteValue(vb.Min.Y.ToPixelSize(units));
				this.WriteValue(" ");
				this.WriteValue((vb.Width).ToPixelSize(units));
				this.WriteValue(" ");
				this.WriteValue((vb.Height).ToPixelSize(units));
				this.WriteEndAttribute();
			}

			this.WriteAttributeString("transform", $"scale(1,-1)");

			if (this.Layout != null)
			{
				this.WriteAttributeString("style", "background-color:white");
			}
		}

		private string svgPoints<T>(IEnumerable<T> points, Transform transform)
			where T : IVector, new()
		{
			if (!points.Any())
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append(points.First().ToPixelSize(this.Units).ToSvg());
			foreach (T point in points.Skip(1))
			{
				sb.Append(' ');
				sb.Append(point.ToPixelSize(this.Units).ToSvg());
			}

			return sb.ToString();
		}

		private void writeArc(Arc arc, Transform transform)
		{
			this.WriteStartElement("path");

			this.writeEntityHeader(arc, transform);

			//A rx ry rotation large-arc-flag sweep-flag x y

			arc.GetEndVertices(out XYZ start, out XYZ end);
			var largeArc = Math.Abs(arc.Sweep) > MathHelper.PI ? 1 : 0;

			StringBuilder sb = new StringBuilder();

			sb.Append($"M {start.ToPixelSize(this.Units).ToSvg()}");
			sb.Append($" ");
			sb.Append($"A {arc.Radius.ToPixelSize(this.Units)} {arc.Radius.ToPixelSize(this.Units)}");
			sb.Append($" ");
			sb.Append($"{0} {largeArc} {1} {end.ToPixelSize(this.Units).ToSvg()}");

			this.WriteAttributeString("d", sb.ToString());

			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeDimension(Dimension dimension, Transform transform)
		{
			this.WriteStartElement("g");

			foreach (Entity e in dimension.Block.Entities)
			{
				this.writeEntity(e, transform);
			}

			this.WriteEndElement();
		}

		private void writeCircle(Circle circle, Transform transform)
		{
			var loc = transform.ApplyTransform(circle.Center);

			this.WriteStartElement("circle");

			this.writeEntityHeader(circle, transform);

			this.WriteAttributeString("r", circle.Radius);
			this.WriteAttributeString("cx", loc.X);
			this.WriteAttributeString("cy", loc.Y);

			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeDashes(IEnumerable<double> dashes)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var d in dashes)
			{
				sb.Append(Math.Abs(d.ToPixelSize(this.Units)));
				sb.Append(' ');
			}

			this.WriteAttributeString("stroke-dasharray", sb.ToString().Trim());
		}

		private void writeDashes(LineType lineType, double pointSize)
		{
			StringBuilder sb = new StringBuilder();
			foreach (LineType.Segment segment in lineType.Segments)
			{
				if (segment.IsPoint)
				{
					sb.Append(pointSize.ToPixelSize(this.Units));
				}
				else
				{
					sb.Append(Math.Abs(segment.Length.ToPixelSize(this.Units)));
				}

				sb.Append(' ');
			}

			this.WriteAttributeString("stroke-dasharray", sb.ToString().Trim());
		}

		private void writeEllipse(Ellipse ellipse, Transform transform)
		{
			if (ellipse.IsFullEllipse)
			{
				this.WriteStartElement("path");

				this.writeEntityHeader(ellipse, transform);

				StringBuilder sb = new StringBuilder();

				XYZ start = ellipse.PolarCoordinateRelativeToCenter(0);
				XYZ end = ellipse.PolarCoordinateRelativeToCenter(Math.PI);

				sb.Append($"M {start.ToPixelSize(this.Units).ToSvg()} ");
				sb.Append($"A {ellipse.MajorAxis / 2} {ellipse.MinorAxis / 2} {MathHelper.RadToDeg(ellipse.Rotation)} {0} {1} {end.ToPixelSize(this.Units).ToSvg()} ");

				start = ellipse.PolarCoordinateRelativeToCenter(Math.PI);
				end = ellipse.PolarCoordinateRelativeToCenter(MathHelper.TwoPI);
				sb.Append($"A {ellipse.MajorAxis / 2} {ellipse.MinorAxis / 2} {MathHelper.RadToDeg(ellipse.Rotation)} {0} {1} {end.ToPixelSize(this.Units).ToSvg()}");

				//A rx ry rotation large-arc-flag sweep-flag x y
				this.WriteAttributeString("d", sb.ToString());

				this.WriteAttributeString("fill", "none");
				this.WriteEndElement();
			}
			else
			{
				this.WriteStartElement("polyline");

				this.writeEntityHeader(ellipse, transform);

				var vertices = ellipse.PolygonalVertexes(256);
				string pts = this.svgPoints(vertices, transform);
				this.WriteAttributeString("points", pts);
				this.WriteAttributeString("fill", "none");

				this.WriteEndElement();

				return;

				//TODO: Fix the ellipse generation
				this.WriteStartElement("path");

				this.writeEntityHeader(ellipse, transform);

				ellipse.GetEndVertices(out XYZ start, out XYZ end);

				//A rx ry rotation large-arc-flag sweep-flag x y
				this.WriteAttributeString("d", $"M {start.ToPixelSize(this.Units).ToSvg()} A {ellipse.MajorAxis} {ellipse.MinorAxis} {MathHelper.RadToDeg(ellipse.Rotation)} {0} {1} {end.ToPixelSize(this.Units).ToSvg()}");

				this.WriteAttributeString("fill", "none");
				this.WriteEndElement();
			}
		}

		private void writeEntity(Entity entity)
		{
			this.writeEntity(entity, new Transform());
		}

		private void writeEntityAsPath<T>(Entity entity, Transform transform, params IEnumerable<T> points)
			where T : IVector
		{
			//Will be needed to write the linetypes that use shapes
			double pointSize = this.getPointSize(entity);
			var lines = entity.GetActiveLineType().CreateLineTypeShape(pointSize, points);

			this.WriteStartElement("path");

			this.writeEntityHeader(entity, transform);

			this.WriteAttributeString("d", this.createPath(lines));

			this.WriteEndElement();
		}

		private void writeEntityHeader(IEntity entity, Transform transform, bool drawStroke = true)
		{
			Color color = entity.GetActiveColor();

			this.WriteAttributeString("vector-effect", "non-scaling-stroke");

			if (drawStroke)
			{
				this.WriteAttributeString("stroke", this.colorSvg(color));
			}
			else
			{
				this.WriteAttributeString("stroke", "none");
			}

			var lineWeight = entity.GetActiveLineWeightType();
			this.WriteAttributeString("stroke-width", $"{this.Configuration.GetLineWeightValue(lineWeight, this.Units).ToSvg(UnitsType.Millimeters)}");

			this.writeTransform(transform);

			LineType lt = entity.GetActiveLineType();
			if (this.drawableLineType(lt))
			{
				this.writeDashes(lt, this.getPointSize(entity));
			}
		}

		private void writeHatch(Hatch hatch, Transform transform)
		{
			this.WriteStartElement("g");

			var patternId = this.writePattern(hatch);

			List<Polyline3D> plines = new List<Polyline3D>();
			foreach (Hatch.BoundaryPath path in hatch.Paths)
			{
				var pline = new Polyline3D(path.GetPoints(this.Configuration.ArcPoints));
				plines.Add(pline);
			}

			this.WriteStartElement("path");

			this.writeEntityHeader(hatch, transform, drawStroke: false);

			this.WriteAttributeString("d", this.createPath(plines));

			this.WriteAttributeString("fill", $"url(#{patternId})");

			this.WriteEndElement();

			this.WriteEndElement();
		}

		private void writePatternHeader(string id)
		{
			this.WriteStartElement("pattern");

			this.WriteAttributeString("id", id);
			this.WriteAttributeString("patternUnits", "userSpaceOnUse");
		}

		private string writePatternHeader(Hatch hatch)
		{
			string id = $"{hatch.Pattern.GetHashCode()}_{hatch.Pattern.Name}";

			this.WriteStartElement("pattern");

			this.WriteAttributeString("id", id);
			this.WriteAttributeString("patternUnits", "userSpaceOnUse");

			return id;
		}

		private string writeSolidPattern(Hatch hatch)
		{
			string id = this.writePatternHeader(hatch);

			this.WriteAttributeString("width", "100%");
			this.WriteAttributeString("height", "100%");

			this.WriteStartElement("rect");

			this.WriteAttributeString("width", "100%");
			this.WriteAttributeString("height", "100%");
			this.WriteAttributeString("fill", this.colorSvg(hatch.Color));

			//rect
			this.WriteEndElement();

			//pattern
			this.WriteEndElement();

			return id;
		}

		private string writePattern(Hatch hatch)
		{
			if (hatch.IsSolid)
			{
				return this.writeSolidPattern(hatch);
			}

			Dictionary<string, BoundingBox> patterns = new();
			foreach (var item in hatch.Pattern.Lines)
			{
				var i = $"{item.GetHashCode()}_line";
				patterns.Add(i, new BoundingBox(XYZ.Zero, new XYZ(item.LineOffset, item.LineOffset, 0)));

				//Each line works individually repeating itself every offset
				this.writePatternHeader(i);
				this.WriteAttributeString("width", item.LineOffset.ToSvg(this.Units));
				this.WriteAttributeString("height", item.LineOffset.ToSvg(this.Units));

				this.writeTransform(name: "patternTransform",
					translation: item.BasePoint.Convert<XYZ>().ToPixelSize(this.Units));
				//rotation: -item.Angle);

				this.WriteStartElement("line");

				//Direction of the line
				var length = item.Offset.GetLength();
				double x = MathHelper.Cos(item.Angle) * 10;
				double y = MathHelper.Sin(item.Angle) * 10;

				//Offset -> is the size of the line box

				//Add BasePoint
				this.WriteAttributeString("x1", 0.0d.ToSvg(this.Units));
				this.WriteAttributeString("y1", 0.0d.ToSvg(this.Units));
				this.WriteAttributeString("x2", (x).ToSvg(this.Units));
				this.WriteAttributeString("y2", (y).ToSvg(this.Units));

				//Rotate the pattern after line
				//this.WriteAttributeString("x1", 0.0d.ToSvg(this.Units));
				//this.WriteAttributeString("y1", (item.LineOffset / 2).ToSvg(this.Units));
				//this.WriteAttributeString("x2", 1.0d.ToSvg(this.Units));
				//this.WriteAttributeString("y2", (item.LineOffset / 2).ToSvg(this.Units));

				this.WriteAttributeString("stroke", this.colorSvg(hatch.GetActiveColor()));
				this.WriteAttributeString("stroke-width", $"{this.Configuration.GetLineWeightValue(hatch.GetActiveLineWeightType(), this.Units).ToSvg(UnitsType.Millimeters)}");

				if (item.DashLengths.Any())
				{
					this.writeDashes(item.DashLengths);
				}

				//Line
				this.WriteEndElement();

				if (false)
				{
					this.WriteStartElement("rect");
					this.WriteAttributeString("width", (item.LineOffset).ToSvg(this.Units));
					this.WriteAttributeString("height", (item.LineOffset).ToSvg(this.Units));
					this.WriteAttributeString("fill", $"none");
					this.WriteAttributeString("stroke", $"red");
					this.WriteEndElement();
				}

				//Pattern
				this.WriteEndElement();
			}

			string id = this.writePatternHeader(hatch);
			var width = patterns.Values.Max(w => w.Width);
			var height = patterns.Values.Max(w => w.Height);

			this.WriteAttributeString("width", width.ToSvg(this.Units));
			this.WriteAttributeString("height", height.ToSvg(this.Units));

			foreach (var item in patterns)
			{
				this.WriteStartElement("rect");
				this.WriteAttributeString("width", (item.Value.Width).ToSvg(this.Units));
				this.WriteAttributeString("height", (item.Value.Height).ToSvg(this.Units));
				this.WriteAttributeString("fill", $"url(#{item.Key})");
				this.WriteEndElement();
			}

			//pattern
			this.WriteEndElement();

			return id;
		}

		private void writeInsert(Insert insert, Transform transform)
		{
			var insertTransform = insert.GetTransform();
			var merged = new Transform(transform.Matrix * insertTransform.Matrix);

			this.WriteStartElement("g");
			this.writeTransform(merged);

			foreach (var e in insert.Block.Entities)
			{
				this.writeEntity(e);
			}

			this.WriteEndElement();
		}

		private void writeLine(Line line, Transform transform)
		{
			this.WriteStartElement("line");

			this.writeEntityHeader(line, transform);

			this.WriteAttributeString("x1", line.StartPoint.X.ToSvg(this.Units));
			this.WriteAttributeString("y1", line.StartPoint.Y.ToSvg(this.Units));
			this.WriteAttributeString("x2", line.EndPoint.X.ToSvg(this.Units));
			this.WriteAttributeString("y2", line.EndPoint.Y.ToSvg(this.Units));

			this.WriteEndElement();
		}

		private void writePoint(Point point, Transform transform)
		{
			this.WriteStartElement("circle");

			this.writeEntityHeader(point, transform);

			this.WriteAttributeString("r", this.Configuration.PointRadius, UnitsType.Unitless);
			this.WriteAttributeString("cx", point.Location.X);
			this.WriteAttributeString("cy", point.Location.Y);

			this.WriteAttributeString("fill", this.colorSvg(point.GetActiveColor()));

			this.WriteEndElement();
		}

		private void writePolyline(IPolyline polyline, Transform transform)
		{
			if (polyline.IsClosed)
			{
				this.WriteStartElement("polygon");
			}
			else
			{
				this.WriteStartElement("polyline");
			}

			this.writeEntityHeader(polyline, transform);

			string pts = this.svgPoints(polyline.GetPoints<XY>(this.Configuration.ArcPoints), transform);

			this.WriteAttributeString("points", pts);
			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeText(IText text, Transform transform)
		{
			XYZ insert;

			if (text is TextEntity lineText
				&& (lineText.HorizontalAlignment != TextHorizontalAlignment.Left
				|| lineText.VerticalAlignment != TextVerticalAlignmentType.Baseline)
				&& !(lineText.HorizontalAlignment == TextHorizontalAlignment.Fit
				|| lineText.HorizontalAlignment == TextHorizontalAlignment.Aligned))
			{
				insert = lineText.AlignmentPoint;
			}
			else
			{
				insert = text.InsertPoint;
			}

			this.WriteStartElement("g");
			this.writeTransform(transform);

			this.WriteStartElement("text");

			this.writeTransform(translation: insert.ToPixelSize(this.Units), scale: new XYZ(1, -1, 0), rotation: text.Rotation != 0 ? text.Rotation : null);

			this.WriteAttributeString("fill", this.colorSvg(text.GetActiveColor()));

			//<text x="20" y="35" class="small">My</text>
			this.WriteStartAttribute("style");
			this.WriteValue("font:");
			this.WriteValue(text.Height.ToSvg(this.Units));
			if (this.Units == UnitsType.Unitless)
			{
				this.WriteValue("px");
			}

			if (text.Style.TrueType.HasFlag(FontFlags.Bold))
			{
				this.WriteValue("bold");
			}

			if (text.Style.TrueType.HasFlag(FontFlags.Italic))
			{
				this.WriteValue("italic");
			}

			this.WriteValue(" ");
			this.WriteValue(Path.GetFileNameWithoutExtension(text.Style.Filename));
			this.WriteEndAttribute();

			switch (text)
			{
				case MText mtext:
					switch (mtext.AttachmentPoint)
					{
						case AttachmentPointType.TopLeft:
							this.WriteAttributeString("alignment-baseline", "hanging");
							this.WriteAttributeString("text-anchor", "start");
							break;
						case AttachmentPointType.TopCenter:
							this.WriteAttributeString("alignment-baseline", "hanging");
							this.WriteAttributeString("text-anchor", "middle");
							break;
						case AttachmentPointType.TopRight:
							this.WriteAttributeString("alignment-baseline", "hanging");
							this.WriteAttributeString("text-anchor", "end");
							break;
						case AttachmentPointType.MiddleLeft:
							this.WriteAttributeString("alignment-baseline", "middle");
							this.WriteAttributeString("text-anchor", "start");
							break;
						case AttachmentPointType.MiddleCenter:
							this.WriteAttributeString("alignment-baseline", "middle");
							this.WriteAttributeString("text-anchor", "middle");
							break;
						case AttachmentPointType.MiddleRight:
							this.WriteAttributeString("alignment-baseline", "middle");
							this.WriteAttributeString("text-anchor", "end");
							break;
						case AttachmentPointType.BottomLeft:
							this.WriteAttributeString("alignment-baseline", "baseline");
							this.WriteAttributeString("text-anchor", "start");
							break;
						case AttachmentPointType.BottomCenter:
							this.WriteAttributeString("alignment-baseline", "baseline");
							this.WriteAttributeString("text-anchor", "middle");
							break;
						case AttachmentPointType.BottomRight:
							this.WriteAttributeString("alignment-baseline", "baseline");
							this.WriteAttributeString("text-anchor", "end");
							break;
						default:
							break;
					}

					foreach (var item in mtext.GetPlainTextLines())
					{
						this.WriteStartElement("tspan");
						this.WriteAttributeString("x", 0);
						this.WriteAttributeString("dy", "1em");
						this.WriteString(item);
						this.WriteEndElement();
					}

					//Line to avoid the strange offset at the end
					this.WriteStartElement("tspan");
					this.WriteAttributeString("x", 0);
					this.WriteAttributeString("dy", "1em");
					this.WriteAttributeString("visibility", "hidden");
					this.WriteString(".");
					this.WriteEndElement();
					break;
				case TextEntity textEntity:

					switch (textEntity.HorizontalAlignment)
					{
						case TextHorizontalAlignment.Left:
							this.WriteAttributeString("text-anchor", "start");
							break;
						case TextHorizontalAlignment.Middle:
						case TextHorizontalAlignment.Center:
							this.WriteAttributeString("text-anchor", "middle");
							break;
						case TextHorizontalAlignment.Right:
							this.WriteAttributeString("text-anchor", "end");
							break;
					}

					switch (textEntity.VerticalAlignment)
					{
						case TextVerticalAlignmentType.Baseline:
						case TextVerticalAlignmentType.Bottom:
							this.WriteAttributeString("alignment-baseline", "baseline");
							break;
						case TextVerticalAlignmentType.Middle:
							this.WriteAttributeString("alignment-baseline", "middle");
							break;
						case TextVerticalAlignmentType.Top:
							this.WriteAttributeString("alignment-baseline", "hanging");
							break;
					}

					this.WriteString(text.Value);
					break;
			}

			this.WriteEndElement();
			this.WriteEndElement();
		}

		private void writeSolid(Solid solid, Transform transform)
		{
			this.WriteStartElement("polygon");

			this.writeEntityHeader(solid, transform);

			string pts = this.svgPoints([solid.FirstCorner, solid.SecondCorner, solid.ThirdCorner, solid.FourthCorner], transform);
			this.WriteAttributeString("points", pts);
			this.WriteAttributeString("fill", this.colorSvg(solid.GetActiveColor()));

			this.WriteEndElement();
		}

		private void writeSpline(Spline spline, Transform transform)
		{
			spline.UpdateFromFitPoints();
			this.writeEntityAsPath(spline, transform, spline.PolygonalVertexes(this.Configuration.ArcPoints));
		}

		private void writeTransform(Transform transform)
		{
			XYZ? translation = transform.Translation != XYZ.Zero ? transform.Translation : null;
			XYZ? scale = transform.Scale != new XYZ(1) ? transform.Scale : null;
			double? rotation = transform.EulerRotation.Z != 0 ? transform.EulerRotation.Z : null;

			this.writeTransform(translation: translation, scale: scale, rotation: rotation);
		}

		private void writeTransform(string name = "transform", XYZ? translation = null, XYZ? scale = null, double? rotation = null)
		{
			StringBuilder sb = new StringBuilder();

			if (translation.HasValue)
			{
				var t = translation.Value;

				sb.Append($"translate(");
				sb.Append($"{t.X.ToString(CultureInfo.InvariantCulture)},");
				sb.Append($"{t.Y.ToString(CultureInfo.InvariantCulture)})");
			}

			if (scale.HasValue)
			{
				var s = scale.Value;

				sb.Append($"scale(");
				sb.Append($"{s.X.ToString(CultureInfo.InvariantCulture)},");
				sb.Append($"{s.Y.ToString(CultureInfo.InvariantCulture)})");
			}

			if (rotation.HasValue)
			{
				var r = -MathHelper.RadToDeg(rotation.Value);

				sb.Append($"rotate(");
				sb.Append($"{r.ToString(CultureInfo.InvariantCulture)})");
			}

			if (sb.ToString().IsNullOrEmpty())
			{
				return;
			}

			this.WriteAttributeString(name, sb.ToString());
		}
	}
}