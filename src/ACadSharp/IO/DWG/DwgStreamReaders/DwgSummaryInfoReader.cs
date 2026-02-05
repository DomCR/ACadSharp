using CSUtilities.Converters;
using CSUtilities.IO;
using CSUtilities.Text;

namespace ACadSharp.IO.DWG
{
	internal class DwgSummaryInfoReader : DwgSectionIO
	{
		public override string SectionName { get { return DwgSectionDefinition.SummaryInfo; } }

		private delegate string readString();

		private readString _readStringMethod;

		private IDwgStreamReader _reader;

		private StreamIO _sreader;

		public DwgSummaryInfoReader(ACadVersion version, IDwgStreamReader reader) : base(version)
		{
			this._reader = reader;
			this._sreader = new StreamIO(reader.Stream);

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
			short textLength = this._sreader.ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				value = this._sreader.ReadString(textLength,
					TextEncoding.GetListedEncoding(CodePage.Windows1252))
					.Replace("\0", "");
			}

			return value;
		}
	}
}
