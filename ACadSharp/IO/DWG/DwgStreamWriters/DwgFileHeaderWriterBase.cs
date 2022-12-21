using CSUtilities.Converters;
using CSUtilities.Text;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal abstract class DwgFileHeaderWriterBase : IDwgFileHeaderWriter
	{
		public abstract int HandleSectionOffset { get; }

		protected abstract int _fileHeaderSize { get; }

		protected DwgFileHeader _fileHeader { get; }

		protected ACadVersion _version;

		protected Encoding _encoding;

		protected Stream _stream;

		protected CadDocument _document;

		public DwgFileHeaderWriterBase(Stream stream, CadDocument model)
		{
			if (!stream.CanSeek || !stream.CanWrite)
			{
				throw new ArgumentException();
			}

			this._document = model;
			this._stream = stream;
			this._version = model.Header.Version;
			this._encoding = TextEncoding.Windows1252();
		}

		public abstract void AddSection(string name, MemoryStream stream, bool isCompressed, int decompsize = 0x7400);

		public abstract void WriteFile();

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

		protected void writeMagicNumber()
		{
			for (int i = 0; i < (int)(this._stream.Position % 0x20); i++)
			{
				this._stream.WriteByte(DwgCheckSumCalculator.MagicSequence[i]);
			}
		}

		protected void applyMagicSequence(MemoryStream stream)
		{
			byte[] buffer = stream.GetBuffer();
			for (int i = 0; i < (int)stream.Length; i++)
			{
				buffer[i] ^= DwgCheckSumCalculator.MagicSequence[i];
			}
		}
	}
}
