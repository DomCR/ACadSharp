using System;
using System.IO;

namespace ACadSharp.IO.DWG;

// Variation of the algorithm LZ77 used in 2004 DWG files.
// [PATCH] Array-direct rewrite: the original issued a Stream.Seek/Read/Write virtual call per back-copy opcode;
// overlapping back-copies (common in CAD data: long repeated runs + small offsets) degenerated into thousands of small Writes;
// the 822 MB object section took a measured 20 s (cold) / 4.6 s (hot) to decompress - only 40.8 MB/s.
// New implementation: the compressed data is block-read into a byte[] on demand and the whole decode is array-based
// (Buffer.BlockCopy + byte-by-byte overlap loops); back-copies have no Stream abstraction.
// Stream-position semantics are unchanged: after decompression the stream position = the bytes actually consumed.
internal static class DwgLZ77AC18Decompressor
{
	private static readonly int MinOpcode = 0x11; // 0x11 : Terminates the input stream.

	/// <summary>Compressed size unknown: consume up to the 0x11 terminator, then seek the stream back to the exact position.</summary>
	public static Stream Decompress(Stream compressed, long decompressedSize)
	{
		long start = compressed.CanSeek ? compressed.Position : -1;
		SrcBuf sb = new SrcBuf(compressed);
		byte[] dst = new byte[Math.Max(1, (int)decompressedSize)];
		int written = DecompressInto(sb, dst);
		if (start >= 0)
			compressed.Position = start + sb.Consumed; // exact restore: advance only the bytes actually consumed

		Stream memoryStream = new MemoryStream(dst, 0, written, writable: false, publiclyVisible: false);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	/// <summary>Compressed size known: exact read, no seek-back.</summary>
	public static Stream Decompress(Stream compressed, long compressedSize, long decompressedSize)
	{
		byte[] src = ReadExactly(compressed, compressedSize);
		byte[] dst = new byte[Math.Max(1, (int)decompressedSize)];
		DecompressCore(src, dst);
		Stream memoryStream = new MemoryStream(dst, 0, dst.Length, writable: false, publiclyVisible: false);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	/// <summary>
	/// Decompress <paramref name="src"/> (the compressed page data) directly into <paramref name="dst"/>.
	/// Keeps the original 2-argument signature (growing work buffer).
	/// </summary>
	public static void DecompressToDest(Stream src, Stream dst)
	{
		long start = src.CanSeek ? src.Position : -1;
		SrcBuf sb = new SrcBuf(src);
		byte[] buf = new byte[Math.Max(4096, 65536)];
		while (true)
		{
			int n = DecompressInto(sb, buf);
			if (n >= 0)
			{
				if (start >= 0)
					src.Position = start + sb.Consumed;
				dst.Write(buf, 0, n);
				return;
			}
			buf = new byte[buf.Length * 2 + 4096];
			sb.Reset();
		}
	}

	/// <summary>
	/// [PATCH] Fast path with the exact decompressed size: the work buffer is allocated once.
	/// </summary>
	public static void DecompressToDest(Stream src, long decompressedSize, Stream dst)
	{
		long start = src.CanSeek ? src.Position : -1;
		SrcBuf sb = new SrcBuf(src);
		byte[] buf = new byte[Math.Max(1, (int)decompressedSize)];
		int written = DecompressInto(sb, buf);
		if (start >= 0)
			src.Position = start + sb.Consumed;
		dst.Write(buf, 0, written);
	}

	/// <summary>
	/// [PATCH] Both sizes known: exact read + single allocation - the fastest path (used by the object-section page loop).
	/// If the real output exceeds <paramref name="decompressedSize"/>, the buffer grows and the attempt is repeated.
	/// </summary>
	public static void DecompressToDest(Stream src, long compressedSize, long decompressedSize, Stream dst)
	{
		byte[] comp = ReadExactly(src, compressedSize);
		SrcBuf sb = new SrcBuf(comp);
		byte[] buf = new byte[Math.Max(1, (int)decompressedSize)];
		while (true)
		{
			int n = DecompressInto(sb, buf);
			if (n >= 0)
			{
				dst.Write(buf, 0, n);
				return;
			}
			sb.Reset();
			buf = new byte[buf.Length * 2 + 4096];
		}
	}

	/// <summary>
	/// [PATCH] Parallel page decompression: reads the compressed page data directly from <paramref name="src"/> at <paramref name="fileOffset"/>,
	/// decompresses up to the 0x11 terminator and returns (output array, actual byte count).
	/// For the page-level parallelism in getSectionBuffer18: each page has its own file offset and a disjoint destination region, no shared stream.
	/// If a page output exceeds 0x7400 (a full page), the buffer grows and the attempt is repeated (same semantics as the sequential path).
	/// </summary>
	public static (byte[] data, int written) DecompressPageToBuffer(Stream src, long fileOffset)
	{
		src.Position = fileOffset;
		SrcBuf sb = new SrcBuf(src);
		byte[] buf = new byte[0x7400]; // full page 29696 B (AC1018 page size)
		while (true)
		{
			int n = DecompressInto(sb, buf);
			if (n >= 0)
				return (buf, n);
			sb.Reset();
			buf = new byte[buf.Length * 2 + 4096];
		}
	}

	private static void DecompressCore(byte[] src, byte[] dst)
	{
		SrcBuf sb = new SrcBuf(src);
		int n = DecompressInto(sb, dst);
		if (n < 0)
			throw new InvalidDataException("LZ77: decompressed size exceeds buffer");
	}

	/// <summary>Compressed source filled in blocks on demand (the stream position advances only by the bytes actually consumed).</summary>
	private sealed class SrcBuf
	{
		private readonly Stream _s;
		private readonly long _start;
		public byte[] Data = new byte[65536];
		public int Len;
		public int Consumed;

		public SrcBuf(Stream s)
		{
			_s = s;
			_start = s.CanSeek ? s.Position : -1;
		}

		public SrcBuf(byte[] data)
		{
			_s = null;
			_start = -1;
			Data = data;
			Len = data.Length;
		}

		public void Reset()
		{
			Consumed = 0;
			if (_s != null)
			{
				Len = 0;
				if (_start >= 0)
					_s.Position = _start; // grow-and-retry: back to the start position of this call
			}
			else
			{
				Len = Data.Length; // array source: all data is present, just restore the length
			}
		}

		public void Ensure(int need)
		{
			while (Len < need)
			{
				if (_s == null)
					throw new EndOfStreamException();
				if (Len == Data.Length)
				{
					byte[] nb = new byte[Data.Length * 2];
					Buffer.BlockCopy(Data, 0, nb, 0, Data.Length);
					Data = nb;
				}
				int r = _s.Read(Data, Len, Data.Length - Len);
				if (r <= 0)
					throw new EndOfStreamException();
				Len += r;
			}
		}
	}

	/// <summary>
	/// Decompress into dst; returns -1 when the buffer is too small (the caller grows and retries).
	/// </summary>
	private static int DecompressInto(SrcBuf sb, byte[] dst)
	{
		int si = 0, di = 0;
		byte[] data = sb.Data;
		int srcLen = sb.Len;

		int ReadB()
		{
			if (si + 1 > srcLen)
			{
				sb.Ensure(si + 1);
				data = sb.Data;
				srcLen = sb.Len;
			}
			return data[si++];
		}

		int CopyLits(int count)
		{
			if (count < 0)
				throw new InvalidDataException($"LZ77: literal run out of range ({count})");
			if (si + count > srcLen)
			{
				sb.Ensure(si + count);
				data = sb.Data;
				srcLen = sb.Len;
			}
			if (di + count > dst.Length)
				return -1; // buffer too small
			Buffer.BlockCopy(data, si, dst, di, count);
			si += count;
			di += count;
			return ReadB();
		}

		void BackCopy(int offset, int len)
		{
			if (offset <= 0)
				throw new InvalidDataException($"LZ77: bad back reference offset ({offset})");
			if (di + len > dst.Length)
			{
#if DEBUG
				// diagnostics: dump the compressed stream head to tell data misalignment from a parse error
				int dump = Math.Min(64, srcLen);
				var hex = new System.Text.StringBuilder();
				for (int i = 0; i < dump; i++)
					hex.Append(data[i].ToString("x2"));
				Console.Error.WriteLine($"[LZ77-DIAG] dstLen={dst.Length} si={si} di={di} offset={offset} len={len} srcHead={hex}");
#endif
				throw new InvalidDataException($"LZ77: back reference out of range (len {len} at {di})");
			}
			int first = Math.Min(len, offset);
			Buffer.BlockCopy(dst, di - offset, dst, di, first);
			int end = di + len;
			for (int i = di + first; i < end; i++)
			{
				dst[i] = dst[i - offset];
			}
			di = end;
		}

		// One-to-one correspondence with the original implementation:
		int opcode1 = ReadB();
		if (opcode1 > MinOpcode)
		{
			opcode1 = CopyLits(opcode1 - 17);
			if (opcode1 < 0)
			{
				sb.Consumed = si;
				return -1;
			}
		}

		if ((opcode1 & 0xF0) == 0)
		{
			opcode1 = CopyLits(LiteralCount(opcode1, sb, ref si, ref srcLen, ref data) + 3);
			if (opcode1 < 0)
			{
				sb.Consumed = si;
				return -1;
			}
		}

		while (opcode1 != MinOpcode)
		{
			int compOffset = 0;
			int compressedBytes = 0;

			if (opcode1 < 0x10 || opcode1 >= 0x40)
			{
				compressedBytes = (opcode1 >> 4) - 1;
				byte opcode2 = (byte)ReadB();
				compOffset = ((opcode1 >> 2 & 3) | (opcode2 << 2)) + 1;
			}
			//0x12 – 0x1F
			else if (opcode1 < 0x20)
			{
				compressedBytes = ReadCompressedBytes(opcode1, 0b0111, sb, ref si, ref srcLen, ref data);
				compOffset = (opcode1 & 8) << 11;
				opcode1 = TwoByteOffset(ref compOffset, 0x4000, sb, ref si, ref srcLen, ref data);
			}
			//0x20
			else if (opcode1 >= 0x20)
			{
				compressedBytes = ReadCompressedBytes(opcode1, 0b00011111, sb, ref si, ref srcLen, ref data);
				opcode1 = TwoByteOffset(ref compOffset, 1, sb, ref si, ref srcLen, ref data);
			}

			BackCopy(compOffset, compressedBytes);

			//Number of uncompressed or literal bytes to be copied
			int litCount = opcode1 & 3;
			//0x00 : litCount is read as the next Literal Length
			if (litCount == 0)
			{
				opcode1 = ReadB();
				if ((opcode1 & 0b11110000) == 0)
				{
					litCount = LiteralCount(opcode1, sb, ref si, ref srcLen, ref data) + 3;
				}
			}

			//Copy as literal
			if (litCount > 0U)
			{
				opcode1 = CopyLits(litCount);
				if (opcode1 < 0)
				{
					sb.Consumed = si;
					return -1;
				}
			}
		}

		sb.Consumed = si;
		return di;
	}

	private static int LiteralCount(int code, SrcBuf sb, ref int si, ref int srcLen, ref byte[] data)
	{
		int lowbits = code & 0b1111;
		//0x00 : Set the running total to 0x0F, and read the next byte.
		//From this point on, a 0x00 byte adds 0xFF to the running total,
		//and a non-zero byte adds that value and terminates. Add 3 at the call site.
		if (lowbits == 0)
		{
			byte lastByte;
			for (lastByte = (byte)ReadByte(sb, ref si, ref srcLen, ref data); lastByte == 0; lastByte = (byte)ReadByte(sb, ref si, ref srcLen, ref data))
			{
				lowbits += byte.MaxValue; //0xFF
			}

			lowbits += 0xF + lastByte;
		}
		return lowbits;
	}

	private static int ReadCompressedBytes(int opcode1, int validBits, SrcBuf sb, ref int si, ref int srcLen, ref byte[] data)
	{
		int compressedBytes = opcode1 & validBits;

		if (compressedBytes == 0)
		{
			byte lastByte;
			for (lastByte = (byte)ReadByte(sb, ref si, ref srcLen, ref data); lastByte == 0; lastByte = (byte)ReadByte(sb, ref si, ref srcLen, ref data))
			{
				compressedBytes += byte.MaxValue;
			}

			compressedBytes += lastByte + validBits;
		}

		return compressedBytes + 2;
	}

	private static int TwoByteOffset(ref int offset, int addedValue, SrcBuf sb, ref int si, ref int srcLen, ref byte[] data)
	{
		int firstByte = ReadByte(sb, ref si, ref srcLen, ref data);

		offset |= firstByte >> 2;
		offset |= ReadByte(sb, ref si, ref srcLen, ref data) << 6;
		offset += addedValue;

		return firstByte;
	}

	private static int ReadByte(SrcBuf sb, ref int si, ref int srcLen, ref byte[] data)
	{
		if (si + 1 > srcLen)
		{
			sb.Ensure(si + 1);
			data = sb.Data;
			srcLen = sb.Len;
		}
		return data[si++];
	}

	private static byte[] ReadExactly(Stream s, long size)
	{
		byte[] buf = new byte[Math.Max(0, (int)size)];
		int off = 0;
		while (off < buf.Length)
		{
			int r = s.Read(buf, off, buf.Length - off);
			if (r <= 0)
				throw new EndOfStreamException();
			off += r;
		}
		return buf;
	}
}
