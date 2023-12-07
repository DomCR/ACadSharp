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
	}
}
