using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG
{
	internal class DwgHeaderWriter : DwgSectionIO
	{
		private IDwgStreamWriter _writer;

		public DwgHeaderWriter(ACadVersion version) : base(version)
		{
		}

		public void Write()
		{

		}
	}
}
