using ACadSharp.IO.DWG.DwgStreamReaders;
using System.IO;

namespace ACadSharp.IO.DWG;

/// <summary>
/// Reader for the drawings older than R13, the data is byte aligned and read with the raw methods.
/// </summary>
internal class DwgStreamReaderAC09 : DwgStreamReaderBase
{
	public DwgStreamReaderAC09(Stream stream, bool resetPosition) : base(stream, resetPosition) { }

	/// <summary>
	/// Read a text stored in a field of fixed size, the text ends at the first null character.
	/// </summary>
	/// <param name="length">Size of the field.</param>
	public string ReadFixedText(int length)
	{
		string text = this.ReadString(length, this.Encoding);

		int end = text.IndexOf('\0');
		if (end >= 0)
		{
			text = text.Substring(0, end);
		}

		return text;
	}

	/// <inheritdoc/>
	/// <remarks>
	/// The text is preceded by its length as a raw short.
	/// </remarks>
	public override string ReadVariableText()
	{
		short length = this.ReadShort();
		if (length <= 0)
		{
			return string.Empty;
		}

		return this.ReadString(length, this.Encoding).Replace("\0", "");
	}
}
