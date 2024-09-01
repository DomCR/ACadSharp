using ACadSharp.Entities;
using CSMath;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace ACadSharp.IO
{
	public class SvgWriter : CadWriterBase<CadWriterConfiguration>
	{
		private SvgXmlWriter _writer;

		public SvgWriter(Stream stream) : base(stream, null)
		{
			StreamWriter textWriter = new StreamWriter(stream);
			this._writer = new SvgXmlWriter(textWriter);
		}

		public SvgWriter(string filename)
			: this(File.Create(filename), null)
		{
		}

		public SvgWriter(string filename, CadDocument document)
			: this(File.Create(filename), document)
		{
		}

		public SvgWriter(Stream stream, CadDocument document) : base(stream, document)
		{
			StreamWriter textWriter = new StreamWriter(this._stream);
			this._writer = new SvgXmlWriter(this._stream, Encoding.Default);

			this._writer.Formatting = Formatting.Indented;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <see cref="SvgWriter"/> will draw all the content in the model space.<br/>
		/// The writer must be initialized with a none null <see cref="CadDocument"/>.
		/// </remarks>
		public override void Write()
		{
			this.Write(this._document);

			throw new NotImplementedException();
		}

		public void Write(CadDocument document)
		{
			if (this._document is null)
			{
				throw new ArgumentNullException("CadDocument cannot be null in the SvgWriter.", "CadDocument");
			}

			this._encoding = this.getListedEncoding(document.Header.CodePage);

			throw new NotImplementedException();
		}

		public void WriteEntity(Entity entity)
		{
			this._writer.WriteStartDocument();

			this._writer.WriteStartElement("svg");

			BoundingBox box = entity.GetBoundingBox();
			this._writer.WriteAttributeString("width", box.Max.X);
			this._writer.WriteAttributeString("height", box.Max.Y);

			switch (entity)
			{
				case Line line:
					this.writeLine(line);
					break;
				default:
					throw new NotImplementedException($"Entity {entity.SubclassMarker} is not implemented.");
			}

			this._writer.WriteEndElement();

			this._writer.WriteEndDocument();

			this._writer.Close();
		}

		private void writeEntityStyle(Entity entity)
		{
			this._writer.WriteAttributeString("style", $"stroke:rgb({entity.Color.R},{entity.Color.G},{entity.Color.B})");
		}

		private void writeLine(Line line)
		{
			this._writer.WriteStartElement("line");

			this.writeEntityStyle(line);

			this._writer.WriteAttributeString("x1", line.StartPoint.X);
			this._writer.WriteAttributeString("y1", line.StartPoint.Y);
			this._writer.WriteAttributeString("x2", line.EndPoint.X);
			this._writer.WriteAttributeString("y2", line.EndPoint.Y);
		}

		public string Convert(Entity entity)
		{
			StringBuilder sb = new StringBuilder();

			BoundingBox box = entity.GetBoundingBox();



			sb.AppendLine($"<svg width=\"{box.Max.X}\" height=\"{box.Max.Y}\">");

			switch (entity)
			{
				case Line line:
					sb.AppendLine(convertLine(line));
					break;
				default:
					throw new NotImplementedException($"Svg convertion not implemented for {entity.SubclassMarker}");
			}

			sb.AppendLine($"</svg>");

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}

		private string convertLine(Line line)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append($"\t");
			sb.Append($"<line ");
			sb.Append($"x1=\"{line.StartPoint.X}\" ");
			sb.Append($"y1=\"{line.StartPoint.Y}\" ");
			sb.Append($"x2=\"{line.EndPoint.X}\" ");
			sb.Append($"y2=\"{line.EndPoint.Y}\" ");
			sb.Append(this.entityStyle(line));
			sb.Append($"/>");

			return sb.ToString();
		}

		private string entityStyle(Entity entity)
		{
			StringBuilder style = new StringBuilder();

			style.Append($"style=");

			style.Append($"\"stroke:rgb({entity.Color.R},{entity.Color.G},{entity.Color.B})\"");

			return style.ToString();
		}
	}

	public class SvgXmlWriter : XmlTextWriter
	{
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
	}
}
