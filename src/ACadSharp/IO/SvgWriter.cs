using ACadSharp.IO.SVG;
using ACadSharp.Objects;
using ACadSharp.Tables;
using System;
using System.IO;
using System.Xml;

namespace ACadSharp.IO
{
	/// <summary>
	/// Writer to support the creation of SVG from <see cref="BlockRecord"/> and <see cref="Layout"/>.
	/// </summary>
	public class SvgWriter : CadWriterBase<SvgConfiguration>
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
		public override void Dispose()
		{
			this._stream.Dispose();
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

		/// <summary>
		/// Write the selected layout into a SVG.
		/// </summary>
		/// <param name="layout"></param>
		public void Write(Layout layout)
		{
			this.createWriter();

			this._writer.WriteLayout(layout);
		}

		private void createWriter()
		{
			StreamWriter textWriter = new StreamWriter(this._stream);
			this._writer = new SvgXmlWriter(this._stream, this._encoding, this.Configuration);
			this._writer.Formatting = Formatting.Indented;
			this._writer.OnNotification += this.triggerNotification;
		}
	}
}