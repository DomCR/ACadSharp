using ACadSharp.Entities;
using ACadSharp.Extensions;
using ACadSharp.IO.DXF;
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

		public UnitsType Units { get; set; }

		public SvgXmlWriter(Stream w, Encoding encoding, SvgConfiguration configuration) : base(w, encoding)
		{
			this.Configuration = configuration;
		}

		public void WriteAttributeString(string localName, double value)
		{
			this.WriteAttributeString(localName, value.ToSvg(this.Units));
		}

		public void WriteBlock(BlockRecord record)
		{
			this.Units = record.Units;

			BoundingBox box = record.GetBoundingBox();

			this.startDocument(box);

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

			XYZ lowerMargin = this.Layout.UnprintableMargin.BottomLeftCorner.Convert<XYZ>();
			BoundingBox margins = new BoundingBox(
				lowerMargin,
				this.Layout.UnprintableMargin.TopCorner.Convert<XYZ>());

			this.startDocument(paper);

			Transform transform = new Transform(
				lowerMargin.ToPixelSize(this.Units),
				new XYZ(layout.PrintScale),
				XYZ.Zero);

			foreach (var e in layout.AssociatedBlock.Entities)
			{
				this.writeEntity(e, transform);
			}

			this.endDocument();
		}

		private string colorSvg(Color color)
		{
			if (this.Layout != null && color.Equals(Color.Default))
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
			this.WriteValue(box.Min.X.ToSvg(this.Units));
			this.WriteValue(" ");
			this.WriteValue(box.Min.Y.ToSvg(this.Units));
			this.WriteValue(" ");
			this.WriteValue((box.Max.X - box.Min.X).ToSvg(this.Units));
			this.WriteValue(" ");
			this.WriteValue((box.Max.Y - box.Min.Y).ToSvg(this.Units));
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
			sb.Append(points.First().ToPixelSize(this.Units).ToSvg());
			foreach (IVector point in points.Skip(1))
			{
				sb.Append(' ');
				sb.Append(point.ToPixelSize(this.Units).ToSvg());
			}

			return sb.ToString();
		}

		private void writeArc(Arc arc, Transform transform)
		{
			//A rx ry rotation large-arc-flag sweep-flag x y

			this.WriteStartElement("polyline");

			this.writeEntityHeader(arc, transform);

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

			this.writeEntityHeader(circle, transform);

			this.WriteAttributeString("r", circle.Radius);
			this.WriteAttributeString("cx", loc.X);
			this.WriteAttributeString("cy", loc.Y);

			this.WriteAttributeString("fill", "none");

			this.WriteEndElement();
		}

		private void writeEllipse(Ellipse ellipse, Transform transform)
		{
			this.WriteStartElement("polygon");

			this.writeEntityHeader(ellipse, transform);

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

		private void writeEntityHeader(IEntity entity, Transform transform)
		{
			Color color = entity.GetActiveColor();

			this.WriteAttributeString("vector-effect", "non-scaling-stroke");
			this.WriteAttributeString("stroke", this.colorSvg(color));

			var lineWeight = entity.LineWeight;
			switch (lineWeight)
			{
				case LineWeightType.ByLayer:
					lineWeight = entity.Layer.LineWeight;
					break;
			}

			this.WriteAttributeString("stroke-width", $"{this.Configuration.GetLineWeightValue(lineWeight, this.Units).ToSvg(UnitsType.Millimeters)}");

			this.writeTransform(transform);
		}

		private void writeHatch(Hatch hatch, Transform transform)
		{
			this.WriteStartElement("g");

			this.writePattern(hatch.Pattern);

			foreach (Hatch.BoundaryPath path in hatch.Paths)
			{
				this.WriteStartElement("polyline");

				this.writeEntityHeader(hatch, transform);

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

		private void writePattern(HatchPattern pattern)
		{
			this.WriteStartElement("pattern");

			this.WriteEndElement();
		}

		private void writePoint(Point point, Transform transform)
		{
			this.WriteStartElement("circle");

			this.writeEntityHeader(point, transform);

			this.WriteAttributeString("r", this.Configuration.PointRadius);
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

		private void writeTransform(Transform transform)
		{
			XYZ? translation = transform.Translation != XYZ.Zero ? transform.Translation : null;
			XYZ? scale = transform.Scale != new XYZ(1) ? transform.Scale : null;
			double? rotation = transform.EulerRotation.Z != 0 ? transform.EulerRotation.Z : null;

			this.writeTransform(translation, scale, rotation);
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

			this.WriteAttributeString("transform", sb.ToString());
		}
	}
}