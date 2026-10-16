using System;
using System.Text;

namespace ACadSharp.IO.DWG.Tangent
{
	/// <summary>
	/// Reads a Tianzheng object's string stream, where the class writes its numbers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An object's bytes are split into three parts: what the class writes inline, a stream of
	/// strings, and the handle section. A Tianzheng class puts its doubles in the middle part,
	/// written as text, so a reader that walks the bytes looking for a number finds none — and a
	/// reader that walks them looking for text finds the numbers, misaligned, as whatever the bits
	/// happen to say.
	/// </para>
	/// <para>
	/// The order of the stream is the write order, so the fields of a class can be read by walking
	/// it in the order <c>dwgInFields</c> reads them. That is what makes a class's field list, and
	/// not a set of offsets, the thing to know.
	/// </para>
	/// <para>
	/// This reader is deliberately minimal: the primitives the Tianzheng classes actually use, and
	/// no attempt at the whole DWG object format, which the library already reads elsewhere.
	/// </para>
	/// </remarks>
	internal sealed class TchStreamReader
	{
		private readonly byte[] _payload;

		private long _position;

		/// <summary>
		/// Initialises a reader over a payload, positioned at a bit offset.
		/// </summary>
		/// <param name="payload">The object's bytes, as they are stored.</param>
		/// <param name="startBit">Where to start reading.</param>
		public TchStreamReader(byte[] payload, long startBit)
		{
			_payload = payload ?? throw new ArgumentNullException(nameof(payload));
			_position = startBit;
		}

		/// <summary>
		/// Gets the current position, in bits.
		/// </summary>
		public long PositionInBits => _position;

		/// <summary>
		/// Gets the number of bits in the payload.
		/// </summary>
		public long LengthInBits => (long)_payload.Length * 8L;

		/// <summary>
		/// Reads a single bit.
		/// </summary>
		public bool ReadBit()
		{
			return this.readBits(1) != 0;
		}

		/// <summary>
		/// Reads a short held in the variable form the format uses.
		/// </summary>
		/// <remarks>
		/// Two bits choose the width, which is how a value of zero costs two bits and a value of
		/// 256 costs two as well.
		/// </remarks>
		public int ReadBitShort()
		{
			int code = (int)this.readBits(2);

			switch (code)
			{
				case 0:
					return (int)this.readBits(16);

				case 1:
					return (int)this.readBits(8);

				case 2:
					return 0;

				default:
					return 256;
			}
		}

		/// <summary>
		/// Reads a short written as two bytes, low first.
		/// </summary>
		/// <remarks>
		/// Two bytes and not sixteen bits: the format writes this one little endian, the way it
		/// writes any other short, so reading it as one run of bits reverses the halves. The size of
		/// a string stream is read this way, and getting it backwards moves the stream's start by
		/// tens of thousands of bits without ever failing outright.
		/// </remarks>
		public int ReadUShort()
		{
			int low = (int)this.readBits(8);

			return low | ((int)this.readBits(8) << 8);
		}

		/// <summary>
		/// Reads a long written as four bytes, low first.
		/// </summary>
		public long ReadRawLong()
		{
			long value = 0;

			for (int i = 0; i < 4; i++)
			{
				value |= (long)this.readBits(8) << (8 * i);
			}

			return value;
		}

		/// <summary>
		/// Reads a double held in the variable form the format uses.
		/// </summary>
		public double ReadBitDouble()
		{
			int code = (int)this.readBits(2);

			switch (code)
			{
				case 0:
					return this.ReadRawDouble();

				case 1:
					return 1.0;

				default:
					return 0.0;
			}
		}

		/// <summary>
		/// Reads a double written as sixty four bits.
		/// </summary>
		public double ReadRawDouble()
		{
			byte[] bytes = new byte[8];

			for (int i = 0; i < 8; i++)
			{
				bytes[i] = (byte)this.readBits(8);
			}

			return BitConverter.ToDouble(bytes, 0);
		}

		/// <summary>
		/// Reads the ciphertext of a number written through the obfuscation layer.
		/// </summary>
		/// <remarks>
		/// The length is a short and the characters are two bytes each, as any other string in the
		/// stream. What makes it ciphertext is the character values, not the framing.
		/// </remarks>
		public string ReadCipherString()
		{
			int length = this.ReadBitShort();

			if (length <= 0)
			{
				return string.Empty;
			}

			StringBuilder text = new StringBuilder(length);

			for (int i = 0; i < length; i++)
			{
				text.Append((char)this.readBits(16));
			}

			return text.ToString();
		}

		private long readBits(int count)
		{
			if (_position + count > this.LengthInBits)
			{
				throw new InvalidOperationException(
					$"Reading {count} bits at {_position} runs past the end of a payload of {this.LengthInBits} bits");
			}

			long value = 0;

			for (int i = 0; i < count; i++)
			{
				long bit = _position + i;
				int octet = _payload[bit >> 3];

				//The format packs bits high first within each byte.
				int taken = (octet >> (7 - (int)(bit & 7))) & 1;

				value = (value << 1) | (uint)taken;
			}

			_position += count;

			return value;
		}
	}
}
