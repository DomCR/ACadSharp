// LZ77 compressor for DWG AC1021 (R2007) format.
// Reverse-engineered against native AutoCAD files (DrawingR2007.dwg /
// sampleR2007.dwg) at the opcode level; stream structure verified with the
// opcode disassembler (lz77dis/pagedis) against our own decompressor, which
// decodes all native streams byte-exactly.
//
// Stream structure (mirrors DwgLZ77AC21Decompressor):
// - Stream start: either a 4-byte degenerate opcode "20 00 00 N" carrying the
//   initial literal count N (1-7) in the low 3 bits of its last byte, or —
//   when the first match occurs at position >= 8 — a plain literal-run opcode
//   directly (native large streams start with e.g. "0F 06 ..." = run of 29).
// - Literal runs: opcode 0x00-0x17 (length = op + 8; op 0x0F extends with
//   255/65535 suffixes), bytes reordered in 32-byte blocks.
// - Match chains: opcode (upper nibble 1-14) + operands; the last operand
//   byte doubles as "nextOp": its low 3 bits carry the trailing-literal count
//   (1-7, bytes inlined after the opcode group), upper nibble selects the
//   next instruction: 1-14 = next match, 0 = end of chain (the byte itself
//   becomes the next literal-run opcode), 15 (0xF_) = unmask to nibble-0 and
//   decode as a long match (19-50). A match ending with trailing = 0 and
//   source exhausted simply omits nextOp.
// - Degradation: when the compressed size reaches the input size the input is
//   stored as-is (readers detect comp == uncomp and copy raw; native does the
//   same, e.g. the 6-byte AcDb:Template page is stored verbatim).
using System;
using System.IO;

namespace ACadSharp.IO.DWG
{
	/// <summary>
	/// <see cref="DwgLZ77AC21Decompressor"/> counterpart.
	/// </summary>
	internal class DwgLZ77AC21Compressor : ICompressor
	{
		public void Compress(byte[] source, int offset, int totalSize, Stream dest)
		{
			byte[] src = source;
			if (offset != 0 || totalSize != source.Length)
			{
				src = new byte[totalSize];
				Array.Copy(source, offset, src, 0, totalSize);
			}

			byte[] compressed = Ac21Compressor.Compress(src);
			dest.Write(compressed, 0, compressed.Length);
		}
	}

	/// <summary>Static implementation of the AC1021 LZ77 variant (spec §5.10).</summary>
	internal sealed class Ac21Compressor
	{
		private const int HASH_SIZE = 1 << 15;
		private const int HASH_MASK = HASH_SIZE - 1;
		private const int MIN_MATCH = 3;
		private const int MAX_CHAIN = 64;
		private const int MAX_OFFSET = 65536;
		private const int WINDOW_MASK = MAX_OFFSET - 1;

		private readonly byte[] _source;
		private readonly System.Collections.Generic.List<byte> _output;
		private readonly int[] _head = new int[HASH_SIZE];
		private readonly int[] _prev = new int[MAX_OFFSET];
		private readonly byte[][] _reorder = new byte[32][];

		private Ac21Compressor(byte[] source)
		{
			_source = source;
			_output = new System.Collections.Generic.List<byte>(source.Length);
			for (int i = 0; i < _head.Length; i++)
			{
				_head[i] = -1;
			}
			for (int i = 0; i < _prev.Length; i++)
			{
				_prev[i] = -1;
			}
			for (int n = 0; n < 32; n++)
			{
				_reorder[n] = buildInversePermutation(n);
			}
		}

		public static byte[] Compress(byte[] source)
		{
			if (source.Length == 0)
			{
				return Array.Empty<byte>();
			}

			byte[] compressed = new Ac21Compressor(source).compress();

			// Degradation (native behaviour): when compression does not pay off
			// the data is stored verbatim; readers see compSize == uncompSize and
			// copy it raw (e.g. the native 6-byte AcDb:Template page).
			if (compressed.Length >= source.Length)
			{
				return source;
			}

			return compressed;
		}

		// ------------------------------------------------------------------
		// Inverse byte-reordering tables
		// ------------------------------------------------------------------

		/// <summary>
		/// Simulate the decompressor's copy on an identity permutation to build
		/// the permutation, then use it directly: forward[d] is the compressed
		/// position where uncompressed byte d should be placed.
		/// </summary>
		private static byte[] buildInversePermutation(int n)
		{
			byte[] forward = new byte[32];

			void assign1(int si, int di) => forward[di] = (byte)si;
			void assign2(int si, int di) { forward[di] = (byte)(si + 1); forward[di + 1] = (byte)si; }
			void assign3(int si, int di) { forward[di] = (byte)(si + 2); forward[di + 1] = (byte)(si + 1); forward[di + 2] = (byte)si; }
			void assign4(int si, int di) { for (int i = 0; i < 4; i++) forward[di + i] = (byte)(si + i); }
			void assign8(int si, int di) { assign4(si, di); assign4(si + 4, di + 4); }
			void assign16(int si, int di) { assign8(si + 8, di); assign8(si, di + 8); }

			switch (n)
			{
				case 0: break;
				case 1: assign1(0, 0); break;
				case 2: assign2(0, 0); break;
				case 3: assign3(0, 0); break;
				case 4: assign4(0, 0); break;
				case 5: assign1(4, 0); assign4(0, 1); break;
				case 6: assign1(5, 0); assign4(1, 1); assign1(0, 5); break;
				case 7: assign2(5, 0); assign4(1, 2); assign1(0, 6); break;
				case 8: assign8(0, 0); break;
				case 9: assign1(8, 0); assign8(0, 1); break;
				case 10: assign1(9, 0); assign8(1, 1); assign1(0, 9); break;
				case 11: assign2(9, 0); assign8(1, 2); assign1(0, 10); break;
				case 12: assign4(8, 0); assign8(0, 4); break;
				case 13: assign1(12, 0); assign4(8, 1); assign8(0, 5); break;
				case 14: assign1(13, 0); assign4(9, 1); assign8(1, 5); assign1(0, 13); break;
				case 15: assign2(13, 0); assign4(9, 2); assign8(1, 6); assign1(0, 14); break;
				case 16: assign16(0, 0); break;
				case 17: assign8(9, 0); assign1(8, 8); assign8(0, 9); break;
				case 18: assign1(17, 0); assign16(1, 1); assign1(0, 17); break;
				case 19: assign3(16, 0); assign16(0, 3); break;
				case 20: assign4(16, 0); assign8(8, 4); assign8(0, 12); break;
				case 21: assign1(20, 0); assign4(16, 1); assign8(8, 5); assign8(0, 13); break;
				case 22: assign2(20, 0); assign4(16, 2); assign8(8, 6); assign8(0, 14); break;
				case 23: assign3(20, 0); assign4(16, 3); assign8(8, 7); assign8(0, 15); break;
				case 24: assign8(16, 0); assign16(0, 8); break;
				case 25: assign8(17, 0); assign1(16, 8); assign16(0, 9); break;
				case 26: assign1(25, 0); assign8(17, 1); assign1(16, 9); assign16(0, 10); break;
				case 27: assign2(25, 0); assign8(17, 2); assign1(16, 10); assign16(0, 11); break;
				case 28: assign4(24, 0); assign8(16, 4); assign8(8, 12); assign8(0, 20); break;
				case 29: assign1(28, 0); assign4(24, 1); assign8(16, 5); assign8(8, 13); assign8(0, 21); break;
				case 30: assign2(28, 0); assign4(24, 2); assign8(16, 6); assign8(8, 14); assign8(0, 22); break;
				case 31:
					assign1(30, 0); assign4(26, 1); assign8(18, 5); assign8(10, 13);
					assign8(2, 21); assign2(0, 29);
					break;
			}

			return forward;
		}

		// ------------------------------------------------------------------
		// Opcode encoders
		// ------------------------------------------------------------------

		private struct MatchEncoding
		{
			public byte[] Bytes;
			public int Len;
		}

		/// <summary>Compact match (upper nibble 3-14): length 3-14, offset 1-512, 2 bytes.</summary>
		private static MatchEncoding encodeCompact(uint length, uint offset, uint trailingLit)
		{
			uint o = offset - 1;
			uint opcode = (length << 4) | (o & 0x0F);
			uint nextOp = ((o >> 4) << 3) | (trailingLit & 0x07);
			return new MatchEncoding { Bytes = new[] { (byte)opcode, (byte)nextOp, (byte)0, (byte)0, (byte)0 }, Len = 2 };
		}

		/// <summary>Short match (upper nibble 1): length 3-18, offset 1-8192, 3 bytes.</summary>
		private static MatchEncoding encodeShort(uint length, uint offset, uint trailingLit)
		{
			uint o = offset - 1;
			uint opcode = 0x10 | ((length - 3) & 0x0F);
			uint byte1 = o & 0xFF;
			uint nextOp = (((o >> 8) & 0x1F) << 3) | (trailingLit & 0x07);
			return new MatchEncoding { Bytes = new[] { (byte)opcode, (byte)byte1, (byte)nextOp, (byte)0, (byte)0 }, Len = 3 };
		}

		/// <summary>Long match (upper nibble 0): length 19-50, offset 1-4096, 3 bytes.</summary>
		private static MatchEncoding encodeLong(uint length, uint offset, uint trailingLit)
		{
			uint o = offset - 1;
			uint lenBase = length - 19;
			uint opcode = lenBase & 0x0F;
			uint byte1 = o & 0xFF;
			uint extraBit = lenBase >= 16 ? 0x80u : 0;
			uint nextOp = extraBit | (((o >> 8) & 0x0F) << 3) | (trailingLit & 0x07);
			return new MatchEncoding { Bytes = new[] { (byte)opcode, (byte)byte1, (byte)nextOp, (byte)0, (byte)0 }, Len = 3 };
		}

		/// <summary>Extended match (upper nibble 2, bit 3 clear): length 0-255, offset 0-65535, 4 bytes.</summary>
		private static MatchEncoding encodeExtended(uint length, uint offset, uint trailingLit)
		{
			uint opcode = 0x20 | (length & 0x07);
			uint byte1 = offset & 0xFF;
			uint byte2 = (offset >> 8) & 0xFF;
			uint nextOp = (length & 0xF8) | (trailingLit & 0x07);
			return new MatchEncoding { Bytes = new[] { (byte)opcode, (byte)byte1, (byte)byte2, (byte)nextOp, (byte)0 }, Len = 4 };
		}

		/// <summary>Extended-long match (upper nibble 2, bit 3 set): length 256-65791, offset 1-65536, 5 bytes.</summary>
		private static MatchEncoding encodeExtendedLong(uint length, uint offset, uint trailingLit)
		{
			uint o = offset - 1;
			uint lenAdj = length - 0x100;
			uint opcode = 0x28 | (lenAdj & 0x07);
			uint byte1 = o & 0xFF;
			uint byte2 = (o >> 8) & 0xFF;
			uint byte3 = (lenAdj >> 3) & 0xFF;
			uint nextOp = (((lenAdj >> 11) & 0x1F) << 3) | (trailingLit & 0x07);
			return new MatchEncoding { Bytes = new[] { (byte)opcode, (byte)byte1, (byte)byte2, (byte)byte3, (byte)nextOp }, Len = 5 };
		}

		/// <summary>Choose the most compact opcode encoding for a match.</summary>
		private static MatchEncoding encodeMatch(uint length, uint offset, uint trailingLit)
		{
			if (length >= 3 && length <= 14 && offset >= 1 && offset <= 512)
			{
				return encodeCompact(length, offset, trailingLit);
			}
			if (length >= 19 && length <= 50 && offset >= 1 && offset <= 4096)
			{
				return encodeLong(length, offset, trailingLit);
			}
			if (length >= 3 && length <= 18 && offset >= 1 && offset <= 8192)
			{
				return encodeShort(length, offset, trailingLit);
			}
			if (length <= 255 && offset <= 0xFFFF)
			{
				return encodeExtended(length, offset, trailingLit);
			}
			if (length >= 256 && offset >= 1 && offset <= 65536)
			{
				return encodeExtendedLong(length, offset, trailingLit);
			}
			// Fallback: clamp to extended-long maximum
			uint clampedLen = Math.Min(length, 65791);
			uint clampedOff = Math.Max(Math.Min(offset, 65536), 1);
			return encodeExtendedLong(clampedLen, clampedOff, trailingLit);
		}

		/// <summary>Encode a literal run length; opcode byte has upper nibble 0 (0x00-0x0F).</summary>
		private static void encodeLiteralLength(System.Collections.Generic.List<byte> dest, uint length)
		{
			uint @base = length - 8;

			if (@base < 0x0F)
			{
				dest.Add((byte)@base);
			}
			else
			{
				dest.Add(0x0F);
				uint remaining = @base - 0x0F;

				if (remaining < 0xFF)
				{
					dest.Add((byte)remaining);
				}
				else
				{
					dest.Add(0xFF);
					remaining -= 0xFF;

					while (true)
					{
						if (remaining < 0xFFFF)
						{
							dest.Add((byte)(remaining & 0xFF));
							dest.Add((byte)((remaining >> 8) & 0xFF));
							break;
						}
						else
						{
							dest.Add(0xFF);
							dest.Add(0xFF);
							remaining -= 0xFFFF;
						}
					}
				}
			}
		}

		// ------------------------------------------------------------------
		// Hash chain match finder
		// ------------------------------------------------------------------

		private int hash3(int pos)
		{
			int h = (_source[pos]) * 31 * 31 + (_source[pos + 1]) * 31 + (_source[pos + 2]);
			return h & HASH_MASK;
		}

		private void insertHash(int pos)
		{
			if (pos + 2 >= _source.Length)
			{
				return;
			}
			int h = hash3(pos);
			_prev[pos & WINDOW_MASK] = _head[h];
			_head[h] = pos;
		}

		private (uint, uint) findMatch(int pos)
		{
			if (pos + 2 >= _source.Length)
			{
				return (0, 0);
			}

			int h = hash3(pos);
			int chainPos = _head[h];
			uint bestLen = 0;
			uint bestOffset = 0;
			int chainCount = 0;

			uint maxLen = (uint)(_source.Length - pos);
			uint maxMatch = Math.Min(maxLen, 65791u);

			while (chainPos >= 0 && chainCount < MAX_CHAIN)
			{
				int candidate = chainPos;
				uint offset = (uint)(pos - candidate);

				if (offset >= MAX_OFFSET || offset == 0)
				{
					chainPos = _prev[candidate & WINDOW_MASK];
					chainCount++;
					continue;
				}

				// Quick check: compare the byte just past the current best to prune early
				if (bestLen >= MIN_MATCH
					&& pos + bestLen < _source.Length
					&& _source[candidate + bestLen] != _source[pos + bestLen])
				{
					chainPos = _prev[candidate & WINDOW_MASK];
					chainCount++;
					continue;
				}

				uint len = 0;
				while (len < maxMatch && _source[candidate + len] == _source[pos + len])
				{
					len++;
				}

				if (len > bestLen)
				{
					bestLen = len;
					bestOffset = offset;
					if (len == maxMatch)
					{
						break;
					}
				}

				chainPos = _prev[candidate & WINDOW_MASK];
				chainCount++;
			}

			if (bestLen < MIN_MATCH)
			{
				return (0, 0);
			}
			return (bestLen, bestOffset);
		}

		// ------------------------------------------------------------------
		// Literal emission
		// ------------------------------------------------------------------

		private void emitLiterals(int start, int count)
		{
			int @base = _output.Count;
			for (int i = 0; i < count; i++)
			{
				_output.Add(0);
			}

			byte[] src = _source;
			var dst = _output;

			// Apply inverse byte reordering: 32-byte blocks first
			int ofs = 0;
			while (ofs + 32 <= count)
			{
				int si = start + ofs;
				int co = @base + ofs;
				// d[24..28] → c[0..4]
				dst[co + 0] = src[si + 24]; dst[co + 1] = src[si + 25]; dst[co + 2] = src[si + 26]; dst[co + 3] = src[si + 27];
				// d[28..32] → c[4..8]
				dst[co + 4] = src[si + 28]; dst[co + 5] = src[si + 29]; dst[co + 6] = src[si + 30]; dst[co + 7] = src[si + 31];
				// d[16..20] → c[8..12]
				dst[co + 8] = src[si + 16]; dst[co + 9] = src[si + 17]; dst[co + 10] = src[si + 18]; dst[co + 11] = src[si + 19];
				// d[20..24] → c[12..16]
				dst[co + 12] = src[si + 20]; dst[co + 13] = src[si + 21]; dst[co + 14] = src[si + 22]; dst[co + 15] = src[si + 23];
				// d[8..12] → c[16..20]
				dst[co + 16] = src[si + 8]; dst[co + 17] = src[si + 9]; dst[co + 18] = src[si + 10]; dst[co + 19] = src[si + 11];
				// d[12..16] → c[20..24]
				dst[co + 20] = src[si + 12]; dst[co + 21] = src[si + 13]; dst[co + 22] = src[si + 14]; dst[co + 23] = src[si + 15];
				// d[0..4] → c[24..28]
				dst[co + 24] = src[si + 0]; dst[co + 25] = src[si + 1]; dst[co + 26] = src[si + 2]; dst[co + 27] = src[si + 3];
				// d[4..8] → c[28..32]
				dst[co + 28] = src[si + 4]; dst[co + 29] = src[si + 5]; dst[co + 30] = src[si + 6]; dst[co + 31] = src[si + 7];
				ofs += 32;
			}

			// Remainder (1-31 bytes) using inverse permutation table
			int remaining = count - ofs;
			if (remaining > 0)
			{
				byte[] table = _reorder[remaining];
				for (int d = 0; d < remaining; d++)
				{
					dst[@base + ofs + table[d]] = src[start + ofs + d];
				}
			}
		}

		// ------------------------------------------------------------------
		// Main compression
		// ------------------------------------------------------------------

		private byte[] compress()
		{
			int srcLen = _source.Length;

			// ── Stream start ──
			// Position 0 is always a literal (no history). Scan forward to the
			// first match position: if it lies within the first 7 bytes the run
			// is carried by the 4-byte degenerate opcode "20 00 00 N", otherwise
			// by a plain literal-run opcode (native streams do exactly this).
			insertHash(0);
			int firstMatchPos = srcLen;
			for (int p = 1; p < srcLen; p++)
			{
				var (ml, _) = findMatch(p);
				if (ml >= MIN_MATCH)
				{
					firstMatchPos = p;
					break;
				}
				insertHash(p);
			}

			int litCount = firstMatchPos;
			if (litCount <= 7)
			{
				_output.Add(0x20);
				_output.Add(0x00);
				_output.Add(0x00);
				_output.Add((byte)litCount);
			}
			else
			{
				encodeLiteralLength(_output, (uint)litCount);
			}
			emitLiterals(0, litCount);

			if (litCount >= srcLen)
			{
				return _output.ToArray();
			}

			int pos = litCount;

			// Pending literal run, flushed lazily so that a tail of < 8 bytes can
			// be merged into it instead of emitting a second literal-run opcode
			// (which the decompressor would misread as a long match).
			int pendingStart = -1;
			int pendingLen = 0;

			// True while the next byte to be written lands on a "nextOp" slot
			// (previous match ended with trailing = 0); false at a fresh slot
			// (after the start, a literal run, or trailing literals), where
			// upper-nibble-0 opcodes decode as long matches and are allowed.
			bool atNextOp = false;

			// --------------------------------------------------------------
			void flushPending()
			{
				if (pendingStart < 0)
				{
					return;
				}
				emitLongLiteralRun(pendingStart, pendingLen);
				pendingStart = -1;
				pendingLen = 0;
			}

			void advance(int start, int count)
			{
				int end = Math.Min(start + count, srcLen);
				for (int i = start; i < end; i++)
				{
					insertHash(i);
				}
			}

			// --------------------------------------------------------------
			while (pos < srcLen)
			{
				var (matchLen, matchOff) = findMatch(pos);

				if (matchLen >= MIN_MATCH && pendingStart >= 0 && pendingLen < 8 && !atNextOp)
				{
					// A match appears before the pending run reaches the minimum
					// literal length of 8 and there is no match to hang the run
					// on as trailing literals: skip it (consume as literal).
					insertHash(pos);
					pos++;
					pendingLen++;
					continue;
				}

				if (matchLen >= MIN_MATCH && pendingStart >= 0 && pendingLen >= 8)
				{
					flushPending();
				}

				if (matchLen >= MIN_MATCH)
				{
					if (pendingStart >= 0)
					{
						// Pending run of 1-7 bytes right after a match with
						// trailing = 0: carry it as the match's trailing literals.
						patchTrailingLiteral((uint)pendingLen);
						emitTrailingLiterals(pos - pendingLen, pendingLen);
						pendingStart = -1;
						pendingLen = 0;
						atNextOp = false;
					}

					if (atNextOp)
					{
						// Chain continues after trailing = 0: upper nibble 0 would
						// terminate the chain, so fall back to the extended format.
						emitChainedMatch(matchLen, matchOff);
					}
					else
					{
						emitMatchSequence(pos, matchLen, matchOff);
					}

					advance(pos, (int)matchLen);
					pos += (int)matchLen;
					atNextOp = true;
					continue;
				}

				// No match here. While sitting on a nextOp slot with an empty
				// pending run, first look for a match within 7 bytes to carry
				// the gap as trailing literals.
				if (atNextOp && pendingStart < 0)
				{
					int gap = 0;
					uint gapLen = 0, gapOff = 0;
					for (int g = 1; g <= 7 && pos + g < srcLen; g++)
					{
						var (gl, go) = findMatch(pos + g);
						if (gl >= MIN_MATCH)
						{
							gap = g;
							gapLen = gl;
							gapOff = go;
							break;
						}
					}

					if (gap > 0)
					{
						patchTrailingLiteral((uint)gap);
						emitTrailingLiterals(pos, gap);
						advance(pos, gap);
						pos += gap;
						emitMatchSequence(pos, gapLen, gapOff);
						advance(pos, (int)gapLen);
						pos += (int)gapLen;
						atNextOp = true;
						continue;
					}

					if (srcLen - pos <= 7)
					{
						// End of input: carry the tail as trailing literals.
						int tail = srcLen - pos;
						patchTrailingLiteral((uint)tail);
						emitTrailingLiterals(pos, tail);
						advance(pos, tail);
						pos = srcLen;
						atNextOp = true;
						break;
					}

					// Gap >= 8: keep trailing = 0; the pending run's literal-run
					// opcode itself serves as the chain-terminating nextOp.
					atNextOp = false;
				}

				// Accumulate literals.
				if (pendingStart < 0)
				{
					pendingStart = pos;
					pendingLen = 0;
				}
				insertHash(pos);
				pos++;
				pendingLen++;
			}

			flushPending();
			return _output.ToArray();
		}

		private void emitLongLiteralRun(int start, int count)
		{
			encodeLiteralLength(_output, (uint)count);
			emitLiterals(start, count);
		}

		/// <summary>Emit the first match in a sequence (trailing literal initially 0).</summary>
		private void emitMatchSequence(int pos, uint length, uint offset)
		{
			MatchEncoding enc = encodeMatch(length, offset, 0);
			for (int i = 0; i < enc.Len; i++)
			{
				_output.Add(enc.Bytes[i]);
			}
		}

		/// <summary>Emit a chained match; upper nibble 0 would break the chain → use extended format.</summary>
		private void emitChainedMatch(uint length, uint offset)
		{
			MatchEncoding enc = encodeMatch(length, offset, 0);

			if (enc.Bytes[0] >> 4 == 0)
			{
				MatchEncoding ext = encodeExtended(length, offset, 0);
				for (int i = 0; i < ext.Len; i++)
				{
					_output.Add(ext.Bytes[i]);
				}
			}
			else
			{
				for (int i = 0; i < enc.Len; i++)
				{
					_output.Add(enc.Bytes[i]);
				}
			}
		}

		/// <summary>Patch the trailing literal count (low 3 bits of the last emitted match byte).</summary>
		private void patchTrailingLiteral(uint count)
		{
			int last = _output.Count - 1;
			_output[last] = (byte)((_output[last] & 0xF8) | (count & 0x07));
		}

		/// <summary>Emit trailing literal bytes (1-7) with inverse reordering.</summary>
		private void emitTrailingLiterals(int start, int count)
		{
			int @base = _output.Count;
			for (int i = 0; i < count; i++)
			{
				_output.Add(0);
			}
			byte[] table = _reorder[count];
			for (int d = 0; d < count; d++)
			{
				_output[@base + table[d]] = _source[start + d];
			}
		}
	}
}
