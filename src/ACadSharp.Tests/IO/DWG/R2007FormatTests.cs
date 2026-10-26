using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using CSMath;
using System.IO;
using System.Linq;
using Xunit;

namespace ACadSharp.Tests.IO.DWG;

public class R2007FormatTests
{
	[Fact]
	public void ReproducesNativeHeaderParityAndCrcSeeds()
	{
		byte[] native = File.ReadAllBytes(Path.Combine(TestVariables.SamplesFolder, "sample_AC1021.dwg")).Skip(0x80).Take(0x400).ToArray();
		byte[] decoded = Enumerable.Range(0, 717).Select(i => native[(i % 239) * 3 + i / 239]).ToArray();
		Assert.Equal(native.Take(765), DwgR2007Encoding.ReedSolomon(decoded, 239));
		int size = System.BitConverter.ToInt32(decoded, 24);
		byte[] payload = new byte[0x110];
		new DwgLZ77AC21Decompressor().Decompress(decoded, 32, (uint)size, payload);
		Assert.Equal(System.BitConverter.ToUInt64(decoded, 16), DwgR2007Encoding.Crc(decoded.Skip(32).Take(size).ToArray(), DwgR2007Encoding.Seed(size, true)));
		ulong expected = System.BitConverter.ToUInt64(payload, 0x108);
		System.Array.Clear(payload, 0x108, 8);
		Assert.Equal(expected, DwgR2007Encoding.Crc(payload, DwgR2007Encoding.Seed(payload.Length, true)));
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2500)]
	public void GeometrySurvivesSingleAndMultiplePages(int count)
	{
		var doc = new CadDocument(); doc.Header.Version = ACadVersion.AC1021;
		for (int i = 0; i < count; i++) doc.Entities.Add(new Line(new XYZ(i, 2, 3), new XYZ(i + 4, 5, 6)));
		doc.Entities.Add(new MText { Value = "Unicode \u03b1\u00e4", InsertPoint = new XYZ(2, 3, 4) });
		var result = ModelerGeometryWriterTests.Roundtrip(doc);
		Assert.Equal(ACadVersion.AC1021, result.Header.Version);
		Assert.Equal(count + 1, result.Entities.Count);
		Assert.Equal(new XYZ(count + 3, 5, 6), result.Entities.OfType<Line>().Last().EndPoint);
		Assert.Equal("Unicode \u03b1\u00e4", Assert.Single(result.Entities.OfType<MText>()).Value);
	}
}
