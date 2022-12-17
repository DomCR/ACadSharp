using System.IO;

namespace ACadSharp.IO.DWG.DwgStreamWriters
{
	internal class DwgFileHeaderWriterAC21 : DwgFileHeaderWriterAC18
	{
		public DwgFileHeaderWriterAC21(Stream stream, CadDocument model) : base(stream, model)
		{
		}
	}
}
