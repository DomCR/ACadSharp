using System;

namespace CSUtilities.Converters
{
	internal class InverseConverter : IEndianConverter
	{
		public byte[] GetBytes(char value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(ulong value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public byte[] GetBytes(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			InverseConverter.fullInverse(bytes);
			return bytes;
		}
		public char ToChar(byte[] arr) => BitConverter.ToChar(InverseConverter.fullInverse(arr), 0);
		public short ToInt16(byte[] arr) => BitConverter.ToInt16(InverseConverter.fullInverse(arr), 0);
		public ushort ToUInt16(byte[] arr) => BitConverter.ToUInt16(InverseConverter.fullInverse(arr), 0);
		public int ToInt32(byte[] arr) => BitConverter.ToInt32(InverseConverter.fullInverse(arr), 0);
		public uint ToUInt32(byte[] arr) => BitConverter.ToUInt32(InverseConverter.fullInverse(arr), 0);
		public long ToInt64(byte[] arr) => BitConverter.ToInt64(InverseConverter.fullInverse(arr), 0);
		public ulong ToUInt64(byte[] arr) => BitConverter.ToUInt64(InverseConverter.fullInverse(arr), 0);
		public double ToDouble(byte[] arr) => BitConverter.ToDouble(InverseConverter.fullInverse(arr), 0);
		public float ToSingle(byte[] arr) => BitConverter.ToSingle(InverseConverter.fullInverse(arr), 0);
		public char ToChar(byte[] arr, int offset) => BitConverter.ToChar(InverseConverter.fullInverse(arr), offset);
		public short ToInt16(byte[] arr, int offset) => BitConverter.ToInt16(InverseConverter.fullInverse(arr, offset), 0);
		public ushort ToUInt16(byte[] arr, int offset) => BitConverter.ToUInt16(InverseConverter.fullInverse(arr, offset), 0);
		public int ToInt32(byte[] arr, int offset) => BitConverter.ToInt32(InverseConverter.fullInverse(arr, offset), 0);
		public uint ToUInt32(byte[] arr, int offset) => BitConverter.ToUInt32(InverseConverter.fullInverse(arr, offset), 0);
		public long ToInt64(byte[] arr, int offset) => BitConverter.ToInt64(InverseConverter.fullInverse(arr, offset), 0);
		public ulong ToUInt64(byte[] arr, int offset) => BitConverter.ToUInt64(InverseConverter.fullInverse(arr, offset), 0);
		public double ToDouble(byte[] arr, int offset) => BitConverter.ToDouble(InverseConverter.fullInverse(arr, offset), 0);
		public float ToSingle(byte[] arr, int offset) => BitConverter.ToSingle(InverseConverter.fullInverse(arr, offset), 0);
		private static byte[] fullInverse(byte[] arr)
		{
			byte[] inverse = new byte[arr.Length];

			for (int i = arr.Length - 1, j = 0; j < arr.Length; i--, j++)
			{
				inverse[i] = arr[j];
			}

			return inverse;
		}
		private static byte[] fullInverse(byte[] arr, int offset)
		{
			byte[] inverse = new byte[arr.Length];

			for (int i = arr.Length - 1, j = 0; j < arr.Length; i--, j++)
			{
				inverse[i] = arr[offset - j];
			}

			return inverse;
		}
	}
}
