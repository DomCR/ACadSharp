using CSUtilities.IO;

namespace ACadSharp
{
	public class DwgPreview
	{
		public enum PreviewType
		{
			Unknown = 0,
			Bmp = 2,
			Wmf = 3,
			Png = 6,
		}

		public PreviewType Code { get; }

		public byte[] RawHeader { get; }

		public byte[] RawImage { get; }

		public DwgPreview(PreviewType code, byte[] rawHeader, byte[] rawImage)
		{
			this.Code = code;
			this.RawHeader = rawHeader;
			this.RawImage = rawImage;
		}

		public void Save(string path)
		{
			bool writeHeader;
			switch (this.Code)
			{
				case PreviewType.Bmp:
				case PreviewType.Wmf:
					writeHeader = true;
					break;
				case PreviewType.Png:
					writeHeader = false;
					break;
				case PreviewType.Unknown:
				default:
					throw new System.NotSupportedException();
			}

			using (StreamIO sw = new StreamIO(path))
			{
				if (writeHeader)
				{
					sw.WriteBytes(this.RawHeader);
				}

				sw.WriteBytes(this.RawImage);
			}
		}
	}
}
