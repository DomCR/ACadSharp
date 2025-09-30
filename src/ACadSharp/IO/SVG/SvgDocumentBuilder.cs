using ACadSharp.Extensions;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ACadSharp.IO.SVG
{
	internal class SvgDocumentBuilder : SvgXmlWriter
	{
		public SvgXmlWriter EntitiesWriter { get; set; }

		public Dictionary<string, SvgXmlWriter> LineTypeWriters { get; } = new();

		public SvgDocumentBuilder(Stream stream, Encoding encoding, SvgConfiguration configuration)
			: base(stream, encoding, configuration)
		{
		}

		public void WriteLayout(Layout layout)
		{
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

		private void writeLineType(LineType lineType)
		{
			if (this.LineTypeWriters.TryGetValue(lineType.Name, out SvgXmlWriter writer))
			{
				return;
			}

			writer = this.createWriter();
			this.LineTypeWriters.Add(lineType.Name, writer);

			writer.WriteStartElement("defs");

			writer.WriteEndElement();
		}

		private SvgXmlWriter createWriter()
		{
			var xmlWriter = new SvgXmlWriter(new MemoryStream(), this.Configuration);
			xmlWriter.Formatting = Formatting.Indented;
			xmlWriter.OnNotification += this.triggerNotification;

			return xmlWriter;
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

		private void endDocument()
		{
			this.WriteEndElement();
			this.WriteEndDocument();
			this.Close();
		}
	}
}
