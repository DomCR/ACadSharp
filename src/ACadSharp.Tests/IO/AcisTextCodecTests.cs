using ACadSharp.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO;

public class AcisTextCodecTests
{
	[Fact]
	public void DecodeIsAnInvolution()
	{
		string text = "400 0 1 0\nbody $-1 $1 $-1 $-1 #";

		string swapped = AcisTextCodec.Decode(text);

		Assert.NotEqual(text, swapped);
		Assert.Equal(text, AcisTextCodec.Decode(swapped));
	}

	[Fact]
	public void DecodeBytesMatchesTextDecode()
	{
		string text = "body $-1 $1 $-1 $-1 #";
		byte[] swapped = Encoding.ASCII.GetBytes(AcisTextCodec.Decode(text));

		byte[] decoded = AcisTextCodec.Decode(swapped);

		Assert.Equal(text, Encoding.ASCII.GetString(decoded));
	}

	[Fact]
	public void IsEncodedDetectsPlainAndSwappedSat()
	{
		string plain = "400 0 1 0";

		Assert.False(AcisTextCodec.IsEncoded(plain));
		Assert.True(AcisTextCodec.IsEncoded(AcisTextCodec.Decode(plain)));
	}

	[Theory]
	[InlineData("End-of-ACIS-data")]
	[InlineData("End-of-ASM-data")]
	public void TrimAtAcisEndCutsAfterAsciiMarker(string marker)
	{
		byte[] payload = Encoding.ASCII.GetBytes("some sat content " + marker);
		byte[] trailing = payload.Concat(new byte[] { 0x00, 0x11, 0x22, 0x33 }).ToArray();

		byte[] trimmed = AcisTextCodec.TrimAtAcisEnd(trailing);

		Assert.Equal(payload.Length, trimmed.Length);
		Assert.EndsWith(marker, Encoding.ASCII.GetString(trimmed));
	}

	[Fact]
	public void TrimAtAcisEndCutsAfterCompoundSabMarker()
	{
		byte[] compound = new byte[]
		{
			0x0E, 0x03, (byte)'E', (byte)'n', (byte)'d',
			0x0E, 0x02, (byte)'o', (byte)'f',
			0x0E, 0x04, (byte)'A', (byte)'C', (byte)'I', (byte)'S',
			0x0D, 0x04, (byte)'d', (byte)'a', (byte)'t', (byte)'a'
		};
		byte[] payload = Encoding.ASCII.GetBytes("ACIS BinaryFile fake content ").Concat(compound).ToArray();
		byte[] trailing = payload.Concat(new byte[] { 0xAA, 0xBB }).ToArray();

		byte[] trimmed = AcisTextCodec.TrimAtAcisEnd(trailing);

		Assert.Equal(payload.Length, trimmed.Length);
	}

	[Fact]
	public void TrimAtAcisEndKeepsPayloadWithoutMarker()
	{
		byte[] payload = Encoding.ASCII.GetBytes("truncated sat content");

		byte[] trimmed = AcisTextCodec.TrimAtAcisEnd(payload);

		Assert.Same(payload, trimmed);
	}
}
