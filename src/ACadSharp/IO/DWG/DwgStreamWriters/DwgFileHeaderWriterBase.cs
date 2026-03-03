using CSUtilities.Converters;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal abstract class DwgFileHeaderWriterBase<T> : IDwgFileHeaderWriter
	where T : DwgFileHeader, new()
{
	public T FileHeader { get; } = new T();

	public abstract int FileHeaderSize { get; }

	public abstract int HandleSectionOffset { get; }

	protected CadDocument _document;

	protected Encoding _encoding;

	protected Stream _stream;

	protected ACadVersion _version;

	public DwgFileHeaderWriterBase(Stream stream, Encoding encoding, CadDocument model)
	{
		if (!stream.CanSeek || !stream.CanWrite)
		{
			throw new ArgumentException();
		}

		this._document = model;
		this._stream = stream;
		this._version = model.Header.Version;
		this._encoding = encoding;
	}

	public abstract void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400);

	public abstract void WriteFile();

	protected void applyMagicSequence(MemoryStream stream)
	{
		byte[] buffer = stream.GetBuffer();
		for (int i = 0; i < (int)stream.Length; i++)
		{
			buffer[i] ^= DwgCheckSumCalculator.MagicSequence[i];
		}
	}

	protected void applyMask(byte[] buffer, int offset, int length)
	{
		byte[] bytes = LittleEndianConverter.Instance.GetBytes(0x4164536B ^ (int)this._stream.Position);
		int diff = offset + length;
		while (offset < diff)
		{
			for (int i = 0; i < 4; i++)
			{
				buffer[offset + i] ^= bytes[i];
			}

			offset += 4;
		}
	}

	protected bool checkEmptyBytes(byte[] buffer, ulong offset, ulong spearBytes)
	{
		bool result = true;
		ulong num = offset + spearBytes;

		for (ulong i = offset; i < num; i++)
		{
			if (buffer[i] != 0)
			{
				result = false;
				break;
			}
		}

		return result;
	}

	protected ushort getFileCodePage()
	{
		ushort codePage = (ushort)CadUtils.GetCodeIndex(CadUtils.GetCodePage(_document.Header.CodePage));
		if (codePage < 1)
		{
			return 30;
		}
		else
		{
			return codePage;
		}
	}

	protected void writeMagicNumber()
	{
		for (int i = 0; i < (int)(this._stream.Position % 0x20); i++)
		{
			this._stream.WriteByte(DwgCheckSumCalculator.MagicSequence[i]);
		}
	}
}