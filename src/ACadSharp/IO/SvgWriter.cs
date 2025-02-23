using ACadSharp.Entities;
using ACadSharp.IO.SVG;
using CSMath;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace ACadSharp.IO
{
	public class SvgWriter : CadWriterBase<CadWriterConfiguration>
	{
		private SvgXmlWriter _writer;

		/// <summary>
		/// Initialize an instance of <see cref="SvgWriter"/> with a default document as a reference.
		/// </summary>
		/// <param name="stream"></param>
		public SvgWriter(Stream stream) : base(stream, new CadDocument())
		{
		}

		/// <summary>
		/// Initialize an instance of <see cref="SvgWriter"/> with a default document as a reference.
		/// </summary>
		/// <param name="filename"></param>
		public SvgWriter(string filename)
			: this(File.Create(filename))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="document">Reference document for the table records.</param>
		public SvgWriter(string filename, CadDocument document)
			: this(File.Create(filename), document)
		{
		}

		public SvgWriter(Stream stream, CadDocument document) : base(stream, document)
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// <see cref="SvgWriter"/> will draw all the content in the model space.<br/>
		/// The writer must be initialized with a none null <see cref="CadDocument"/>.
		/// </remarks>
		public override void Write()
		{
			this.createWriter();

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

		private void createWriter()
		{
			base.Write();

			StreamWriter textWriter = new StreamWriter(this._stream);
			this._writer = new SvgXmlWriter(this._stream, this._encoding);
			this._writer.Formatting = Formatting.Indented;
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
}
