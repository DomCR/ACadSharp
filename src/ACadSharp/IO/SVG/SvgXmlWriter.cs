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
			this.WriteStartDocument();

			this.WriteStartElement("svg");
			this.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");

			BoundingBox box = record.GetBoundingBox();

			this.WriteAttributeString("width", box.Max.X - box.Min.X);
			this.WriteAttributeString("height", box.Max.Y - box.Min.Y);
			this.WriteAttributeString("transform", "scale(1,-1)");

			Transform transform = new Transform(-box.Min, new XYZ(1), XYZ.Zero);
			foreach (var e in record.Entities)
			{
				switch (e)
				{
					case Line line:
						this.writeLine(line, transform);
						break;
					default:
						this.notify($"[{e.ObjectName}] Entity not implemented.", NotificationType.NotImplemented);
						break;
				}
			}

			this.WriteEndElement();

			this.WriteEndDocument();

			this.Close();
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

		private void writeEntityStyle(Entity entity)
		{
			this.WriteAttributeString("style", $"stroke:rgb({entity.Color.R},{entity.Color.G},{entity.Color.B})");
		}

		private void notify(string message, NotificationType type, Exception ex = null)
		{
			this.OnNotification?.Invoke(this, new NotificationEventArgs(message, type, ex));
		}
	}
}