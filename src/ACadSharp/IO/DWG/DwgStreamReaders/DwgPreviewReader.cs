using System.Diagnostics;

namespace ACadSharp.IO.DWG.DwgStreamReaders
{
	internal class DwgPreviewReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.Preview; } }

		private readonly byte[] _startSentinel = DwgSectionDefinition.StartSentinels[DwgSectionDefinition.Preview];

		private readonly byte[] _endSentinel = DwgSectionDefinition.EndSentinels[DwgSectionDefinition.Preview];

		private readonly IDwgStreamReader _reader;

		private readonly long _previewAddress;

		public DwgPreviewReader(ACadVersion version, IDwgStreamReader reader, long previewAddress) : base(version)
		{
			this._reader = reader;
			this._previewAddress = previewAddress;
		}

		public DwgPreview Read()
		{
			//{0x1F,0x25,0x6D,0x07,0xD4,0x36,0x28,0x28,0x9D,0x57,0xCA,0x3F,0x9D,0x44,0x10,0x2B }
			byte[] sentinel = this._reader.ReadSentinel();
			Debug.Assert(DwgSectionIO.CheckSentinel(sentinel, this._startSentinel));

			//overall size	RL	overall size of image area
			long overallSize = this._reader.ReadRawLong();

			//images present RC counter indicating what is present here
			byte imagespresent = (byte)this._reader.ReadRawChar();

			long? headerDataStart = null;
			long? headerDataSize = null;
			long? startOfImage = null;
			long? sizeImage = null;

			DwgPreview.PreviewType previewCode = DwgPreview.PreviewType.Unknown;
			for (int i = 0; i < imagespresent; i++)
			{
				//Code RC code indicating what follows
				byte code = (byte)this._reader.ReadRawChar();
				switch (code)
				{
					case 1:
						//header data start RL start of header data
						headerDataStart = this._reader.ReadRawLong();
						headerDataSize = this._reader.ReadRawLong();
						break;
					default:
						previewCode = (DwgPreview.PreviewType)code;
						startOfImage = this._reader.ReadRawLong();
						sizeImage = this._reader.ReadRawLong();
						break;
				}
			}

			byte[] header = null;
			header = this._reader.ReadBytes((int)headerDataSize.Value);

			byte[] body = null;
			if (sizeImage.HasValue)
			{
				body = this._reader.ReadBytes((int)sizeImage);
			}
			else
			{
				body = new byte[0];
			}

			//0xE0,0xDA,0x92,0xF8,0x2B,0xc9,0xD7,0xD7,0x62,0xA8,0x35,0xC0,0x62,0xBB,0xEF,0xD4
			byte[] endSentinel = this._reader.ReadSentinel();
			Debug.Assert(DwgSectionIO.CheckSentinel(endSentinel, this._endSentinel));

			return new DwgPreview(previewCode, header, body);
		}
	}
}