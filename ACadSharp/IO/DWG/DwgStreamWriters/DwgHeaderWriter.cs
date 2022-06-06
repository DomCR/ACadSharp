using ACadSharp.Header;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderWriter : DwgSectionIO
	{
		private MemoryStream _msbegin;
		private MemoryStream _msmain;

		private IDwgStreamWriter _swbegin;
		private IDwgStreamWriter _writer;

		private Stream _stream;

		private CadDocument _document;
		private CadHeader _header;

		public DwgHeaderWriter(Stream stream, CadDocument document) : base(document.Header.Version)
		{
			this._stream = stream;
			this._document = document;
			this._header = document.Header;

			this._msbegin = new MemoryStream();
			this._msmain = new MemoryStream();

			this._swbegin = DwgStreamWriter.GetStreamHandler(document.Header.Version, this._msbegin, Encoding.Default);
			this._writer = DwgStreamWriter.GetStreamHandler(document.Header.Version, _msmain, Encoding.Default);
		}

		public void Write()
		{
			//Common:
			//BD : Unknown, default value 412148564080.0
			this._writer.WriteBitDouble(412148564080.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);
			//BD: Unknown, default value 1.0
			this._writer.WriteBitDouble(1.0);

			//TV: Unknown text string, default "m"
			this._writer.WriteVariableText("m");
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);
			//TV: Unknown text string, default ""
			this._writer.WriteVariableText(string.Empty);

			//BL : Unknown long, default value 24L
			//this._writer.WriteBitLong(0);
			//BL: Unknown long, default value 0L;
			//this._writer.WriteBitLong(0);

			//Write the size and merge the streams
			this.writeSectionBegin();
			this.mergeStreams();
		}

		private void writeSectionBegin()
		{
			//0xCF,0x7B,0x1F,0x23,0xFD,0xDE,0x38,0xA9,0x5F,0x7C,0x68,0xB8,0x4E,0x6D,0x33,0x5F
			_swbegin.WriteBytes(DwgSectionDefinition.StartSentinels[DwgSectionDefinition.Header]);

			//RL : Size of the section.
			_swbegin.WriteRawLong(this._msmain.Length);
		}

		private void mergeStreams()
		{
			this._stream.Write(this._msbegin.GetBuffer(), 0, (int)_msbegin.Length);
			this._stream.Write(this._msmain.GetBuffer(), 0, (int)_msmain.Length);
		}
	}
}
