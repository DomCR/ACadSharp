using System;

namespace ACadSharp.IO.DWG.DwgStreamWriters;

internal class ReedSolomon
{
	public uint BlockSize { get; }

	public uint CodewordLength { get; }

	private uint ParityLength => this.CodewordLength - this.BlockSize;

	public static readonly byte[] PrimitivePolynomialA = new byte[8] { 1, 0, 0, 1, 0, 1, 1, 0 };

	public static readonly byte[] PrimitivePolynomialB = new byte[8] { 1, 0, 1, 1, 1, 0, 0, 0 };

	public static readonly ReedSolomon Rs255_239 = new ReedSolomon(PrimitivePolynomialA, 8u, 8u);

	public static readonly ReedSolomon Rs255_251 = new ReedSolomon(PrimitivePolynomialB, 8u, 2u);

	private uint[] _alphaTo;

	private uint[] _alphaToExtended;

	private uint _errorCorrection;

	private uint[] _generator;

	private uint[] _indexOf;

	private byte[] _primitivePolynomial;

	private uint _symbolBits = 8u;

	public ReedSolomon(byte[] primitivePolynomial, uint symbolBits, uint errorCorrection)
	{
		this._primitivePolynomial = primitivePolynomial;
		this._symbolBits = symbolBits;
		this.CodewordLength = (uint)((1 << (int)symbolBits) - 1);
		this._errorCorrection = errorCorrection;
		this.BlockSize = this.CodewordLength - 2 * this._errorCorrection;
		this._alphaTo = new uint[this.CodewordLength + 1];
		this._indexOf = new uint[this.CodewordLength + 1];
		this._generator = new uint[this.CodewordLength - this.BlockSize + 1];
		this.generateGaloisField();
		this.generatePolynomial();

		this._alphaToExtended = new uint[2 * this.CodewordLength];
		for (int i = 0; i < this._alphaToExtended.Length; i++)
		{
			this._alphaToExtended[i] = this._alphaTo[i % this.CodewordLength];
		}
	}

	public byte[] Encode(byte[] data, uint dataOffset)
	{
		byte[] parity = new byte[this.ParityLength];
		this.Encode(data, dataOffset, parity, 0u);
		return parity;
	}

	public void Encode(byte[] data, uint dataOffset, byte[] parity, uint parityOffset)
	{
		uint[] alphaTo = this._alphaToExtended;
		uint[] indexOf = this._indexOf;
		uint[] generator = this._generator;
		int parityLength = (int)this.ParityLength;
		int baseOffset = (int)parityOffset;
		int last = baseOffset + parityLength - 1;

		for (int dataIndex = (int)(dataOffset + this.BlockSize - 1); dataIndex >= (int)dataOffset; dataIndex--)
		{
			uint feedback = indexOf[data[dataIndex] ^ parity[last]];
			if (feedback != uint.MaxValue)
			{
				for (int j = parityLength - 1; j > 0; j--)
				{
					uint g = generator[j];
					if (g != uint.MaxValue)
					{
						parity[baseOffset + j] = (byte)(parity[baseOffset + j - 1] ^ alphaTo[g + feedback]);
					}
					else
					{
						parity[baseOffset + j] = parity[baseOffset + j - 1];
					}
				}
				parity[baseOffset] = (byte)alphaTo[generator[0] + feedback];
			}
			else
			{
				Array.Copy(parity, baseOffset, parity, baseOffset + 1, parityLength - 1);
				parity[baseOffset] = 0;
			}
		}
	}

	private void generateGaloisField()
	{
		uint mask = 1u;
		this._alphaTo[this._symbolBits] = 0u;
		for (uint i = 0u; i < this._symbolBits; i++)
		{
			this._alphaTo[i] = mask;
			this._indexOf[mask] = i;
			if (this._primitivePolynomial[i] != 0)
			{
				this._alphaTo[this._symbolBits] ^= mask;
			}
			mask <<= 1;
		}
		this._indexOf[this._alphaTo[this._symbolBits]] = this._symbolBits;
		mask >>= 1;
		for (uint i = this._symbolBits + 1; i < this.CodewordLength; i++)
		{
			uint previous = this._alphaTo[i - 1];
			uint value;
			if (previous >= mask)
			{
				value = this._alphaTo[this._symbolBits] ^ ((previous ^ mask) << 1);
			}
			else
			{
				value = previous << 1;
			}
			this._alphaTo[i] = value;
			this._indexOf[value] = i;
		}
		this._indexOf[0] = uint.MaxValue;
	}

	private void generatePolynomial()
	{
		this._generator[0] = 2u;
		this._generator[1] = 1u;
		for (uint i = 2u; i <= this.ParityLength; i++)
		{
			this._generator[i] = 1u;
			for (uint j = i - 1; j != 0; j--)
			{
				if (this._generator[j] != 0)
				{
					this._generator[j] = this._generator[j - 1] ^ this._alphaTo[(this._indexOf[this._generator[j]] + i) % this.CodewordLength];
				}
				else
				{
					this._generator[j] = this._generator[j - 1];
				}
			}
			this._generator[0] = this._alphaTo[(this._indexOf[this._generator[0]] + i) % this.CodewordLength];
		}
		for (uint i = 0u; i <= this.ParityLength; i++)
		{
			this._generator[i] = this._indexOf[this._generator[i]];
		}
	}
}