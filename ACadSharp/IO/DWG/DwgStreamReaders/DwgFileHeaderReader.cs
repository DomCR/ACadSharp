using ACadSharp.Exceptions;
using CSUtilities.IO;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgFileHeaderReader : DwgSectionIO
	{
		public override string SectionName { get { return string.Empty; } }

		private StreamIO _stream;

		public DwgFileHeaderReader(ACadVersion version, Stream stream) : base(version)
		{
			this._stream = new StreamIO(stream);
		}

		public async Task<DwgFileHeader> ReadAsync(CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}

		public DwgFileHeader Read()
		{
			DwgFileHeader fileHeader = DwgFileHeader.CreateFileHeader(this._version);

			//Reset the stream position at the begining
			this._stream.Position = 0L;

			//Read the file header
			switch (fileHeader.AcadVersion)
			{
				case ACadVersion.Unknown:
					throw new DwgNotSupportedException();
				case ACadVersion.MC0_0:
				case ACadVersion.AC1_2:
				case ACadVersion.AC1_4:
				case ACadVersion.AC1_50:
				case ACadVersion.AC2_10:
				case ACadVersion.AC1002:
				case ACadVersion.AC1003:
				case ACadVersion.AC1004:
				case ACadVersion.AC1006:
				case ACadVersion.AC1009:
					throw new DwgNotSupportedException(this._version);
				case ACadVersion.AC1012:
				case ACadVersion.AC1014:
				case ACadVersion.AC1015:
					this.readFileHeaderAC15(fileHeader as DwgFileHeaderAC15, _stream);
					break;
				case ACadVersion.AC1018:
					this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, _stream);
					break;
				case ACadVersion.AC1021:
					this.readFileHeaderAC21(fileHeader as DwgFileHeaderAC21, _stream);
					break;
				case ACadVersion.AC1024:
				case ACadVersion.AC1027:
				case ACadVersion.AC1032:
					//Check if it works...
					this.readFileHeaderAC18(fileHeader as DwgFileHeaderAC18, _stream);
					break;
				default:
					break;
			}

			return fileHeader;
		}
	}
}
