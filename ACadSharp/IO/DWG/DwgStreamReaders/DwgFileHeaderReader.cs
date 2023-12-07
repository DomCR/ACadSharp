using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG.DwgStreamReaders
{
	internal class DwgFileHeaderReader : DwgSectionIO
	{
		public override string SectionName { get { return string.Empty; } }

		public DwgFileHeaderReader(ACadVersion version) : base(version)
		{
		}
	}
}
