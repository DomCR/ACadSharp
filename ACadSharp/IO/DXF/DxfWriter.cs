using ACadSharp.IO.DXF;
using System.IO;
using System.Text;

namespace ACadSharp.IO
{
	public class DxfWriter : CadWriterBase
	{
		/// <summary>
		/// Flag indicating if the dxf will be writen as a binary file
		/// </summary>
		public bool IsBinary { get; }

		private IDxfStreamWriter _writer;
		private CadObjectHolder _objectHolder = new CadObjectHolder();

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfWriter"/> class.
		/// </summary>
		/// <param name="filename">The file to write into.</param>
		/// <param name="document"></param>
		/// <param name="binary"></param>
		public DxfWriter(string filename, CadDocument document, bool binary)
			: this(File.Create(filename), document, binary)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DxfWriter"/> class.
		/// </summary>
		/// <param name="stream">The stream to write into</param>
		/// <param name="document"></param>
		/// <param name="binary"></param>
		public DxfWriter(Stream stream, CadDocument document, bool binary) : base(stream, document)
		{
			this.IsBinary = binary;
		}

		/// <inheritdoc/>
		public override void Write()
		{
			this.createStreamWriter();

			this._objectHolder.Objects.Enqueue(_document.RootDictionary);

			this.writeHeader();

			this.writeDxfClasses();

			this.writeTables();

			this.writeBlocks();

			this.writeEntities();

			this.writeObjects();

			this.writeACDSData();

			this._writer.Write(DxfCode.Start, DxfFileToken.EndOfFile);

			this._writer.Flush();

			if (this.CloseStream)
			{
				this._writer.Close();
			}
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			this._writer.Dispose();
		}

		/// <summary>
		/// Write a <see cref="CadDocument"/> into a file
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="document"></param>
		/// <param name="binary"></param>
		/// <param name="notification"></param>
		public static void Write(string filename, CadDocument document, bool binary, NotificationEventHandler notification = null)
		{
			using (DxfWriter writer = new DxfWriter(filename, document, binary))
			{
				writer.OnNotification += notification;
				writer.Write();
			}
		}

		/// <summary>
		/// Write a <see cref="CadDocument"/> intio a <see cref="Stream"/>
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="document"></param>
		/// <param name="binary"></param>
		public static void Write(Stream stream, CadDocument document, bool binary, NotificationEventHandler notification = null)
		{
			using (DxfWriter writer = new DxfWriter(stream, document, binary))
			{
				writer.OnNotification += notification;
				writer.Write();
			}
		}

		private void createStreamWriter()
		{
			Encoding encoding = this.getListedEncoding(this._document.Header.CodePage);

			if (this.IsBinary)
			{
				this._writer = new DxfBinaryWriter(new BinaryWriter(this._stream, encoding));
			}
			else
			{
				this._writer = new DxfAsciiWriter(new StreamWriter(this._stream, encoding));
			}
		}

		private void writeHeader()
		{
			var writer = new DxfHeaderSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeDxfClasses()
		{
			var writer = new DxfClassesSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeTables()
		{
			var writer = new DxfTablesSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeBlocks()
		{
			var writer = new DxfBlocksSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeEntities()
		{
			var writer = new DxfEntitiesSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeObjects()
		{
			var writer = new DxfObjectsSectionWriter(this._writer, this._document, this._objectHolder);
			writer.OnNotification += this.triggerNotification;

			writer.Write();
		}

		private void writeACDSData()
		{
		}
	}
}
