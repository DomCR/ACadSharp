using System.IO;

namespace ACadSharp.IO.DWG
{
	internal interface ICompressor
	{
		void Compress(byte[] source, int offset, int totalSize, Stream dest);
	}
}
