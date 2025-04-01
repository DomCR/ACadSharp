using CSUtilities.IO;

namespace ACadSharp
{
	/// <summary>
	/// Stores the thumbnail information to generate the preview for a CadDocument.
	/// </summary>
	public class DwgPreview
	{
		/// <summary>
		/// Type of media stored in the preview.
		/// </summary>
		public enum PreviewType
		{
			Unknown = 0,
			Bmp = 2,
			Wmf = 3,
			Png = 6,
		}

		/// <summary>
		/// Code that specifies the type of media stored in the preview.
		/// </summary>
		public PreviewType Code { get; }

		/// <summary>
		/// Header for the preview section.
		/// </summary>
		/// <remarks>
		/// Usually is formed by an empty array of 80 zeros.
		/// </remarks>
		public byte[] RawHeader { get; }

		/// <summary>
		/// Bytes conforming the thumbnail.
		/// </summary>
		public byte[] RawImage { get; }

		/// <summary>
		/// Default constructor with an unknown media type.
		/// </summary>
		public DwgPreview()
		{
			this.Code = PreviewType.Unknown;
			this.RawHeader = new byte[0];
			this.RawImage = new byte[0];
		}

		/// <summary>
		/// Constructor to define a preview instance.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="rawHeader"></param>
		/// <param name="rawImage"></param>
		public DwgPreview(PreviewType code, byte[] rawHeader, byte[] rawImage)
		{
			this.Code = code;
			this.RawHeader = rawHeader;
			this.RawImage = rawImage;
		}

		/// <summary>
		/// Save the image into a file.
		/// </summary>
		/// <param name="path">Path where the image has to be stored.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		public void Save(string path)
		{
			bool writeHeader;
			switch (this.Code)
			{
				case PreviewType.Bmp:
				case PreviewType.Wmf:
				case PreviewType.Png:
					writeHeader = false;
					break;
				case PreviewType.Unknown:
				default:
					throw new System.NotSupportedException($"Preview with code {this.Code} not supported.");
			}

			using (StreamIO sw = new StreamIO(path, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
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
