using ACadSharp.IO.DWG;
using ACadSharp.IO.DWG.DwgStreamReaders;
using System.IO;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class ReaderReuseRegressionTests
{
	[Fact]
	public void PresentTextStreamClearsPreviousEmptyFlag()
	{
		// Zero-sized text stream ending at bit 16, followed by an absent stream at bit 24.
		using var stream = new MemoryStream(new byte[] { 0, 0, 0x80, 0 });
		var reader = DwgStreamReaderBase.GetStreamHandler(ACadVersion.AC1024, stream);
		reader.SetPositionByFlag(24);
		Assert.True(reader.IsEmpty);
		reader.SetPositionByFlag(16);
		Assert.False(reader.IsEmpty);
		Assert.Equal(0, reader.PositionInBits());
	}
}
