using CSUtilities.IO;
using CSUtilities.Text;
using System.IO;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgPreviewWriter : DwgSectionIO
	{
		public override string SectionName => DwgSectionDefinition.Preview;

		private IDwgStreamWriter _swriter;

		private readonly byte[] _startSentinel = DwgSectionDefinition.StartSentinels[DwgSectionDefinition.Preview];

		private readonly byte[] _endSentinel = DwgSectionDefinition.EndSentinels[DwgSectionDefinition.Preview];

		public DwgPreviewWriter(ACadVersion version, Stream stream) : base(version)
		{
			this._swriter = DwgStreamWriterBase.GetStreamWriter(version, stream, TextEncoding.Windows1252());
		}

		public void Write()
		{
			this._swriter.WriteBytes(this._startSentinel);
			//overall size	RL	overall size of image area
			this._swriter.WriteRawLong(1);
			//images present RC counter indicating what is present here
			this._swriter.WriteByte(0);
			this._swriter.WriteBytes(this._endSentinel);
		}

		public void Write(DwgPreview preview, long startPos)
		{
			var size = preview.RawHeader.Length + preview.RawImage.Length + 19;

			this._swriter.WriteBytes(this._startSentinel);

			//overall size	RL	overall size of image area
			this._swriter.WriteRawLong(size);

			//images present RC counter indicating what is present here
			this._swriter.WriteByte(2);

			//Code RC code indicating what follows
			this._swriter.WriteByte(1);

			var headerOffset = startPos + this._swriter.Stream.Position + 12 + 5 + 32;
			//header data start RL start of header data
			this._swriter.WriteRawLong(headerOffset);

			//header data size RL size of header data
			this._swriter.WriteRawLong(preview.RawHeader.Length);

			//Code RC code indicating what follows
			this._swriter.WriteByte((byte)preview.Code);

			var imageOffset = headerOffset + preview.RawHeader.Length;
			//image data start RL start of image data
			this._swriter.WriteRawLong(imageOffset);

			//image data size RL size of image data
			this._swriter.WriteRawLong(preview.RawImage.Length);

			this._swriter.WriteBytes(preview.RawHeader);

			this._swriter.WriteBytes(preview.RawImage);

			this._swriter.WriteBytes(this._endSentinel);
		}
	}
}
