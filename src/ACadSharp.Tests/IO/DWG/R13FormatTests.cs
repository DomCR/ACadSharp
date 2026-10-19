using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using CSMath;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class R13FormatTests
{
	[Fact]
	public void NativeFixtureAndFiveRecordHeaderRoundtrip()
	{
		CadDocument document;
		using (var reader = new DwgReader(Path.Combine(TestVariables.SamplesFolder, "legacy", "r13-native.dwg")))
		{
			reader.Configuration.Failsafe = false;
			document = reader.Read();
		}
		Assert.Equal(ACadVersion.AC1012, document.Header.Version);
		Assert.Equal(3, document.Entities.Count);
		Assert.Equal(new XYZ(1, 2, 3), Assert.Single(document.Entities.OfType<Line>()).StartPoint);
		Assert.Equal("R13 sample", Assert.Single(document.Entities.OfType<TextEntity>()).Value);
		using var stream = new MemoryStream();
		DwgWriter.Write(stream, document, new DwgWriterConfiguration { CloseStream = false });
		byte[] bytes = stream.ToArray();
		Assert.Equal("AC1012", Encoding.ASCII.GetString(bytes, 0, 6));
		Assert.Equal(5, BitConverter.ToInt32(bytes, 0x15));
		Assert.Equal((ushort)(CRC8StreamHandler.GetCRCValue(0, bytes, 0, 0x46) ^ 0x3CC4), BitConverter.ToUInt16(bytes, 0x46));
		using var rereader = new DwgReader(new MemoryStream(bytes));
		rereader.Configuration.Failsafe = false;
		var actual = rereader.Read();
		Assert.Equal(3, actual.Entities.Count);
		Assert.Equal(new XYZ(4, 5, 6), Assert.Single(actual.Entities.OfType<Line>()).EndPoint);
		Assert.Equal(3, Assert.Single(actual.Entities.OfType<Circle>()).Radius);
		Assert.Equal("R13 sample", Assert.Single(actual.Entities.OfType<TextEntity>()).Value);
	}
}
