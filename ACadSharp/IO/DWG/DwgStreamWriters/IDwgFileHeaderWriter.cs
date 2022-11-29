using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal interface IDwgFileHeaderWriter
	{
		void Init();

		void CreateSection(string name, System.IO.MemoryStream stream, bool isCompressed, int decompsize = 0x7400);

		void WriteFile();
	}
}
