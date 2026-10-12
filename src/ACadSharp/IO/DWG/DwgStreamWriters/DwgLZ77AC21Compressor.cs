// LZ77 compressor for DWG AC1021 (R2007) format.
// Ported from cadcodec (Rust reference implementation, compressor_ac21.rs).
// Stream structure (spec §5.10): literal runs (>= 8 bytes, byte-reordered)
// alternate with match opcode chains; each match opcode carries a 3-bit
// trailing-literal count in the low bits of its last byte.
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
				// Empty input: emit a literal length of 8 with no actual data.
				return new byte[] { 0x00 };
			}

			return new Ac21Compressor(source).compress();
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
			if (srcLen == 0)
			{
				return new byte[] { 0x00 };
			}

			// Inputs < 8 bytes: emit as a single padded literal run of exactly 8 bytes.
			if (srcLen < 8)
			{
				encodeLiteralLength(_output, 8);
				byte[] padded = new byte[8];
				Array.Copy(_source, padded, srcLen);
				for (int i = 0; i < 8; i++)
				{
					_output.Add(padded[i]); // length 8 has identity permutation
				}
				return _output.ToArray();
			}

			int pos = 0;
			bool lastWasMatch = false;

			// The stream must start with a literal run (minimum 8 bytes).
			int firstLiteralEnd = findFirstMatchAfterMinLiteral(pos);

			int literalCount = Math.Min(firstLiteralEnd, srcLen);
			encodeLiteralLength(_output, (uint)literalCount);
			emitLiterals(pos, literalCount);
			pos = literalCount;

			while (pos < srcLen)
			{
				var (matchLen, matchOff) = findMatch(pos);

				if (matchLen < MIN_MATCH || pos + matchLen > srcLen)
				{
					// Accumulate literals until a match is found and >= 8 bytes accumulated.
					int literalStart = pos;
					insertHash(pos);
					pos++;

					while (pos < srcLen)
					{
						var (ml, _) = findMatch(pos);
						if (ml >= MIN_MATCH && (pos - literalStart) >= 8)
						{
							break;
						}
						insertHash(pos);
						pos++;
					}

					int litLen = pos - literalStart;

					if (litLen >= 8)
					{
						emitLongLiteralRun(literalStart, litLen);
						lastWasMatch = false;
					}
					else if (pos >= srcLen && lastWasMatch && litLen <= 7)
					{
						// End of input with < 8 unmatched bytes after a match:
						// carry them via the last match's trailing literal field.
						patchTrailingLiteral((uint)litLen);
						emitTrailingLiterals(literalStart, litLen);
					}
					else if (pos >= srcLen)
					{
						// End of input, no preceding match to patch: padded literal.
						int paddedLen = Math.Max(litLen, 8);
						encodeLiteralLength(_output, (uint)paddedLen);
						int @base = _output.Count;
						for (int i = 0; i < paddedLen; i++)
						{
							_output.Add(0);
						}
						byte[] table = _reorder[paddedLen];
						for (int d = 0; d < litLen; d++)
						{
							_output[@base + table[d]] = _source[literalStart + d];
						}
						lastWasMatch = false;
					}
					else
					{
						throw new InvalidOperationException("literal accumulation ended with lit_len < 8 and pos < src_len");
					}

					continue;
				}

				// Emit the match.
				emitMatchSequence(pos, matchLen, matchOff);
				lastWasMatch = true;
				int end = pos + (int)matchLen;
				for (int i = pos; i < end; i++)
				{
					insertHash(i);
				}
				pos = end;

				// Chain more matches using trailing literal counts.
				while (true)
				{
					if (pos >= srcLen)
					{
						break;
					}

					var (nextLen, nextOff) = findMatch(pos);

					if (nextLen >= MIN_MATCH)
					{
						emitChainedMatch(nextLen, nextOff);
						int e = pos + (int)nextLen;
						for (int i = pos; i < e; i++)
						{
							insertHash(i);
						}
						pos = e;
						continue;
					}

					// No match at current pos: look ahead 1-7 bytes for a match.
					int gap = 1;
					bool foundAfterGap = false;
					while (gap <= 7 && pos + gap < srcLen)
					{
						var (gl, go) = findMatch(pos + gap);
						if (gl >= MIN_MATCH)
						{
							patchTrailingLiteral((uint)gap);
							emitTrailingLiterals(pos, gap);
							for (int i = pos; i < pos + gap; i++)
							{
								insertHash(i);
							}
							pos += gap;

							// Fresh copy_decompressed_chunks call: no 0xF_ → 0x0_ unmask,
							// so use emit_match_sequence (which allows upper nibble 0).
							emitMatchSequence(pos, gl, go);
							int e2 = pos + (int)gl;
							for (int i = pos; i < e2; i++)
							{
								insertHash(i);
							}
							pos = e2;
							foundAfterGap = true;
							break;
						}
						gap++;
					}

					if (!foundAfterGap)
					{
						int remaining = srcLen - pos;
						if (remaining > 0 && remaining <= 7)
						{
							patchTrailingLiteral((uint)remaining);
							emitTrailingLiterals(pos, remaining);
							for (int i = pos; i < srcLen; i++)
							{
								insertHash(i);
							}
							pos = srcLen;
						}
						break;
					}
				}
			}

			return _output.ToArray();
		}

		/// <summary>Find the first position >= 8 where a match exists.</summary>
		private int findFirstMatchAfterMinLiteral(int start)
		{
			int srcLen = _source.Length;
			if (srcLen <= 8)
			{
				return srcLen;
			}

			for (int i = start; i < start + Math.Min(8, srcLen); i++)
			{
				insertHash(i);
			}

			int pos = start + 8;
			while (pos + 2 < srcLen)
			{
				insertHash(pos);
				var (ml, _) = findMatch(pos);
				if (ml >= MIN_MATCH)
				{
					return pos;
				}
				pos++;
			}
			return srcLen;
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
