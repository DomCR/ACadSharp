using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;
using System.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgSummaryInfoReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.SummaryInfo; } }

		private delegate string readString();

		private readString _readStringMethod;

		private IDwgStreamReader _reader;

		private StreamIO _sreader;

		// [PATCH] Decode encoding for the file's graphics code page (SummaryInfo strings are encoded in the document code page, e.g. ANSI_936->GBK)
		private readonly System.Text.Encoding _encoding;

		public DwgSummaryInfoReader(ACadVersion version, IDwgStreamReader reader, CodePage codePage = CodePage.Windows1252) : base(version)
		{
			this._reader = reader;
			this._sreader = new StreamIO(reader.Stream);
			this._encoding = ResolveEncoding(codePage);

			if (version < ACadVersion.AC1021)
			{
				this._readStringMethod = this.readUnicodeString;
			}
			else
			{
				this._readStringMethod = this._reader.ReadTextUnicode;
			}
		}

		public CadSummaryInfo Read()
		{
			CadSummaryInfo summary = new CadSummaryInfo();

			try
			{
				//This section contains summary information about the drawing. 
				//Strings are encoded as a 16-bit length, followed by the character bytes (0-terminated).

				//String	2 + n	Title
				summary.Title = this._readStringMethod();
				//String	2 + n	Subject
				summary.Subject = this._readStringMethod();
				//String	2 + n	Author
				summary.Author = this._readStringMethod();
				//String	2 + n	Keywords
				summary.Keywords = this._readStringMethod();
				//String	2 + n	Comments
				summary.Comments = this._readStringMethod();
				//String	2 + n	LastSavedBy
				summary.LastSavedBy = this._readStringMethod();
				//String	2 + n	RevisionNumber
				summary.RevisionNumber = this._readStringMethod();
				//String	2 + n	RevisionNumber
				summary.HyperlinkBase = this._readStringMethod();

				//?	8	Total editing time(ODA writes two zero Int32’s)
				this._reader.ReadInt();
				this._reader.ReadInt();

				//Julian date	8	Create date time
				summary.CreatedDate = this._reader.Read8BitJulianDate();

				//Julian date	8	Modified date timez
				summary.ModifiedDate = this._reader.Read8BitJulianDate();

				//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
				short nproperties = this._reader.ReadShort();
				for (int i = 0; i < nproperties; i++)
				{
					string propName = this._readStringMethod();
					string propValue = this._readStringMethod();

					//Add the property
					try
					{
						summary.Properties.Add(propName, propValue);
					}
					catch (System.Exception ex)
					{
						this.notify("[SummaryInfo] An error ocurred while adding a property in the SummaryInfo", NotificationType.Error, ex);
					}
				}

				//Int32	4	Unknown(write 0)
				this._reader.ReadInt();
				//Int32	4	Unknown(write 0)
				this._reader.ReadInt();

			}
			catch (System.Exception ex)
			{
				if (this._reader.Stream.Position != this._reader.Stream.Length)
				{
					this.notify("An error occurred while reading the Summary Info", NotificationType.Error, ex);
				}
			}

			return summary;
		}

		private string readUnicodeString()
		{
			// [PATCH] _sreader and _reader share the same underlying stream:
			// the bit reader has an internal byte buffer - sync the stream position before a direct read, invalidate the buffer afterwards.
			this._reader.SyncStreamPosition();
			short textLength = this._sreader.ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				// [PATCH] Decode with the file code page (the original fixed Windows-1252 read CJK attributes as garbage)
				value = this._sreader.ReadString(textLength, this._encoding)
					.Replace("\0", "");
			}

			this._reader.MarkStreamAdvanced();
			return value;
		}

		// [PATCH] TextEncoding.GetListedEncoding returns null for most enum values (e.g. Gb2312):
		// fall back to CodePagesEncodingProvider with the Windows code page number (same strategy as CadUtils.GetListedEncoding)
		private static Encoding ResolveEncoding(CodePage codePage)
		{
			var e = TextEncoding.GetListedEncoding(codePage);
			if (e != null) return e;
			try
			{
#if !NET48
				Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
				return Encoding.GetEncoding((int)codePage);
			}
			catch
			{
				return TextEncoding.Windows1252();
			}
		}
	}
}
