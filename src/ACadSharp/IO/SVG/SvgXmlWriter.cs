using ACadSharp.Entities;
using ACadSharp.Tables;
using CSMath;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace ACadSharp.IO.SVG
{
	internal class SvgXmlWriter : XmlTextWriter
	{
		public event NotificationEventHandler OnNotification;

		public SvgXmlWriter(TextWriter w) : base(w)
		{
		}

		public SvgXmlWriter(Stream w, Encoding encoding) : base(w, encoding)
		{
		}

		public SvgXmlWriter(string filename, Encoding encoding) : base(filename, encoding)
		{
		}

		public void WriteAttributeString(string localName, double value)
		{
			this.WriteAttributeString(localName, value.ToString(CultureInfo.InvariantCulture));
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
		}

		private void endDocument()
		{
			this.WriteEndElement();
			this.WriteEndDocument();
			this.Close();
		}

		private void writeEntity(Entity entity)
		{
			this.writeEntity(entity, new Transform());
		}

		private void writeEntity(Entity entity, Transform transform)
		{
			switch (entity)
			{
				case Line line:
					this.writeLine(line, transform);
					break;
				case Point point:
					this.writePoint(point, transform);
					break;
				default:
					this.notify($"[{entity.ObjectName}] Entity not implemented.", NotificationType.NotImplemented);
					break;
			}
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

		private void writePoint(Point point, Transform transform)
		{
			var loc = transform.ApplyTransform(point.Location);

			this.WriteStartElement("circle");

			this.writeEntityStyle(point);

			this.WriteAttributeString("r", 0.5);
			this.WriteAttributeString("cx", loc.X);
			this.WriteAttributeString("cy", loc.Y);

			this.WriteAttributeString("fill", colorSvg(point.GetActiveColor()));

			this.WriteEndElement();
		}

		private void writeEntityStyle(Entity entity)
		{
			Color color = entity.GetActiveColor();

			this.WriteAttributeString("style", $"stroke:{colorSvg(color)}");
		}

		private string colorSvg(Color color)
		{
			return $"rgb({color.R},{color.G},{color.B})";
		}

		private void notify(string message, NotificationType type, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, type, ex));
		}
	}
}