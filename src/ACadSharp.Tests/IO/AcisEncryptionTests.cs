using ACadSharp.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace ACadSharp.Tests.IO;

public class AcisEncryptionTests
{
	[Fact]
	public void DecodeBytesMatchesTextDecode()
	{
		string text = "body $-1 $1 $-1 $-1 #";
		byte[] swapped = Encoding.ASCII.GetBytes(AcisEncryption.Decode(text));

		byte[] decoded = AcisEncryption.Decode(swapped);

		Assert.Equal(text, Encoding.ASCII.GetString(decoded));
	}

	[Fact]
	public void DecodeIsAnInvolution()
	{
		string text = "400 0 1 0\nbody $-1 $1 $-1 $-1 #";

		string swapped = AcisEncryption.Encode(text);

		Assert.NotEqual(text, swapped);
		Assert.Equal(text, AcisEncryption.Decode(swapped));
	}

	[Fact]
	public void EncodeBytesOnlyMirrorsPrintableAscii()
	{
		byte[] plain = new byte[] { (byte)'1', (byte)'~', (byte)'\t', (byte)' ', 0x01, 0x7F, 0x80, 0x9E, 0xFF };

		byte[] encoded = AcisEncryption.Encode(plain);

		Assert.Equal(new byte[] { 0x9F - (byte)'1', (byte)'!', (byte)'\t', (byte)' ', 0x01, 0x7F, 0x80, 0x9E, 0xFF }, encoded);
	}

	[Fact]
	public void EncodeDecodeBytesKeepNullAndEmptyInputs()
	{
		byte[] empty = new byte[0];

		Assert.Null(AcisEncryption.Encode((byte[])null));
		Assert.Null(AcisEncryption.Decode((byte[])null));
		Assert.Same(empty, AcisEncryption.Encode(empty));
		Assert.Same(empty, AcisEncryption.Decode(empty));
	}

	[Fact]
	public void EncodeDecodeBytesMatchTextVersions()
	{
		string plain = "400 0 1 0\nbody $-1 $1 $-1 $-1 # ^~{}";

		byte[] encoded = AcisEncryption.Encode(Encoding.ASCII.GetBytes(plain));

		Assert.Equal(AcisEncryption.Encode(plain), Encoding.ASCII.GetString(encoded));
		Assert.Equal(
			AcisEncryption.Decode(AcisEncryption.Encode(plain)),
			Encoding.ASCII.GetString(AcisEncryption.Decode(encoded)));
	}

	[Fact]
	public void EncodeDecodeBytesRoundTrip()
	{
		byte[] plain = Encoding.ASCII.GetBytes("400 0 1 0\r\nbody $-1 $1 $-1 $-1 #");

		byte[] encoded = AcisEncryption.Encode(plain);

		Assert.NotEqual(plain, encoded);
		Assert.True(AcisEncryption.IsEncoded(encoded));
		Assert.Equal(plain, AcisEncryption.Decode(encoded));
	}

	[Fact]
	public void EncodeDecodeBytesRoundTripDataNotStartingWithDigit()
	{
		byte[] plain = Encoding.ASCII.GetBytes("fo body $-1 #");

		Assert.Equal(plain, AcisEncryption.Decode(AcisEncryption.Encode(plain)));
	}

	[Fact]
	public void EncodeDecodeBytesRoundTripEveryByte()
	{
		byte[] all = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

		Assert.Equal(all, AcisEncryption.Decode(AcisEncryption.Encode(all)));
		Assert.Equal(all, AcisEncryption.Encode(AcisEncryption.Decode(all)));
	}

	[Fact]
	public void IsEncodedBytesDetectsPlainAndEncodedSat()
	{
		byte[] plain = Encoding.ASCII.GetBytes("400 0 1 0");

		Assert.False(AcisEncryption.IsEncoded(plain));
		Assert.True(AcisEncryption.IsEncoded(AcisEncryption.Encode(plain)));
	}

	[Fact]
	public void IsEncodedDetectsPlainAndSwappedSat()
	{
		string plain = "400 0 1 0";

		Assert.False(AcisEncryption.IsEncoded(plain));
		Assert.True(AcisEncryption.IsEncoded(AcisEncryption.Encode(plain)));
	}

	[Theory]
	[InlineData("End-of-ACIS-data")]
	[InlineData("End-of-ASM-data")]
	public void TrimAtAcisEndCutsAfterAsciiMarker(string marker)
	{
		byte[] payload = Encoding.ASCII.GetBytes("some sat content " + marker);
		byte[] trailing = payload.Concat(new byte[] { 0x00, 0x11, 0x22, 0x33 }).ToArray();

		byte[] trimmed = AcisEncryption.TrimAtAcisEnd(trailing);

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

		byte[] trimmed = AcisEncryption.TrimAtAcisEnd(trailing);

		Assert.Equal(payload.Length, trimmed.Length);
	}

	[Fact]
	public void TrimAtAcisEndKeepsPayloadWithoutMarker()
	{
		byte[] payload = Encoding.ASCII.GetBytes("truncated sat content");

		byte[] trimmed = AcisEncryption.TrimAtAcisEnd(payload);

		Assert.Same(payload, trimmed);
	}
}