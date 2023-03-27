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
			this._reader =  reader;
			this._sreader = new StreamIO(reader.Stream);

			if(version < ACadVersion.AC1021)
			{
				_readStringMethod = this.readUnicodeString;
			}
			else
			{
				_readStringMethod = this._reader.ReadTextUnicode;
			}
		}

		public CadSummaryInfo Read()
		{
			CadSummaryInfo summary = new CadSummaryInfo();

			//This section contains summary information about the drawing. 
			//Strings are encoded as a 16-bit length, followed by the character bytes (0-terminated).

			//String	2 + n	Title
			summary.Title = _readStringMethod();
			//String	2 + n	Subject
			summary.Subject = _readStringMethod();
			//String	2 + n	Author
			summary.Author = _readStringMethod();
			//String	2 + n	Keywords
			summary.Keywords = _readStringMethod();
			//String	2 + n	Comments
			summary.Comments = _readStringMethod();
			//String	2 + n	LastSavedBy
			summary.LastSavedBy = _readStringMethod();
			//String	2 + n	RevisionNumber
			summary.RevisionNumber = _readStringMethod();
			//String	2 + n	RevisionNumber
			summary.HyperlinkBase = _readStringMethod();

			//?	8	Total editing time(ODA writes two zero Int32’s)
			_reader.ReadInt();
			_reader.ReadInt();

			//Julian date	8	Create date time
			summary.CreatedDate = _reader.Read8BitJulianDate();

			//Julian date	8	Modified date timez
			summary.ModifiedDate = _reader.Read8BitJulianDate();

			//Int16	2 + 2 * (2 + n)	Property count, followed by PropertyCount key/value string pairs.
			short nproperties = _reader.ReadShort();
			for (int i = 0; i < nproperties; i++)
			{
				string propName = _readStringMethod();
				string propValue = _readStringMethod();

				//Add the property
				summary.Properties.Add(propName, propValue);
			}

			//Int32	4	Unknown(write 0)
			_reader.ReadInt();
			//Int32	4	Unknown(write 0)
			_reader.ReadInt();

			return summary;
		}

		private string readUnicodeString()
		{
			short textLength = _sreader.ReadShort<LittleEndianConverter>();
			string value;
			if (textLength == 0)
			{
				value = string.Empty;
			}
			else
			{
				//Read the string and get rid of the empty bytes
				value = _sreader.ReadString(textLength,
					TextEncoding.GetListedEncoding(CodePage.Windows1252))
					.Replace("\0", "");
			}

			return value;
		}
	}
}
