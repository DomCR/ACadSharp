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

			byte[] codes = new byte[imagespresent];
			long[] sizes = new long[imagespresent];
			for (int i = 0; i < imagespresent; i++)
			{
				//Code RC code indicating what follows
				codes[i] = (byte)this._reader.ReadRawChar();
				//data start RL start of the data
				this._reader.ReadRawLong();
				//data size RL size of the data
				sizes[i] = this._reader.ReadRawLong();
			}

			byte[] header = new byte[0];
			byte[] body = new byte[0];
			DwgPreview.PreviewType previewCode = DwgPreview.PreviewType.Unknown;

			//The data follows the records in the same order
			for (int i = 0; i < imagespresent; i++)
			{
				byte[] data = this._reader.ReadBytes((int)sizes[i]);
				switch (codes[i])
				{
					case 1:
						header = data;
						break;
					default:
						//Keep the first image, a file may store a second lower quality bitmap after it
						if (previewCode == DwgPreview.PreviewType.Unknown)
						{
							previewCode = (DwgPreview.PreviewType)codes[i];
							body = data;
						}
						break;
				}
			}

			//0xE0,0xDA,0x92,0xF8,0x2B,0xc9,0xD7,0xD7,0x62,0xA8,0x35,0xC0,0x62,0xBB,0xEF,0xD4
			byte[] endSentinel = this._reader.ReadSentinel();
			Debug.Assert(DwgSectionIO.CheckSentinel(endSentinel, this._endSentinel));

			return new DwgPreview(previewCode, header, body);
		}
	}
}