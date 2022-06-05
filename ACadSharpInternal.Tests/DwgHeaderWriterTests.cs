using ACadSharp;
using ACadSharp.IO.DWG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ACadSharpInternal.Tests
{
	public class DwgHeaderWriterTests
	{
		[Fact()]
		public void WriteTest()
		{
			Stream stream = new MemoryStream();
			CadDocument document = new CadDocument();
			document.Header.Version = ACadVersion.AC1018;

			DwgHeaderWriter writer = new DwgHeaderWriter(stream, document);
			writer.Write();

			DwgHeaderReader reader = new DwgHeaderReader(ACadVersion.AC1018);
			IDwgStreamReader sreader = DwgStreamReader.GetStreamHandler(ACadVersion.AC1018, stream, true);
			reader.Read(sreader, 0, out _);
		}
	}
}
