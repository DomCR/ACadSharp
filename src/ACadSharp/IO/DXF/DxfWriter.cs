using ACadSharp.IO.DXF;
using System.IO;

namespace ACadSharp.IO;

/// <summary>
/// Class for writing a DXF from a <see cref="CadDocument"/>.
/// </summary>
public class DxfWriter : CadWriterBase<DxfWriterConfiguration>
{
	/// <summary>
	/// Flag indicating if the dxf will be writen as a binary file
	/// </summary>
	public bool IsBinary { get; }

	private CadObjectHolder _objectHolder = new CadObjectHolder();

	private IDxfStreamWriter _writer;

	/// <summary>
	/// Initializes a new instance of the <see cref="DxfWriter"/> class.
	/// </summary>
	/// <param name="filename">The file to write into.</param>
	/// <param name="document"></param>
	/// <param name="binary"></param>
	public DxfWriter(string filename, CadDocument document, bool binary = false)
		: this(File.Create(filename), document, binary)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DxfWriter"/> class.
	/// </summary>
	/// <param name="stream">The stream to write into</param>
	/// <param name="document"></param>
	/// <param name="binary"></param>
	public DxfWriter(Stream stream, CadDocument document, bool binary = false) : base(stream, document)
	{
		this.IsBinary = binary;
	}

	/// <summary>
	/// Write a <see cref="CadDocument"/> into a file.
	/// </summary>
	/// <param name="filename"></param>
	/// <param name="document"></param>
	/// <param name="binary"></param>
	/// <param name="configuration"></param>
	/// <param name="notification"></param>
	public static void Write(string filename, CadDocument document, bool binary = false, DxfWriterConfiguration configuration = null, NotificationEventHandler notification = null)
	{
		Write(File.Create(filename), document, binary, configuration, notification);
	}

	/// <summary>
	/// Write a <see cref="CadDocument"/> into a <see cref="Stream"/>.
	/// </summary>
	/// <param name="stream"></param>
	/// <param name="document"></param>
	/// <param name="binary"></param>
	/// <param name="configuration"></param>
	/// <param name="notification"></param>
	public static void Write(Stream stream, CadDocument document, bool binary = false, DxfWriterConfiguration configuration = null, NotificationEventHandler notification = null)
	{
		using (DxfWriter writer = new DxfWriter(stream, document, binary))
		{
			if (configuration != null)
			{
				writer.Configuration = configuration;
			}

			writer.OnNotification += notification;
			writer.Write();
		}
	}

	/// <inheritdoc/>
	public override void Dispose()
	{
		this._writer.Dispose();
	}

	/// <inheritdoc/>
	public override void Write()
	{
		base.Write();

		this.createStreamWriter();

		this._objectHolder.Objects.Enqueue(this._document.RootDictionary);

		this.writeHeader();

		this.writeDxfClasses();

		this.writeTables();

		this.writeBlocks();

		this.writeEntities();

		this.writeObjects();

		this.writeACDSData();

		this._writer.Write(DxfCode.Start, DxfFileToken.EndOfFile);

		this._writer.Flush();

		if (this.Configuration.CloseStream)
		{
			this._writer.Close();
		}
	}

	private void createStreamWriter()
	{
		this._encoding ??= this.getListedEncoding(this._document.Header.CodePage);

		if (this.IsBinary)
		{
			this._writer = new DxfBinaryWriter(new BinaryWriter(this._stream, this._encoding));
		}
		else
		{
			this._writer = new DxfAsciiWriter(new StreamWriter(this._stream, this._encoding));
		}

		this._writer.WriteOptional = this.Configuration.WriteOptionalValues;
	}

	private void writeACDSData()
	{
	}

	private void writeBlocks()
	{
		var writer = new DxfBlocksSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}

	private void writeDxfClasses()
	{
		var writer = new DxfClassesSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}

	private void writeEntities()
	{
		var writer = new DxfEntitiesSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}

	private void writeHeader()
	{
		var writer = new DxfHeaderSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}

	private void writeObjects()
	{
		var writer = new DxfObjectsSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}

	private void writeTables()
	{
		var writer = new DxfTablesSectionWriter(this._writer, this._document, this._objectHolder, this.Configuration);
		writer.OnNotification += this.triggerNotification;

		writer.Write();
	}
}