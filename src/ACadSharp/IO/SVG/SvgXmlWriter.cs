using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
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

		public PlotPaperUnits PlotPaperUnits
		{
			get
			{
				if (this.Layout == null)
				{
					return PlotPaperUnits.Milimeters;
				}
				else
				{
					return Layout.PaperUnits;
				}
			}
		}

		public SvgXmlWriter(Stream w, Encoding encoding, SvgConfiguration configuration) : base(w, encoding)
		{
			this.Configuration = configuration;
		}

		public void WriteAttributeString(string localName, double value)
		{
			this.WriteAttributeString(
				localName,
				SvgConfiguration.ToPixelSize(value, PlotPaperUnits)
				.ToString(CultureInfo.InvariantCulture));
		}

		public void WriteBlock(BlockRecord record)
		{
			BoundingBox box = record.GetBoundingBox();
			this.startDocument(box);

			Transform transform = new Transform(-box.Min, new XYZ(1), XYZ.Zero);
			foreach (var e in record.Entities)
			{
				this.writeEntity(e);
			}

			this.endDocument();
		}

		public void WriteLayout(Layout layout)
		{
			this.Layout = layout;

			XYZ lowerCorner = XYZ.Zero;
			XYZ upperCorner = new XYZ(layout.PaperWidth, layout.PaperHeight, 0.0);
			BoundingBox box = new BoundingBox(layout.MinExtents, layout.MaxExtents);
			this.startDocument(box);

			foreach (var e in layout.AssociatedBlock.Entities)
			{
				if (e.Layer.Name.Equals("control", StringComparison.OrdinalIgnoreCase))
				{
				}

				this.writeEntity(e);
			}

			this.endDocument();
		}

		public override void WriteValue(double value)
		{
			base.WriteValue(SvgConfiguration.ToPixelSize(value, this.PlotPaperUnits));
		}

		private string colorSvg(Color color)
		{
			if (Layout != null && color.Equals(Color.Default))
			{
				color = Color.Black;
			}

			return $"rgb({color.R},{color.G},{color.B})";
		}

		private void endDocument()
		{
			this.WriteEndElement();
			this.WriteEndDocument();
			this.Close();
		}

		private void notify(string message, NotificationType type, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, type, ex));
		}

		private void startDocument(BoundingBox box)
		{
			this.WriteStartDocument();

			this.WriteStartElement("svg");
			this.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");

			this.WriteAttributeString("width", box.Max.X - box.Min.X);
			this.WriteAttributeString("height", box.Max.Y - box.Min.Y);

			this.WriteStartAttribute("viewBox");
			this.WriteValue(box.Min.X);
			this.WriteValue(" ");
			this.WriteValue(box.Min.Y);
			this.WriteValue(" ");
			this.WriteValue(box.Max.X - box.Min.X);
			this.WriteValue(" ");
			this.WriteValue(box.Max.Y - box.Min.Y);
			this.WriteEndAttribute();

			this.WriteAttributeString("transform", $"scale(1,-1)");

			if (this.Layout != null)
			{
				this.WriteAttributeString("style", "background-color:white");
			}
		}

		private string svgPoints(IEnumerable<IVector> points, Transform transform)
		{
			if (!points.Any())
			{
				return string.Empty;
			}

			StringBuilder sb = new StringBuilder();
			sb.Append(transform.ApplyTransform(points.First().Convert<XYZ>()).SvgPoint());
			foreach (IVector point in points.Skip(1))
			{
				sb.Append(' ');
				sb.Append(transform.ApplyTransform(point.Convert<XYZ>()).SvgPoint());
			}

			return sb.ToString();
		}

		private string toStringFormat(double value)
		{
			return SvgConfiguration.ToPixelSize(value, this.PlotPaperUnits).ToString(CultureInfo.InvariantCulture);
		}

		private void writeArc(Arc arc, Transform transform)
		{
			//A rx ry rotation large-arc-flag sweep-flag x y

			this.WriteStartElement("polyline");

			this.writeEntityStyle(arc);

			IEnumerable<IVector> vertices = arc.PolygonalVertexes(256).OfType<IVector>();
			string pts = this.svgPoints(vertices, transform);
			this.WriteAttributeString("points", pts);
			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeCircle(Circle circle, Transform transform)
		{
			var loc = transform.ApplyTransform(circle.Center);

			this.WriteStartElement("circle");

			this.writeEntityStyle(circle);

			this.WriteAttributeString("r", circle.Radius);
			this.WriteAttributeString("cx", loc.X);
			this.WriteAttributeString("cy", loc.Y);

			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeEllipse(Ellipse ellipse, Transform transform)
		{
			this.WriteStartElement("polygon");

			this.writeEntityStyle(ellipse);

			IEnumerable<IVector> vertices = ellipse.PolygonalVertexes(256).OfType<IVector>();
			string pts = this.svgPoints(vertices, transform);
			this.WriteAttributeString("points", pts);
			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeEntity(Entity entity)
		{
			this.writeEntity(entity, new Transform());
		}

		private void writeEntity(Entity entity, Transform transform)
		{
			switch (entity)
			{
				case Arc arc:
					this.writeArc(arc, transform);
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
				//case Hatch hatch:
				//	this.writeHatch(hatch, transform);
				//	break;
				case Insert insert:
					this.writeInsert(insert, transform);
					break;
				case IPolyline polyline:
					this.writePolyline(polyline, transform);
					break;
				case IText text:
					this.writeText(text, transform);
					break;
				default:
					this.notify($"[{entity.ObjectName}] Entity not implemented.", NotificationType.NotImplemented);
					break;
			}
		}

		private void writeEntityStyle(IEntity entity)
		{
			Color color = entity.GetActiveColor();

			this.WriteAttributeString("stroke", this.colorSvg(color));

			var lineWeight = entity.LineWeight;
			switch (lineWeight)
			{
				case LineweightType.ByLayer:
					lineWeight = entity.Layer.LineWeight;
					break;
			}

			this.WriteAttributeString("stroke-width", Configuration.GetLineWeightValue(lineWeight).ToString(CultureInfo.InvariantCulture));
		}

		private void writeHatch(Hatch hatch, Transform transform)
		{
			this.WriteStartElement("g");

			this.writePattern(hatch.Pattern);

			foreach (Hatch.BoundaryPath path in hatch.Paths)
			{
				this.WriteStartElement("polyline");

				this.writeEntityStyle(hatch);

				foreach (var item in path.Edges)
				{
					//TODO: svg edges for hatch drawing
				}

				//this.WriteAttributeString("points", pts);

				this.WriteAttributeString("fill", "none");

				this.WriteEndElement();
			}

			this.WriteEndElement();
		}

		private void writeInsert(Insert insert, Transform transform)
		{
			var insertTransform = insert.GetTransform();
			var merged = new Transform(transform.Matrix * insertTransform.Matrix);

			StringBuilder sb = new StringBuilder();
			sb.Append($"translate(");
			sb.Append($"{insert.InsertPoint.X.ToString(CultureInfo.InvariantCulture)},");
			sb.Append($"{insert.InsertPoint.Y.ToString(CultureInfo.InvariantCulture)})");
			sb.Append(' ');
			sb.Append($"scale(");
			sb.Append($"{insert.XScale.ToString(CultureInfo.InvariantCulture)},");
			sb.Append($"{insert.YScale.ToString(CultureInfo.InvariantCulture)})");
			sb.Append(' ');
			sb.Append($"rotate(");
			sb.Append($"{insert.Rotation.ToString(CultureInfo.InvariantCulture)})");

			this.WriteStartElement("g");
			this.WriteAttributeString("transform", sb.ToString());

			foreach (var e in insert.Block.Entities)
			{
				this.writeEntity(e);
			}

			this.WriteEndElement();
		}

		private void writeTransform(Transform transform)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append($"translate(");
			sb.Append($"{transform.Translation.X.ToString(CultureInfo.InvariantCulture)},");
			sb.Append($"{transform.Translation.Y.ToString(CultureInfo.InvariantCulture)})");
			sb.Append(' ');
			sb.Append($"scale(");
			sb.Append($"{transform.Scale.X.ToString(CultureInfo.InvariantCulture)},");
			sb.Append($"{transform.Scale.Y.ToString(CultureInfo.InvariantCulture)})");
			sb.Append(' ');
			sb.Append($"rotate(");
			sb.Append($"{transform.EulerRotation.X.ToString(CultureInfo.InvariantCulture)})");

			this.WriteAttributeString("transform", sb.ToString());
		}

		private void writeTransform(XYZ? translation = null, XYZ? scale = null, double? rotation = null)
		{
			StringBuilder sb = new StringBuilder();

			if (translation.HasValue)
			{
				var t = translation.Value;

				sb.Append($"translate(");
				sb.Append($"{t.X.ToString(CultureInfo.InvariantCulture)},");
				sb.Append($"{t.Y.ToString(CultureInfo.InvariantCulture)})");
				sb.Append(' ');
			}

			if (scale.HasValue)
			{
				var s = scale.Value;

				sb.Append($"scale(");
				sb.Append($"{s.X.ToString(CultureInfo.InvariantCulture)},");
				sb.Append($"{s.Y.ToString(CultureInfo.InvariantCulture)})");
				sb.Append(' ');
			}

			if (rotation.HasValue)
			{
				var r = -MathHelper.RadToDeg(rotation.Value);

				sb.Append($"rotate(");
				sb.Append($"{r.ToString(CultureInfo.InvariantCulture)})");
			}

			this.WriteAttributeString("transform", sb.ToString());
		}

		private void writeLine(Line line, Transform transform)
		{
			var start = transform.ApplyTransform(line.StartPoint);
			var end = transform.ApplyTransform(line.EndPoint);

			this.WriteStartElement("line");

			this.writeEntityStyle(line);

			this.WriteAttributeString("x1", start.X);
			this.WriteAttributeString("y1", start.Y);
			this.WriteAttributeString("x2", end.X);
			this.WriteAttributeString("y2", end.Y);

			this.WriteEndElement();
		}

		private void writePattern(HatchPattern pattern)
		{
			this.WriteStartElement("pattern");

			this.WriteEndElement();
		}

		private void writePoint(Point point, Transform transform)
		{
			var loc = transform.ApplyTransform(point.Location);

			this.WriteStartElement("circle");

			this.writeEntityStyle(point);

			this.WriteAttributeString("r", this.Configuration.PointRadius);
			this.WriteAttributeString("cx", loc.X);
			this.WriteAttributeString("cy", loc.Y);

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

			this.writeEntityStyle(polyline);

			var vertices = polyline.Vertices.Select(v => v.Location).ToList();

			string pts = this.svgPoints(polyline.Vertices.Select(v => v.Location), transform);
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

			insert = transform.ApplyTransform(insert);

			this.WriteStartElement("g");
			this.WriteAttributeString("transform", $"translate({toStringFormat(insert.X)},{toStringFormat(insert.Y)})");

			this.WriteStartElement("text");

			this.writeTransform(scale: new XYZ(1, -1, 0), rotation: text.Rotation != 0 ? text.Rotation : null);

			this.WriteAttributeString("fill", this.colorSvg(text.GetActiveColor()));

			//<text x="20" y="35" class="small">My</text>
			this.WriteStartAttribute("style");
			this.WriteValue("font:");
			this.WriteValue(text.Height);
			this.WriteValue("px");
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

					foreach (var item in mtext.GetTextLines())
					{
						this.WriteStartElement("tspan");
						this.WriteAttributeString("x", 0);
						this.WriteAttributeString("dy", "1em");
						this.WriteString(item);
						this.WriteEndElement();
					}
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
	}
}