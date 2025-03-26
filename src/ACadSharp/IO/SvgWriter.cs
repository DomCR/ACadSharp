using ACadSharp.Entities;
using ACadSharp.IO.SVG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSMath;
using System;
using System.IO;
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
		public SvgWriter(Stream stream) : base(stream, null)
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
		/// <param name="document">Document to export from.</param>
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
			this.Write(this._document.ModelSpace);
		}

		public void Write(BlockRecord record)
		{
			this.createWriter();

			this._writer.WriteBlock(record);
		}

		public void Write(Layout layout)
		{
			throw new NotImplementedException();
		}

		public void WriteEntity(Entity entity)
		{
			this.createWriter();

			this._writer.WriteStartDocument();

			this._writer.WriteStartElement("svg");
			this._writer.WriteAttributeString("xmlns", "http://www.w3.org/2000/svg");

			BoundingBox box = entity.GetBoundingBox();
			this._writer.WriteAttributeString("width", box.Max.X);
			this._writer.WriteAttributeString("height", box.Max.Y);
			this._writer.WriteAttributeString(" transform", "scale(1,-1)");

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
			StreamWriter textWriter = new StreamWriter(this._stream);
			this._writer = new SvgXmlWriter(this._stream, this._encoding);
			this._writer.Formatting = Formatting.Indented;
			this._writer.OnNotification += this.triggerNotification;
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

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._stream.Dispose();
		}
	}
}
